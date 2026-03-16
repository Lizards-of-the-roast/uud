#pragma once

#include <memory>

#include "engine/action_queue.hpp"
#include "mtg/game_state.pb.h"
#include <cle/game/game_context.hpp>

namespace mtg::engine {

class Game;

class GameContextImpl : public cle::game::GameContext {
public:
    explicit GameContextImpl(Game& game);

    void draw_cards(uint64_t player_id, int count) override;
    void discard_cards(uint64_t player_id, int count) override;
    void gain_life(uint64_t player_id, int amount) override;
    void lose_life(uint64_t player_id, int amount) override;
    void add_mana(uint64_t player_id, const std::string& color, int amount) override;

    void deal_damage(uint64_t source_id, uint64_t target_id, int amount) override;
    void destroy_permanent(uint64_t permanent_id) override;
    void exile_card(uint64_t card_id) override;
    void return_to_hand(uint64_t card_id) override;
    void tap_permanent(uint64_t permanent_id) override;
    void untap_permanent(uint64_t permanent_id) override;

    auto create_token(const std::string& name, cle::core::CardType type, int power, int toughness)
        -> uint64_t override;
    auto create_token_tapped(const std::string& name, cle::core::CardType type, int power,
                             int toughness) -> uint64_t override;

    [[nodiscard]] auto get_permanents_with_type(uint64_t player_id, cle::core::CardType type) const
        -> std::vector<uint64_t> override;
    [[nodiscard]] auto get_cards_in_graveyard(uint64_t player_id) const
        -> std::vector<uint64_t> override;

    void add_counter(uint64_t permanent_id, const std::string& counter_type, int amount) override;
    void remove_counter(uint64_t permanent_id, const std::string& counter_type,
                        int amount) override;

    auto choose_target(uint64_t player_id, const std::string& filter) -> uint64_t override;

    [[nodiscard]] auto get_controller(uint64_t permanent_id) const -> uint64_t override;
    [[nodiscard]] auto get_owner(uint64_t card_id) const -> uint64_t override;
    [[nodiscard]] auto get_permanent_id(uint64_t card_instance_id) const -> uint64_t override;
    [[nodiscard]] auto get_card_zone(uint64_t card_id) const -> std::string override;

    void scry(uint64_t player_id, int count) override;
    void surveil(uint64_t player_id, int count) override;
    auto search_library(uint64_t player_id, const std::string& filter) -> uint64_t override;
    void return_from_graveyard(uint64_t player_id, uint64_t card_id) override;
    void mill(uint64_t player_id, int count) override;

    void fight(uint64_t creature_a, uint64_t creature_b) override;

    void modify_power_toughness(uint64_t permanent_id, int power_mod, int toughness_mod) override;
    [[nodiscard]] auto get_power(uint64_t permanent_id) const -> int override;
    [[nodiscard]] auto get_toughness(uint64_t permanent_id) const -> int override;

    void grant_keyword(uint64_t permanent_id, const std::string& keyword) override;
    void remove_keyword(uint64_t permanent_id, const std::string& keyword) override;

    void attach(uint64_t equipment_id, uint64_t target_id) override;

    [[nodiscard]] auto get_opponents(uint64_t player_id) const -> std::vector<uint64_t> override;
    [[nodiscard]] auto get_all_creatures() const -> std::vector<uint64_t> override;
    [[nodiscard]] auto get_life_total(uint64_t player_id) const -> int override;
    [[nodiscard]] auto get_counters(uint64_t permanent_id, const std::string& counter_type) const
        -> int override;

    void blight(uint64_t player_id, int count) override;

    auto choose_mode(uint64_t player_id, int min_choices, int max_choices, int total_modes)
        -> std::vector<int> override;

    [[nodiscard]] auto get_current_phase() const -> std::string override;

    [[nodiscard]] auto get_permanents_with_subtype(uint64_t player_id,
                                                   const std::string& subtype) const
        -> std::vector<uint64_t> override;
    [[nodiscard]] auto get_graveyard_cards_with_subtype(uint64_t player_id,
                                                        const std::string& subtype) const
        -> std::vector<uint64_t> override;

    auto choose_creature_type(uint64_t player_id) -> std::string override;
    auto player_may(uint64_t player_id, const std::string& prompt) -> bool override;
    auto choose_color(uint64_t player_id) -> std::string override;

    auto search_library_to_battlefield(uint64_t player_id, const std::string& filter, bool tapped)
        -> uint64_t override;

    void register_delayed_trigger(uint64_t permanent_id, const std::string& event_type,
                                  const std::string& effect_description) override;
    void register_delayed_trigger(uint64_t permanent_id, const std::string& event_type,
                                  const std::string& effect_description,
                                  sol::function callback) override;

    void animate(uint64_t permanent_id, int power, int toughness,
                 bool until_eot) override;

    [[nodiscard]] auto get_subtypes(uint64_t permanent_id) const
        -> std::vector<std::string> override;
    [[nodiscard]] auto get_mana_value(uint64_t permanent_id) const -> int override;

    void sacrifice(uint64_t player_id, uint64_t permanent_id) override;
    void counter_spell(uint64_t stack_entry_id) override;
    auto choose_sacrifice(uint64_t player_id, const std::string& filter) -> uint64_t override;

    auto choose_from_legends(uint64_t player_id, const std::vector<uint64_t>& legend_ids)
        -> uint64_t;

private:
    auto send_prompt_and_wait(uint64_t player_id, proto::ActionPrompt prompt) -> ActionData;
    void emit_zone_transfer(uint64_t card_id, proto::ZoneType from, proto::ZoneType to,
                            uint64_t player_id);
    void fire_death_triggers(uint64_t permanent_id, uint64_t controller_id);

    Game& game_;
};

}  // namespace mtg::engine
