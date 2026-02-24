#pragma once

#include <cstdint>
#include <string>
#include <vector>

#include <cle/core/card_type.hpp>

namespace cle::game {

class GameContext {
public:
    virtual ~GameContext() = default;

    // player actions
    virtual void draw_cards(uint64_t player_id, int count) = 0;
    virtual void discard_cards(uint64_t player_id, int count) = 0;
    virtual void gain_life(uint64_t player_id, int amount) = 0;
    virtual void lose_life(uint64_t player_id, int amount) = 0;
    virtual void add_mana(uint64_t player_id, const std::string& color, int amount) = 0;

    // permanent actions
    virtual void deal_damage(uint64_t source_id, uint64_t target_id, int amount) = 0;
    virtual void destroy_permanent(uint64_t permanent_id) = 0;
    virtual void exile_card(uint64_t card_id) = 0;
    virtual void return_to_hand(uint64_t card_id) = 0;
    virtual void tap_permanent(uint64_t permanent_id) = 0;
    virtual void untap_permanent(uint64_t permanent_id) = 0;

    // tokens
    [[nodiscard]] virtual auto create_token(const std::string& name, core::CardType type, int power,
                                            int toughness) -> uint64_t = 0;

    // queries
    [[nodiscard]] virtual auto get_permanents_with_type(uint64_t player_id,
                                                        core::CardType type) const
        -> std::vector<uint64_t> = 0;
    [[nodiscard]] virtual auto get_cards_in_graveyard(uint64_t player_id) const
        -> std::vector<uint64_t> = 0;

    // counters
    virtual void add_counter(uint64_t permanent_id, const std::string& counter_type,
                             int amount) = 0;
    virtual void remove_counter(uint64_t permanent_id, const std::string& counter_type,
                                int amount) = 0;

    // targeting
    [[nodiscard]] virtual auto choose_target(uint64_t player_id, const std::string& filter)
        -> uint64_t = 0;

    // info
    [[nodiscard]] virtual auto get_controller(uint64_t permanent_id) const -> uint64_t = 0;
    [[nodiscard]] virtual auto get_owner(uint64_t card_id) const -> uint64_t = 0;
    [[nodiscard]] virtual auto get_permanent_id(uint64_t card_instance_id) const -> uint64_t = 0;
    [[nodiscard]] virtual auto get_card_zone(uint64_t card_id) const -> std::string = 0;

    // library/graveyard
    virtual void surveil(uint64_t player_id, int count) = 0;
    [[nodiscard]] virtual auto search_library(uint64_t player_id, const std::string& filter)
        -> uint64_t = 0;
    virtual void return_from_graveyard(uint64_t player_id, uint64_t card_id) = 0;
    virtual void mill(uint64_t player_id, int count) = 0;

    // combat
    virtual void fight(uint64_t creature_a, uint64_t creature_b) = 0;

    // power/toughness
    virtual void modify_power_toughness(uint64_t permanent_id, int power_mod,
                                        int toughness_mod) = 0;
    [[nodiscard]] virtual auto get_power(uint64_t permanent_id) const -> int = 0;
    [[nodiscard]] virtual auto get_toughness(uint64_t permanent_id) const -> int = 0;

    // keywords
    virtual void grant_keyword(uint64_t permanent_id, const std::string& keyword) = 0;
    virtual void remove_keyword(uint64_t permanent_id, const std::string& keyword) = 0;

    // equipment/aura
    virtual void attach(uint64_t equipment_id, uint64_t target_id) = 0;

    // multiplayer queries
    [[nodiscard]] virtual auto get_opponents(uint64_t player_id) const -> std::vector<uint64_t> = 0;
    [[nodiscard]] virtual auto get_all_creatures() const -> std::vector<uint64_t> = 0;
    [[nodiscard]] virtual auto get_life_total(uint64_t player_id) const -> int = 0;
    [[nodiscard]] virtual auto get_counters(uint64_t permanent_id,
                                            const std::string& counter_type) const -> int = 0;

    // blight (put -1/-1 counters on own creature)
    virtual void blight(uint64_t player_id, int count) = 0;

    // modal choice
    [[nodiscard]] virtual auto choose_mode(uint64_t player_id, int min_choices, int max_choices,
                                           int total_modes) -> std::vector<int> = 0;

    // phase detection
    [[nodiscard]] virtual auto get_current_phase() const -> std::string = 0;

    // tapped token creation
    [[nodiscard]] virtual auto create_token_tapped(const std::string& name, core::CardType type,
                                                   int power, int toughness) -> uint64_t = 0;

    // subtype-filtered queries
    [[nodiscard]] virtual auto get_permanents_with_subtype(uint64_t player_id,
                                                           const std::string& subtype) const
        -> std::vector<uint64_t> = 0;
    [[nodiscard]] virtual auto get_graveyard_cards_with_subtype(uint64_t player_id,
                                                                const std::string& subtype) const
        -> std::vector<uint64_t> = 0;

    // player choices
    [[nodiscard]] virtual auto choose_creature_type(uint64_t player_id) -> std::string = 0;
    [[nodiscard]] virtual auto player_may(uint64_t player_id, const std::string& prompt)
        -> bool = 0;
    [[nodiscard]] virtual auto choose_color(uint64_t player_id) -> std::string = 0;

    // search library and put onto battlefield
    [[nodiscard]] virtual auto search_library_to_battlefield(uint64_t player_id,
                                                             const std::string& filter, bool tapped)
        -> uint64_t = 0;

    // delayed triggers
    virtual void register_delayed_trigger(uint64_t permanent_id, const std::string& event_type,
                                          const std::string& effect_description) = 0;

    // turn noncreature into creature until end of turn
    virtual void animate(uint64_t permanent_id, int power, int toughness) = 0;

    // permanent info
    [[nodiscard]] virtual auto get_subtypes(uint64_t permanent_id) const
        -> std::vector<std::string> = 0;
    [[nodiscard]] virtual auto get_mana_value(uint64_t permanent_id) const -> int = 0;

    // sacrifice
    virtual void sacrifice(uint64_t player_id, uint64_t permanent_id) = 0;

protected:
    GameContext() = default;
    GameContext(const GameContext&) = default;
    GameContext(GameContext&&) = default;
    auto operator=(const GameContext&) -> GameContext& = default;
    auto operator=(GameContext&&) -> GameContext& = default;
};

}  // namespace cle::game
