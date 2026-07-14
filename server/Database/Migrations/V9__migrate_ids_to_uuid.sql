-- V9: Migrate all table IDs from BIGSERIAL/BIGINT to UUID
-- V1 (users) and V8 (users.uid) already use UUID — no changes needed there.
-- This migration drops and recreates tables V2–V7 with UUID primary keys.
-- Safe to run: tables are empty at this stage (fresh environment).

-- ============================================================
-- Step 1: Drop all dependent tables in reverse FK order
-- ============================================================
DROP TABLE IF EXISTS alerts;
DROP TABLE IF EXISTS request_logs;
DROP TABLE IF EXISTS rate_limit_rules;
DROP TABLE IF EXISTS api_keys;
DROP TABLE IF EXISTS route_configs;
DROP TABLE IF EXISTS gateways;

-- ============================================================
-- Step 2: Remove stale schema_version entries so migrations
--         V2–V7 re-run cleanly with the new UUID schema
-- ============================================================
DELETE FROM schema_version
WHERE version IN (
    'V2__create_gateways.sql',
    'V3__create_route_configs.sql',
    'V4__create_api_keys.sql',
    'V5__create_rate_limit_rules.sql',
    'V6__create_request_logs.sql',
    'V7__create_alerts.sql'
);

-- ============================================================
-- Step 3: Recreate gateways with UUID id
-- ============================================================
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE TABLE gateways (
  id                        UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id                   UUID         NOT NULL,
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
CREATE INDEX ix_gateways_user_id ON gateways(user_id);

-- ============================================================
-- Step 4: Recreate route_configs with UUID ids
-- ============================================================
CREATE TABLE route_configs (
  id           UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
  gateway_id   UUID         NOT NULL,
  path         VARCHAR(500) NOT NULL,
  methods      VARCHAR(100) NOT NULL,
  strip_prefix BOOLEAN      NOT NULL DEFAULT FALSE,
  is_active    BOOLEAN      NOT NULL DEFAULT TRUE,
  created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_route_configs_gateways
    FOREIGN KEY (gateway_id) REFERENCES gateways(id) ON DELETE CASCADE
);
CREATE INDEX ix_route_configs_gateway_id ON route_configs(gateway_id);

-- ============================================================
-- Step 5: Recreate api_keys with UUID ids
-- ============================================================
CREATE TABLE api_keys (
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
CREATE UNIQUE INDEX ix_api_keys_hash ON api_keys(key_hash);
CREATE INDEX ix_api_keys_gateway_id ON api_keys(gateway_id);

-- ============================================================
-- Step 6: Recreate rate_limit_rules with UUID ids
-- ============================================================
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

-- ============================================================
-- Step 7: Recreate request_logs with UUID ids
-- ============================================================
CREATE TABLE request_logs (
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
CREATE INDEX ix_request_logs_gateway_timestamp
  ON request_logs(gateway_id, timestamp DESC);

-- ============================================================
-- Step 8: Recreate alerts with UUID ids
-- ============================================================
CREATE TABLE alerts (
  id                UUID           PRIMARY KEY DEFAULT gen_random_uuid(),
  gateway_id        UUID           NOT NULL,
  name              VARCHAR(200)   NOT NULL,
  metric_type       VARCHAR(50)    NOT NULL,
  threshold_value   DECIMAL(10,2)  NOT NULL,
  threshold_unit    VARCHAR(20)    NOT NULL,
  is_active         BOOLEAN        NOT NULL DEFAULT TRUE,
  last_triggered_at TIMESTAMPTZ    NULL,
  created_at        TIMESTAMPTZ    NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_alerts_gateways
    FOREIGN KEY (gateway_id) REFERENCES gateways(id) ON DELETE CASCADE
);
