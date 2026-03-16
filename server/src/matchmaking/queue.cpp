#include "matchmaking/queue.hpp"

#include <algorithm>
#include <cmath>

namespace mtg::matchmaking {

auto MatchmakingQueue::add(QueueEntry entry) -> bool {
    const std::lock_guard lock{mutex_};
    if (entries_.size() >= max_queue_size) {
        return false;
    }
    if (std::ranges::any_of(entries_,
                            [id = entry.player_id](const QueueEntry& e) { return e.player_id == id; })) {
        return false;
    }
    entries_.push_back(std::move(entry));
    return true;
}

void MatchmakingQueue::remove(uint64_t player_id) {
    const std::lock_guard lock{mutex_};
    std::erase_if(entries_, [player_id](const QueueEntry& e) { return e.player_id == player_id; });
}

auto MatchmakingQueue::find_match() -> std::optional<std::pair<QueueEntry, QueueEntry>> {
    const std::lock_guard lock{mutex_};
    if (entries_.size() < 2) {
        return std::nullopt;
    }

    auto now = std::chrono::steady_clock::now();
    for (size_t i = 0; i < entries_.size(); ++i) {
        auto wait_i =
            std::chrono::duration_cast<std::chrono::seconds>(now - entries_[i].enqueued_at).count();
        const int range = (100 + (static_cast<int>(wait_i) * 10));
        for (size_t j = i + 1; j < entries_.size(); ++j) {
            if (std::abs(entries_[i].elo - entries_[j].elo) <= range) {
                auto a = entries_[i];
                auto b = entries_[j];
                entries_.erase(entries_.begin() + static_cast<long>(j));
                entries_.erase(entries_.begin() + static_cast<long>(i));
                return std::pair{a, b};
            }
        }
    }
    return std::nullopt;
}

size_t MatchmakingQueue::size() const {
    const std::lock_guard lock{mutex_};
    return entries_.size();
}

auto MatchmakingQueue::contains(uint64_t player_id) const -> bool {
    const std::lock_guard lock{mutex_};
    return std::ranges::any_of(
        entries_, [player_id](const QueueEntry& e) { return e.player_id == player_id; });
}

auto MatchmakingQueue::position(uint64_t player_id) const -> int {
    const std::lock_guard lock{mutex_};
    int pos = 0;
    for (const auto& e : entries_) {
        ++pos;
        if (e.player_id == player_id) {
            return pos;
        }
    }
    return 0;
}

auto MatchmakingQueue::count() const -> int {
    const std::lock_guard lock{mutex_};
    return static_cast<int>(entries_.size());
}

}  // namespace mtg::matchmaking
