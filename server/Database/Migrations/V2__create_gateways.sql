CREATE TABLE gateways (
  id                        BIGSERIAL    PRIMARY KEY,
  user_id                   UUID         NOT NULL,
  gateway_uid               VARCHAR(50)  NOT NULL,
  name                      VARCHAR(200) NOT NULL,
  description               VARCHAR(500) NULL,
  target_base_url           VARCHAR(500) NOT NULL,
  status                    VARCHAR(20)  NOT NULL DEFAULT 'active',
  default_rate_limit_per_min INT         NULL DEFAULT 100,
  created_at                TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  updated_at                TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_gateways_users
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX ix_gateways_uid ON gateways(gateway_uid);
CREATE INDEX ix_gateways_user_id ON gateways(user_id);