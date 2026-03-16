#include "engine/the_stack.hpp"

namespace mtg::engine {

uint64_t TheStack::push(StackEntry entry) {
    entry.id = next_id_++;
    entries_.push_back(std::move(entry));
    return entries_.back().id;
}

auto TheStack::resolve_top() -> std::optional<StackEntry> {
    if (entries_.empty()) {
        return std::nullopt;
    }
    auto top = std::move(entries_.back());
    entries_.pop_back();
    return top;
}

auto TheStack::remove(uint64_t entry_id) -> std::optional<StackEntry> {
    for (auto it = entries_.begin(); it != entries_.end(); ++it) {
        if (it->id == entry_id) {
            auto entry = std::move(*it);
            entries_.erase(it);
            return entry;
        }
    }
    return std::nullopt;
}

auto TheStack::peek() const -> const StackEntry* {
    return entries_.empty() ? nullptr : &entries_.back();
}

}  // namespace mtg::engine
