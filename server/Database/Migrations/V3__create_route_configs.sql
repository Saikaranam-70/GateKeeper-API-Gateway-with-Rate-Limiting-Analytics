CREATE TABLE route_configs (
  id         UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  gateway_id UUID         NOT NULL,
  path       VARCHAR(500) NOT NULL,
  methods    VARCHAR(100) NOT NULL,
  strip_prefix BOOLEAN    NOT NULL DEFAULT FALSE,
  is_active  BOOLEAN      NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_route_configs_gateways
    FOREIGN KEY (gateway_id) REFERENCES gateways(id) ON DELETE CASCADE
);
CREATE INDEX ix_route_configs_gateway_id ON route_configs(gateway_id);