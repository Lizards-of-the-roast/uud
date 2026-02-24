#pragma once

#include <string>
#include <vector>

#include <sol/sol.hpp>

namespace cle::abilities {

struct ModalChoice {
    std::string text;
    sol::function effect;
};

struct ModalAbility {
    int min_choices{1};
    int max_choices{1};
    std::vector<ModalChoice> modes;
};

}  // namespace cle::abilities
