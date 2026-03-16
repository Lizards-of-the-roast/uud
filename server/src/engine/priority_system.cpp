#include "engine/priority_system.hpp"

#include <algorithm>

namespace mtg::engine {

void PrioritySystem::set_player_order(std::vector<uint64_t> order) {
    player_order_ = std::move(order);
}

void PrioritySystem::begin_round(uint64_t active_player_id) {
    consecutive_passes_ = 0;
    auto it = std::ranges::find(player_order_, active_player_id);
    current_index_ =
        it != player_order_.end() ? static_cast<int>(std::distance(player_order_.begin(), it)) : 0;
}

uint64_t PrioritySystem::current_priority_holder() const {
    if (player_order_.empty()) {
        return 0;
    }
    return player_order_[static_cast<size_t>(current_index_) % player_order_.size()];
}

bool PrioritySystem::pass() {
    ++consecutive_passes_;
    if (consecutive_passes_ >= static_cast<int>(player_order_.size())) {
        return true;
    }
    current_index_ = (current_index_ + 1) % static_cast<int>(player_order_.size());
    return false;
}

void PrioritySystem::interrupt() {
    consecutive_passes_ = 0;
}

bool PrioritySystem::all_passed() const {
    return consecutive_passes_ >= static_cast<int>(player_order_.size());
}

void PrioritySystem::skip_to_next() {
    current_index_ = (current_index_ + 1) % static_cast<int>(player_order_.size());
}

}  // namespace mtg::engine
