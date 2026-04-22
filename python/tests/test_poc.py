from __future__ import annotations

from turkcell_bt_python.poc import build_output_block


class StubManager:
    def __init__(self, values: dict[str, str]) -> None:
        self._values = values

    def get_property(self, key: str) -> str | None:
        return self._values.get(key)


def test_build_output_block_renders_expected_values() -> None:
    manager = StubManager(
        {
            "bt.acc.RDS.localaccount": "CyberArk123!",
            "bt.safe.DEMO_APP_1_DEV.secret1.password": "secret-password",
            "bt.safe.DEMO_APP_1_TEST.secret1.username": "test",
        }
    )

    output = build_output_block(
        manager,
        "bt.acc.RDS.localaccount",
        "bt.safe.DEMO_APP_1_DEV.secret1.password",
        "bt.safe.DEMO_APP_1_TEST.secret1.username",
    )

    assert "Managed Account Sample (bt.acc.RDS.localaccount) = CyberArk123!" in output
    assert "Secret Safe Password Sample (bt.safe.DEMO_APP_1_DEV.secret1.password) = secret-password" in output
    assert "Secret Safe Username Sample (bt.safe.DEMO_APP_1_TEST.secret1.username) = test" in output


def test_build_output_block_renders_blank_values_for_missing_keys() -> None:
    manager = StubManager({})

    output = build_output_block(
        manager,
        "bt.acc.RDS.localaccount",
        "bt.safe.DEMO_APP_1_DEV.secret1.password",
        "bt.safe.DEMO_APP_1_TEST.secret1.username",
    )

    assert "Managed Account Sample (bt.acc.RDS.localaccount) = " in output
    assert "Secret Safe Password Sample (bt.safe.DEMO_APP_1_DEV.secret1.password) = " in output
    assert "Secret Safe Username Sample (bt.safe.DEMO_APP_1_TEST.secret1.username) = " in output
