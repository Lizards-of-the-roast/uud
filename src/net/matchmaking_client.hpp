#pragma once

#include <atomic>
#include <mutex>
#include <string>
#include <thread>

#include "async_queue.hpp"

struct Matchmaking_Update {
    bool matched;
    std::string game_id;
    int32_t queue_position;
    int32_t estimated_wait_seconds;
    bool error;
    std::string error_message;
};

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
    std::optional<Matchmaking_Update> Poll_Update();

    bool In_Queue();

   private:
    Async_Queue<Matchmaking_Join_Result> join_results_;
    Async_Queue<Matchmaking_Update> updates_;
    std::mutex ticket_mutex_;
    std::string queue_ticket_;
    std::atomic<bool> in_queue_{false};
    std::atomic<bool> stream_active_{false};

    void Start_Status_Stream();
};

extern Matchmaking_Client matchmaking_client;
