#pragma once

#include <chrono>
#include <cstdint>
#include <optional>
#include <unordered_map>

namespace mtg::engine {

class GameClock {
public:
    explicit GameClock(std::chrono::seconds time_per_player);

    void add_player(uint64_t player_id);
    void start_clock(uint64_t player_id);
    void stop_clock();

    void update();

    [[nodiscard]] auto remaining_time(uint64_t player_id) const -> std::chrono::milliseconds;
    [[nodiscard]] auto is_expired(uint64_t player_id) const -> bool;
    [[nodiscard]] auto active_player_id() const -> std::optional<uint64_t> {
        return active_player_id_;
    }
    [[nodiscard]] auto is_enabled() const -> bool {
        return time_per_player_ > std::chrono::seconds{0};
    }

    void add_time(uint64_t player_id, std::chrono::seconds amount);

    void set_action_timeout(std::chrono::seconds timeout) { action_timeout_ = timeout; }
    [[nodiscard]] auto action_time_remaining() const -> std::chrono::milliseconds;
    [[nodiscard]] auto is_action_timeout_expired() const -> bool;
    void reset_action_timer();

private:
    std::chrono::seconds time_per_player_;
    std::unordered_map<uint64_t, std::chrono::milliseconds> remaining_;
    std::optional<uint64_t> active_player_id_;
    std::chrono::steady_clock::time_point last_tick_;
    std::chrono::seconds action_timeout_{0};
    std::chrono::steady_clock::time_point action_start_;
};

}  // namespace mtg::engine
