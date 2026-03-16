CREATE TABLE IF NOT EXISTS ratings (
    user_id   BIGINT NOT NULL REFERENCES users(id),
    format    VARCHAR(32) NOT NULL DEFAULT 'standard',
    elo       INT NOT NULL DEFAULT 1200,
    wins      INT NOT NULL DEFAULT 0,
    losses    INT NOT NULL DEFAULT 0,
    PRIMARY KEY (user_id, format)
);
