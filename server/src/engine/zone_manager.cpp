#include "engine/zone_manager.hpp"

#include <algorithm>

namespace mtg::engine {

void ZoneManager::init_player(uint64_t player_id) {
    libraries_[player_id] = {};
    hands_[player_id] = {};
    graveyards_[player_id] = {};
}

void ZoneManager::clear_all() {
    libraries_.clear();
    hands_.clear();
    battlefield_.clear();
    graveyards_.clear();
    exile_.clear();
    card_owner_.clear();
    card_zone_.clear();
}

void ZoneManager::set_library(uint64_t player_id,
                              std::vector<std::shared_ptr<cle::core::Card>> cards) {
    for (const auto& c : cards) {
        card_owner_[c->instance_id()] = player_id;
        card_zone_[c->instance_id()] = "Library";
    }
    auto& lib = libraries_[player_id];
    lib.assign(std::make_move_iterator(cards.begin()), std::make_move_iterator(cards.end()));
}

auto ZoneManager::draw_card(uint64_t player_id) -> std::optional<std::shared_ptr<cle::core::Card>> {
    auto& lib = libraries_[player_id];
    if (lib.empty()) {
        return std::nullopt;
    }
    auto card = std::move(lib.back());
    lib.pop_back();
    card_zone_[card->instance_id()] = "Hand";
    hands_[player_id].push_back(card);
    return card;
}

void ZoneManager::shuffle_library(uint64_t player_id) {
    auto& lib = libraries_[player_id];
    std::ranges::shuffle(lib, rng_);
}

int ZoneManager::library_size(uint64_t player_id) const {
    auto it = libraries_.find(player_id);
    return it != libraries_.end() ? static_cast<int>(it->second.size()) : 0;
}

void ZoneManager::return_hand_to_library(uint64_t player_id) {
    auto& hand = hands_[player_id];
    auto& lib = libraries_[player_id];
    for (auto& card : hand) {
        card_zone_[card->instance_id()] = "Library";
        lib.push_back(std::move(card));
    }
    hand.clear();
}

void ZoneManager::put_on_top(uint64_t player_id, std::shared_ptr<cle::core::Card> card) {
    card_zone_[card->instance_id()] = "Library";
    libraries_[player_id].push_back(std::move(card));
}

void ZoneManager::put_on_bottom(uint64_t player_id, std::shared_ptr<cle::core::Card> card) {
    card_zone_[card->instance_id()] = "Library";
    libraries_[player_id].push_front(std::move(card));
}

auto ZoneManager::peek_top(uint64_t player_id) -> std::optional<std::shared_ptr<cle::core::Card>> {
    auto& lib = libraries_[player_id];
    if (lib.empty()) {
        return std::nullopt;
    }
    return lib.back();
}

auto ZoneManager::remove_top(uint64_t player_id)
    -> std::optional<std::shared_ptr<cle::core::Card>> {
    auto& lib = libraries_[player_id];
    if (lib.empty()) {
        return std::nullopt;
    }
    auto card = std::move(lib.back());
    lib.pop_back();
    card_zone_.erase(card->instance_id());
    return card;
}

auto ZoneManager::get_hand(uint64_t player_id) -> std::vector<std::shared_ptr<cle::core::Card>>& {
    return hands_[player_id];
}

auto ZoneManager::get_hand(uint64_t player_id) const
    -> const std::vector<std::shared_ptr<cle::core::Card>>& {
    auto it = hands_.find(player_id);
    if (it == hands_.end()) {
        return empty_card_vec_;
    }
    return it->second;
}

int ZoneManager::hand_size(uint64_t player_id) const {
    auto it = hands_.find(player_id);
    return it != hands_.end() ? static_cast<int>(it->second.size()) : 0;
}

auto ZoneManager::remove_from_hand(uint64_t player_id, uint64_t card_instance_id)
    -> std::shared_ptr<cle::core::Card> {
    auto& hand = hands_[player_id];
    for (auto it = hand.begin(); it != hand.end(); ++it) {
        if ((*it)->instance_id() == card_instance_id) {
            auto card = std::move(*it);
            hand.erase(it);
            card_zone_.erase(card_instance_id);
            return card;
        }
    }
    return nullptr;
}

auto ZoneManager::add_to_battlefield(std::shared_ptr<cle::core::Card> card, uint64_t controller_id,
                                     uint64_t owner_id) -> uint64_t {
    uint64_t const perm_id = card->instance_id();
    card_zone_[perm_id] = "Battlefield";
    battlefield_.emplace(perm_id, Permanent{std::move(card), controller_id, owner_id});
    return perm_id;
}

auto ZoneManager::remove_from_battlefield(uint64_t permanent_id) -> std::optional<Permanent> {
    auto it = battlefield_.find(permanent_id);
    if (it == battlefield_.end()) {
        return std::nullopt;
    }
    auto perm = std::move(it->second);
    battlefield_.erase(it);
    card_zone_.erase(permanent_id);
    return perm;
}

auto ZoneManager::find_permanent(uint64_t permanent_id) -> Permanent* {
    auto it = battlefield_.find(permanent_id);
    return it != battlefield_.end() ? &it->second : nullptr;
}

auto ZoneManager::find_permanent(uint64_t permanent_id) const -> const Permanent* {
    auto it = battlefield_.find(permanent_id);
    return it != battlefield_.end() ? &it->second : nullptr;
}

auto ZoneManager::get_permanents_controlled_by(uint64_t player_id) const -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    for (const auto& [id, perm] : battlefield_) {
        if (perm.controller_id() == player_id) {
            result.push_back(id);
        }
    }
    return result;
}

auto ZoneManager::get_permanents_with_type(uint64_t player_id, cle::core::CardType type) const
    -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    for (const auto& [id, perm] : battlefield_) {
        if (perm.controller_id() == player_id && perm.card()->type() == type) {
            result.push_back(id);
        }
    }
    return result;
}

auto ZoneManager::get_permanents_with_subtype(uint64_t player_id, const std::string& subtype) const
    -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    for (const auto& [id, perm] : battlefield_) {
        if (perm.controller_id() != player_id) {
            continue;
        }
        const auto& subs = perm.card()->subtypes();
        if (std::ranges::find(subs, subtype) != subs.end()) {
            result.push_back(id);
        }
    }
    return result;
}

auto ZoneManager::get_all_creatures() const -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    for (const auto& [id, perm] : battlefield_) {
        if (perm.is_creature()) {
            result.push_back(id);
        }
    }
    return result;
}

void ZoneManager::add_to_graveyard(uint64_t player_id, std::shared_ptr<cle::core::Card> card) {
    card_zone_[card->instance_id()] = "Graveyard";
    graveyards_[player_id].push_back(std::move(card));
}

auto ZoneManager::get_graveyard(uint64_t player_id)
    -> std::vector<std::shared_ptr<cle::core::Card>>& {
    return graveyards_[player_id];
}

auto ZoneManager::get_graveyard(uint64_t player_id) const
    -> const std::vector<std::shared_ptr<cle::core::Card>>& {
    auto it = graveyards_.find(player_id);
    if (it == graveyards_.end()) {
        return empty_card_vec_;
    }
    return it->second;
}

auto ZoneManager::remove_from_graveyard(uint64_t player_id, uint64_t card_id)
    -> std::shared_ptr<cle::core::Card> {
    auto& gy = graveyards_[player_id];
    for (auto it = gy.begin(); it != gy.end(); ++it) {
        if ((*it)->instance_id() == card_id) {
            auto card = std::move(*it);
            gy.erase(it);
            card_zone_.erase(card_id);
            return card;
        }
    }
    return nullptr;
}

auto ZoneManager::get_graveyard_cards_with_subtype(uint64_t player_id,
                                                   const std::string& subtype) const
    -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    auto it = graveyards_.find(player_id);
    if (it == graveyards_.end()) {
        return result;
    }
    for (const auto& card : it->second) {
        const auto& subs = card->subtypes();
        if (std::ranges::find(subs, subtype) != subs.end()) {
            result.push_back(card->instance_id());
        }
    }
    return result;
}

void ZoneManager::add_to_exile(std::shared_ptr<cle::core::Card> card, uint64_t owner_id,
                               bool face_down, uint64_t exiled_by) {
    card_zone_[card->instance_id()] = "Exile";
    exile_.push_back(ExiledCard{
        .card = std::move(card),
        .owner_id = owner_id,
        .face_down = face_down,
        .exiled_by = exiled_by,
    });
}

auto ZoneManager::get_exile_for_player(uint64_t player_id) const -> std::vector<const ExiledCard*> {
    std::vector<const ExiledCard*> result;
    for (const auto& exiled : exile_) {
        if (exiled.owner_id == player_id) {
            result.push_back(&exiled);
        }
    }
    return result;
}

auto ZoneManager::remove_from_exile(uint64_t card_id) -> std::optional<ExiledCard> {
    for (auto it = exile_.begin(); it != exile_.end(); ++it) {
        if (it->card->instance_id() == card_id) {
            auto exiled = std::move(*it);
            exile_.erase(it);
            card_zone_.erase(card_id);
            return exiled;
        }
    }
    return std::nullopt;
}

auto ZoneManager::find_card_zone(uint64_t card_id) const -> std::string {
    auto it = card_zone_.find(card_id);
    return it != card_zone_.end() ? it->second : "Unknown";
}

auto ZoneManager::find_card_owner(uint64_t card_id) const -> uint64_t {
    auto it = card_owner_.find(card_id);
    return it != card_owner_.end() ? it->second : 0;
}

}  // namespace mtg::engine
