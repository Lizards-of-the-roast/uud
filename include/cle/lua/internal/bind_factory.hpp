#pragma once

#include <cle/core/card_type.hpp>
#include <cle/lua/builder.hpp>
#include <sol/sol.hpp>

namespace cle::lua::internal {

inline void register_factory_functions(sol::state& lua) {
#define FACTORY(type) \
    lua[#type] = [](const std::string& name) { return CardBuilder(name, core::CardType::type); };
    FACTORY(Creature)
    FACTORY(Instant)
    FACTORY(Sorcery)
    FACTORY(Enchantment)
    FACTORY(Artifact)
    FACTORY(Planeswalker)
    FACTORY(Land)
#undef FACTORY
}

}  // namespace cle::lua::internal
