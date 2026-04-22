from __future__ import annotations

import os
from collections.abc import Mapping
from dataclasses import dataclass
from typing import Any


@dataclass(slots=True)
class BeyondTrustOptions:
    DEFAULT_REFRESH_INTERVAL_SECONDS = 1800

    enabled: bool = True
    api_url: str = ""
    api_key: str = ""
    use_app_user: bool = False
    use_app_user_configured: bool = False
    client_id: str | None = None
    client_secret: str | None = None
    run_as_user: str | None = None
    ignore_ssl_errors: bool = False
    certificate_content: str | None = None
    refresh_interval_seconds: int = DEFAULT_REFRESH_INTERVAL_SECONDS
    managed_accounts: str | None = None
    all_managed_accounts_enabled: bool = False
    secret_safe_paths: str | None = None
    all_secrets_enabled: bool = False

    @classmethod
    def from_mapping(cls, mapping: Mapping[str, Any] | None = None) -> "BeyondTrustOptions":
        source = mapping if mapping is not None else os.environ

        options = cls(
            enabled=_read_boolean(source, "BEYONDTRUST_ENABLED", True),
            api_url=_read_string(source, "BEYONDTRUST_API_URL") or "",
            api_key=_read_string(source, "BEYONDTRUST_API_KEY") or "",
            client_id=_read_string(source, "BEYONDTRUST_CLIENT_ID"),
            client_secret=_read_string(source, "BEYONDTRUST_CLIENT_SECRET"),
            run_as_user=_read_string(source, "BEYONDTRUST_RUNAS_USER"),
            ignore_ssl_errors=_read_boolean(source, "BEYONDTRUST_IGNORE_SSL_ERRORS", False),
            certificate_content=_read_string(source, "BEYONDTRUST_CERTIFICATE_CONTENT"),
            refresh_interval_seconds=_read_refresh_interval(source),
            managed_accounts=_read_string(source, "BEYONDTRUST_MANAGED_ACCOUNTS"),
            all_managed_accounts_enabled=_read_boolean(source, "BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED", False),
            secret_safe_paths=_read_string(source, "BEYONDTRUST_SECRET_SAFE_PATHS"),
            all_secrets_enabled=_read_boolean(source, "BEYONDTRUST_ALL_SECRETS_ENABLED", False),
        )

        use_app_user = _read_explicit_boolean(source, "BEYONDTRUST_USE_APP_USER")
        if use_app_user is not None:
            options.use_app_user = use_app_user
            options.use_app_user_configured = True

        return options


def _read_string(mapping: Mapping[str, Any], key: str) -> str | None:
    value = mapping.get(key)
    if value is None:
        return None

    return str(value)


def _read_boolean(mapping: Mapping[str, Any], key: str, default_value: bool) -> bool:
    value = _read_string(mapping, key)
    if value is None or not value.strip():
        return default_value

    normalized = value.strip().lower()
    if normalized == "true":
        return True
    if normalized == "false":
        return False

    raise ValueError(f"Invalid {key} value. Expected 'true' or 'false'.")


def _read_explicit_boolean(mapping: Mapping[str, Any], key: str) -> bool | None:
    value = _read_string(mapping, key)
    if value is None or not value.strip():
        return None

    normalized = value.strip().lower()
    if normalized == "true":
        return True
    if normalized == "false":
        return False

    raise ValueError(f"Invalid {key} value. Expected 'true' or 'false'.")


def _read_refresh_interval(mapping: Mapping[str, Any]) -> int:
    canonical_value = _read_string(mapping, "BEYONDTRUST_REFRESH_INTERVAL")
    if canonical_value is not None and canonical_value.strip():
        parsed = _try_parse_integer(canonical_value)
        if parsed is not None:
            return parsed

        raise ValueError("Invalid BEYONDTRUST_REFRESH_INTERVAL value. Expected an integer number of seconds.")

    legacy_value = _read_string(mapping, "BT_REFRESH_TIME")
    if legacy_value is not None and legacy_value.strip():
        parsed = _try_parse_integer(legacy_value)
        if parsed is not None:
            return parsed

        return BeyondTrustOptions.DEFAULT_REFRESH_INTERVAL_SECONDS

    return BeyondTrustOptions.DEFAULT_REFRESH_INTERVAL_SECONDS


def _try_parse_integer(raw_value: str) -> int | None:
    try:
        return int(raw_value.strip())
    except ValueError:
        return None
