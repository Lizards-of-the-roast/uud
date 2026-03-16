#pragma once

#include <chrono>
#include <cstdint>
#include <string>
#include <vector>

#include "auth/connection_pool.hpp"

namespace mtg::auth {

struct GameHistoryPlayerRecord {
    uint64_t player_id{0};
    std::vector<std::string> deck_list;
    int final_life{0};
};

struct GameHistoryRecord {
    std::string game_id;
    uint64_t winner_id{0};
    int duration_seconds{0};
    std::chrono::system_clock::time_point started_at;
    std::vector<GameHistoryPlayerRecord> players;
};

struct GameHistoryEntry {
    std::string game_id;
    uint64_t winner_id{0};
    int duration_seconds{0};
    std::string started_at;
    std::string finished_at;
    std::vector<GameHistoryPlayerRecord> players;
};

class GameHistoryStore {
public:
    explicit GameHistoryStore(const std::string& connection_string, int pool_size = 2);

    void record_game(const GameHistoryRecord& record);

    [[nodiscard]] auto get_player_history(uint64_t player_id, int limit = 20, int offset = 0)
        -> std::vector<GameHistoryEntry>;

    [[nodiscard]] auto get_player_game_count(uint64_t player_id) -> int;

private:
    ConnectionPool pool_;
};

}  // namespace mtg::auth
