#include "game_state_local.hpp"

#include <string>
#include <variant>

void Local_Game_State::Apply_Snapshot(const Game_Snapshot &snapshot) {
    snapshot_ = snapshot;
    has_snapshot_ = true;
}

void Local_Game_State::Apply_Event(const Game_Event &event) {
    std::visit([this](const auto &e) {
        using T = std::decay_t<decltype(e)>;

        if constexpr (std::is_same_v<T, Game_Snapshot_Event>) {
            Apply_Snapshot(e.snapshot);
        } else if constexpr (std::is_same_v<T, Game_Over_Event>) {
            game_over_ = true;
            game_over_msg_ = "Game Over - Winner ID: " + std::to_string(e.winner_id);
        } else if constexpr (std::is_same_v<T, Life_Changed_Event>) {
            if (has_snapshot_) {
                for (auto &player : snapshot_.players)
                    if (player.player_id == e.player_id)
                        player.life_total = e.new_total;
            }
        } else if constexpr (std::is_same_v<T, Phase_Changed_Event>) {
            if (has_snapshot_)
                snapshot_.current_phase = e.new_phase;
        }
    }, event.event);
}

bool Local_Game_State::Has_Snapshot() const {
    return has_snapshot_;
}

const Game_Snapshot &Local_Game_State::Snapshot() const {
    return snapshot_;
}

const Player_State *Local_Game_State::My_State(uint64_t my_user_id) const {
    if (!has_snapshot_)
        return nullptr;
    for (const auto &player : snapshot_.players)
        if (player.player_id == my_user_id)
            return &player;
    return nullptr;
}

const Player_State *Local_Game_State::Opponent_State(uint64_t my_user_id) const {
    if (!has_snapshot_)
        return nullptr;
    for (const auto &player : snapshot_.players)
        if (player.player_id != my_user_id)
            return &player;
    return nullptr;
}

std::string Local_Game_State::Phase_Name() const {
    if (!has_snapshot_)
        return "Unknown";
    switch (snapshot_.current_phase) {
        case Phase::Untap:              return "Untap";
        case Phase::Upkeep:             return "Upkeep";
        case Phase::Draw:               return "Draw";
        case Phase::Main_1:             return "Main 1";
        case Phase::Beginning_Of_Combat: return "Begin Combat";
        case Phase::Declare_Attackers:  return "Declare Attackers";
        case Phase::Declare_Blockers:   return "Declare Blockers";
        case Phase::First_Strike_Damage: return "First Strike Damage";
        case Phase::Combat_Damage:      return "Combat Damage";
        case Phase::End_Of_Combat:      return "End Combat";
        case Phase::Main_2:             return "Main 2";
        case Phase::End_Step:           return "End Step";
        case Phase::Cleanup:            return "Cleanup";
        default:                        return "Unknown";
    }
}

bool Local_Game_State::Is_Game_Over() const {
    return game_over_;
}

std::string Local_Game_State::Game_Over_Message() const {
    return game_over_msg_;
}
