#include "engine/win_condition.hpp"

#include "engine/game.hpp"

namespace mtg::engine {

auto WinConditionChecker::check(const Game& game) -> std::optional<GameOverResult> {
    const auto& players = game.players();
    std::vector<uint64_t> alive;

    for (const auto& p : players) {
        if (p.is_alive()) {
            alive.push_back(p.id());
        }
    }

    if (alive.size() == 1) {
        return GameOverResult{alive[0], "Last player standing"};
    }
    if (alive.empty()) {
        return GameOverResult{.winner_id = 0, .reason = "All players eliminated (draw)", .is_draw = true};
    }
    return std::nullopt;
}

}  // namespace mtg::engine
