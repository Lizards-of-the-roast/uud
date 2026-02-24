#pragma once

#include <cstdint>
#include <expected>

namespace cle::mana {

enum class ManaColor : uint8_t { White, Blue, Black, Red, Green, Colorless };

enum class ManaParseError { InvalidFormat, UnknownToken };

inline auto char_to_color(char c) -> std::expected<ManaColor, ManaParseError> {
    switch (c) {
        case 'W':
            return ManaColor::White;
        case 'U':
            return ManaColor::Blue;
        case 'B':
            return ManaColor::Black;
        case 'R':
            return ManaColor::Red;
        case 'G':
            return ManaColor::Green;
        case 'C':
            return ManaColor::Colorless;
        default:
            return std::unexpected(ManaParseError::UnknownToken);
    }
}

}  // namespace cle::mana
