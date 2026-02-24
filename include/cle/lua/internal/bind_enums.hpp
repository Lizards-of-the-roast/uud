#pragma once

#include <cle/core/card_type.hpp>
#include <cle/core/color.hpp>
#include <cle/game/zone.hpp>
#include <cle/triggers/trigger_type.hpp>
#include <sol/sol.hpp>

namespace cle::lua::internal {

inline void register_enums(sol::state& lua) {
    lua.new_enum<core::CardType>("CardType", {{"Creature", core::CardType::Creature},
                                              {"Instant", core::CardType::Instant},
                                              {"Sorcery", core::CardType::Sorcery},
                                              {"Enchantment", core::CardType::Enchantment},
                                              {"Artifact", core::CardType::Artifact},
                                              {"Planeswalker", core::CardType::Planeswalker},
                                              {"Land", core::CardType::Land}});

    lua.new_enum<core::Color>("Color", {{"White", core::Color::White},
                                        {"Blue", core::Color::Blue},
                                        {"Black", core::Color::Black},
                                        {"Red", core::Color::Red},
                                        {"Green", core::Color::Green},
                                        {"Colorless", core::Color::Colorless}});
    // clang-format off
    lua.new_enum<game::Zone>("Zone", {
#define X(val, str) {str, game::Zone::val},
        ZONE_LIST(X)
#undef X
    });

    lua.new_enum<triggers::TriggerType>("TriggerType", {
#define X(method, enum_val) {#enum_val, triggers::TriggerType::enum_val},
        CARD_TRIGGER_LIST(X)
#undef X
    });
    // clang-format on
}

}  // namespace cle::lua::internal
