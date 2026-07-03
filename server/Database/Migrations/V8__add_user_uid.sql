CREATE EXTENSION IF NOT EXISTS "pgcrypto";

ALTER TABLE users
ADD COLUMN IF NOT EXISTS uid UUID NOT NULL DEFAULT gen_random_uuid();

UPDATE users
SET uid = gen_random_uuid()
WHERE uid IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ix_users_uid ON users(uid);
