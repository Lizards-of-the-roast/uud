#include "engine/turn_machine.hpp"

namespace mtg::engine {

std::string phase_to_string(Phase phase) {
    switch (phase) {
        case Phase::Untap:
            return "Untap";
        case Phase::Upkeep:
            return "Upkeep";
        case Phase::Draw:
            return "Draw";
        case Phase::Main1:
            return "Main1";
        case Phase::BeginningOfCombat:
            return "BeginningOfCombat";
        case Phase::DeclareAttackers:
            return "DeclareAttackers";
        case Phase::DeclareBlockers:
            return "DeclareBlockers";
        case Phase::FirstStrikeDamage:
            return "FirstStrikeDamage";
        case Phase::CombatDamage:
            return "CombatDamage";
        case Phase::EndOfCombat:
            return "EndOfCombat";
        case Phase::Main2:
            return "Main2";
        case Phase::EndStep:
            return "EndStep";
        case Phase::Cleanup:
            return "Cleanup";
    }
    return "Unknown";
}

int phase_count() {
    return 13;
}

bool TurnMachine::is_main_phase() const {
    return phase_ == Phase::Main1 || phase_ == Phase::Main2;
}

bool TurnMachine::is_combat_phase() const {
    return phase_ >= Phase::BeginningOfCombat && phase_ <= Phase::EndOfCombat;
}

bool TurnMachine::phase_needs_priority() const {
    return phase_ != Phase::Untap && phase_ != Phase::Cleanup;
}

void TurnMachine::advance_phase() {
    int const next = static_cast<int>(phase_) + 1;
    if (next >= phase_count()) {
        phase_ = Phase::Untap;
    } else {
        phase_ = static_cast<Phase>(next);
    }
}

void TurnMachine::new_turn(uint64_t next_active_player) {
    ++turn_number_;
    active_player_ = next_active_player;
    phase_ = Phase::Untap;
}

}  // namespace mtg::engine
