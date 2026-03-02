#pragma once

#include <atomic>
#include <memory>
#include <mutex>
#include <string>
#include <vector>

#include "async_queue.hpp"
#include "mtg/game_service.grpc.pb.h"
#include "mtg/game_state.pb.h"

struct Game_Join_Result {
    bool success;
    std::string error;
};

struct Game_Deck_Result {
    bool valid;
    std::vector<std::string> errors;
};

struct Game_Client {
    void Join_Game(const std::string &game_id);
    void Submit_Deck(const std::string &game_id, const std::vector<std::string> &card_names);
    void Get_State(const std::string &game_id);

    void Start_Action_Stream(const std::string &game_id);
    void Send_Action(const mtg::proto::PlayerAction &action);
    void Stop_Action_Stream();

    std::optional<Game_Join_Result> Poll_Join();
    std::optional<Game_Deck_Result> Poll_Deck();
    std::optional<mtg::proto::GameSnapshot> Poll_Snapshot();
    std::optional<mtg::proto::GameEvent> Poll_Event();

    bool Stream_Active();

   private:
    Async_Queue<Game_Join_Result> join_results_;
    Async_Queue<Game_Deck_Result> deck_results_;
    Async_Queue<mtg::proto::GameSnapshot> snapshots_;
    Async_Queue<mtg::proto::GameEvent> events_;

    std::atomic<bool> stream_active_{false};
    std::mutex stream_mutex_;
    std::shared_ptr<mtg::proto::GameService::Stub> stream_stub_;
    std::unique_ptr<grpc::ClientContext> stream_ctx_;
    std::unique_ptr<grpc::ClientReaderWriter<mtg::proto::PlayerAction, mtg::proto::GameEvent>>
        stream_;
};

extern Game_Client game_client;
