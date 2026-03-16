#pragma once

#include <cstdint>
#include <string>

namespace mtg::engine {

enum class Phase : std::uint8_t {
    Untap,
    Upkeep,
    Draw,
    Main1,
    BeginningOfCombat,
    DeclareAttackers,
    DeclareBlockers,
    FirstStrikeDamage,
    CombatDamage,
    EndOfCombat,
    Main2,
    EndStep,
    Cleanup
};

auto phase_to_string(Phase phase) -> std::string;
auto phase_count() -> int;

class TurnMachine {
public:
    [[nodiscard]] auto current_phase() const -> Phase { return phase_; }
    [[nodiscard]] auto active_player_id() const -> uint64_t { return active_player_; }
    [[nodiscard]] auto turn_number() const -> int { return turn_number_; }
    [[nodiscard]] auto is_main_phase() const -> bool;
    [[nodiscard]] auto is_combat_phase() const -> bool;
    [[nodiscard]] auto phase_needs_priority() const -> bool;

    void set_active_player(uint64_t player_id) { active_player_ = player_id; }
    void set_phase(Phase phase) { phase_ = phase; }
    void advance_phase();
    void new_turn(uint64_t next_active_player);

private:
    Phase phase_{Phase::Untap};
    uint64_t active_player_{0};
    int turn_number_{0};
};

}  // namespace mtg::engine
