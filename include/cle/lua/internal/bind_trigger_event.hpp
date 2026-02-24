#pragma once

#include <print>

#include <cle/core/card.hpp>
#include <cle/game/internal/game_actions.hpp>
#include <cle/triggers/trigger_event.hpp>
#include <sol/sol.hpp>

namespace cle::lua::internal {

inline void register_trigger_event(sol::state& lua) {
    // clang-format off
    lua.new_usertype<triggers::TriggerEvent>("TriggerEvent", sol::no_constructor,
#define X(name) #name, sol::property([](const triggers::TriggerEvent& e) { return e.name; }),
        TRIGGER_EVENT_FIELD_LIST(X)
#undef X
        sol::meta_function::to_string, []() { return "TriggerEvent"; }
    );
    // clang-format on

    lua["print_card"] = [](const core::Card& card) {
        std::println("{}: {} ({})", card.instance_id(), card.name(), card.oracle_text());
    };
}

}  // namespace cle::lua::internal
