CREATE TABLE IF NOT EXISTS request_logs (
  id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  gateway_id      UUID         NOT NULL,
  api_key_id      UUID         NULL,
  method          VARCHAR(10)  NOT NULL,
  path            VARCHAR(500) NOT NULL,
  status_code     INT          NOT NULL,
  latency_ms      INT          NOT NULL,
  client_ip       VARCHAR(45)  NOT NULL,
  is_rate_limited BOOLEAN      NOT NULL DEFAULT FALSE,
  timestamp       TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_request_logs_gateways
    FOREIGN KEY (gateway_id) REFERENCES gateways(id) ON DELETE CASCADE,
  CONSTRAINT fk_request_logs_api_keys
    FOREIGN KEY (api_key_id) REFERENCES api_keys(id) ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS ix_request_logs_gateway_timestamp
  ON request_logs(gateway_id, timestamp DESC);