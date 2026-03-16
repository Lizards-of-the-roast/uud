#pragma once

#include <cle/core/card.hpp>
#include <sol/sol.hpp>

namespace cle::lua::internal {

inline void register_card(sol::state& lua) {
    lua.new_usertype<core::Card>(
        "Card", sol::no_constructor, "name", &core::Card::name, "type", &core::Card::type,
        "mana_cost", &core::Card::mana_cost, "colors", &core::Card::colors, "oracle_text",
        &core::Card::oracle_text, "flavor_text", &core::Card::flavor_text, "subtypes",
        &core::Card::subtypes, "keywords", &core::Card::keywords, "creature_stats",
        &core::Card::creature_stats, "instance_id", &core::Card::instance_id, "activated_abilities",
        sol::property([](const core::Card& c) { return c.activated_abilities(); }),
        "static_abilities", sol::property([](const core::Card& c) { return c.static_abilities(); }),
        "modal", sol::property([](const core::Card& c) { return c.modal(); }));
}

}  // namespace cle::lua::internal
