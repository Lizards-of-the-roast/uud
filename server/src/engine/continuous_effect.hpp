#pragma once

#include <cstdint>
#include <functional>
#include <optional>
#include <string>
#include <vector>

namespace mtg::engine {

class Permanent;

enum class Layer : uint8_t {
    CopyEffects = 1,
    ControlChanging = 2,
    TextChanging = 3,
    TypeChanging = 4,
    ColorChanging = 5,
    AbilityAddingRemoving = 6,
    PowerToughnessChanging = 7,
};

struct ContinuousEffect {
    uint64_t source_permanent_id;
    Layer layer;
    std::optional<uint64_t> until_end_of_turn_id;
    std::function<void(Permanent&)> apply;
};

class ContinuousEffectManager {
public:
    void register_effect(ContinuousEffect effect);
    void remove_effects_from(uint64_t source_permanent_id);
    void remove_end_of_turn_effects(uint64_t turn_id);
    void apply_all(std::vector<std::pair<uint64_t, Permanent&>>& permanents);
    void clear();

private:
    static constexpr size_t max_effects = 512;
    std::vector<ContinuousEffect> effects_;
};

}  // namespace mtg::engine
