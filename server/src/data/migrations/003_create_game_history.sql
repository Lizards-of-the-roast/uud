CREATE TABLE IF NOT EXISTS game_history (
    id               BIGSERIAL PRIMARY KEY,
    game_id          VARCHAR(32) NOT NULL,
    format           VARCHAR(32) NOT NULL DEFAULT 'standard',
    winner_id        BIGINT NOT NULL,
    duration_seconds INT NOT NULL,
    started_at       TIMESTAMPTZ NOT NULL,
    finished_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS game_history_players (
    game_history_id  BIGINT NOT NULL REFERENCES game_history(id),
    player_id        BIGINT NOT NULL,
    deck_list        TEXT[] NOT NULL DEFAULT '{}',
    final_life       INT NOT NULL DEFAULT 0,
    PRIMARY KEY (game_history_id, player_id)
);

CREATE INDEX IF NOT EXISTS idx_game_history_game_id ON game_history (game_id);
CREATE INDEX IF NOT EXISTS idx_game_history_players_player_id ON game_history_players (player_id);
