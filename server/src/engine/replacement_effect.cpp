#include "engine/replacement_effect.hpp"

#include <algorithm>

namespace mtg::engine {

void ReplacementEffectRegistry::register_effect(ReplacementEffect effect) {
    if (effects_.size() >= max_effects) {
        return;
    }
    effects_.push_back(std::move(effect));
}

void ReplacementEffectRegistry::remove_effects_from(uint64_t source_permanent_id) {
    std::erase_if(effects_, [source_permanent_id](const ReplacementEffect& e) {
        return e.source_permanent_id == source_permanent_id;
    });
}

void ReplacementEffectRegistry::apply_replacements(ReplacementEvent& event) {
    for (auto& effect : effects_) {
        if (effect.event_type != event.type) {
            continue;
        }
        if (effect.source_permanent_id == event.source_id) {
            if (effect.apply(event)) {
                event.replaced = true;
                return;
            }
        }
    }
    for (auto& effect : effects_) {
        if (effect.event_type != event.type) {
            continue;
        }
        if (effect.source_permanent_id == event.source_id) {
            continue;
        }
        if (effect.apply(event)) {
            event.replaced = true;
            return;
        }
    }
}

void ReplacementEffectRegistry::clear() {
    effects_.clear();
}

}  // namespace mtg::engine
