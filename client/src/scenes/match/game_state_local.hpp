#pragma once

#include <cstdint>
#include <optional>
#include <set>
#include <string>
#include <unordered_map>
#include <vector>

#include "game/events.hpp"
#include "game/game_log.hpp"
#include "game/game_snapshot.hpp"
#include "game/player.hpp"
#include "game/prompts.hpp"
#include <SDL3/SDL.h>

struct Damage_Float {
    uint64_t target_id = 0;
    SDL_FPoint pos = {0, 0};
    bool pos_resolved = false;
    int amount = 0;
    Uint64 start_ms = 0;
};

struct Life_Pulse {
    uint64_t player_id;
    int delta;
    Uint64 start_ms;
};

struct Phase_Flash {
    Uint64 start_ms = 0;
};

struct Card_Anim_Hint {
    uint64_t card_id = 0;
    std::string card_name;
    int from_zone = 0;
    int to_zone = 0;
    bool is_opponent = false;
};

struct Combat_UI_State {
    bool attacker_prompt_active = false;
    bool blocker_prompt_active = false;
    std::set<uint64_t> eligible_attackers;
    std::set<uint64_t> eligible_blockers;
    std::vector<uint64_t> attacking_creatures;
    std::vector<uint64_t> selected_attackers;
    std::vector<std::pair<uint64_t, uint64_t>> selected_blockers;
    uint64_t pending_blocker = 0;
    std::unordered_map<uint64_t, SDL_FRect> permanent_rects;
    uint64_t hovered_card_id = 0;

    std::set<uint64_t> legal_targets;
    uint64_t clicked_target = 0;

    void Clear() {
        attacker_prompt_active = false;
        blocker_prompt_active = false;
        eligible_attackers.clear();
        eligible_blockers.clear();
        attacking_creatures.clear();
        selected_attackers.clear();
        selected_blockers.clear();
        pending_blocker = 0;
        permanent_rects.clear();
        legal_targets.clear();
        clicked_target = 0;
    }
};

struct Local_Game_State {
    void Set_Local_User_Id(uint64_t id) { local_user_id_ = id; }

    void Apply_Snapshot(const Game::Game_Snapshot &snapshot);
    void Apply_Event(const Game::Game_Event &event);

    bool Has_Snapshot() const;
    const Game::Game_Snapshot &Snapshot() const;
    Game::Game_Snapshot &Snapshot_Mut();

    const Game::Player_State *My_State(uint64_t my_user_id) const;
    const Game::Player_State *Opponent_State(uint64_t my_user_id) const;
    Game::Player_State *My_State_Mut(uint64_t my_user_id);
    std::string Phase_Name() const;
    bool Is_Game_Over() const;
    std::string Game_Over_Message() const;
    uint64_t Game_Over_Winner() const { return game_over_winner_; }
    std::string Game_Over_Reason() const { return game_over_reason_; }
    bool Game_Over_Is_Draw() const { return game_over_is_draw_; }

    bool Has_Prompt() const;
    const Game::Action_Prompt &Pending_Prompt() const;
    void Clear_Prompt();

    bool Has_Draw_Offer() const;
    uint64_t Draw_Offer_From() const;
    void Clear_Draw_Offer();
    bool Draw_Offered_By_Us() const { return draw_offered_by_us_; }

    bool Already_Activated(uint64_t permanent_id) const;
    void Mark_Activated(uint64_t permanent_id);

    Game_Log &Log() { return log_; }
    const Game_Log &Log() const { return log_; }

    std::vector<Damage_Float> pending_damage_floats;
    std::vector<Life_Pulse> pending_life_pulses;
    std::vector<Card_Anim_Hint> pending_card_anims;
    bool phase_just_changed = false;
    bool turn_just_changed = false;

private:
    uint64_t local_user_id_ = 0;
    Game::Game_Snapshot snapshot_;
    bool has_snapshot_ = false;
    bool game_over_ = false;
    std::string game_over_msg_;
    uint64_t game_over_winner_ = 0;
    std::string game_over_reason_;
    bool game_over_is_draw_ = false;
    std::optional<Game::Action_Prompt> pending_prompt_;
    std::optional<uint64_t> draw_offer_from_;
    bool draw_offered_by_us_ = false;
    std::set<uint64_t> activated_this_phase_;
    int last_phase_turn_key_ = -1;
    Game_Log log_;
};
