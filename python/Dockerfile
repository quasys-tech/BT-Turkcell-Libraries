FROM python:3.11-slim AS runtime
WORKDIR /app

COPY . /app
RUN python -m pip install --no-cache-dir .

ENTRYPOINT ["turkcell-bt-poc"]
