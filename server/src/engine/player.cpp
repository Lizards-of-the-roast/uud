#include "player.hpp"

namespace mtg::engine {

Player::Player(uint64_t id, std::string username) : id_{id}, username_{std::move(username)} {}

auto Player::can_play_land() const -> bool {
    return lands_played_ < max_land_plays_;
}

auto Player::is_alive() const -> bool {
    return !has_lost_;
}

void Player::reset_for_turn() {
    lands_played_ = 0;
    has_drawn_for_turn_ = false;
    mana_pool_.clear();
}

}  // namespace mtg::engine
