#pragma once

#include <cstdint>
#include <optional>
#include <string>
#include <vector>

#include "card.hpp"

struct Stack_Entry {
    uint64_t entry_id = 0;
    std::optional<Card> spell;
    std::optional<std::string> ability_description;
    uint64_t controller_id = 0;
    std::vector<uint64_t> targets;
};
