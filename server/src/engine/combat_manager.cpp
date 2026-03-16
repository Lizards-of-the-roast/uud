#include "engine/combat_manager.hpp"

#include <algorithm>

#include "engine/permanent.hpp"
#include "engine/zone_manager.hpp"

namespace mtg::engine {

void CombatManager::begin_combat() {
    state_ = CombatState{};
    state_.active = true;
}

void CombatManager::set_attackers(std::vector<AttackDeclaration> attackers) {
    state_.attackers = std::move(attackers);
}

void CombatManager::set_blockers(std::vector<BlockDeclaration> blockers) {
    state_.blockers = std::move(blockers);
    state_.damage_order.clear();
    for (const auto& b : state_.blockers) {
        state_.damage_order[b.attacker_id].push_back(b.blocker_id);
    }
}

bool CombatManager::is_blocked(uint64_t attacker_id) const {
    return state_.damage_order.contains(attacker_id);
}

std::vector<uint64_t> CombatManager::get_blockers_for(uint64_t attacker_id) const {
    auto it = state_.damage_order.find(attacker_id);
    if (it == state_.damage_order.end()) {
        return {};
    }
    return it->second;
}

auto CombatManager::resolve_combat_damage(ZoneManager& zones, bool first_strike_only)
    -> std::vector<DamageAssignment> {
    std::vector<DamageAssignment> damage;

    for (const auto& atk : state_.attackers) {
        auto* attacker = zones.find_permanent(atk.attacker_id);
        if ((attacker == nullptr) || !attacker->is_creature()) {
            continue;
        }

        bool const has_first_strike =
            attacker->has_keyword("First Strike") || attacker->has_keyword("Double Strike");
        bool const has_regular =
            !attacker->has_keyword("First Strike") || attacker->has_keyword("Double Strike");

        if (first_strike_only && !has_first_strike) {
            continue;
        }
        if (!first_strike_only && !has_regular) {
            continue;
        }

        int const power = attacker->effective_power();
        if (power <= 0) {
            continue;
        }

        auto blocker_ids = get_blockers_for(atk.attacker_id);
        if (blocker_ids.empty()) {
            damage.push_back({atk.attacker_id, atk.defending_player_id, power, true});
        } else if (auto cit = custom_damage_.find(atk.attacker_id); cit != custom_damage_.end()) {
            const auto& cd = cit->second;
            for (size_t i = 0; i < blocker_ids.size() && i < cd.blocker_damage.size(); ++i) {
                if (cd.blocker_damage[i] > 0) {
                    damage.push_back({atk.attacker_id, blocker_ids[i], cd.blocker_damage[i], true});
                }
            }
            if (cd.player_damage > 0) {
                damage.push_back(
                    {atk.attacker_id, atk.defending_player_id, cd.player_damage, true});
            }
        } else {
            int remaining = power;
            for (uint64_t const blocker_id : blocker_ids) {
                auto* blocker = zones.find_permanent(blocker_id);
                if ((blocker == nullptr) || remaining <= 0) {
                    continue;
                }

                bool const deathtouch = attacker->has_keyword("Deathtouch");
                int const needed =
                    deathtouch ? 1 : blocker->effective_toughness() - blocker->damage_marked();
                int const assigned = std::min(remaining, std::max(needed, 0));
                damage.push_back({atk.attacker_id, blocker_id, assigned, true});
                remaining -= assigned;
            }
            if (remaining > 0 && attacker->has_keyword("Trample")) {
                damage.push_back({atk.attacker_id, atk.defending_player_id, remaining, true});
            }
        }
    }

    for (const auto& blk : state_.blockers) {
        auto* blocker = zones.find_permanent(blk.blocker_id);
        if ((blocker == nullptr) || !blocker->is_creature()) {
            continue;
        }

        bool const has_first_strike =
            blocker->has_keyword("First Strike") || blocker->has_keyword("Double Strike");
        bool const has_regular =
            !blocker->has_keyword("First Strike") || blocker->has_keyword("Double Strike");

        if (first_strike_only && !has_first_strike) {
            continue;
        }
        if (!first_strike_only && !has_regular) {
            continue;
        }

        int const power = blocker->effective_power();
        if (power > 0) {
            damage.push_back({blk.blocker_id, blk.attacker_id, power, true});
        }
    }

    return damage;
}

void CombatManager::end_combat() {
    state_ = CombatState{};
    custom_damage_.clear();
}

void CombatManager::set_custom_damage(uint64_t attacker_id, std::vector<int> blocker_damage,
                                      int player_damage) {
    custom_damage_[attacker_id] = CustomDamage{std::move(blocker_damage), player_damage};
}

void CombatManager::clear_custom_damage() {
    custom_damage_.clear();
}

void CombatManager::set_damage_order(uint64_t attacker_id, std::vector<uint64_t> ordered_blockers) {
    state_.damage_order[attacker_id] = std::move(ordered_blockers);
}

auto CombatManager::get_attackers_needing_blocker_order() const -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    for (const auto& [attacker_id, blockers] : state_.damage_order) {
        if (blockers.size() >= 2) {
            result.push_back(attacker_id);
        }
    }
    return result;
}

auto CombatManager::get_trample_attackers(ZoneManager& zones, bool first_strike_only) const
    -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    for (const auto& atk : state_.attackers) {
        auto* perm = zones.find_permanent(atk.attacker_id);
        if ((perm == nullptr) || !perm->is_creature()) {
            continue;
        }
        if (!perm->has_keyword("Trample")) {
            continue;
        }
        if (!is_blocked(atk.attacker_id)) {
            continue;
        }

        bool const has_first_strike =
            perm->has_keyword("First Strike") || perm->has_keyword("Double Strike");
        bool const has_regular =
            !perm->has_keyword("First Strike") || perm->has_keyword("Double Strike");

        if (first_strike_only && !has_first_strike) {
            continue;
        }
        if (!first_strike_only && !has_regular) {
            continue;
        }

        result.push_back(atk.attacker_id);
    }
    return result;
}

bool CombatManager::validate_attacker(uint64_t creature_id, const ZoneManager& zones) const {
    const auto* perm = zones.find_permanent(creature_id);
    if (perm == nullptr) {
        return false;
    }
    if (!perm->is_creature()) {
        return false;
    }
    if (perm->is_tapped()) {
        return false;
    }
    if (perm->has_summoning_sickness() && !perm->has_keyword("Haste")) {
        return false;
    }
    if (perm->has_keyword("Defender")) {
        return false;
    }
    if (perm->has_keyword("Can't Attack")) {
        return false;
    }
    return true;
}

auto CombatManager::get_must_attack_creatures(uint64_t player_id,
                                               const ZoneManager& zones) const
    -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    for (const auto& [id, perm] : zones.get_all_permanents()) {
        if (perm.controller_id() != player_id) {
            continue;
        }
        if (!perm.is_creature() || perm.is_tapped()) {
            continue;
        }
        if (perm.has_summoning_sickness() && !perm.has_keyword("Haste")) {
            continue;
        }
        if (perm.has_keyword("Defender")) {
            continue;
        }
        if (perm.has_keyword("Must Attack")) {
            result.push_back(id);
        }
    }
    return result;
}

bool CombatManager::validate_blocker(uint64_t blocker_id, uint64_t attacker_id,
                                     const ZoneManager& zones) const {
    const auto* blocker = zones.find_permanent(blocker_id);
    const auto* attacker = zones.find_permanent(attacker_id);
    if ((blocker == nullptr) || (attacker == nullptr)) {
        return false;
    }
    if (!blocker->is_creature()) {
        return false;
    }
    if (blocker->is_tapped()) {
        return false;
    }
    if (blocker->has_keyword("Can't Block")) {
        return false;
    }
    if (attacker->has_keyword("Flying") && !blocker->has_keyword("Flying") &&
        !blocker->has_keyword("Reach")) {
        return false;
    }
    if (attacker->has_keyword("Intimidate")) {
        bool const shares_color =
            (attacker->card()->colors() & blocker->card()->colors()) !=
            cle::core::Color::Colorless;
        if (blocker->card()->type() != cle::core::CardType::Artifact && !shares_color) {
            return false;
        }
    }
    if (attacker->has_protection_from(*blocker)) {
        return false;
    }
    return true;
}

bool CombatManager::validate_menace(const ZoneManager& zones) const {
    return std::ranges::all_of(state_.damage_order, [&](const auto& entry) {
        const auto& [attacker_id, blocker_ids] = entry;
        if (blocker_ids.empty()) {
            return true;
        }
        const auto* attacker = zones.find_permanent(attacker_id);
        if (attacker == nullptr) {
            return true;
        }
        return !(attacker->has_keyword("Menace") && blocker_ids.size() < 2);
    });
}

}  // namespace mtg::engine
