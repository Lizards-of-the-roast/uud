#include "engine/match.hpp"

#include <algorithm>

namespace mtg::engine {

Match::Match(std::string match_id, MatchConfig config)
    : match_id_{std::move(match_id)}, config_{config} {}

void Match::record_game_result(uint64_t winner_id, const std::string& reason) {
    results_.push_back({current_game_number_, winner_id, reason});
}

auto Match::is_match_over() const -> bool {
    int const needed = (config_.best_of / 2) + 1;
    for (const auto& [game_num, winner, reason] : results_) {
        if (wins_for(winner) >= needed) {
            return true;
        }
    }
    return false;
}

auto Match::match_winner() const -> uint64_t {
    int const needed = (config_.best_of / 2) + 1;
    for (const auto& [game_num, winner, reason] : results_) {
        if (wins_for(winner) >= needed) {
            return winner;
        }
    }
    return 0;
}

auto Match::wins_for(uint64_t player_id) const -> int {
    return static_cast<int>(
        std::ranges::count_if(results_, [player_id](const GameResult& r) {
            return r.winner_id == player_id;
        }));
}

}  // namespace mtg::engine
