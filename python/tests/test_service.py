from __future__ import annotations

import json

import pytest

from turkcell_bt_python.options import BeyondTrustOptions
from turkcell_bt_python.service import BeyondTrustService, parse_credential_value, parse_request_id


TEST_PEM = """-----BEGIN CERTIFICATE-----
MIIC6jCCAdKgAwIBAgIJAIxowur/x1WsMA0GCSqGSIb3DQEBCwUAMBcxFTATBgNV
BAMTDGJ0LWphdmEtdGVzdDAeFw0yNjA0MTUxNTU4NDdaFw0yNjA1MTYxNTU4NDda
MBcxFTATBgNVBAMTDGJ0LWphdmEtdGVzdDCCASIwDQYJKoZIhvcNAQEBBQADggEP
ADCCAQoCggEBAJpDZVqd0TwQekEmbiNsYkcgR9xPnVD9NWYmmvEPpZFQWnoFGP/H
kPw9/GUXxvuhev+jgYn/34AqTvmfQTvmjxCJzXZnWnhrQg02JNG9RHB6r26s6S2Q
SU3RxGhIM+Ml89HDwPVHy9v+fyeACNwkk6n7ZifINFpeQKADQM64htChpMo2+JYh
uiYTKe8qdMtD3935ZYVehK1mA8nVaBXKtiizx9MqxC/FbRSI2TALTlQzBw1jf6gi
wjHzEAv/qznLb9M6j7mPT+0/Zp7MoVD3Dj6iubWYmdzG/YWSCOT7OnlKsBShDmIF
mslyrpA5Pf7Vb/WhIKzSi7V6tAaKxngvU7kCAwEAAaM5MDcwCQYDVR0TBAIwADAL
BgNVHQ8EBAMCB4AwHQYDVR0OBBYEFLqZsO9eeYrqCLSAV4GL2PwiuO3sMA0GCSqG
SIb3DQEBCwUAA4IBAQAHWH6Y454QU3mX2Zex8m1pcXJRARv2si14Va3rpt2lBgdq
uMF97G9mOpkWenzMO/y+5sC9IhGqRBLAR8KBcwgr+4kxjuIjg7TDGz5QUrMhHisk
tgX6+6ts2eRjXebz56ViTbJ1FX/w80/1MX/QXiTwePnOSuypM2c3O0TYZNyMgJhC
/dpTlEVOcjlXitCnxTHeUBZPcCPo79SbD8b0ddspX4oGhQyuyu0QCXf98Wg1HIB3
mZdMtO0kfF8+YXA5yncRYxwyDP55/rdOnGLjjTkBYRPxLluBpKXcrBwsvQaEoAfy
lDBe4+VlaVe9XkBAbZCTJmZk3CteUPc7RHvwnRDz
-----END CERTIFICATE-----
"""


class FakeResponse:
    def __init__(self, status_code: int, body: str) -> None:
        self.status_code = status_code
        self.text = body

    def json(self):
        return json.loads(self.text)


class FakeSession:
    def __init__(self, router):
        self.router = router
        self.requests: list[dict[str, object]] = []

    def request(self, method: str, url: str, **kwargs):
        self.requests.append(
            {
                "method": method,
                "url": url,
                "headers": kwargs.get("headers", {}),
                "json": kwargs.get("json"),
                "data": kwargs.get("data"),
                "verify": kwargs.get("verify"),
            }
        )
        return self.router(method, url, kwargs)

    def close(self) -> None:
        return None


def test_classic_api_mode_uses_merged_authorization_header() -> None:
    session = FakeSession(
        lambda method, url, kwargs: FakeResponse(200, "[]")
        if url.endswith("/ManagedAccounts")
        else FakeResponse(200, "{}")
    )
    service = BeyondTrustService(
        BeyondTrustOptions(
            enabled=True,
            api_url="https://pam.example.com/BeyondTrust/api/public/v3",
            use_app_user=False,
            use_app_user_configured=True,
            api_key="raw-api-key",
            run_as_user="svc-demo",
            all_managed_accounts_enabled=True,
        ),
        session=session,
    )

    snapshot = service.fetch_all_secrets()
    service.close()

    assert snapshot == {}
    sign_in_request = next(request for request in session.requests if request["url"].endswith("/Auth/SignAppin"))
    assert sign_in_request["headers"]["Authorization"] == "PS-Auth key=raw-api-key; runas=svc-demo;"


def test_oauth_mode_uses_token_then_bearer_flow() -> None:
    def router(method: str, url: str, kwargs: dict[str, object]) -> FakeResponse:
        if url.endswith("/Auth/Connect/Token"):
            return FakeResponse(200, '{"access_token":"oauth-token"}')
        if url.endswith("/Auth/SignAppin"):
            return FakeResponse(200, "{}")
        return FakeResponse(200, "[]")

    session = FakeSession(router)
    service = BeyondTrustService(
        BeyondTrustOptions(
            enabled=True,
            api_url="https://pam.example.com/BeyondTrust/api/public/v3",
            use_app_user=True,
            use_app_user_configured=True,
            client_id="client-id",
            client_secret="client-secret",
            all_managed_accounts_enabled=True,
        ),
        session=session,
    )

    service.fetch_all_secrets()
    service.close()

    token_request = next(request for request in session.requests if request["url"].endswith("/Auth/Connect/Token"))
    sign_in_request = next(request for request in session.requests if request["url"].endswith("/Auth/SignAppin"))

    assert token_request["headers"]["Authorization"] == "PS-Auth"
    assert sign_in_request["headers"]["Authorization"] == "Bearer oauth-token"


def test_managed_account_conflict_flow_uses_exact_key_naming() -> None:
    def router(method: str, url: str, kwargs: dict[str, object]) -> FakeResponse:
        if url.endswith("/Auth/SignAppin"):
            return FakeResponse(200, "{}")
        if url.endswith("/ManagedAccounts"):
            return FakeResponse(200, '[{"SystemName":"System A","AccountName":"Account A","SystemID":11,"AccountID":22}]')
        if url.endswith("/Requests") and method == "POST":
            return FakeResponse(409, "{}")
        if url.endswith("/Requests") and method == "GET":
            return FakeResponse(200, '[{"RequestID":444,"SystemID":11,"AccountID":22}]')
        if url.endswith("/Credentials/444"):
            return FakeResponse(200, '"managed-password"')
        if url.endswith("/Requests/444/Checkin"):
            return FakeResponse(200, "{}")
        raise AssertionError(f"Unexpected request: {method} {url}")

    session = FakeSession(router)
    service = BeyondTrustService(
        BeyondTrustOptions(
            enabled=True,
            api_url="https://pam.example.com/BeyondTrust/api/public/v3",
            use_app_user=False,
            use_app_user_configured=True,
            api_key="PS-Auth key=api-key; runas=svc-demo;",
            managed_accounts="System A.Account A",
        ),
        session=session,
        sleep=lambda _: None,
    )

    snapshot = service.fetch_all_secrets()
    service.close()

    assert snapshot["bt.acc.System A.Account A"] == "managed-password"


def test_secret_safe_username_falls_back_to_account() -> None:
    def router(method: str, url: str, kwargs: dict[str, object]) -> FakeResponse:
        if url.endswith("/Auth/SignAppin"):
            return FakeResponse(200, "{}")
        if "Secrets-Safe/Secrets?Path=FolderA" in url:
            return FakeResponse(200, '[{"Folder":"FolderA","Title":"TitleA","Account":"account-user","Password":"secret-value"}]')
        raise AssertionError(f"Unexpected request: {method} {url}")

    session = FakeSession(router)
    service = BeyondTrustService(
        BeyondTrustOptions(
            enabled=True,
            api_url="https://pam.example.com/BeyondTrust/api/public/v3",
            use_app_user=False,
            use_app_user_configured=True,
            api_key="api-key",
            secret_safe_paths="FolderA",
        ),
        session=session,
    )

    snapshot = service.fetch_all_secrets()
    service.close()

    assert snapshot["bt.safe.FolderA.TitleA.password"] == "secret-value"
    assert snapshot["bt.safe.FolderA.TitleA.username"] == "account-user"


@pytest.mark.parametrize(
    ("payload", "expected"),
    [
        ('{"RequestID":123}', "123"),
        ('"456"', "456"),
        ("789", "789"),
    ],
)
def test_parse_request_id_supports_supported_payload_shapes(payload: str, expected: str) -> None:
    assert parse_request_id(payload) == expected


@pytest.mark.parametrize(
    ("payload", "expected"),
    [
        ('{"Credential":"secret"}', "secret"),
        ('{"Password":"secret"}', "secret"),
        ('"secret"', "secret"),
    ],
)
def test_parse_credential_value_supports_supported_payload_shapes(payload: str, expected: str) -> None:
    assert parse_credential_value(payload) == expected


def test_certificate_content_is_used_for_secure_verify() -> None:
    service = BeyondTrustService(
        BeyondTrustOptions(
            api_url="https://pam.example.com/BeyondTrust/api/public/v3",
            certificate_content=TEST_PEM,
        ),
        session=FakeSession(lambda method, url, kwargs: FakeResponse(200, "{}")),
    )

    try:
        assert isinstance(service._verify, str)
    finally:
        service.close()


def test_invalid_certificate_content_raises_error_when_ignore_ssl_errors_is_false() -> None:
    with pytest.raises(ValueError, match="BEYONDTRUST_CERTIFICATE_CONTENT"):
        BeyondTrustService(
            BeyondTrustOptions(
                api_url="https://pam.example.com/BeyondTrust/api/public/v3",
                certificate_content="invalid-pem",
            ),
            session=FakeSession(lambda method, url, kwargs: FakeResponse(200, "{}")),
        )


def test_ignore_ssl_errors_opt_in_uses_insecure_verify_mode() -> None:
    service = BeyondTrustService(
        BeyondTrustOptions(
            api_url="https://pam.example.com/BeyondTrust/api/public/v3",
            ignore_ssl_errors=True,
            certificate_content="invalid-pem",
        ),
        session=FakeSession(lambda method, url, kwargs: FakeResponse(200, "{}")),
    )

    try:
        assert service._verify is False
    finally:
        service.close()
