#pragma once

#include <atomic>
#include <chrono>
#include <condition_variable>
#include <expected>
#include <functional>
#include <memory>
#include <mutex>
#include <string>
#include <thread>
#include <unordered_map>
#include <vector>

#include "engine/card_registry.hpp"
#include "engine/game.hpp"

namespace mtg::util {
class MetricsRegistry;
}  // namespace mtg::util

namespace mtg::engine {

struct GameFinishData {
    std::string game_id;
    uint64_t winner_id{0};
    std::vector<uint64_t> player_ids;
    std::vector<std::vector<std::string>> deck_lists;
    std::vector<int> final_life_totals;
    int duration_seconds{0};
    std::chrono::system_clock::time_point started_at;
};

using GameFinishCallback = std::function<void(const GameFinishData&)>;

class GameManager {
public:
    explicit GameManager(const CardRegistry& registry,
                         mtg::util::MetricsRegistry* metrics = nullptr, int clock_seconds = 0,
                         int max_games = 100);
    ~GameManager();

    GameManager(const GameManager&) = delete;
    GameManager& operator=(const GameManager&) = delete;
    GameManager(GameManager&&) = delete;
    GameManager& operator=(GameManager&&) = delete;

    [[nodiscard]] auto registry() const -> const CardRegistry& { return registry_; }
    [[nodiscard]] auto create_game() -> std::expected<std::string, std::string>;
    [[nodiscard]] auto get_game(const std::string& game_id) -> std::shared_ptr<Game>;
    void remove_game(const std::string& game_id);
    [[nodiscard]] auto active_game_count() const -> size_t;
    struct GameListEntry {
        std::string game_id;
        GameState state;
        int player_count;
    };
    [[nodiscard]] auto list_games() const -> std::vector<GameListEntry>;

    void set_on_game_finish(GameFinishCallback cb) { on_game_finish_ = std::move(cb); }

    void stop_all_games();

    void start_reaper(int disconnect_timeout_seconds, int reap_interval_seconds);
    void stop_reaper();

private:
    auto generate_game_id() -> std::string;
    void reaper_loop();

    const CardRegistry& registry_;
    mtg::util::MetricsRegistry* metrics_{nullptr};
    mutable std::mutex mutex_;
    std::unordered_map<std::string, std::shared_ptr<Game>> games_;
    uint64_t next_id_{1};
    GameFinishCallback on_game_finish_;
    int clock_seconds_{0};
    int max_games_{100};

    std::thread reaper_thread_;
    std::atomic<bool> reaper_running_{false};
    std::mutex reaper_mutex_;
    std::condition_variable reaper_cv_;
    std::chrono::seconds disconnect_timeout_{120};
    std::chrono::seconds reap_interval_{10};
};

}  // namespace mtg::engine
