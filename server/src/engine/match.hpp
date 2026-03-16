#pragma once

#include <cstdint>
#include <memory>
#include <string>
#include <vector>

namespace mtg::engine {

class Game;

struct MatchConfig {
    int best_of{3};
    int sideboard_time_seconds{180};
};

struct GameResult {
    int game_number{0};
    uint64_t winner_id{0};
    std::string reason;
};

enum class MatchState : uint8_t {
    WaitingForPlayers,
    InGame,
    Sideboarding,
    Finished,
};

class Match {
public:
    explicit Match(std::string match_id, MatchConfig config = {});

    [[nodiscard]] auto match_id() const -> const std::string& { return match_id_; }
    [[nodiscard]] auto state() const -> MatchState { return state_; }
    [[nodiscard]] auto current_game_number() const -> int { return current_game_number_; }
    [[nodiscard]] auto results() const -> const std::vector<GameResult>& { return results_; }
    [[nodiscard]] auto config() const -> const MatchConfig& { return config_; }

    void record_game_result(uint64_t winner_id, const std::string& reason);
    [[nodiscard]] auto is_match_over() const -> bool;
    [[nodiscard]] auto match_winner() const -> uint64_t;
    [[nodiscard]] auto wins_for(uint64_t player_id) const -> int;

    void begin_sideboarding() { state_ = MatchState::Sideboarding; }
    void begin_next_game() {
        ++current_game_number_;
        state_ = MatchState::InGame;
    }
    void finish() { state_ = MatchState::Finished; }

private:
    std::string match_id_;
    MatchConfig config_;
    MatchState state_{MatchState::WaitingForPlayers};
    int current_game_number_{0};
    std::vector<GameResult> results_;
};

}  // namespace mtg::engine
