#pragma once

// clang-format off
#define TRIGGER_EVENT_FIELD_LIST(X) \
    X(type)                         \
    X(source_id)                    \
    X(target_id)                    \
    X(player_id)                    \
    X(amount)                       \
    X(extra_data)

#define GAME_CONTEXT_ACTION_LIST(X) \
    X(draw_cards)                   \
    X(discard_cards)                \
    X(gain_life)                    \
    X(lose_life)                    \
    X(add_mana)                     \
    X(deal_damage)                  \
    X(destroy_permanent)            \
    X(exile_card)                   \
    X(return_to_hand)               \
    X(tap_permanent)                \
    X(untap_permanent)              \
    X(create_token)                 \
    X(get_permanents_with_type)     \
    X(get_cards_in_graveyard)       \
    X(add_counter)                  \
    X(remove_counter)               \
    X(choose_target)                \
    X(get_controller)               \
    X(get_owner)                    \
    X(get_permanent_id)             \
    X(get_card_zone)                \
    X(surveil)                      \
    X(search_library)               \
    X(return_from_graveyard)        \
    X(mill)                         \
    X(fight)                        \
    X(modify_power_toughness)       \
    X(grant_keyword)                \
    X(remove_keyword)               \
    X(attach)                       \
    X(get_opponents)                \
    X(get_all_creatures)            \
    X(get_life_total)               \
    X(get_counters)                 \
    X(blight)                       \
    X(choose_mode)                  \
    X(get_power)                    \
    X(get_toughness)                \
    X(get_current_phase)            \
    X(create_token_tapped)          \
    X(get_permanents_with_subtype)  \
    X(get_graveyard_cards_with_subtype) \
    X(choose_creature_type)         \
    X(player_may)                   \
    X(choose_color)                 \
    X(search_library_to_battlefield) \
    X(register_delayed_trigger)     \
    X(animate)                      \
    X(get_subtypes)                 \
    X(get_mana_value)               \
    X(sacrifice)
// clang-format on
