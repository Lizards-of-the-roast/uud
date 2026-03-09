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

using namespace Game;

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
    std::vector<Game_Summary> games;
    std::string error;
};

struct Game_Rejoin_Result {
    bool success;
    std::string error;
    std::optional<Game_Snapshot> snapshot;
};

struct Game_Client {
    Game_Client();
    ~Game_Client();

    void Create_Game(const std::string &format);
    void Join_Game(const std::string &game_id);
    void Leave_Game(const std::string &game_id);
    void List_Games(const std::string &format_filter = "");
    void Rejoin_Game(const std::string &game_id);
    void Submit_Deck(const std::string &game_id, const std::vector<std::string> &card_names);
    void Get_State(const std::string &game_id);

    void Start_Action_Stream(const std::string &game_id);
    void Send_Action(const Player_Action &action);
    void Stop_Action_Stream();

    void Start_State_Stream(const std::string &game_id, uint64_t last_sequence = 0);
    void Stop_State_Stream();

    std::optional<Game_Create_Result> Poll_Create();
    std::optional<Game_Join_Result> Poll_Join();
    std::optional<Game_Leave_Result> Poll_Leave();
    std::optional<Game_List_Result> Poll_List();
    std::optional<Game_Rejoin_Result> Poll_Rejoin();
    std::optional<Deck_Submission_Result> Poll_Deck();
    std::optional<Game_Snapshot> Poll_Snapshot();
    std::optional<Game_Event> Poll_Event();

    bool Stream_Active();
    bool State_Stream_Active();

private:
    Async_Queue<Game_Create_Result> create_results_;
    Async_Queue<Game_Join_Result> join_results_;
    Async_Queue<Game_Leave_Result> leave_results_;
    Async_Queue<Game_List_Result> list_results_;
    Async_Queue<Game_Rejoin_Result> rejoin_results_;
    Async_Queue<Deck_Submission_Result> deck_results_;
    Async_Queue<Game_Snapshot> snapshots_;
    Async_Queue<Game_Event> events_;

    std::atomic<bool> stream_active_{false};
    std::atomic<bool> state_stream_active_{false};

    struct Impl;
    std::unique_ptr<Impl> impl_;
};

extern Game_Client game_client;
