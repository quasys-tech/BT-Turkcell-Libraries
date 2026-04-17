from __future__ import annotations

from turkcell_bt_python.demo_app import build_example_output_lines


def test_example_account_output_is_rendered() -> None:
    lines = build_example_output_lines(
        {"bt.acc.SampleSystem.SampleAccount": "demo-password"},
        "bt.acc.SampleSystem.SampleAccount",
        None,
        None,
    )

    assert "Managed Account Sample (bt.acc.SampleSystem.SampleAccount) = demo-password" in lines


def test_example_safe_password_output_is_rendered() -> None:
    lines = build_example_output_lines(
        {"bt.safe.SampleFolder.SampleTitle.password": "demo-secret"},
        None,
        "bt.safe.SampleFolder.SampleTitle.password",
        None,
    )

    assert "Secret Safe Password Sample (bt.safe.SampleFolder.SampleTitle.password) = demo-secret" in lines


def test_example_safe_username_output_is_rendered() -> None:
    lines = build_example_output_lines(
        {"bt.safe.SampleFolder.SampleTitle.username": "demo-user"},
        None,
        None,
        "bt.safe.SampleFolder.SampleTitle.username",
    )

    assert "Secret Safe Username Sample (bt.safe.SampleFolder.SampleTitle.username) = demo-user" in lines


def test_unset_example_safe_username_outputs_skip_message() -> None:
    lines = build_example_output_lines({}, None, None, None)

    assert "BT_EXAMPLE_SAFE_USERNAME not set; skipping example username output" in lines


def test_missing_example_key_outputs_not_found_message() -> None:
    lines = build_example_output_lines({}, None, None, "bt.safe.Missing.Title.username")

    assert "Demo example key not found: bt.safe.Missing.Title.username" in lines
