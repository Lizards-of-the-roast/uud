#pragma once

#include <string>

#include <sol/sol.hpp>

namespace cle::abilities {

struct StaticAbility {
    std::string description;
    sol::function apply;
};

}  // namespace cle::abilities
