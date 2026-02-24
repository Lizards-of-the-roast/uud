#pragma once

#include <cle/core/keywords.hpp>
#include <cle/lua/builder.hpp>
#include <cle/triggers/trigger_type.hpp>
#include <sol/sol.hpp>

namespace cle::lua::internal {

inline void register_card_builder(sol::state& lua) {
    // clang-format off
    lua.new_usertype<CardBuilder>("CardBuilder", sol::no_constructor,
        "mana_cost", &CardBuilder::mana_cost,
        "colors", &CardBuilder::colors,
        "oracle_text", &CardBuilder::oracle_text,
        "flavor_text", &CardBuilder::flavor_text,
        "subtypes", &CardBuilder::subtypes,
        "keywords", &CardBuilder::keywords,
        "power_toughness", &CardBuilder::power_toughness,
#define X(name, _) #name, &CardBuilder::name,
        CARD_TRIGGER_LIST(X)
        CARD_KEYWORD_LIST(X)
#undef X
        "activated_ability", &CardBuilder::activated_ability,
        "static_ability", &CardBuilder::static_ability,
        "modal", &CardBuilder::modal,
        "mode", &CardBuilder::mode,
        "build", &CardBuilder::build
    );
    // clang-format on
}

}  // namespace cle::lua::internal
