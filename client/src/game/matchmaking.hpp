#pragma once

#include <cstdint>
#include <optional>
#include <string>
#include <vector>

namespace Game {

struct Queue_Status {
    bool matched = false;
    std::string game_id;
    int queue_position = 0;
    int estimated_wait_seconds = 0;
    int elo = 0;
    std::optional<std::string> error;
};

struct Queue_Info {
    int total_queued = 0;
    int queued_players = 0;
};

struct Game_Summary {
    std::string game_id;
    int player_count = 0;
    int max_players = 0;
    std::string status;
};

struct Deck_Submission_Result {
    bool valid = false;
    std::vector<std::string> errors;
};

struct Preset_Deck_Info {
    std::string name;
    int card_count = 0;
};

struct Match_History_Player {
    uint64_t player_id = 0;
    int final_life = 0;
};

struct Match_History_Entry {
    std::string game_id;
    uint64_t winner_id = 0;
    int duration_seconds = 0;
    std::string started_at;
    std::string finished_at;
    std::vector<Match_History_Player> players;
};

} //namespace Game
