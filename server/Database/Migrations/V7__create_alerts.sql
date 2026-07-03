CREATE TABLE alerts (
  id                BIGSERIAL      PRIMARY KEY,
  gateway_id        BIGINT         NOT NULL,
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