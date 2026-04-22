from __future__ import annotations

from itertools import count

import pytest

from turkcell_bt_python.configuration_manager import BeyondTrustConfigurationManager
from turkcell_bt_python.options import BeyondTrustOptions


def build_classic_options(refresh_interval_seconds: int = 0) -> BeyondTrustOptions:
    return BeyondTrustOptions(
        enabled=True,
        api_url="https://pam.example.com/BeyondTrust/api/public/v3",
        use_app_user=False,
        use_app_user_configured=True,
        api_key="api-key",
        refresh_interval_seconds=refresh_interval_seconds,
    )


def test_initial_load_success_stores_snapshot() -> None:
    manager = BeyondTrustConfigurationManager(
        build_classic_options(),
        snapshot_loader=lambda: {"bt.acc.Sys.Account": "value"},
    )

    try:
        manager.load()

        assert manager.get_property("bt.acc.Sys.Account") == "value"
        assert manager.get_all_properties() == {"bt.acc.Sys.Account": "value"}
    finally:
        manager.close()


def test_initial_load_failure_raises_error() -> None:
    manager = BeyondTrustConfigurationManager(
        build_classic_options(),
        snapshot_loader=lambda: (_ for _ in ()).throw(RuntimeError("simulated initial failure")),
    )

    with pytest.raises(RuntimeError, match="Initial load failed"):
        manager.load()
    manager.close()


def test_refresh_failure_keeps_last_successful_snapshot() -> None:
    calls = count(1)

    def loader() -> dict[str, str]:
        if next(calls) == 1:
            return {"bt.acc.Sys.Account": "stable-value"}
        raise RuntimeError("simulated refresh failure")

    manager = BeyondTrustConfigurationManager(build_classic_options(), snapshot_loader=loader)

    try:
        manager.load()
        manager._refresh_internal()

        assert manager.get_property("bt.acc.Sys.Account") == "stable-value"
        assert "ERROR_" not in (manager.get_property("bt.acc.Sys.Account") or "")
    finally:
        manager.close()


def test_refresh_success_replaces_snapshot_atomically_and_removes_stale_keys() -> None:
    calls = count(1)

    def loader() -> dict[str, str]:
        if next(calls) == 1:
            return {
                "bt.acc.Sys.Account": "first-value",
                "bt.safe.Team.Api.password": "first-password",
            }
        return {"bt.acc.Sys.Account": "second-value"}

    manager = BeyondTrustConfigurationManager(build_classic_options(), snapshot_loader=loader)

    try:
        manager.load()
        manager._refresh_internal()

        assert manager.get_property("bt.acc.Sys.Account") == "second-value"
        assert manager.get_property("bt.safe.Team.Api.password") is None
    finally:
        manager.close()


def test_refresh_interval_zero_disables_background_refresh_thread() -> None:
    manager = BeyondTrustConfigurationManager(
        build_classic_options(refresh_interval_seconds=0),
        snapshot_loader=lambda: {"bt.acc.Sys.Account": "value"},
    )

    try:
        manager.load()

        assert manager._refresh_thread is None
    finally:
        manager.close()


def test_disabled_manager_keeps_empty_snapshot() -> None:
    manager = BeyondTrustConfigurationManager(
        BeyondTrustOptions(enabled=False),
        snapshot_loader=lambda: {"bt.acc.Sys.Account": "value"},
    )

    try:
        manager.load()

        assert manager.get_property("bt.acc.Sys.Account") is None
        assert manager.get_all_properties() == {}
    finally:
        manager.close()
