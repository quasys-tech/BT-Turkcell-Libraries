from __future__ import annotations

import pytest

from turkcell_bt_python.auth_parsing import try_parse_api_key
from turkcell_bt_python.configuration_manager import BeyondTrustConfigurationManager
from turkcell_bt_python.options import BeyondTrustOptions


def test_enabled_defaults_to_true() -> None:
    options = BeyondTrustOptions.from_mapping({})

    assert options.enabled is True


def test_invalid_enabled_raises_error() -> None:
    with pytest.raises(ValueError, match="BEYONDTRUST_ENABLED"):
        BeyondTrustOptions.from_mapping({"BEYONDTRUST_ENABLED": "invalid"})


def test_enabled_true_requires_explicit_use_app_user() -> None:
    options = BeyondTrustOptions.from_mapping(
        {
            "BEYONDTRUST_ENABLED": "true",
            "BEYONDTRUST_API_URL": "https://pam.example.com/BeyondTrust/api/public/v3",
            "BEYONDTRUST_API_KEY": "api-key",
        }
    )

    manager = BeyondTrustConfigurationManager(options, snapshot_loader=dict)
    with pytest.raises(ValueError, match="BEYONDTRUST_USE_APP_USER"):
        manager.load()
    manager.close()


def test_explicit_use_app_user_true_selects_oauth_mode() -> None:
    options = BeyondTrustOptions.from_mapping({"BEYONDTRUST_USE_APP_USER": "true"})

    assert options.use_app_user is True
    assert options.use_app_user_configured is True


def test_explicit_use_app_user_false_selects_classic_mode() -> None:
    options = BeyondTrustOptions.from_mapping({"BEYONDTRUST_USE_APP_USER": "false"})

    assert options.use_app_user is False
    assert options.use_app_user_configured is True


def test_missing_oauth_fields_raise_error() -> None:
    options = BeyondTrustOptions.from_mapping(
        {
            "BEYONDTRUST_ENABLED": "true",
            "BEYONDTRUST_API_URL": "https://pam.example.com/BeyondTrust/api/public/v3",
            "BEYONDTRUST_USE_APP_USER": "true",
        }
    )

    manager = BeyondTrustConfigurationManager(options, snapshot_loader=dict)
    with pytest.raises(ValueError) as error:
        manager.load()
    manager.close()

    assert "BEYONDTRUST_CLIENT_ID" in str(error.value)
    assert "BEYONDTRUST_CLIENT_SECRET" in str(error.value)


def test_missing_classic_api_key_raises_error() -> None:
    options = BeyondTrustOptions.from_mapping(
        {
            "BEYONDTRUST_ENABLED": "true",
            "BEYONDTRUST_API_URL": "https://pam.example.com/BeyondTrust/api/public/v3",
            "BEYONDTRUST_USE_APP_USER": "false",
        }
    )

    manager = BeyondTrustConfigurationManager(options, snapshot_loader=dict)
    with pytest.raises(ValueError, match="BEYONDTRUST_API_KEY"):
        manager.load()
    manager.close()


@pytest.mark.parametrize(
    ("key", "value"),
    [
        ("BEYONDTRUST_USE_APP_USER", "invalid"),
        ("BEYONDTRUST_IGNORE_SSL_ERRORS", "invalid"),
        ("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED", "invalid"),
        ("BEYONDTRUST_ALL_SECRETS_ENABLED", "invalid"),
    ],
)
def test_invalid_boolean_values_raise_error(key: str, value: str) -> None:
    with pytest.raises(ValueError, match=key):
        BeyondTrustOptions.from_mapping({key: value})


def test_canonical_refresh_valid_value_is_used() -> None:
    options = BeyondTrustOptions.from_mapping({"BEYONDTRUST_REFRESH_INTERVAL": "240"})

    assert options.refresh_interval_seconds == 240


def test_canonical_refresh_invalid_value_raises_error() -> None:
    with pytest.raises(ValueError, match="BEYONDTRUST_REFRESH_INTERVAL"):
        BeyondTrustOptions.from_mapping({"BEYONDTRUST_REFRESH_INTERVAL": "invalid"})


def test_legacy_refresh_valid_value_is_used_when_canonical_is_missing() -> None:
    options = BeyondTrustOptions.from_mapping({"BT_REFRESH_TIME": "120"})

    assert options.refresh_interval_seconds == 120


def test_legacy_refresh_invalid_value_uses_default_when_canonical_is_missing() -> None:
    options = BeyondTrustOptions.from_mapping({"BT_REFRESH_TIME": "invalid"})

    assert options.refresh_interval_seconds == BeyondTrustOptions.DEFAULT_REFRESH_INTERVAL_SECONDS


def test_both_refresh_values_use_canonical_when_valid() -> None:
    options = BeyondTrustOptions.from_mapping(
        {
            "BEYONDTRUST_REFRESH_INTERVAL": "300",
            "BT_REFRESH_TIME": "120",
        }
    )

    assert options.refresh_interval_seconds == 300


def test_both_refresh_values_raise_when_canonical_is_invalid() -> None:
    with pytest.raises(ValueError, match="BEYONDTRUST_REFRESH_INTERVAL"):
        BeyondTrustOptions.from_mapping(
            {
                "BEYONDTRUST_REFRESH_INTERVAL": "invalid",
                "BT_REFRESH_TIME": "120",
            }
        )


def test_refresh_interval_zero_disables_background_refresh() -> None:
    options = BeyondTrustOptions.from_mapping({"BEYONDTRUST_REFRESH_INTERVAL": "0"})

    assert options.refresh_interval_seconds == 0


def test_raw_api_key_parsing_is_supported() -> None:
    parsed = try_parse_api_key("raw-api-key", None)

    assert parsed is not None
    assert parsed.key == "raw-api-key"
    assert parsed.run_as_user is None
    assert parsed.to_authorization_header() == "PS-Auth key=raw-api-key;"


def test_ps_auth_api_key_input_is_supported() -> None:
    parsed = try_parse_api_key("PS-Auth key=api-key; runas=svc-inline;", None)

    assert parsed is not None
    assert parsed.key == "api-key"
    assert parsed.run_as_user == "svc-inline"


def test_explicit_runas_overrides_inline_runas() -> None:
    parsed = try_parse_api_key("PS-Auth key=api-key; runas=svc-inline;", "svc-explicit")

    assert parsed is not None
    assert parsed.run_as_user == "svc-explicit"
    assert parsed.to_authorization_header() == "PS-Auth key=api-key; runas=svc-explicit;"
