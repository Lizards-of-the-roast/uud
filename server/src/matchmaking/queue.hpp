#pragma once

#include <chrono>
#include <cstdint>
#include <mutex>
#include <optional>
#include <string>
#include <vector>

namespace mtg::matchmaking {

struct QueueEntry {
    uint64_t player_id{0};
    std::string username;
    std::string deck_id;
    int elo{1200};
    std::chrono::steady_clock::time_point enqueued_at;
};

class MatchmakingQueue {
public:
    static constexpr size_t max_queue_size = 10000;

    [[nodiscard]] auto add(QueueEntry entry) -> bool;
    void remove(uint64_t player_id);
    [[nodiscard]] auto find_match() -> std::optional<std::pair<QueueEntry, QueueEntry>>;
    [[nodiscard]] auto size() const -> size_t;
    [[nodiscard]] auto contains(uint64_t player_id) const -> bool;
    [[nodiscard]] auto position(uint64_t player_id) const -> int;
    [[nodiscard]] auto count() const -> int;

private:
    mutable std::mutex mutex_;
    std::vector<QueueEntry> entries_;
};

}  // namespace mtg::matchmaking
