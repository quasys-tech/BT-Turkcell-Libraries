from __future__ import annotations

import os
import time
from datetime import datetime
from typing import Mapping

from .configuration_manager import BeyondTrustConfigurationManager
from .options import BeyondTrustOptions
from .poc import print_minimal_integration_example


def main() -> int:
    options = BeyondTrustOptions.from_mapping()

    print_banner(options)
    print_minimal_integration_example()

    try:
        with BeyondTrustConfigurationManager.create_and_load() as manager:
            previous_snapshot_hash = ""

            while True:
                snapshot = sorted(
                    (
                        (key, value)
                        for key, value in manager.get_all_properties().items()
                        if key.lower().startswith("bt.")
                    ),
                    key=lambda item: item[0].lower(),
                )

                current_snapshot_hash = "|".join(f"{key}={value}" for key, value in snapshot)
                if current_snapshot_hash != previous_snapshot_hash:
                    print()
                    print(f"[{datetime.now().strftime('%H:%M:%S')}] Snapshot updated. {len(snapshot)} BeyondTrust key(s) loaded.")
                    print_all_keys(snapshot)
                    print_sample_values(dict(snapshot))
                    previous_snapshot_hash = current_snapshot_hash

                if options.refresh_interval_seconds <= 0:
                    break

                time.sleep(2)
    except KeyboardInterrupt:
        print()
        print("Demo app interrupted by user.")

    return 0


def print_banner(options: BeyondTrustOptions) -> None:
    print("============================================================")
    print("DEMO ONLY - RAW SECRET LOGGING ENABLED - DO NOT USE THIS LOGGING STYLE IN PRODUCTION")
    print("============================================================")
    print(f"Auth Mode : {'OAuth / App User' if options.use_app_user else 'Classic API'}")
    print(f"API Url   : {options.api_url if options.api_url.strip() else '<not configured>'}")
    print(f"Refresh   : {options.refresh_interval_seconds} second(s)")
    print()


def print_all_keys(snapshot: list[tuple[str, str]]) -> None:
    print("--- Loaded bt.* keys ---")

    if not snapshot:
        print("No BeyondTrust keys are currently available. Check the required environment variables and API connectivity.")
    else:
        for key, value in snapshot:
            print(f"{key} = {value}")

    print("------------------------")


def print_sample_values(snapshot: Mapping[str, str]) -> None:
    for line in build_example_output_lines(
        snapshot,
        os.getenv("BT_EXAMPLE_ACCOUNT"),
        os.getenv("BT_EXAMPLE_SAFE_PASSWORD"),
        os.getenv("BT_EXAMPLE_SAFE_USERNAME"),
    ):
        print(line)


def build_example_output_lines(
    snapshot: Mapping[str, str | None],
    example_account_key: str | None,
    example_safe_password_key: str | None,
    example_safe_username_key: str | None,
) -> list[str]:
    lines: list[str] = []
    _append_example_output(lines, snapshot, "BT_EXAMPLE_ACCOUNT", "example account", "Managed Account Sample", example_account_key)
    _append_example_output(lines, snapshot, "BT_EXAMPLE_SAFE_PASSWORD", "example password", "Secret Safe Password Sample", example_safe_password_key)
    _append_example_output(lines, snapshot, "BT_EXAMPLE_SAFE_USERNAME", "example username", "Secret Safe Username Sample", example_safe_username_key)
    return lines


def _append_example_output(
    lines: list[str],
    snapshot: Mapping[str, str | None],
    parameter_name: str,
    friendly_name: str,
    sample_label: str,
    configured_key: str | None,
) -> None:
    if configured_key is None or not configured_key.strip():
        lines.append(f"{parameter_name} not set; skipping {friendly_name} output")
        return

    if configured_key not in snapshot:
        lines.append(f"Demo example key not found: {configured_key}")
        return

    lines.append(f"{sample_label} ({configured_key}) = {snapshot[configured_key]}")
