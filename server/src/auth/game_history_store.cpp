#include "auth/game_history_store.hpp"

#include <algorithm>
#include <sstream>
#include <unordered_map>

#include "auth/scoped_connection.hpp"
#include <spdlog/spdlog.h>

namespace mtg::auth {

GameHistoryStore::GameHistoryStore(const std::string& connection_string, int pool_size)
    : pool_{connection_string, pool_size} {
    spdlog::info("GameHistoryStore: initialized with {} connections", pool_size);
}

void GameHistoryStore::record_game(const GameHistoryRecord& record) {
    ScopedConnection sc{pool_.acquire(), [this](pqxx::connection& c) { pool_.release(c); }};
    try {
        pqxx::work txn{sc.get()};

        auto started_epoch =
            std::chrono::duration_cast<std::chrono::seconds>(record.started_at.time_since_epoch())
                .count();

        auto result = txn.exec_params(
            "INSERT INTO game_history (game_id, format, winner_id, duration_seconds, started_at) "
            "VALUES ($1, 'default', $2, $3, to_timestamp($4)) RETURNING id",
            record.game_id, static_cast<int64_t>(record.winner_id), record.duration_seconds,
            started_epoch);

        if (result.empty()) {
            spdlog::error("GameHistoryStore: failed to insert game_history");
            return;
        }

        auto history_id = result[0][0].as<int64_t>();

        for (const auto& player : record.players) {
            std::ostringstream arr;
            arr << "{";
            for (size_t i = 0; i < player.deck_list.size(); ++i) {
                if (i > 0) {
                    arr << ",";
                }
                arr << "\"";
                for (const char c : player.deck_list[i]) {
                    if (c == '"' || c == '\\') {
                        arr << '\\';
                    }
                    arr << c;
                }
                arr << "\"";
            }
            arr << "}";

            txn.exec_params(
                "INSERT INTO game_history_players (game_history_id, player_id, deck_list, "
                "final_life) VALUES ($1, $2, $3::text[], $4)",
                history_id, static_cast<int64_t>(player.player_id), arr.str(), player.final_life);
        }

        txn.commit();

        spdlog::info("GameHistoryStore: recorded game {} (winner={}, {} players)", record.game_id,
                     record.winner_id, record.players.size());
    } catch (const std::exception& e) {
        spdlog::error("GameHistoryStore: record_game failed: {}", e.what());
    }
}

auto GameHistoryStore::get_player_history(uint64_t player_id, int limit, int offset)
    -> std::vector<GameHistoryEntry> {
    constexpr int max_limit = 100;
    if (limit <= 0) {
        limit = 20;
    }
    limit = std::min(limit, max_limit);
    offset = std::max(offset, 0);

    ScopedConnection sc{pool_.acquire(), [this](pqxx::connection& c) { pool_.release(c); }};
    std::vector<GameHistoryEntry> entries;
    try {
        pqxx::work txn{sc.get()};

        auto rows = txn.exec_params(
            "SELECT gh.id, gh.game_id, gh.winner_id, gh.duration_seconds, "
            "       gh.started_at::text, gh.finished_at::text "
            "FROM game_history gh "
            "JOIN game_history_players ghp ON ghp.game_history_id = gh.id "
            "WHERE ghp.player_id = $1 "
            "ORDER BY gh.finished_at DESC "
            "LIMIT $2 OFFSET $3",
            static_cast<int64_t>(player_id), limit, offset);

        std::vector<int64_t> history_ids;
        std::unordered_map<int64_t, size_t> id_to_index;
        for (const auto& row : rows) {
            GameHistoryEntry entry;
            auto history_id = row[0].as<int64_t>();
            entry.game_id = row[1].as<std::string>();
            entry.winner_id = static_cast<uint64_t>(row[2].as<int64_t>());
            entry.duration_seconds = row[3].as<int>();
            entry.started_at = row[4].as<std::string>();
            entry.finished_at = row[5].as<std::string>();

            id_to_index[history_id] = entries.size();
            history_ids.push_back(history_id);
            entries.push_back(std::move(entry));
        }

        if (!history_ids.empty()) {
            std::ostringstream id_list;
            for (size_t i = 0; i < history_ids.size(); ++i) {
                if (i > 0) {
                    id_list << ",";
                }
                id_list << history_ids[i];
            }
            auto player_rows = txn.exec(
                "SELECT game_history_id, player_id, final_life "
                "FROM game_history_players "
                "WHERE game_history_id IN (" + id_list.str() + ")");
            for (const auto& pr : player_rows) {
                auto gid = pr[0].as<int64_t>();
                if (auto it = id_to_index.find(gid); it != id_to_index.end()) {
                    GameHistoryPlayerRecord player;
                    player.player_id = static_cast<uint64_t>(pr[1].as<int64_t>());
                    player.final_life = pr[2].as<int>();
                    entries[it->second].players.push_back(std::move(player));
                }
            }
        }

        txn.commit();
    } catch (const std::exception& e) {
        spdlog::error("GameHistoryStore: get_player_history failed: {}", e.what());
    }
    return entries;
}

auto GameHistoryStore::get_player_game_count(uint64_t player_id) -> int {
    ScopedConnection sc{pool_.acquire(), [this](pqxx::connection& c) { pool_.release(c); }};
    try {
        pqxx::work txn{sc.get()};
        auto result =
            txn.exec_params("SELECT COUNT(*) FROM game_history_players WHERE player_id = $1",
                            static_cast<int64_t>(player_id));
        txn.commit();
        if (!result.empty()) {
            return result[0][0].as<int>();
        }
    } catch (const std::exception& e) {
        spdlog::error("GameHistoryStore: get_player_game_count failed: {}", e.what());
    }
    return 0;
}

}  // namespace mtg::auth
