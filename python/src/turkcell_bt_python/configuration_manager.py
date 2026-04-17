from __future__ import annotations

import threading
from contextlib import AbstractContextManager
from types import MappingProxyType
from typing import Callable, Mapping

from .auth_parsing import try_parse_api_key
from .options import BeyondTrustOptions
from .service import BeyondTrustService


SnapshotLoader = Callable[[], dict[str, str]]


class BeyondTrustConfigurationManager(AbstractContextManager["BeyondTrustConfigurationManager"]):
    def __init__(
        self,
        options: BeyondTrustOptions,
        snapshot_loader: SnapshotLoader | None = None,
    ) -> None:
        self._options = options
        self._snapshot_loader = snapshot_loader or self._default_snapshot_loader
        self._snapshot: Mapping[str, str] = MappingProxyType({})
        self._reload_lock = threading.Lock()
        self._stop_event = threading.Event()
        self._refresh_thread: threading.Thread | None = None

    @classmethod
    def create_and_load(
        cls,
        mapping: Mapping[str, object] | None = None,
    ) -> "BeyondTrustConfigurationManager":
        manager = cls(BeyondTrustOptions.from_mapping(mapping))
        manager.load()
        return manager

    def __enter__(self) -> "BeyondTrustConfigurationManager":
        return self

    def __exit__(self, exc_type, exc_value, traceback) -> None:
        self.close()

    def load(self) -> "BeyondTrustConfigurationManager":
        if not self._options.enabled:
            print("[BeyondTrust] Configuration manager is disabled because BEYONDTRUST_ENABLED=false.")
            return self

        missing_settings = self._validate_required_settings()
        if missing_settings:
            print("[BeyondTrust] Configuration manager was not started because required settings are missing.")
            for missing_setting in missing_settings:
                print(f"[BeyondTrust] Missing setting: {missing_setting}")
            raise ValueError("Required settings are missing: " + ", ".join(missing_settings))

        with self._reload_lock:
            snapshot = self._load_snapshot("Initial load")
            self._swap_snapshot(snapshot)
            print(f"[BeyondTrust] Initial load completed. Loaded {len(snapshot)} key(s).")

            if self._options.refresh_interval_seconds > 0:
                self._start_refresh_thread(self._options.refresh_interval_seconds)
                print(f"[BeyondTrust] Background refresh enabled with {self._options.refresh_interval_seconds}s interval.")
            else:
                print("[BeyondTrust] Background refresh is disabled because BEYONDTRUST_REFRESH_INTERVAL=0.")

        return self

    def get_property(self, key: str) -> str | None:
        return self._snapshot.get(key)

    def get_all_properties(self) -> dict[str, str]:
        return dict(self._snapshot)

    def close(self) -> None:
        self._stop_event.set()
        if self._refresh_thread and self._refresh_thread.is_alive():
            self._refresh_thread.join(timeout=1)

    def _default_snapshot_loader(self) -> dict[str, str]:
        with BeyondTrustService(self._options) as service:
            return service.fetch_all_secrets()

    def _validate_required_settings(self) -> list[str]:
        missing_settings: list[str] = []

        if not self._options.api_url.strip():
            missing_settings.append("BEYONDTRUST_API_URL")

        if not self._options.use_app_user_configured:
            missing_settings.append("BEYONDTRUST_USE_APP_USER")
        elif self._options.use_app_user:
            if not self._options.client_id or not self._options.client_id.strip():
                missing_settings.append("BEYONDTRUST_CLIENT_ID")

            if not self._options.client_secret or not self._options.client_secret.strip():
                missing_settings.append("BEYONDTRUST_CLIENT_SECRET")
        elif try_parse_api_key(self._options.api_key, self._options.run_as_user) is None:
            missing_settings.append("BEYONDTRUST_API_KEY")

        return missing_settings

    def _start_refresh_thread(self, period_seconds: int) -> None:
        if self._refresh_thread and self._refresh_thread.is_alive():
            return

        self._refresh_thread = threading.Thread(
            target=self._refresh_loop,
            args=(period_seconds,),
            name="BeyondTrust-Refresher",
            daemon=True,
        )
        self._refresh_thread.start()

    def _refresh_loop(self, period_seconds: int) -> None:
        while not self._stop_event.wait(period_seconds):
            self._refresh_internal()

    def _refresh_internal(self) -> None:
        with self._reload_lock:
            previous_snapshot = dict(self._snapshot)
            try:
                snapshot = self._load_snapshot("Refresh")
            except Exception:
                print("[BeyondTrust] Refresh failed. Keeping the last successful snapshot.")
                return

            if previous_snapshot == snapshot:
                print("[BeyondTrust] Refresh completed with no snapshot changes.")
                return

            self._swap_snapshot(snapshot)
            print(f"[BeyondTrust] Refresh completed. Loaded {len(snapshot)} key(s).")

    def _load_snapshot(self, operation: str) -> dict[str, str]:
        try:
            loaded_snapshot = self._snapshot_loader() or {}
            return dict(loaded_snapshot)
        except Exception as exc:
            print(f"[BeyondTrust] {operation} failed: {exc}")
            raise RuntimeError(f"{operation} failed: {exc}") from exc

    def _swap_snapshot(self, snapshot: dict[str, str]) -> None:
        self._snapshot = MappingProxyType(dict(snapshot))
