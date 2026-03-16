#pragma once

#include <cle/lua/internal/bind_card.hpp>
#include <cle/lua/internal/bind_card_builder.hpp>
#include <cle/lua/internal/bind_enums.hpp>
#include <cle/lua/internal/bind_factory.hpp>
#include <cle/lua/internal/bind_game_context.hpp>
#include <cle/lua/internal/bind_trigger_event.hpp>
#include <sol/sol.hpp>

namespace cle::lua {

inline void register_all(sol::state& lua) {
    internal::register_enums(lua);
    internal::register_card(lua);
    internal::register_card_builder(lua);
    internal::register_factory_functions(lua);
    internal::register_game_context(lua);
    internal::register_trigger_event(lua);
}

}  // namespace cle::lua
