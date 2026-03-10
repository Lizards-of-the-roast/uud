#pragma once

#include <cstdint>
#include <optional>
#include <string>
#include <vector>

#include "card.hpp"
#include "counter.hpp"

namespace Game {

typedef uint64_t Permanent_ID;
struct Permanent_State {
    Permanent_ID permanent_id = 0;
    Card_ID card;
    uint64_t controller_id = 0;
    uint64_t owner_id = 0;
    bool tapped = false;
    std::vector<Counter> counters;
    int damage_marked = 0;
    bool summoning_sick = false;
    std::vector<std::string> granted_keywords;
    int power_modifier = 0;
    int toughness_modifier = 0;
    std::optional<uint64_t> attached_to;
    std::vector<uint64_t> attachments;
    bool is_token = false;
};

} //namespace Game
