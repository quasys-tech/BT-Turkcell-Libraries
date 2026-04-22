from __future__ import annotations

import os
import time

from .configuration_manager import BeyondTrustConfigurationManager
from .options import BeyondTrustOptions


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


def build_output_block(
    manager: BeyondTrustConfigurationManager,
    example_account_key: str,
    example_safe_password_key: str,
    example_safe_username_key: str,
) -> str:
    managed_account_value = manager.get_property(example_account_key) if example_account_key else None
    safe_password_value = manager.get_property(example_safe_password_key) if example_safe_password_key else None
    safe_username_value = manager.get_property(example_safe_username_key) if example_safe_username_key else None

    return "\n".join(
        [
            f"Managed Account Sample ({example_account_key}) = {managed_account_value or ''}",
            f"Secret Safe Password Sample ({example_safe_password_key}) = {safe_password_value or ''}",
            f"Secret Safe Username Sample ({example_safe_username_key}) = {safe_username_value or ''}",
        ]
    )


def main() -> int:
    options = BeyondTrustOptions.from_mapping()
    example_account_key = (os.getenv("BT_EXAMPLE_ACCOUNT") or "").strip()
    example_safe_password_key = (os.getenv("BT_EXAMPLE_SAFE_PASSWORD") or "").strip()
    example_safe_username_key = (os.getenv("BT_EXAMPLE_SAFE_USERNAME") or "").strip()
    previous_output = ""

    try:
        with BeyondTrustConfigurationManager.create_and_load() as manager:
            while True:
                current_output = build_output_block(
                    manager,
                    example_account_key,
                    example_safe_password_key,
                    example_safe_username_key,
                )

                if current_output != previous_output:
                    print(current_output)
                    previous_output = current_output

                if options.refresh_interval_seconds <= 0:
                    break

                time.sleep(1)
    except KeyboardInterrupt:
        return 0

    return 0
