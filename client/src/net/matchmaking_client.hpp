#pragma once

#include <atomic>
#include <mutex>
#include <string>
#include <thread>

#include "async_queue.hpp"
#include "game/matchmaking.hpp"


struct Matchmaking_Join_Result {
    bool success;
    std::string queue_ticket;
    std::string error;
};

struct Queue_Info_Result {
    bool success;
    Game::Queue_Info info;
    std::string error;
};

struct Matchmaking_Client {
    void Join_Queue(const std::string &deck_id);
    void Leave_Queue();
    void Stop_Stream();
    void Get_Queue_Info();

    std::optional<Matchmaking_Join_Result> Poll_Join();
    std::optional<Game::Queue_Status> Poll_Update();
    std::optional<Queue_Info_Result> Poll_Queue_Info();

    bool In_Queue();

private:
    Async_Queue<Matchmaking_Join_Result> join_results_;
    Async_Queue<Game::Queue_Status> updates_;
    Async_Queue<Queue_Info_Result> queue_info_results_;
    std::mutex ticket_mutex_;
    std::string queue_ticket_;
    std::atomic<bool> in_queue_{false};
    std::atomic<bool> stream_active_{false};

    void Start_Status_Stream();
};

extern Matchmaking_Client matchmaking_client;
