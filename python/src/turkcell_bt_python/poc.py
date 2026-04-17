from __future__ import annotations

from .configuration_manager import BeyondTrustConfigurationManager


def print_minimal_integration_example() -> None:
    print("Minimal integration example:")
    print(
        """
with BeyondTrustConfigurationManager.create_and_load() as manager:
    managed_password = manager.get_property("bt.acc.<SystemName>.<AccountName>")
    secret_password = manager.get_property("bt.safe.<Folder>.<Title>.password")
        """.strip()
    )
    print()


def main() -> int:
    print_minimal_integration_example()

    with BeyondTrustConfigurationManager.create_and_load() as manager:
        managed_password = manager.get_property("bt.acc.<SystemName>.<AccountName>")
        secret_password = manager.get_property("bt.safe.<Folder>.<Title>.password")

        print(f"Managed account password lookup result: {managed_password}")
        print(f"Secret safe password lookup result: {secret_password}")

    return 0
