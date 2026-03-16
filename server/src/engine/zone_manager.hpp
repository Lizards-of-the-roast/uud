#pragma once

#include <algorithm>
#include <cstdint>
#include <deque>
#include <memory>
#include <optional>
#include <random>
#include <unordered_map>
#include <vector>

#include "engine/permanent.hpp"
#include <cle/core/card.hpp>

namespace mtg::engine {

class ZoneManager {
public:
    void set_library(uint64_t player_id, std::vector<std::shared_ptr<cle::core::Card>> cards);
    [[nodiscard]] auto draw_card(uint64_t player_id)
        -> std::optional<std::shared_ptr<cle::core::Card>>;
    void shuffle_library(uint64_t player_id);
    [[nodiscard]] auto library_size(uint64_t player_id) const -> int;
    void put_on_top(uint64_t player_id, std::shared_ptr<cle::core::Card> card);
    void put_on_bottom(uint64_t player_id, std::shared_ptr<cle::core::Card> card);
    [[nodiscard]] auto peek_top(uint64_t player_id)
        -> std::optional<std::shared_ptr<cle::core::Card>>;
    [[nodiscard]] auto remove_top(uint64_t player_id)
        -> std::optional<std::shared_ptr<cle::core::Card>>;

    [[nodiscard]] auto get_hand(uint64_t player_id)
        -> std::vector<std::shared_ptr<cle::core::Card>>&;
    [[nodiscard]] auto get_hand(uint64_t player_id) const
        -> const std::vector<std::shared_ptr<cle::core::Card>>&;
    [[nodiscard]] auto hand_size(uint64_t player_id) const -> int;
    [[nodiscard]] auto remove_from_hand(uint64_t player_id, uint64_t card_instance_id)
        -> std::shared_ptr<cle::core::Card>;
    void return_hand_to_library(uint64_t player_id);

    [[nodiscard]] auto add_to_battlefield(std::shared_ptr<cle::core::Card> card,
                                          uint64_t controller_id, uint64_t owner_id) -> uint64_t;
    [[nodiscard]] auto remove_from_battlefield(uint64_t permanent_id) -> std::optional<Permanent>;
    [[nodiscard]] auto find_permanent(uint64_t permanent_id) -> Permanent*;
    [[nodiscard]] auto find_permanent(uint64_t permanent_id) const -> const Permanent*;
    [[nodiscard]] auto get_all_permanents() -> std::unordered_map<uint64_t, Permanent>& {
        return battlefield_;
    }
    [[nodiscard]] auto get_all_permanents() const
        -> const std::unordered_map<uint64_t, Permanent>& {
        return battlefield_;
    }
    [[nodiscard]] auto get_permanents_controlled_by(uint64_t player_id) const
        -> std::vector<uint64_t>;
    [[nodiscard]] auto get_permanents_with_type(uint64_t player_id, cle::core::CardType type) const
        -> std::vector<uint64_t>;
    [[nodiscard]] auto get_permanents_with_subtype(uint64_t player_id,
                                                   const std::string& subtype) const
        -> std::vector<uint64_t>;
    [[nodiscard]] auto get_all_creatures() const -> std::vector<uint64_t>;

    void add_to_graveyard(uint64_t player_id, std::shared_ptr<cle::core::Card> card);
    [[nodiscard]] auto get_graveyard(uint64_t player_id)
        -> std::vector<std::shared_ptr<cle::core::Card>>&;
    [[nodiscard]] auto get_graveyard(uint64_t player_id) const
        -> const std::vector<std::shared_ptr<cle::core::Card>>&;
    [[nodiscard]] auto remove_from_graveyard(uint64_t player_id, uint64_t card_id)
        -> std::shared_ptr<cle::core::Card>;
    [[nodiscard]] auto get_graveyard_cards_with_subtype(uint64_t player_id,
                                                        const std::string& subtype) const
        -> std::vector<uint64_t>;

    struct ExiledCard {
        std::shared_ptr<cle::core::Card> card;
        uint64_t owner_id;
        bool face_down{false};
        uint64_t exiled_by{0};
    };
    void add_to_exile(std::shared_ptr<cle::core::Card> card, uint64_t owner_id,
                      bool face_down = false, uint64_t exiled_by = 0);
    [[nodiscard]] auto get_exile() const -> const std::vector<ExiledCard>& { return exile_; }
    [[nodiscard]] auto get_exile_for_player(uint64_t player_id) const
        -> std::vector<const ExiledCard*>;
    [[nodiscard]] auto remove_from_exile(uint64_t card_id) -> std::optional<ExiledCard>;

    [[nodiscard]] auto find_card_zone(uint64_t card_id) const -> std::string;
    [[nodiscard]] auto find_card_owner(uint64_t card_id) const -> uint64_t;

    void init_player(uint64_t player_id);

    void clear_all();

private:
    std::unordered_map<uint64_t, std::deque<std::shared_ptr<cle::core::Card>>> libraries_;
    std::unordered_map<uint64_t, std::vector<std::shared_ptr<cle::core::Card>>> hands_;
    std::unordered_map<uint64_t, Permanent> battlefield_;
    std::unordered_map<uint64_t, std::vector<std::shared_ptr<cle::core::Card>>> graveyards_;
    std::vector<ExiledCard> exile_;

    std::unordered_map<uint64_t, uint64_t> card_owner_;
    std::unordered_map<uint64_t, std::string> card_zone_;

    std::mt19937 rng_{std::random_device{}()};
    // NOLINTNEXTLINE(readability-identifier-naming)
    static inline const std::vector<std::shared_ptr<cle::core::Card>> empty_card_vec_;
};

}  // namespace mtg::engine
