CREATE TABLE rate_limit_rules (
  id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  gateway_id          UUID         NOT NULL,
  scope               VARCHAR(20)  NOT NULL,
  api_key_id          UUID         NULL,
  requests_per_window INT          NOT NULL,
  window_seconds      INT          NOT NULL,
  algorithm           VARCHAR(30)  NOT NULL DEFAULT 'sliding-window',
  burst_allowance     INT          NOT NULL DEFAULT 0,
  is_active           BOOLEAN      NOT NULL DEFAULT TRUE,
  created_at          TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_rate_limit_rules_gateways
    FOREIGN KEY (gateway_id) REFERENCES gateways(id) ON DELETE CASCADE,
  CONSTRAINT fk_rate_limit_rules_api_keys
    FOREIGN KEY (api_key_id) REFERENCES api_keys(id) ON DELETE SET NULL
);
CREATE INDEX ix_rate_limit_rules_gateway_id ON rate_limit_rules(gateway_id);