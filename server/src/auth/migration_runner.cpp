#include "auth/migration_runner.hpp"

#include <spdlog/spdlog.h>

namespace mtg::auth {

MigrationRunner::MigrationRunner(pqxx::connection& conn) : conn_{conn} {}

auto MigrationRunner::all_migrations() -> const std::vector<Migration>& {
    static const std::vector<Migration> migrations = {
        {.version = 1,
         .name = "create_users_table",
         .sql =
             R"(
            CREATE TABLE IF NOT EXISTS users (
                id            BIGSERIAL PRIMARY KEY,
                username      VARCHAR(64) UNIQUE NOT NULL,
                email         VARCHAR(256) NOT NULL DEFAULT '',
                password_hash TEXT NOT NULL,
                created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
            )
        )"},
        {.version = 2,
         .name = "create_ratings_table",
         .sql =
             R"(
            CREATE TABLE IF NOT EXISTS ratings (
                user_id   BIGINT NOT NULL REFERENCES users(id),
                format    VARCHAR(32) NOT NULL DEFAULT 'standard',
                elo       INT NOT NULL DEFAULT 1200,
                wins      INT NOT NULL DEFAULT 0,
                losses    INT NOT NULL DEFAULT 0,
                PRIMARY KEY (user_id, format)
            )
        )"},
        {.version = 3,
         .name = "create_game_history_tables",
         .sql =
             R"(
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
        )"},
    };
    return migrations;
}

void MigrationRunner::ensure_migrations_table() {
    pqxx::work txn{conn_};
    txn.exec0(R"(
        CREATE TABLE IF NOT EXISTS schema_migrations (
            version    INT PRIMARY KEY,
            name       VARCHAR(256) NOT NULL,
            applied_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
        )
    )");
    txn.commit();
}

auto MigrationRunner::current_version() -> int {
    pqxx::work txn{conn_};
    auto result = txn.exec("SELECT COALESCE(MAX(version), 0) FROM schema_migrations");
    txn.commit();
    return result[0][0].as<int>();
}

auto MigrationRunner::run_pending() -> int {
    ensure_migrations_table();

    constexpr int64_t migration_lock_id = 1;
    {
        pqxx::work lock_txn{conn_};
        auto result = lock_txn.exec_params("SELECT pg_try_advisory_lock($1)", migration_lock_id);
        lock_txn.commit();
        if (!result[0][0].as<bool>()) {
            spdlog::info("Another process is running migrations, skipping");
            return 0;
        }
    }

    int current = current_version();
    int applied = 0;

    for (const auto& migration : all_migrations()) {
        if (migration.version <= current) {
            continue;
        }

        spdlog::info("Running migration {}: {}", migration.version, migration.name);
        pqxx::work txn{conn_};
        txn.exec0(migration.sql);
        txn.exec_params("INSERT INTO schema_migrations (version, name) VALUES ($1, $2)",
                        migration.version, migration.name);
        txn.commit();
        applied++;
        spdlog::info("Migration {} applied successfully", migration.version);
    }

    {
        pqxx::work unlock_txn{conn_};
        unlock_txn.exec_params("SELECT pg_advisory_unlock($1)", migration_lock_id);
        unlock_txn.commit();
    }

    if (applied == 0) {
        spdlog::info("Database schema is up to date (version {})", current);
    } else {
        spdlog::info("Applied {} migration(s)", applied);
    }

    return applied;
}

}  // namespace mtg::auth
