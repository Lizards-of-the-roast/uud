#include "game_client.hpp"

#include <iostream>
#include <thread>

#include "net_client.hpp"

Game_Client game_client;

void Game_Client::Join_Game(const std::string &game_id) {
    std::thread([this, game_id]() {
        auto stub = net.Game();
        Game_Join_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Not connected to server";
            join_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::JoinGameRequest req;
        req.set_game_id(game_id);

        mtg::proto::JoinGameResponse resp;
        grpc::Status status = stub->JoinGame(&ctx, req, &resp);

        if (status.ok() && resp.success()) {
            result.success = true;
        } else {
            result.success = false;
            result.error = status.ok() ? resp.error() : status.error_message();
        }
        join_results_.Push(result);
    }).detach();
}

void Game_Client::Submit_Deck(const std::string &game_id,
                              const std::vector<std::string> &card_names) {
    std::thread([this, game_id, card_names]() {
        auto stub = net.Game();
        Game_Deck_Result result;
        if (!stub) {
            result.valid = false;
            result.errors.push_back("Not connected to server");
            deck_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::SubmitDeckRequest req;
        req.set_game_id(game_id);
        for (const auto &name : card_names)
            req.add_card_names(name);

        mtg::proto::SubmitDeckResponse resp;
        grpc::Status status = stub->SubmitDeck(&ctx, req, &resp);

        if (status.ok()) {
            result.valid = resp.valid();
            for (const auto &err : resp.errors())
                result.errors.push_back(err);
        } else {
            result.valid = false;
            result.errors.push_back(status.error_message());
        }
        deck_results_.Push(result);
    }).detach();
}

void Game_Client::Get_State(const std::string &game_id) {
    std::thread([this, game_id]() {
        auto stub = net.Game();
        if (!stub)
            return;

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::GetGameStateRequest req;
        req.set_game_id(game_id);

        mtg::proto::GetGameStateResponse resp;
        grpc::Status status = stub->GetGameState(&ctx, req, &resp);

        if (status.ok() && resp.has_snapshot())
            snapshots_.Push(resp.snapshot());
    }).detach();
}

void Game_Client::Start_Action_Stream(const std::string &game_id) {
    if (stream_active_)
        return;

    {
        std::lock_guard lock(stream_mutex_);
        stream_stub_ = net.Game();
        if (!stream_stub_)
            return;
        stream_ctx_ = std::make_unique<grpc::ClientContext>();
        net.Attach_Auth(*stream_ctx_);
        stream_ = stream_stub_->SubmitAction(stream_ctx_.get());
    }
    stream_active_ = true;

    std::thread([this]() {
        mtg::proto::GameEvent event;
        while (stream_active_) {
            bool ok = false;
            {
                std::lock_guard lock(stream_mutex_);
                if (!stream_)
                    break;
                ok = stream_->Read(&event);
            }
            if (!ok)
                break;
            events_.Push(event);
        }

        std::lock_guard lock(stream_mutex_);
        if (stream_) {
            grpc::Status status = stream_->Finish();
            if (!status.ok())
                std::cerr << "game stream ended: " << status.error_message() << '\n';
        }
        stream_.reset();
        stream_ctx_.reset();
        stream_stub_.reset();
        stream_active_ = false;
    }).detach();
}

void Game_Client::Send_Action(const mtg::proto::PlayerAction &action) {
    std::lock_guard lock(stream_mutex_);
    if (stream_ && stream_active_)
        stream_->Write(action);
}

void Game_Client::Stop_Action_Stream() {
    stream_active_ = false;
    std::lock_guard lock(stream_mutex_);
    if (stream_)
        stream_->WritesDone();
    if (stream_ctx_)
        stream_ctx_->TryCancel();
}

std::optional<Game_Join_Result> Game_Client::Poll_Join() {
    return join_results_.Poll();
}

std::optional<Game_Deck_Result> Game_Client::Poll_Deck() {
    return deck_results_.Poll();
}

std::optional<mtg::proto::GameSnapshot> Game_Client::Poll_Snapshot() {
    return snapshots_.Poll();
}

std::optional<mtg::proto::GameEvent> Game_Client::Poll_Event() {
    return events_.Poll();
}

bool Game_Client::Stream_Active() {
    return stream_active_;
}
