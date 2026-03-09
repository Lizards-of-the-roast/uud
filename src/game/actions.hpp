#pragma once

#include <cstdint>
#include <string>
#include <variant>
#include <vector>

#include "card.hpp"
#include "mana_pool.hpp"

namespace Game {

struct Pass_Priority_Action {};

struct Play_Card_Action {
    uint64_t card_instance_id = 0;
};

struct Play_Land_Action {
    uint64_t card_instance_id = 0;
};

struct Activate_Ability_Action {
    uint64_t permanent_id = 0;
    int ability_index = 0;
};

struct Attacker_Declaration {
    uint64_t creature_id = 0;
    uint64_t defending_player_id = 0;
};

struct Declare_Attackers_Action {
    std::vector<Attacker_Declaration> attackers;
};

struct Blocker_Declaration {
    uint64_t blocker_id = 0;
    uint64_t attacker_id = 0;
};

struct Declare_Blockers_Action {
    std::vector<Blocker_Declaration> blockers;
};

struct Select_Target_Action {
    uint64_t target_id = 0;
};

struct Select_Mode_Action {
    std::vector<int> chosen_modes;
};

struct Yes_No_Action {
    bool choice = false;
};

struct Discard_Action {
    std::vector<uint64_t> card_instance_ids;
};

struct Select_Color_Action {
    Mana_Color color;
};

struct Select_Creature_Type_Action {
    std::string creature_type;
};

struct Pay_Mana_Action {
    Mana_Pool payment;
};

struct Order_Blockers_Action {
    std::vector<uint64_t> ordered_blocker_ids;
};

struct Damage_Assignment_Action {
    std::vector<int> damage_to_each_blocker;
    int damage_to_player = 0;
};

struct Concede_Action {};

struct Player_Action {
    std::string game_id;
    std::string prompt_id;
    std::variant<
        Pass_Priority_Action,
        Play_Card_Action,
        Play_Land_Action,
        Activate_Ability_Action,
        Declare_Attackers_Action,
        Declare_Blockers_Action,
        Select_Target_Action,
        Select_Mode_Action,
        Yes_No_Action,
        Discard_Action,
        Select_Color_Action,
        Select_Creature_Type_Action,
        Pay_Mana_Action,
        Order_Blockers_Action,
        Damage_Assignment_Action,
        Concede_Action>
        action;
};

} //namespace Game
