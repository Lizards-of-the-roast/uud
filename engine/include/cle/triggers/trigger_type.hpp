#pragma once

namespace cle::triggers {

#define CARD_TRIGGER_LIST(X)                               \
    X(on_enter_battlefield, OnEnterBattlefield)            \
    X(on_leave_battlefield, OnLeaveBattlefield)            \
    X(on_death, OnDeath)                                   \
    X(on_cast, OnCast)                                     \
    X(on_draw, OnDraw)                                     \
    X(on_tap, OnTap)                                       \
    X(on_untap, OnUntap)                                   \
    X(on_damage_dealt, OnDamageDealt)                      \
    X(on_damage_taken, OnDamageTaken)                      \
    X(on_beginning_of_upkeep, BeginningOfUpkeep)           \
    X(on_end_of_turn, EndOfTurn)                           \
    X(on_attack, OnAttack)                                 \
    X(on_block, OnBlock)                                   \
    X(on_discard, OnDiscard)                               \
    X(on_sacrifice, OnSacrifice)                           \
    X(on_exile, OnExile)                                   \
    X(on_return_to_hand, OnReturnToHand)                   \
    X(on_milled, OnMilledFromLibrary)                      \
    X(on_targeted, OnTargeted)                             \
    X(on_countered, OnCountered)                           \
    X(on_beginning_of_combat, BeginningOfCombat)           \
    X(on_end_of_combat, EndOfCombat)                       \
    X(on_blocked_by, OnBlockedBy)                          \
    X(on_blocks_creature, OnBlocksCreature)                \
    X(on_deals_combat_damage, OnDealsCombatDamage)         \
    X(on_deals_player_damage, OnDealsPlayerDamage)         \
    X(on_life_gain, OnLifeGain)                            \
    X(on_life_loss, OnLifeLoss)                            \
    X(on_mana_added, OnManaAdded)                          \
    X(on_another_creature_enters, OnAnotherCreatureEnters) \
    X(on_another_creature_dies, OnAnotherCreatureDies)     \
    X(on_landfall, OnLandfall)                             \
    X(on_artifact_enters, OnArtifactEnters)                \
    X(on_enchantment_enters, OnEnchantmentEnters)          \
    X(on_spell_cast, OnSpellCast)                          \
    X(on_instant_or_sorcery_cast, OnInstantOrSorceryCast)  \
    X(on_beginning_of_end_step, BeginningOfEndStep)        \
    X(on_beginning_of_draw_step, BeginningOfDrawStep)      \
    X(on_beginning_of_main_phase, BeginningOfMainPhase)

enum class TriggerType {
#define X(method, enum_val) enum_val,
    CARD_TRIGGER_LIST(X)
#undef X
};

}  // namespace cle::triggers
