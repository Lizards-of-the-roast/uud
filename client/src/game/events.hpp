#pragma once

#include <cstdint>
#include <string>
#include <variant>
#include <vector>

#include "game_snapshot.hpp"
#include "permanent.hpp"
#include "phase.hpp"
#include "prompts.hpp"
#include "zone.hpp"

namespace Game {

struct Card_Drawn_Event {
    uint64_t player_id = 0;
    Card card;
};

struct Card_Played_Event {
    uint64_t player_id = 0;
    Card card;
    uint64_t stack_entry_id = 0;
};

struct Permanent_Destroyed_Event {
    uint64_t permanent_id = 0;
    std::string card_name;
    std::string animation_hint;
};

struct Damage_Dealt_Event {
    uint64_t source_id = 0;
    uint64_t target_id = 0;
    int amount = 0;
    std::string animation_hint;
};

struct Life_Changed_Event {
    uint64_t player_id = 0;
    int new_total = 0;
    int delta = 0;
};

struct Phase_Changed_Event {
    Phase new_phase = Phase::Untap;
    uint64_t active_player_id = 0;
    int turn_number = 0;
};

struct Clock_Update {
    uint64_t player_id = 0;
    int clock_remaining_ms = 0;
};

struct Priority_Changed_Event {
    uint64_t player_id = 0;
    std::vector<Clock_Update> clocks;
};

struct Attack_Declared_Event {
    uint64_t attacker_id = 0;
    uint64_t defending_player_id = 0;
};

struct Block_Declared_Event {
    uint64_t blocker_id = 0;
    uint64_t attacker_id = 0;
};

struct Spell_Resolved_Event {
    uint64_t stack_entry_id = 0;
    std::string card_name;
};

struct Ability_Activated_Event {
    uint64_t permanent_id = 0;
    std::string ability_text;
    uint64_t stack_entry_id = 0;
};

struct Trigger_Fired_Event {
    uint64_t source_id = 0;
    std::string trigger_type;
    uint64_t stack_entry_id = 0;
    std::string description;
    uint64_t controller_id = 0;
};

struct Token_Created_Event {
    Permanent_State token;
};

struct Permanent_Entered_Battlefield_Event {
    uint64_t permanent_id = 0;
    uint64_t controller_id = 0;
    Card card;
    bool tapped = false;
    bool is_token = false;
    std::string animation_hint;
};

struct Zone_Transfer_Event {
    uint64_t card_id = 0;
    Zone_Type from_zone;
    Zone_Type to_zone;
    uint64_t player_id = 0;
    std::string animation_hint;
};

struct Counter_Changed_Event {
    uint64_t permanent_id = 0;
    std::string counter_type;
    int new_count = 0;
    int delta = 0;
};

struct Mana_Added_Event {
    uint64_t player_id = 0;
    Mana_Color color;
    int amount = 0;
};

struct Game_Over_Event {
    uint64_t winner_id = 0;
    std::string reason;
    bool is_draw = false;
};

struct Game_Snapshot_Event {
    Game_Snapshot snapshot;
};

struct Draw_Offer_Event {
    uint64_t from_player_id = 0;
};

struct Draw_Declined_Event {
    uint64_t by_player_id = 0;
};

struct Player_Eliminated_Event {
    uint64_t player_id = 0;
    std::string reason;
};

struct Rope_Warning_Event {
    uint64_t player_id = 0;
    int seconds_remaining = 0;
};

struct Game_Log_Entry_Event {
    std::string text;
    uint64_t player_id = 0;
    std::string category;
};

struct Unknown_Event {
    std::string description;
};

struct Game_Event {
    std::string game_id;
    uint64_t sequence_number = 0;
    std::variant<
        Card_Drawn_Event,
        Card_Played_Event,
        Permanent_Destroyed_Event,
        Damage_Dealt_Event,
        Life_Changed_Event,
        Phase_Changed_Event,
        Priority_Changed_Event,
        Attack_Declared_Event,
        Block_Declared_Event,
        Spell_Resolved_Event,
        Ability_Activated_Event,
        Trigger_Fired_Event,
        Token_Created_Event,
        Permanent_Entered_Battlefield_Event,
        Zone_Transfer_Event,
        Counter_Changed_Event,
        Mana_Added_Event,
        Game_Over_Event,
        Game_Snapshot_Event,
        Action_Prompt,
        Draw_Offer_Event,
        Draw_Declined_Event,
        Player_Eliminated_Event,
        Rope_Warning_Event,
        Game_Log_Entry_Event,
        Unknown_Event>
        event;
};

} //namespace Game
