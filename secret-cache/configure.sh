#!/bin/sh

set -e

set -- \
  -u "$SCACHEUSER" \
  -k "$APIKEY" \
  -h "$APPLIANCE_NAME_OR_URL" \
  -a "$SCACHESERVER" \
  -s /opt/pbps/server.pem \
  -p /opt/pbps/server.key

if [ -n "${PASSWORDSAFEVERIFY:-}" ]; then
  set -- "$@" -V "$PASSWORDSAFEVERIFY"
fi

if [ -s /opt/pbps/password-safe-ca.pem ]; then
  set -- "$@" -T /opt/pbps/password-safe-ca.pem
fi

/opt/pbps/pspca cfg "$@"

/opt/pbps/pspca
echo "BeyondTrust Secret Cache Service started."
tail -f /var/opt/pbps/log/pspca.log
#tail -f /dev/null
