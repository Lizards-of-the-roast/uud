#pragma once

#include <string>

#include "game/events.hpp"
#include "game/game_snapshot.hpp"
#include "game/player.hpp"

struct Local_Game_State {
    void Apply_Snapshot(const Game::Game_Snapshot &snapshot);
    void Apply_Event(const Game::Game_Event &event);

    bool Has_Snapshot() const;
    const Game::Game_Snapshot &Snapshot() const;

    const Game::Player_State *My_State(uint64_t my_user_id) const;
    const Game::Player_State *Opponent_State(uint64_t my_user_id) const;
    std::string Phase_Name() const;
    bool Is_Game_Over() const;
    std::string Game_Over_Message() const;

private:
    Game::Game_Snapshot snapshot_;
    bool has_snapshot_ = false;
    bool game_over_ = false;
    std::string game_over_msg_;
};
