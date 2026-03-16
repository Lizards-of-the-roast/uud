#include "matchmaking/matchmaker.hpp"

#include "engine/game_manager.hpp"
#include <spdlog/spdlog.h>

namespace mtg::matchmaking {

Matchmaker::Matchmaker(MatchmakingQueue& queue, mtg::engine::GameManager& game_manager)
    : queue_{queue}, game_manager_{game_manager} {}

void Matchmaker::start(int poll_interval_ms) {
    if (running_) {
        return;
    }
    running_ = true;
    worker_ = std::thread([this, poll_interval_ms] { match_loop(poll_interval_ms); });
    spdlog::info("Matchmaker started");
}

void Matchmaker::stop() {
    running_ = false;
    if (worker_.joinable()) {
        worker_.join();
    }
}

void Matchmaker::match_loop(int interval_ms) {
    while (running_) {
        auto match = queue_.find_match();
        if (match) {
            auto& [a, b] = *match;
            auto result = game_manager_.create_game();
            if (!result) {
                spdlog::warn("Matchmaker: cannot create game: {}", result.error());
                const bool requeued_a = queue_.add(a);
                const bool requeued_b = queue_.add(b);

                if (!requeued_a || !requeued_b) {
                    spdlog::error(
                        "Matchmaker: failed to requeue players (a={}, b={}, add_a={}, add_b={})",
                        a.player_id, b.player_id, requeued_a, requeued_b);
                }
            } else {
                auto game = game_manager_.get_game(*result);
                if (game) {
                    game->add_player(a.player_id, a.username);
                    game->add_player(b.player_id, b.username);
                    spdlog::info("Matched players {} and {} -> game {}", a.player_id, b.player_id,
                                 *result);
                    if (on_match_) {
                        on_match_(a.player_id, b.player_id, *result);
                    }
                }
            }
        }
        std::this_thread::sleep_for(std::chrono::milliseconds(interval_ms));
    }
}

}  // namespace mtg::matchmaking
