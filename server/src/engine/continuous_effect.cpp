#include "engine/continuous_effect.hpp"

#include <algorithm>

#include "engine/permanent.hpp"

namespace mtg::engine {

void ContinuousEffectManager::register_effect(ContinuousEffect effect) {
    if (effects_.size() >= max_effects) {
        return;
    }
    effects_.push_back(std::move(effect));
}

void ContinuousEffectManager::remove_effects_from(uint64_t source_permanent_id) {
    std::erase_if(effects_, [source_permanent_id](const ContinuousEffect& e) {
        return e.source_permanent_id == source_permanent_id;
    });
}

void ContinuousEffectManager::remove_end_of_turn_effects(uint64_t turn_id) {
    std::erase_if(effects_, [turn_id](const ContinuousEffect& e) {
        return e.until_end_of_turn_id && *e.until_end_of_turn_id == turn_id;
    });
}

void ContinuousEffectManager::apply_all(
    std::vector<std::pair<uint64_t, Permanent&>>& permanents) {
    auto sorted = effects_;
    std::stable_sort(sorted.begin(), sorted.end(),
                     [](const ContinuousEffect& a, const ContinuousEffect& b) {
                         return static_cast<uint8_t>(a.layer) < static_cast<uint8_t>(b.layer);
                     });

    for (const auto& effect : sorted) {
        for (auto& [id, perm] : permanents) {
            effect.apply(perm);
        }
    }
}

void ContinuousEffectManager::clear() {
    effects_.clear();
}

}  // namespace mtg::engine
