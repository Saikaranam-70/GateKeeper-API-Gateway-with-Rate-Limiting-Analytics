CREATE TABLE api_keys (
  id           BIGSERIAL    PRIMARY KEY,
  gateway_id   BIGINT       NOT NULL,
  key_hash     VARCHAR(500) NOT NULL,
  key_prefix   VARCHAR(20)  NOT NULL,
  label        VARCHAR(100) NULL,
  is_active    BOOLEAN      NOT NULL DEFAULT TRUE,
  expires_at   TIMESTAMPTZ  NULL,
  last_used_at TIMESTAMPTZ  NULL,
  created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_api_keys_gateways
    FOREIGN KEY (gateway_id) REFERENCES gateways(id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX ix_api_keys_hash ON api_keys(key_hash);
CREATE INDEX ix_api_keys_gateway_id ON api_keys(gateway_id);