#pragma once

namespace mtg::engine {

class Game;

class StateBasedActionChecker {
public:
    [[nodiscard]] auto check_and_apply(Game& game) -> bool;
};

}  // namespace mtg::engine
