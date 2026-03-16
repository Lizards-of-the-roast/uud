#pragma once

#include <cstdint>
#include <optional>
#include <string>

namespace mtg::engine {

class Game;

struct GameOverResult {
    uint64_t winner_id{0};
    std::string reason;
    bool is_draw{false};
};

class WinConditionChecker {
public:
    [[nodiscard]] auto check(const Game& game) -> std::optional<GameOverResult>;
};

}  // namespace mtg::engine
