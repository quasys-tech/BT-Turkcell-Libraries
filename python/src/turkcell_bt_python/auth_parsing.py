from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True)
class ParsedApiKey:
    key: str
    run_as_user: str | None = None

    def to_authorization_header(self) -> str:
        if not self.run_as_user:
            return f"PS-Auth key={self.key};"

        return f"PS-Auth key={self.key}; runas={self.run_as_user};"


def try_parse_api_key(raw_value: str | None, explicit_run_as_user: str | None) -> ParsedApiKey | None:
    candidate = (raw_value or "").strip()
    if candidate.lower().startswith("ps-auth"):
        candidate = candidate[len("PS-Auth") :].strip()

    key: str | None = None
    inline_run_as: str | None = None

    for part in candidate.split(";"):
        trimmed = part.strip()
        if not trimmed:
            continue

        if trimmed.lower().startswith("key="):
            key = trimmed[4:].strip()
        elif trimmed.lower().startswith("runas="):
            inline_run_as = trimmed[6:].strip()
        elif not key:
            key = trimmed

    if not key:
        return None

    run_as_user = explicit_run_as_user.strip() if explicit_run_as_user and explicit_run_as_user.strip() else inline_run_as
    normalized_run_as = run_as_user.strip() if run_as_user and run_as_user.strip() else None

    return ParsedApiKey(key=key.strip(), run_as_user=normalized_run_as)
