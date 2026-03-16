#pragma once

#include <atomic>
#include <functional>
#include <thread>

#include "matchmaking/queue.hpp"

namespace mtg::engine {
class GameManager;
}  // namespace mtg::engine

namespace mtg::matchmaking {

class Matchmaker {
public:
    using MatchCallback =
        std::function<void(uint64_t player_a, uint64_t player_b, const std::string& game_id)>;

    Matchmaker(MatchmakingQueue& queue, mtg::engine::GameManager& game_manager);

    void start(int poll_interval_ms = 500);
    void stop();
    void set_match_callback(MatchCallback cb) { on_match_ = std::move(cb); }

private:
    void match_loop(int interval_ms);

    MatchmakingQueue& queue_;
    mtg::engine::GameManager& game_manager_;
    std::thread worker_;
    std::atomic<bool> running_{false};
    MatchCallback on_match_;
};

}  // namespace mtg::matchmaking
