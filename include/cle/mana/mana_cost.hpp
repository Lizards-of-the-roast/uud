#pragma once

#include <cctype>
#include <charconv>
#include <expected>
#include <string_view>
#include <vector>

#include <cle/mana/hybrid_cost.hpp>
#include <cle/mana/mana_color.hpp>

namespace cle::mana {

struct ManaCost {
    int colorless{0}, white{0}, blue{0}, black{0}, red{0}, green{0}, x_count{0};
    std::vector<HybridCost> hybrid_costs;

    static auto from_string(std::string_view input) -> std::expected<ManaCost, ManaParseError> {
        if (input.empty() || input.front() != '{' || input.back() != '}') {
            return std::unexpected(ManaParseError::InvalidFormat);
        }

        ManaCost cost;

        size_t i = 0;
        while (i < input.size()) {
            if (input[i] != '{') {
                return std::unexpected(ManaParseError::InvalidFormat);
            }

            size_t close = input.find('}', i + 1);
            if (close == std::string_view::npos) {
                return std::unexpected(ManaParseError::InvalidFormat);
            }

            std::string_view token = input.substr(i + 1, close - i - 1);
            if (auto result = cost.apply_token(token); !result)
                return std::unexpected(std::move(result).error());

            i = close + 1;
        }

        return cost;
    }

    // cmc
    [[nodiscard]] auto mana_value() const noexcept -> int {
        int total = colorless + white + blue + black + red + green;

        for (const auto &hybrid : hybrid_costs) {
            total += (hybrid.primary == ManaColor::Colorless) ? 2 : 1;
        }

        return total;
    }

private:
    auto apply_token(std::string_view token) -> std::expected<void, ManaParseError> {
        if (token.empty()) {
            return std::unexpected(ManaParseError::InvalidFormat);
        }

        // x cost
        if (token == "X") {
            x_count++;
            return {};
        }

        // hybrids
        if (token.find('/') != std::string_view::npos) {
            if (auto result = parse_hybrid(token); !result)
                return std::unexpected(std::move(result).error());
            return {};
        }

        // colorless
        if (std::isdigit(static_cast<unsigned char>(token[0]))) {
            if (auto result = parse_colorless(token); !result)
                return std::unexpected(std::move(result).error());
            return {};
        }

        // plain symbols
        if (auto result = parse_color(token); !result)
            return std::unexpected(std::move(result).error());
        return {};
    }

    auto parse_colorless(std::string_view token) -> std::expected<void, ManaParseError> {
        int value = 0;
        auto [ptr, ec] = std::from_chars(token.data(), token.data() + token.size(), value);

        if (ec != std::errc{} || ptr != token.data() + token.size()) {
            return std::unexpected(ManaParseError::UnknownToken);
        }

        colorless += value;
        return {};
    }

    auto parse_color(std::string_view token) -> std::expected<void, ManaParseError> {
        if (token.size() != 1) {
            return std::unexpected(ManaParseError::InvalidFormat);
        }

        auto result_color_result = char_to_color(token[0]);
        if (!result_color_result)
            return std::unexpected(std::move(result_color_result).error());
        auto color_result = std::move(*result_color_result);

        add_color(color_result);
        return {};
    }

    auto parse_hybrid(std::string_view token) -> std::expected<void, ManaParseError> {
        size_t slash_pos = token.find('/');
        if (slash_pos == std::string_view::npos || slash_pos == 0 ||
            slash_pos == token.size() - 1) {
            return std::unexpected(ManaParseError::InvalidFormat);
        }

        std::string_view first_part = token.substr(0, slash_pos);
        std::string_view second_part = token.substr(slash_pos + 1);

        // second part must be exactly one color character
        if (second_part.size() != 1) {
            return std::unexpected(ManaParseError::InvalidFormat);
        }

        auto result_second_color = char_to_color(second_part[0]);
        if (!result_second_color)
            return std::unexpected(std::move(result_second_color).error());
        auto second_color = std::move(*result_second_color);

        if (first_part.size() != 1) {
            return std::unexpected(ManaParseError::InvalidFormat);
        }
        auto first_result = std::isdigit(static_cast<unsigned char>(first_part[0]))
                                ? std::expected<ManaColor, ManaParseError>{ManaColor::Colorless}
                                : char_to_color(first_part[0]);
        if (!first_result)
            return std::unexpected(first_result.error());
        auto first_color = *first_result;

        hybrid_costs.push_back({first_color, second_color});
        return {};
    }

    void add_color(ManaColor color) noexcept {
        switch (color) {
            case ManaColor::White:
                white++;
                break;
            case ManaColor::Blue:
                blue++;
                break;
            case ManaColor::Black:
                black++;
                break;
            case ManaColor::Red:
                red++;
                break;
            case ManaColor::Green:
                green++;
                break;
            case ManaColor::Colorless:
                colorless++;
                break;
        }
    }
};

}  // namespace cle::mana
