#pragma once

#include <cle/game/game_context.hpp>
#include <cle/game/internal/game_actions.hpp>
#include <sol/sol.hpp>

namespace cle::lua::internal {

inline void register_game_context(sol::state& lua) {
    // clang-format off
    lua.new_usertype<game::GameContext>("GameContext", sol::no_constructor,
#define X(name) #name, &game::GameContext::name,
        GAME_CONTEXT_ACTION_LIST(X)
#undef X
        "register_delayed_trigger", sol::overload(
            static_cast<void(game::GameContext::*)(uint64_t, const std::string&, const std::string&)>(
                &game::GameContext::register_delayed_trigger),
            static_cast<void(game::GameContext::*)(uint64_t, const std::string&, const std::string&, sol::function)>(
                &game::GameContext::register_delayed_trigger)
        ),
        sol::meta_function::to_string, []() { return "GameContext"; }
    );
    // clang-format on
}

}  // namespace cle::lua::internal
