#pragma once

#include <cstdint>
#include <string>
#include <vector>

#include "phase.hpp"
#include "player.hpp"
#include "stack_entry.hpp"

namespace Game {

struct Game_Snapshot {
    std::string game_id;
    Phase current_phase = Phase::Untap;
    uint64_t active_player_id = 0;
    uint64_t priority_player_id = 0;
    int turn_number = 0;
    std::vector<Player_State> players;
    std::vector<Stack_Entry> stack;
};

} //namespace Game
