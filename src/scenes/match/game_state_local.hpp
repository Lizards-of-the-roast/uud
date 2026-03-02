#pragma once

#include <mutex>
#include <string>
#include <vector>

#include "mtg/game_state.pb.h"

struct Local_Game_State {
    void Apply_Snapshot(const mtg::proto::GameSnapshot &snapshot);
    void Apply_Event(const mtg::proto::GameEvent &event);

    bool Has_Snapshot() const;
    const mtg::proto::GameSnapshot &Snapshot() const;

    const mtg::proto::PlayerState *My_State(uint64_t my_user_id) const;
    const mtg::proto::PlayerState *Opponent_State(uint64_t my_user_id) const;
    std::string Phase_Name() const;
    bool Is_Game_Over() const;
    std::string Game_Over_Message() const;

private:
    mtg::proto::GameSnapshot snapshot_;
    bool has_snapshot_ = false;
    bool game_over_ = false;
    std::string game_over_msg_;
};
