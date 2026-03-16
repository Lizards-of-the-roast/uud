#pragma once

#include <cstdint>
#include <unordered_map>
#include <vector>

namespace mtg::engine {

class ZoneManager;

struct AttackDeclaration {
    uint64_t attacker_id;
    uint64_t defending_player_id;
};

struct BlockDeclaration {
    uint64_t blocker_id;
    uint64_t attacker_id;
};

struct DamageAssignment {
    uint64_t source_id;
    uint64_t target_id;
    int amount;
    bool is_combat_damage;
};

struct CombatState {
    std::vector<AttackDeclaration> attackers;
    std::vector<BlockDeclaration> blockers;
    std::unordered_map<uint64_t, std::vector<uint64_t>> damage_order;
    bool active{false};
};

class CombatManager {
public:
    void begin_combat();
    void set_attackers(std::vector<AttackDeclaration> attackers);
    void set_blockers(std::vector<BlockDeclaration> blockers);
    void set_damage_order(uint64_t attacker_id, std::vector<uint64_t> ordered_blockers);
    [[nodiscard]] auto resolve_combat_damage(ZoneManager& zones, bool first_strike_only)
        -> std::vector<DamageAssignment>;
    void end_combat();

    [[nodiscard]] auto get_attackers_needing_blocker_order() const -> std::vector<uint64_t>;
    [[nodiscard]] auto get_trample_attackers(ZoneManager& zones, bool first_strike_only) const
        -> std::vector<uint64_t>;

    void set_custom_damage(uint64_t attacker_id, std::vector<int> blocker_damage,
                           int player_damage);
    void clear_custom_damage();

    [[nodiscard]] auto state() const -> const CombatState& { return state_; }
    [[nodiscard]] auto is_active() const -> bool { return state_.active; }
    [[nodiscard]] auto get_attackers() const -> const std::vector<AttackDeclaration>& {
        return state_.attackers;
    }
    [[nodiscard]] auto is_blocked(uint64_t attacker_id) const -> bool;
    [[nodiscard]] auto get_blockers_for(uint64_t attacker_id) const -> std::vector<uint64_t>;

    [[nodiscard]] auto validate_attacker(uint64_t creature_id, const ZoneManager& zones) const
        -> bool;
    [[nodiscard]] auto validate_blocker(uint64_t blocker_id, uint64_t attacker_id,
                                        const ZoneManager& zones) const -> bool;
    [[nodiscard]] auto get_must_attack_creatures(uint64_t player_id,
                                                  const ZoneManager& zones) const
        -> std::vector<uint64_t>;
    [[nodiscard]] auto validate_menace(const ZoneManager& zones) const -> bool;

private:
    struct CustomDamage {
        std::vector<int> blocker_damage;
        int player_damage{0};
    };

    CombatState state_;
    std::unordered_map<uint64_t, CustomDamage> custom_damage_;
};

}  // namespace mtg::engine
