#pragma once

#include <cstdint>
#include <string>
#include <variant>
#include <vector>

#include "card.hpp"
#include "mana_pool.hpp"

namespace Game {

struct Priority_Prompt {
    std::vector<std::string> legal_actions;
    std::vector<uint64_t> castable_card_ids;
    std::vector<uint64_t> activatable_permanent_ids;
    bool can_play_land = false;
};

struct Target_Prompt {
    std::string filter;
    std::vector<uint64_t> legal_targets;
};

struct Mode_Choice_Prompt {
    int min_choices = 0;
    int max_choices = 0;
    std::vector<std::string> modes;
};

struct Yes_No_Prompt {
    std::string question;
};

struct Attacker_Prompt {
    std::vector<uint64_t> eligible_attackers;
    std::vector<uint64_t> defending_players;
};

struct Blocker_Prompt {
    std::vector<uint64_t> eligible_blockers;
    std::vector<uint64_t> attacking_creatures;
};

struct Discard_Prompt {
    int count = 0;
    std::vector<Card> hand;
};

struct Color_Choice_Prompt {
    std::string reason;
    std::vector<std::string> legal_colors;
};

struct Creature_Type_Prompt {
    std::string reason;
    std::vector<std::string> suggestions;
};

struct Mana_Payment_Prompt {
    std::string cost_description;
    Mana_Pool available;
};

struct Order_Blockers_Prompt {
    uint64_t attacker_id = 0;
    std::vector<uint64_t> unordered_blockers;
};

struct Damage_Assignment_Prompt {
    uint64_t attacker_id = 0;
    std::vector<uint64_t> ordered_blockers;
    int total_damage = 0;
    uint64_t defending_player_id = 0;
};

struct X_Cost_Prompt {
    std::string card_name;
    int max_x = 0;
    int x_count = 1;
};

struct Concede_Confirm_Prompt {
    std::string message;
};

struct Action_Prompt {
    uint64_t player_id = 0;
    std::string prompt_id;
    std::variant<
        Priority_Prompt,
        Target_Prompt,
        Mode_Choice_Prompt,
        Yes_No_Prompt,
        Attacker_Prompt,
        Blocker_Prompt,
        Discard_Prompt,
        Color_Choice_Prompt,
        Creature_Type_Prompt,
        Mana_Payment_Prompt,
        Order_Blockers_Prompt,
        Damage_Assignment_Prompt,
        X_Cost_Prompt,
        Concede_Confirm_Prompt>
        prompt;
};

} //namespace Game
