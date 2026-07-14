CREATE TABLE IF NOT EXISTS api_keys (
  id           UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  gateway_id   UUID         NOT NULL,
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
CREATE UNIQUE INDEX IF NOT EXISTS ix_api_keys_hash ON api_keys(key_hash);
CREATE INDEX IF NOT EXISTS ix_api_keys_gateway_id ON api_keys(gateway_id);