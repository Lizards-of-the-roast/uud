#pragma once

#include <optional>
#include <string>
#include <vector>

namespace Game {

struct Queue_Status {
    bool matched = false;
    std::string game_id;
    int queue_position = 0;
    int estimated_wait_seconds = 0;
    std::optional<std::string> error;
};

struct Game_Summary {
    std::string game_id;
    int player_count = 0;
    int max_players = 0;
    std::string format;
    std::string status;
};

struct Deck_Submission_Result {
    bool valid = false;
    std::vector<std::string> errors;
};

} //namespace Game
