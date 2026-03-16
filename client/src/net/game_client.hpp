#pragma once

#include <atomic>
#include <memory>
#include <string>
#include <vector>

#include "async_queue.hpp"
#include "game/actions.hpp"
#include "game/events.hpp"
#include "game/game_snapshot.hpp"
#include "game/matchmaking.hpp"

struct Game_Join_Result {
    bool success;
    std::string error;
};

struct Game_Create_Result {
    bool success;
    std::string game_id;
    std::string error;
};

struct Game_Leave_Result {
    bool success;
    std::string error;
};

struct Game_List_Result {
    bool success;
    std::vector<Game::Game_Summary> games;
    std::string error;
};

struct Game_Rejoin_Result {
    bool success;
    std::string error;
    std::optional<Game::Game_Snapshot> snapshot;
};

struct Preset_Decks_Result {
    bool success;
    std::vector<Game::Preset_Deck_Info> decks;
    std::string error;
};

struct Match_History_Result {
    bool success;
    std::vector<Game::Match_History_Entry> matches;
    int total_count = 0;
    std::string error;
};

struct Game_Client {
    Game_Client();
    ~Game_Client();

    void Create_Game();
    void Join_Game(const std::string &game_id);
    void Leave_Game(const std::string &game_id);
    void List_Games();
    void Rejoin_Game(const std::string &game_id);
    void Submit_Deck(const std::string &game_id, const std::vector<std::string> &card_names);
    void Submit_Preset_Deck(const std::string &game_id, const std::string &preset_deck_name);
    void Get_State(const std::string &game_id);
    void List_Preset_Decks();
    void Get_Match_History(int limit = 10, int offset = 0);

    void Start_Action_Stream(const std::string &game_id);
    bool Send_Action(const Game::Player_Action &action);
    void Stop_Action_Stream();

    void Start_State_Stream(const std::string &game_id, uint64_t last_sequence = 0);
    void Stop_State_Stream();

    void Drain_All();

    std::optional<Game_Create_Result> Poll_Create();
    std::optional<Game_Join_Result> Poll_Join();
    std::optional<Game_Leave_Result> Poll_Leave();
    std::optional<Game_List_Result> Poll_List();
    std::optional<Game_Rejoin_Result> Poll_Rejoin();
    std::optional<Preset_Decks_Result> Poll_Preset_Decks();
    std::optional<Match_History_Result> Poll_Match_History();
    std::optional<Game::Deck_Submission_Result> Poll_Deck();
    std::optional<Game::Game_Snapshot> Poll_Snapshot();
    std::optional<Game::Game_Event> Poll_Event();

    bool Stream_Active();
    bool State_Stream_Active();

private:
    Async_Queue<Game_Create_Result> create_results_;
    Async_Queue<Game_Join_Result> join_results_;
    Async_Queue<Game_Leave_Result> leave_results_;
    Async_Queue<Game_List_Result> list_results_;
    Async_Queue<Game_Rejoin_Result> rejoin_results_;
    Async_Queue<Preset_Decks_Result> preset_decks_results_;
    Async_Queue<Match_History_Result> match_history_results_;
    Async_Queue<Game::Deck_Submission_Result> deck_results_;
    Async_Queue<Game::Game_Snapshot> snapshots_;
    Async_Queue<Game::Game_Event> events_;

    std::atomic<bool> stream_active_{false};
    std::atomic<bool> state_stream_active_{false};

    struct Impl;
    std::unique_ptr<Impl> impl_;
};

extern Game_Client game_client;
