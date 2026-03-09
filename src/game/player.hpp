#pragma once

#include <cstdint>
#include <string>
#include <vector>

#include "card.hpp"
#include "mana_pool.hpp"
#include "permanent.hpp"

namespace Game {

struct Player_State {
    uint64_t player_id = 0;
    std::string username;
    int life_total = 0;
    int poison_counters = 0;
    Mana_Pool mana_pool;
    int hand_count = 0;
    int library_count = 0;
    std::vector<Card> hand;
    std::vector<Permanent_State> battlefield;
    std::vector<Card> graveyard;
    std::vector<Card> exile;
    bool has_priority = false;
    int lands_played_this_turn = 0;
};

} //namespace Game
