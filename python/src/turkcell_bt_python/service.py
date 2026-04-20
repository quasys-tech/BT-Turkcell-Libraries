from __future__ import annotations

import json
import os
import ssl
import tempfile
import time
from contextlib import AbstractContextManager
from typing import Any, Callable, Iterable, Mapping
from urllib.parse import quote

import requests

from .auth_parsing import try_parse_api_key
from .models import ManagedAccountDto, SecretSafeItemDto, TokenResponseDto
from .options import BeyondTrustOptions


def parse_request_id(payload: str | None) -> str:
    if payload is None or not payload.strip():
        return ""

    trimmed = payload.strip()
    try:
        root = json.loads(trimmed)
    except json.JSONDecodeError:
        return trimmed.strip('"')

    if isinstance(root, (int, float, str)):
        return str(root)

    if isinstance(root, Mapping):
        value = _read_value_ignore_case(root, "RequestID")
        return "" if value is None else str(value)

    return ""


def parse_credential_value(payload: str | None) -> str:
    if payload is None or not payload.strip():
        return ""

    trimmed = payload.strip()
    try:
        root = json.loads(trimmed)
    except json.JSONDecodeError:
        return trimmed.strip('"')

    if isinstance(root, str):
        return root

    if isinstance(root, Mapping):
        credential = _read_value_ignore_case(root, "Credential")
        if credential is not None:
            return str(credential)

        password = _read_value_ignore_case(root, "Password")
        if password is not None:
            return str(password)

    return trimmed.strip('"')


class BeyondTrustService(AbstractContextManager["BeyondTrustService"]):
    REQUEST_TIMEOUT_SECONDS = 30

    def __init__(
        self,
        options: BeyondTrustOptions,
        session: requests.Session | None = None,
        sleep: Callable[[float], None] = time.sleep,
    ) -> None:
        self._options = options
        self._session = session or requests.Session()
        self._owns_session = session is None
        self._sleep = sleep
        self._verify: bool | str = True
        self._certificate_file_path: str | None = None
        self._access_token: str | None = None
        self._debug_enabled = (os.getenv("BEYONDTRUST_DEBUG") or "").strip().lower() == "true"
        if hasattr(self._session, "trust_env"):
            self._session.trust_env = False
        self._configure_tls()

    def __enter__(self) -> "BeyondTrustService":
        return self

    def __exit__(self, exc_type, exc_value, traceback) -> None:
        self.close()

    def close(self) -> None:
        if self._owns_session:
            self._session.close()

        if self._certificate_file_path and os.path.exists(self._certificate_file_path):
            os.unlink(self._certificate_file_path)
            self._certificate_file_path = None

    def fetch_all_secrets(self) -> dict[str, str]:
        snapshot: dict[str, str] = {}

        try:
            if self._debug_enabled:
                print(
                    "[BeyondTrust][DEBUG] FetchAllSecrets started. "
                    f"Auth mode: {'OAuth App User' if self._options.use_app_user else 'Classic API'}, "
                    f"Managed accounts enabled: {self._options.all_managed_accounts_enabled}, "
                    f"Managed account list: {self._options.managed_accounts or '<empty>'}, "
                    f"Secret Safe paths: {self._options.secret_safe_paths or '<empty>'}, "
                    f"ApiUrl: {self._options.api_url}"
                )

            self._authenticate()

            if self._options.all_managed_accounts_enabled or _has_value(self._options.managed_accounts):
                self._process_managed_accounts(snapshot)

            if _has_value(self._options.secret_safe_paths):
                self._process_secret_safe(snapshot)

            if self._options.all_secrets_enabled:
                print(
                    "[BeyondTrust] BEYONDTRUST_ALL_SECRETS_ENABLED is accepted for compatibility, "
                    "but Secret Safe loading still uses BEYONDTRUST_SECRET_SAFE_PATHS."
                )

            return snapshot
        except Exception as exc:  # pragma: no cover
            raise RuntimeError(f"BeyondTrust secret loading failed: {exc}") from exc

    def _authenticate(self) -> None:
        if self._debug_enabled:
            print(
                f"[BeyondTrust][DEBUG] Starting authentication using "
                f"{'OAuth App User' if self._options.use_app_user else 'Classic API'} mode."
            )

        if self._options.use_app_user:
            self._login_with_oauth()
            return

        self._login_with_api_key()

    def _login_with_oauth(self) -> None:
        self._access_token = None

        response = self._send_request(
            "Auth/Connect/Token",
            "POST",
            "Auth/Connect/Token",
            headers={"Authorization": "PS-Auth"},
            data={
                "grant_type": "client_credentials",
                "client_id": self._options.client_id or "",
                "client_secret": self._options.client_secret or "",
            },
        )
        _ensure_success(response.status_code, "OAuth token request")

        token_payload = response.json()
        access_token = TokenResponseDto.from_dict(token_payload).access_token if isinstance(token_payload, Mapping) else None
        if not _has_value(access_token):
            raise RuntimeError("OAuth token response did not contain access_token.")

        self._access_token = access_token
        self._post_sign_appin()

    def _login_with_api_key(self) -> None:
        parsed = try_parse_api_key(self._options.api_key, self._options.run_as_user)
        if parsed is None:
            raise RuntimeError("Classic API authentication requires a valid BEYONDTRUST_API_KEY value.")

        self._access_token = None
        if self._debug_enabled:
            print(
                f"[BeyondTrust][DEBUG] Classic API key parsed. RunAs present: {bool(parsed.run_as_user and parsed.run_as_user.strip())}"
            )

        try:
            self._post_sign_appin()
        except Exception as exc:
            print(f"[BeyondTrust] Auth/SignAppin failed in Classic API mode, continuing without it: {exc}")

    def _post_sign_appin(self) -> None:
        response = self._send_request(
            "Auth/SignAppin",
            "POST",
            "Auth/SignAppin",
            headers=self._authorization_headers(),
            json={},
        )
        _ensure_success(response.status_code, "Auth/SignAppin")

    def _process_managed_accounts(self, snapshot: dict[str, str]) -> None:
        response = self._send_request("ManagedAccounts", "GET", "ManagedAccounts", headers=self._authorization_headers())
        _ensure_success(response.status_code, "ManagedAccounts")

        payload = response.json()
        accounts = [
            ManagedAccountDto.from_dict(item)
            for item in payload
            if isinstance(item, Mapping)
        ] if isinstance(payload, list) else []

        for account in self._filter_accounts(accounts):
            password = self._fetch_managed_account_password(account.system_id, account.account_id)
            config_key = f"bt.acc.{account.system_name.strip()}.{account.account_name.strip()}"
            snapshot[config_key] = password

    def _process_secret_safe(self, snapshot: dict[str, str]) -> None:
        for path in _split_values(self._options.secret_safe_paths):
            response = self._send_request(
                f"Secrets-Safe/Secrets?Path={path}",
                "GET",
                f"Secrets-Safe/Secrets?Path={quote(path, safe='')}",
                headers=self._authorization_headers(),
            )
            _ensure_success(response.status_code, f"Secrets-Safe for path '{path}'")

            payload = response.json()
            items = [
                SecretSafeItemDto.from_dict(item)
                for item in payload
                if isinstance(item, Mapping)
            ] if isinstance(payload, list) else []

            for item in items:
                folder = item.folder.strip() if _has_value(item.folder) else path
                title = item.title.strip() if _has_value(item.title) else "Untitled"
                base_key = f"bt.safe.{folder}.{title}"

                snapshot[f"{base_key}.password"] = item.password or ""

                username = item.username.strip() if _has_value(item.username) else (
                    item.account.strip() if _has_value(item.account) else None
                )
                if _has_value(username):
                    snapshot[f"{base_key}.username"] = username

    def _fetch_managed_account_password(self, system_id: int, account_id: int) -> str:
        request_id = ""

        try:
            response = self._send_request(
                "Requests (create)",
                "POST",
                "Requests",
                headers=self._authorization_headers(),
                json={
                    "systemId": system_id,
                    "accountId": account_id,
                    "durationMinutes": 5,
                    "reason": "TurkcellAutoFetch",
                },
            )

            if _is_success(response.status_code):
                request_id = parse_request_id(response.text)
            elif response.status_code in {403, 409}:
                request_id = self._find_existing_request_id(system_id, account_id)
            else:
                raise RuntimeError(f"Request creation failed with status {response.status_code}.")

            if not _has_value(request_id):
                raise RuntimeError("Request ID could not be resolved for the managed account credential flow.")

            for attempt in range(5):
                credential_response = self._send_request(
                    f"Credentials/{request_id}",
                    "GET",
                    f"Credentials/{quote(request_id, safe='')}",
                    headers=self._authorization_headers(),
                )
                if _is_success(credential_response.status_code):
                    return parse_credential_value(credential_response.text)

                self._sleep(attempt + 1)

            raise RuntimeError(f"Credential retrieval failed for RequestID '{request_id}'.")
        finally:
            if _has_value(request_id):
                self._try_check_in(request_id)

    def _try_check_in(self, request_id: str) -> None:
        try:
            response = self._send_request(
                f"Requests/{request_id}/Checkin",
                "PUT",
                f"Requests/{quote(request_id, safe='')}/Checkin",
                headers=self._authorization_headers(),
                json={"reason": "Done"},
            )

            if not _is_success(response.status_code):
                print(f"[BeyondTrust] Check-in failed for RequestID '{request_id}' with status {response.status_code}.")
        except Exception as exc:  # pragma: no cover
            print(f"[BeyondTrust] Check-in failed for RequestID '{request_id}': {exc}")

    def _find_existing_request_id(self, system_id: int, account_id: int) -> str:
        response = self._send_request("Requests (lookup)", "GET", "Requests", headers=self._authorization_headers())
        _ensure_success(response.status_code, "Requests lookup")

        payload = response.json()
        if not isinstance(payload, list):
            return ""

        for item in payload:
            if not isinstance(item, Mapping):
                continue

            current_system_id = _read_integer_ignore_case(item, "SystemID")
            current_account_id = _read_integer_ignore_case(item, "AccountID")
            if current_system_id == system_id and current_account_id == account_id:
                request_id = _read_value_ignore_case(item, "RequestID")
                return "" if request_id is None else str(request_id)

        return ""

    def _filter_accounts(self, all_accounts: Iterable[ManagedAccountDto]) -> list[ManagedAccountDto]:
        if self._options.all_managed_accounts_enabled:
            return list(all_accounts)

        requested_accounts = _split_values(self._options.managed_accounts, separators=(";",))
        if not requested_accounts:
            return []

        requested_lookup = {value.casefold(): value for value in requested_accounts}
        filtered_accounts: list[ManagedAccountDto] = []
        returned_lookup: set[str] = set()

        for account in all_accounts:
            key = f"{account.system_name.strip()}.{account.account_name.strip()}"
            if key.casefold() in requested_lookup:
                filtered_accounts.append(account)
                returned_lookup.add(key.casefold())

        for requested_account in requested_accounts:
            if requested_account.casefold() not in returned_lookup:
                print(f"[BeyondTrust] Managed account was requested but not returned by the API: {requested_account}")

        return filtered_accounts

    def _authorization_headers(self) -> dict[str, str]:
        if self._options.use_app_user:
            if not _has_value(self._access_token):
                raise RuntimeError("OAuth authentication did not produce an access token.")
            return {"Authorization": f"Bearer {self._access_token}"}

        parsed = try_parse_api_key(self._options.api_key, self._options.run_as_user)
        if parsed is None:
            raise RuntimeError("Classic API authentication requires a valid BEYONDTRUST_API_KEY value.")

        return {"Authorization": parsed.to_authorization_header()}

    def _send_request(self, operation_name: str, method: str, path: str, **kwargs: Any) -> requests.Response:
        started_at = time.monotonic()
        if self._debug_enabled:
            print(f"[BeyondTrust][DEBUG] Starting {operation_name}")

        try:
            response = self._request(method, path, **kwargs)
        except requests.Timeout as exc:
            if self._debug_enabled:
                elapsed_ms = int((time.monotonic() - started_at) * 1000)
                print(f"[BeyondTrust][DEBUG] {operation_name} timed out after {elapsed_ms} ms")
            raise RuntimeError(f"{operation_name} timed out after {self.REQUEST_TIMEOUT_SECONDS} seconds.") from exc
        except requests.RequestException as exc:
            if self._debug_enabled:
                elapsed_ms = int((time.monotonic() - started_at) * 1000)
                print(f"[BeyondTrust][DEBUG] {operation_name} failed after {elapsed_ms} ms: {exc}")
            raise RuntimeError(f"{operation_name} failed: {exc}") from exc

        if self._debug_enabled:
            elapsed_ms = int((time.monotonic() - started_at) * 1000)
            print(f"[BeyondTrust][DEBUG] Completed {operation_name} with status {response.status_code} in {elapsed_ms} ms")

        return response

    def _request(self, method: str, path: str, **kwargs: Any) -> requests.Response:
        headers = dict(kwargs.pop("headers", {}))
        return self._session.request(
            method=method,
            url=self._build_url(path),
            headers=headers,
            timeout=self.REQUEST_TIMEOUT_SECONDS,
            verify=self._verify,
            **kwargs,
        )

    def _build_url(self, path: str) -> str:
        return f"{_normalize_base_url(self._options.api_url)}{path}"

    def _configure_tls(self) -> None:
        if self._options.ignore_ssl_errors:
            self._verify = False
            return

        if not _has_value(self._options.certificate_content):
            self._verify = True
            return

        normalized_pem = self._options.certificate_content.replace("\\n", "\n")
        try:
            ssl.create_default_context(cadata=normalized_pem)
        except Exception as exc:
            raise ValueError("BEYONDTRUST_CERTIFICATE_CONTENT does not contain a valid certificate.") from exc

        temp_file = tempfile.NamedTemporaryFile(
            mode="w",
            suffix=".pem",
            delete=False,
            encoding="utf-8",
            newline="\n",
        )
        try:
            temp_file.write(normalized_pem)
        finally:
            temp_file.close()

        self._certificate_file_path = temp_file.name
        self._verify = temp_file.name


def _has_value(value: str | None) -> bool:
    return value is not None and bool(value.strip())


def _split_values(value: str | None, separators: tuple[str, ...] | None = None) -> list[str]:
    if not _has_value(value):
        return []

    actual_separators = separators or (",", ";")
    normalized = value
    for separator in actual_separators:
        normalized = normalized.replace(separator, ",")

    return [item.strip() for item in normalized.split(",") if item.strip()]


def _normalize_base_url(api_url: str | None) -> str:
    if not _has_value(api_url):
        raise RuntimeError("BEYONDTRUST_API_URL must be configured before creating the service.")

    return f"{api_url.rstrip('/')}/"


def _ensure_success(status_code: int, operation_name: str) -> None:
    if not _is_success(status_code):
        raise RuntimeError(f"{operation_name} failed with status {status_code}.")


def _is_success(status_code: int) -> bool:
    return 200 <= status_code < 300


def _read_value_ignore_case(payload: Mapping[str, Any], key: str) -> Any | None:
    for current_key, value in payload.items():
        if str(current_key).lower() == key.lower():
            return value

    return None


def _read_integer_ignore_case(payload: Mapping[str, Any], key: str) -> int:
    value = _read_value_ignore_case(payload, key)
    if value is None:
        return -1

    try:
        return int(str(value).strip())
    except (TypeError, ValueError):
        return -1
