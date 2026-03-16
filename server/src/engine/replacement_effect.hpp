#pragma once

#include <cstdint>
#include <functional>
#include <string>
#include <vector>

namespace mtg::engine {

enum class ReplacementEventType {
    WouldDraw,
    WouldDie,
    WouldDealDamage,
    WouldGainLife,
    WouldEnterBattlefield,
};

struct ReplacementEvent {
    ReplacementEventType type;
    uint64_t source_id{0};
    uint64_t target_id{0};
    uint64_t player_id{0};
    int amount{0};
    bool replaced{false};
};

struct ReplacementEffect {
    uint64_t source_permanent_id;
    ReplacementEventType event_type;
    std::function<bool(ReplacementEvent&)> apply;
};

class ReplacementEffectRegistry {
public:
    void register_effect(ReplacementEffect effect);
    void remove_effects_from(uint64_t source_permanent_id);
    void apply_replacements(ReplacementEvent& event);
    void clear();

private:
    static constexpr size_t max_effects = 256;
    std::vector<ReplacementEffect> effects_;
};

}  // namespace mtg::engine
