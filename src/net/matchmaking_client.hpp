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

struct Matchmaking_Client {
    void Join_Queue(const std::string &format, const std::string &deck_id);
    void Leave_Queue();
    void Stop_Stream();

    std::optional<Matchmaking_Join_Result> Poll_Join();
    std::optional<Queue_Status> Poll_Update();

    bool In_Queue();

private:
    Async_Queue<Matchmaking_Join_Result> join_results_;
    Async_Queue<Queue_Status> updates_;
    std::mutex ticket_mutex_;
    std::string queue_ticket_;
    std::atomic<bool> in_queue_{false};
    std::atomic<bool> stream_active_{false};

    void Start_Status_Stream();
};

extern Matchmaking_Client matchmaking_client;
