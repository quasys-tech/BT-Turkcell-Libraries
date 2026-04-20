FROM registry.redhat.io/ubi8/ubi

RUN dnf install -y rpm openssl net-tools curl  && dnf clean all

COPY beyondtrust-secrets-cache-24.3.16-1.el8.x86_64.rpm /tmp/

RUN rpm -i /tmp/beyondtrust-secrets-cache-24.3.16-1.el8.x86_64.rpm

WORKDIR /opt/pbps

COPY configure.sh /opt/pbps/

RUN chmod +x /opt/pbps/configure.sh

ENV SCACHEUSER=""
ENV APIKEY=""
ENV APPLIANCE_NAME_OR_URL=""
ENV SCACHESERVER=""

EXPOSE 8443
EXPOSE 443

ENTRYPOINT ["/opt/pbps/configure.sh"]
