#!/bin/bash


/opt/pbps/pspca cfg --help

/opt/pbps/pspca cfg -u "$SCACHEUSER" -k "$APIKEY" -h "$APPLIANCE_NAME_OR_URL" -a "$SCACHESERVER" -s /opt/pbps/server.pem -p /opt/pbps/server.key

/opt/pbps/pspca
echo "BeyondTrust Secret Cache Service started."
tail -f /var/opt/pbps/log/pspca.log
#tail -f /dev/null