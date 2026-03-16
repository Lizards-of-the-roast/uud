#pragma once

#include <format>
#include <string>

namespace cle::core {

// creatures have power and toughness
struct CreatureStats {
    int power{0};
    int toughness{0};

    // formats power/toughness the same way as its shown on mtg cards
    [[nodiscard]] auto to_string() const -> std::string {
        return std::format("{}/{}", power, toughness);
    }
};

}  // namespace cle::core
