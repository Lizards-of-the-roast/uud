#pragma once

#include <cstdint>
#include <functional>
#include <memory>
#include <optional>
#include <string>
#include <variant>
#include <vector>

#include <cle/core/card.hpp>
#include <cle/triggers/trigger_event.hpp>
#include <sol/sol.hpp>

namespace mtg::engine {

struct SpellEntry {
    std::shared_ptr<cle::core::Card> card;
};

struct ActivatedAbilityEntry {
    uint64_t source_permanent_id;
    int ability_index;
    sol::function effect;
};

struct TriggeredAbilityEntry {
    uint64_t source_id;
    cle::triggers::TriggerType type;
    cle::triggers::TriggerEvent event;
    sol::function effect;
};

struct StackEntry {
    uint64_t id;
    std::variant<SpellEntry, ActivatedAbilityEntry, TriggeredAbilityEntry> content;
    uint64_t controller_id;
    std::vector<uint64_t> targets;
    std::string target_filter;
    std::vector<int> chosen_modes;
    int x_value{0};
    bool kicked{false};
    bool flashback{false};
    bool adventure{false};
};

class TheStack {
public:
    [[nodiscard]] auto push(StackEntry entry) -> uint64_t;
    [[nodiscard]] auto resolve_top() -> std::optional<StackEntry>;
    [[nodiscard]] auto remove(uint64_t entry_id) -> std::optional<StackEntry>;
    [[nodiscard]] auto peek() const -> const StackEntry*;
    [[nodiscard]] auto is_empty() const -> bool { return entries_.empty(); }
    [[nodiscard]] auto size() const -> size_t { return entries_.size(); }
    [[nodiscard]] auto entries() const -> const std::vector<StackEntry>& { return entries_; }
    void clear() { entries_.clear(); }

private:
    std::vector<StackEntry> entries_;
    uint64_t next_id_{1};
};

}  // namespace mtg::engine
