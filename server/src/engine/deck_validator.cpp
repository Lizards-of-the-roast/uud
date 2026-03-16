#include "engine/deck_validator.hpp"

#include <unordered_map>

namespace mtg::engine {

DeckValidator::DeckValidator(const CardRegistry& registry) : registry_{registry} {}

auto DeckValidator::validate(const std::vector<std::string>& card_names) const
    -> DeckValidationResult {
    DeckValidationResult result;

    constexpr int required_size = 40;
    if (static_cast<int>(card_names.size()) != required_size) {
        result.valid = false;
        result.errors.push_back("Deck must contain exactly " + std::to_string(required_size) +
                                " cards (has " + std::to_string(card_names.size()) + ")");
    }

    constexpr int max_copies = 4;
    std::unordered_map<std::string, int> counts;
    for (const auto& name : card_names) {
        if (!is_basic_land(name)) {
            counts[name]++;
            if (counts[name] > max_copies) {
                result.valid = false;
                result.errors.push_back("Cannot have more than " + std::to_string(max_copies) +
                                        " copies of '" + name + "'");
            }
        }
    }

    std::unordered_map<std::string, bool> checked;
    for (const auto& name : card_names) {
        if (auto [it, inserted] = checked.try_emplace(name, true); inserted) {
            if (!registry_.has_card(name)) {
                result.valid = false;
                result.errors.push_back("Unknown card: '" + name + "'");
            }
        }
    }

    return result;
}

auto DeckValidator::validate_sideboard(const std::vector<std::string>& sideboard_names) const
    -> DeckValidationResult {
    DeckValidationResult result;

    constexpr int max_sideboard_size = 15;
    if (static_cast<int>(sideboard_names.size()) > max_sideboard_size) {
        result.valid = false;
        result.errors.push_back("Sideboard cannot exceed " + std::to_string(max_sideboard_size) +
                                " cards (has " + std::to_string(sideboard_names.size()) + ")");
    }

    for (const auto& name : sideboard_names) {
        if (!registry_.has_card(name)) {
            result.valid = false;
            result.errors.push_back("Unknown sideboard card: '" + name + "'");
        }
    }

    return result;
}

auto DeckValidator::is_basic_land(const std::string& name) -> bool {
    return name == "Plains" || name == "Island" || name == "Swamp" || name == "Mountain" ||
           name == "Forest" || name == "Wastes";
}

}  // namespace mtg::engine
