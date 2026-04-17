from __future__ import annotations

from dataclasses import dataclass
from typing import Any, Mapping


def _read_value_ignore_case(payload: Mapping[str, Any], key: str) -> Any | None:
    for current_key, value in payload.items():
        if str(current_key).lower() == key.lower():
            return value

    return None


def _read_integer_ignore_case(payload: Mapping[str, Any], key: str) -> int:
    value = _read_value_ignore_case(payload, key)
    if value is None:
        return 0

    try:
        return int(str(value).strip())
    except (TypeError, ValueError):
        return 0


@dataclass(frozen=True)
class TokenResponseDto:
    access_token: str | None
    expires_in: int = 0
    token_type: str | None = None
    scope: str | None = None

    @classmethod
    def from_dict(cls, payload: Mapping[str, Any]) -> "TokenResponseDto":
        return cls(
            access_token=None if _read_value_ignore_case(payload, "access_token") is None else str(_read_value_ignore_case(payload, "access_token")),
            expires_in=_read_integer_ignore_case(payload, "expires_in"),
            token_type=None if _read_value_ignore_case(payload, "token_type") is None else str(_read_value_ignore_case(payload, "token_type")),
            scope=None if _read_value_ignore_case(payload, "scope") is None else str(_read_value_ignore_case(payload, "scope")),
        )


@dataclass(frozen=True)
class ManagedAccountDto:
    system_id: int
    system_name: str
    account_id: int
    account_name: str

    @classmethod
    def from_dict(cls, payload: Mapping[str, Any]) -> "ManagedAccountDto":
        return cls(
            system_id=_read_integer_ignore_case(payload, "SystemID"),
            system_name=str(_read_value_ignore_case(payload, "SystemName") or ""),
            account_id=_read_integer_ignore_case(payload, "AccountID"),
            account_name=str(_read_value_ignore_case(payload, "AccountName") or ""),
        )


@dataclass(frozen=True)
class SecretSafeItemDto:
    folder: str | None
    title: str | None
    username: str | None
    account: str | None
    password: str | None
    secret_type: str | None

    @classmethod
    def from_dict(cls, payload: Mapping[str, Any]) -> "SecretSafeItemDto":
        def read_string(key: str) -> str | None:
            value = _read_value_ignore_case(payload, key)
            return None if value is None else str(value)

        return cls(
            folder=read_string("Folder"),
            title=read_string("Title"),
            username=read_string("Username"),
            account=read_string("Account"),
            password=read_string("Password"),
            secret_type=read_string("SecretType"),
        )
