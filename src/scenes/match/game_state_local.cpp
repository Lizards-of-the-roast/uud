#include "game_state_local.hpp"

#include <string>

void Local_Game_State::Apply_Snapshot(const mtg::proto::GameSnapshot &snapshot) {
    snapshot_ = snapshot;
    has_snapshot_ = true;
}

void Local_Game_State::Apply_Event(const mtg::proto::GameEvent &event) {
    if (event.has_snapshot()) {
        Apply_Snapshot(event.snapshot().snapshot());
        return;
    }

    if (event.has_game_over()) {
        game_over_ = true;
        game_over_msg_ = "Game Over - Winner ID: " + std::to_string(event.game_over().winner_id());
    }

    if (event.has_life_changed() && has_snapshot_) {
        for (auto &player : *snapshot_.mutable_players()) {
            if (player.player_id() == event.life_changed().player_id()) {
                player.set_life_total(event.life_changed().new_total());
            }
        }
    }

    if (event.has_phase_changed() && has_snapshot_) {
        snapshot_.set_current_phase(event.phase_changed().new_phase());
    }
}

bool Local_Game_State::Has_Snapshot() const {
    return has_snapshot_;
}

const mtg::proto::GameSnapshot &Local_Game_State::Snapshot() const {
    return snapshot_;
}

const mtg::proto::PlayerState *Local_Game_State::My_State(uint64_t my_user_id) const {
    if (!has_snapshot_)
        return nullptr;
    for (const auto &player : snapshot_.players())
        if (player.player_id() == my_user_id)
            return &player;
    return nullptr;
}

const mtg::proto::PlayerState *Local_Game_State::Opponent_State(uint64_t my_user_id) const {
    if (!has_snapshot_)
        return nullptr;
    for (const auto &player : snapshot_.players())
        if (player.player_id() != my_user_id)
            return &player;
    return nullptr;
}

std::string Local_Game_State::Phase_Name() const {
    if (!has_snapshot_)
        return "Unknown";
    switch (snapshot_.current_phase()) {
        case mtg::proto::PHASE_UNTAP:
            return "Untap";
        case mtg::proto::PHASE_UPKEEP:
            return "Upkeep";
        case mtg::proto::PHASE_DRAW:
            return "Draw";
        case mtg::proto::PHASE_MAIN_1:
            return "Main 1";
        case mtg::proto::PHASE_BEGINNING_OF_COMBAT:
            return "Begin Combat";
        case mtg::proto::PHASE_DECLARE_ATTACKERS:
            return "Declare Attackers";
        case mtg::proto::PHASE_DECLARE_BLOCKERS:
            return "Declare Blockers";
        case mtg::proto::PHASE_FIRST_STRIKE_DAMAGE:
            return "First Strike Damage";
        case mtg::proto::PHASE_COMBAT_DAMAGE:
            return "Combat Damage";
        case mtg::proto::PHASE_END_OF_COMBAT:
            return "End Combat";
        case mtg::proto::PHASE_MAIN_2:
            return "Main 2";
        case mtg::proto::PHASE_END_STEP:
            return "End Step";
        case mtg::proto::PHASE_CLEANUP:
            return "Cleanup";
        default:
            return "Unknown";
    }
}

bool Local_Game_State::Is_Game_Over() const {
    return game_over_;
}

std::string Local_Game_State::Game_Over_Message() const {
    return game_over_msg_;
}
