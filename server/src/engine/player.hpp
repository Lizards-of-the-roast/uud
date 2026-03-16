#pragma once

#include <cstdint>
#include <memory>
#include <string>
#include <vector>

#include "mana_pool.hpp"
#include <cle/core/card.hpp>

namespace mtg::engine {

class Player {
public:
    Player(uint64_t id, std::string username);

    [[nodiscard]] auto id() const -> uint64_t { return id_; }
    [[nodiscard]] auto username() const -> const std::string& { return username_; }

    [[nodiscard]] auto life() const -> int { return life_; }
    void set_life(int life) { life_ = life; }
    void gain_life(int amount) { life_ += amount; }
    void lose_life(int amount) { life_ -= amount; }

    [[nodiscard]] auto poison_counters() const -> int { return poison_counters_; }
    void add_poison(int amount) { poison_counters_ += amount; }

    [[nodiscard]] auto mana_pool() -> ManaPool& { return mana_pool_; }
    [[nodiscard]] auto mana_pool() const -> const ManaPool& { return mana_pool_; }

    [[nodiscard]] auto lands_played_this_turn() const -> int { return lands_played_; }
    [[nodiscard]] auto max_land_plays() const -> int { return max_land_plays_; }
    void set_max_land_plays(int n) { max_land_plays_ = n; }
    void play_land() { lands_played_++; }
    [[nodiscard]] auto can_play_land() const -> bool;

    [[nodiscard]] auto has_drawn_for_turn() const -> bool { return has_drawn_for_turn_; }
    void set_drawn_for_turn(bool drawn) { has_drawn_for_turn_ = drawn; }

    void set_deck(std::vector<std::shared_ptr<cle::core::Card>> deck) { deck_ = std::move(deck); }
    [[nodiscard]] auto deck() const -> const std::vector<std::shared_ptr<cle::core::Card>>& {
        return deck_;
    }
    void clear_deck() { deck_.clear(); }
    void set_submitted_deck_names(std::vector<std::string> names) {
        submitted_deck_names_ = std::move(names);
        deck_submitted_ = true;
    }
    [[nodiscard]] auto submitted_deck_names() const -> const std::vector<std::string>& {
        return submitted_deck_names_;
    }
    [[nodiscard]] auto deck_submitted() const -> bool { return deck_submitted_; }

    void set_submitted_sideboard_names(std::vector<std::string> names) {
        submitted_sideboard_names_ = std::move(names);
    }
    [[nodiscard]] auto submitted_sideboard_names() const -> const std::vector<std::string>& {
        return submitted_sideboard_names_;
    }
    void set_sideboard(std::vector<std::shared_ptr<cle::core::Card>> sb) {
        sideboard_ = std::move(sb);
    }
    [[nodiscard]] auto sideboard() const
        -> const std::vector<std::shared_ptr<cle::core::Card>>& {
        return sideboard_;
    }
    [[nodiscard]] auto sideboard() -> std::vector<std::shared_ptr<cle::core::Card>>& {
        return sideboard_;
    }

    [[nodiscard]] auto is_alive() const -> bool;
    [[nodiscard]] auto has_lost() const -> bool { return has_lost_; }
    void eliminate() { has_lost_ = true; }

    [[nodiscard]] auto failed_to_draw() const -> bool { return failed_to_draw_; }
    void set_failed_to_draw() { failed_to_draw_ = true; }

    void reset_for_turn();

private:
    uint64_t id_;
    std::string username_;
    int life_{20};
    int poison_counters_{0};
    ManaPool mana_pool_;
    int lands_played_{0};
    int max_land_plays_{1};
    bool has_drawn_for_turn_{false};
    bool has_lost_{false};
    bool failed_to_draw_{false};
    bool deck_submitted_{false};
    std::vector<std::shared_ptr<cle::core::Card>> deck_;
    std::vector<std::string> submitted_deck_names_;
    std::vector<std::shared_ptr<cle::core::Card>> sideboard_;
    std::vector<std::string> submitted_sideboard_names_;
};

}  // namespace mtg::engine
