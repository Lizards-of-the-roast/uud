#include "engine/game_clock.hpp"

namespace mtg::engine {

GameClock::GameClock(std::chrono::seconds time_per_player)
    : time_per_player_{time_per_player}, last_tick_{std::chrono::steady_clock::now()} {}

void GameClock::add_player(uint64_t player_id) {
    remaining_[player_id] = std::chrono::duration_cast<std::chrono::milliseconds>(time_per_player_);
}

void GameClock::start_clock(uint64_t player_id) {
    update();
    active_player_id_ = player_id;
    last_tick_ = std::chrono::steady_clock::now();
}

void GameClock::stop_clock() {
    update();
    active_player_id_ = std::nullopt;
}

void GameClock::update() {
    if (!active_player_id_) {
        return;
    }

    auto now = std::chrono::steady_clock::now();
    auto elapsed = std::chrono::duration_cast<std::chrono::milliseconds>(now - last_tick_);
    last_tick_ = now;

    auto it = remaining_.find(*active_player_id_);
    if (it != remaining_.end()) {
        it->second -= elapsed;
        if (it->second < std::chrono::milliseconds{0}) {
            it->second = std::chrono::milliseconds{0};
        }
    }
}

auto GameClock::remaining_time(uint64_t player_id) const -> std::chrono::milliseconds {
    auto it = remaining_.find(player_id);
    if (it == remaining_.end()) {
        return std::chrono::milliseconds{0};
    }
    return it->second;
}

auto GameClock::is_expired(uint64_t player_id) const -> bool {
    auto it = remaining_.find(player_id);
    if (it == remaining_.end()) {
        return false;
    }
    return it->second <= std::chrono::milliseconds{0};
}

void GameClock::add_time(uint64_t player_id, std::chrono::seconds amount) {
    auto it = remaining_.find(player_id);
    if (it != remaining_.end()) {
        it->second += std::chrono::duration_cast<std::chrono::milliseconds>(amount);
    }
}

void GameClock::reset_action_timer() {
    action_start_ = std::chrono::steady_clock::now();
}

auto GameClock::action_time_remaining() const -> std::chrono::milliseconds {
    if (action_timeout_ == std::chrono::seconds{0}) {
        return std::chrono::milliseconds::max();
    }
    auto elapsed = std::chrono::steady_clock::now() - action_start_;
    auto timeout_ms = std::chrono::duration_cast<std::chrono::milliseconds>(action_timeout_);
    auto elapsed_ms = std::chrono::duration_cast<std::chrono::milliseconds>(elapsed);
    return std::max(std::chrono::milliseconds{0}, timeout_ms - elapsed_ms);
}

auto GameClock::is_action_timeout_expired() const -> bool {
    if (action_timeout_ == std::chrono::seconds{0}) {
        return false;
    }
    return action_time_remaining() <= std::chrono::milliseconds{0};
}

}  // namespace mtg::engine
