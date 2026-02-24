#pragma once

#include <cstdint>
#include <expected>
#include <string>

namespace cle::core {

// bitwise enum, thanks matt
enum class Color : uint8_t {
    White = 1 << 0,
    Blue = 1 << 1,
    Black = 1 << 2,
    Red = 1 << 3,
    Green = 1 << 4,
    Colorless = 0
};

inline auto operator|(Color a, Color b) -> Color {
    return static_cast<Color>(static_cast<uint8_t>(a) | static_cast<uint8_t>(b));
}

inline auto operator&(Color a, Color b) -> Color {
    return static_cast<Color>(static_cast<uint8_t>(a) & static_cast<uint8_t>(b));
}

inline auto color_from_string(const std::string& s) -> std::expected<Color, std::string> {
    if (s == "W" || s == "White")
        return Color::White;
    if (s == "U" || s == "Blue")
        return Color::Blue;
    if (s == "B" || s == "Black")
        return Color::Black;
    if (s == "R" || s == "Red")
        return Color::Red;
    if (s == "G" || s == "Green")
        return Color::Green;
    if (s == "C" || s == "Colorless")
        return Color::Colorless;

    return std::unexpected(std::string{"Invalid color"});
}

}  // namespace cle::core
