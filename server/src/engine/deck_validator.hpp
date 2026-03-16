#pragma once

#include <string>
#include <vector>

#include "engine/card_registry.hpp"

namespace mtg::engine {

struct DeckValidationResult {
    bool valid{true};
    std::vector<std::string> errors;
};

class DeckValidator {
public:
    explicit DeckValidator(const CardRegistry& registry);

    [[nodiscard]] auto validate(const std::vector<std::string>& card_names) const
        -> DeckValidationResult;

    [[nodiscard]] auto validate_sideboard(const std::vector<std::string>& sideboard_names) const
        -> DeckValidationResult;

private:
    [[nodiscard]] static auto is_basic_land(const std::string& name) -> bool;

    const CardRegistry& registry_;
};

}  // namespace mtg::engine
