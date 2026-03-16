#pragma once

#include <string_view>

#include <cle/core/card_type.hpp>

namespace cle::game {

// clang-format off
#define ZONE_LIST(X)      \
    X(Library, "Library")   \
    X(Hand, "Hand")         \
    X(Battlefield, "Battlefield") \
    X(Graveyard, "Graveyard")     \
    X(Exile, "Exile")       \
    X(Stack, "Stack")       \
    X(Command, "Command")
// clang-format on

enum class Zone {
#define X(val, _) val,
    ZONE_LIST(X)
#undef X
};

[[nodiscard]] constexpr auto zone_to_string(Zone zone) -> std::string_view {
    switch (zone) {
#define X(val, str) \
    case Zone::val: \
        return str;
        ZONE_LIST(X)
#undef X
    }
    return "Unknown";
}

// cards on the battlefield are permanents
[[nodiscard]] constexpr auto is_public_zone(Zone zone) -> bool {
    return zone == Zone::Battlefield || zone == Zone::Graveyard || zone == Zone::Exile ||
           zone == Zone::Stack || zone == Zone::Command;
}

// cards in hidden zones are not visible to opponents
[[nodiscard]] constexpr auto is_hidden_zone(Zone zone) -> bool {
    return !is_public_zone(zone);
}

// only permanents exist on the battlefield
[[nodiscard]] constexpr auto can_contain_permanents(Zone zone) -> bool {
    return zone == Zone::Battlefield;
}

// types that can exist on the battlefield as permanents
[[nodiscard]] constexpr auto is_permanent_type(core::CardType type) -> bool {
    return type == core::CardType::Creature || type == core::CardType::Artifact ||
           type == core::CardType::Enchantment || type == core::CardType::Planeswalker ||
           type == core::CardType::Land;
}

}  // namespace cle::game
