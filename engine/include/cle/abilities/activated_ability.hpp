#pragma once

#include <string>

#include <sol/sol.hpp>

namespace cle::abilities {

struct ActivatedAbility {
    std::string cost_text;
    std::string effect_text;
    sol::function effect;
    bool sorcery_speed_only{false};
    bool is_mana_ability{false};
};

}  // namespace cle::abilities
