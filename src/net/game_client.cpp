#include "game_client.hpp"

#include <iostream>
#include <mutex>
#include <thread>

#include "convert/proto_convert.hpp"
#include "mtg/game_service.grpc.pb.h"
#include "mtg/game_state.pb.h"
#include "net_client.hpp"

using namespace Game;

struct Game_Client::Impl {
    std::mutex stream_mutex;
    std::shared_ptr<mtg::proto::GameService::Stub> stream_stub;
    std::unique_ptr<grpc::ClientContext> stream_ctx;
    std::unique_ptr<grpc::ClientReaderWriter<mtg::proto::PlayerAction, mtg::proto::GameEvent>>
        stream;

    std::mutex state_stream_mutex;
    std::shared_ptr<mtg::proto::GameService::Stub> state_stream_stub;
    std::unique_ptr<grpc::ClientContext> state_stream_ctx;
    std::unique_ptr<grpc::ClientReader<mtg::proto::GameEvent>> state_stream;
};

Game_Client game_client;

Game_Client::Game_Client() : impl_(std::make_unique<Impl>()) {}
Game_Client::~Game_Client() = default;

void Game_Client::Create_Game(const std::string &format) {
    std::thread([this, format]() {
        auto stub = net.Game();
        Game_Create_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Not connected to server";
            create_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::CreateGameRequest req;
        req.set_format(format);

        mtg::proto::CreateGameResponse resp;
        grpc::Status status = stub->CreateGame(&ctx, req, &resp);

        if (status.ok()) {
            result.success = true;
            result.game_id = resp.game_id();
        } else {
            result.success = false;
            result.error = status.error_message();
        }
        create_results_.Push(result);
    }).detach();
}

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
        Deck_Submission_Result result;
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

void Game_Client::Leave_Game(const std::string &game_id) {
    std::thread([this, game_id]() {
        auto stub = net.Game();
        Game_Leave_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Not connected to server";
            leave_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::LeaveGameRequest req;
        req.set_game_id(game_id);

        mtg::proto::LeaveGameResponse resp;
        grpc::Status status = stub->LeaveGame(&ctx, req, &resp);

        result.success = status.ok();
        if (!status.ok())
            result.error = status.error_message();
        leave_results_.Push(result);
    }).detach();
}

void Game_Client::List_Games(const std::string &format_filter) {
    std::thread([this, format_filter]() {
        auto stub = net.Game();
        Game_List_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Not connected to server";
            list_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::ListGamesRequest req;
        if (!format_filter.empty())
            req.set_format_filter(format_filter);

        mtg::proto::ListGamesResponse resp;
        grpc::Status status = stub->ListGames(&ctx, req, &resp);

        if (status.ok()) {
            result.success = true;
            for (const auto &g : resp.games()) {
                result.games.push_back({
                    .game_id = g.game_id(),
                    .player_count = g.player_count(),
                    .max_players = g.max_players(),
                    .format = g.format(),
                    .status = g.status(),
                });
            }
        } else {
            result.success = false;
            result.error = status.error_message();
        }
        list_results_.Push(result);
    }).detach();
}

void Game_Client::Rejoin_Game(const std::string &game_id) {
    std::thread([this, game_id]() {
        auto stub = net.Game();
        Game_Rejoin_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Not connected to server";
            rejoin_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::RejoinGameRequest req;
        req.set_game_id(game_id);

        mtg::proto::RejoinGameResponse resp;
        grpc::Status status = stub->RejoinGame(&ctx, req, &resp);

        if (status.ok() && resp.success()) {
            result.success = true;
            if (resp.has_snapshot())
                result.snapshot = convert::From_Proto(resp.snapshot());
        } else {
            result.success = false;
            result.error = status.ok() ? resp.error() : status.error_message();
        }
        rejoin_results_.Push(result);
    }).detach();
}

void Game_Client::Get_State(const std::string &game_id) {
    std::thread([this, game_id]() {
        auto stub = net.Game();
        if (!stub) {
            std::cerr << "game: Get_State failed - not connected\n";
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::GetGameStateRequest req;
        req.set_game_id(game_id);

        mtg::proto::GetGameStateResponse resp;
        grpc::Status status = stub->GetGameState(&ctx, req, &resp);

        if (status.ok() && resp.has_snapshot())
            snapshots_.Push(convert::From_Proto(resp.snapshot()));
        else
            std::cerr << "game: Get_State failed - " << status.error_message() << '\n';
    }).detach();
}

void Game_Client::Start_Action_Stream(const std::string &game_id) {
    if (stream_active_.exchange(true))
        return;

    {
        std::lock_guard lock(impl_->stream_mutex);
        impl_->stream_stub = net.Game();
        if (!impl_->stream_stub) {
            stream_active_ = false;
            return;
        }
        impl_->stream_ctx = std::make_unique<grpc::ClientContext>();
        net.Attach_Auth(*impl_->stream_ctx);
        impl_->stream = impl_->stream_stub->SubmitAction(impl_->stream_ctx.get());
    }

    std::thread([this]() {
        mtg::proto::GameEvent event;
        while (stream_active_) {
            bool ok = false;
            {
                std::lock_guard lock(impl_->stream_mutex);
                if (!impl_->stream)
                    break;
                ok = impl_->stream->Read(&event);
            }
            if (!ok)
                break;
            events_.Push(convert::From_Proto(event));
        }

        std::lock_guard lock(impl_->stream_mutex);
        if (impl_->stream) {
            grpc::Status status = impl_->stream->Finish();
            if (!status.ok())
                std::cerr << "game stream ended: " << status.error_message() << '\n';
        }
        impl_->stream.reset();
        impl_->stream_ctx.reset();
        impl_->stream_stub.reset();
        stream_active_ = false;
    }).detach();
}

void Game_Client::Send_Action(const Player_Action &action) {
    std::lock_guard lock(impl_->stream_mutex);
    if (impl_->stream && stream_active_)
        impl_->stream->Write(convert::To_Proto(action));
}

void Game_Client::Stop_Action_Stream() {
    stream_active_ = false;
    std::lock_guard lock(impl_->stream_mutex);
    if (impl_->stream)
        impl_->stream->WritesDone();
    if (impl_->stream_ctx)
        impl_->stream_ctx->TryCancel();
}

void Game_Client::Start_State_Stream(const std::string &game_id, uint64_t last_sequence) {
    if (state_stream_active_.exchange(true))
        return;

    {
        std::lock_guard lock(impl_->state_stream_mutex);
        impl_->state_stream_stub = net.Game();
        if (!impl_->state_stream_stub) {
            state_stream_active_ = false;
            return;
        }
        impl_->state_stream_ctx = std::make_unique<grpc::ClientContext>();
        net.Attach_Auth(*impl_->state_stream_ctx);

        mtg::proto::StreamRequest req;
        req.set_game_id(game_id);
        req.set_last_sequence(last_sequence);
        impl_->state_stream =
            impl_->state_stream_stub->StreamGameState(impl_->state_stream_ctx.get(), req);
    }

    std::thread([this]() {
        mtg::proto::GameEvent event;
        while (state_stream_active_) {
            bool ok = false;
            {
                std::lock_guard lock(impl_->state_stream_mutex);
                if (!impl_->state_stream)
                    break;
                ok = impl_->state_stream->Read(&event);
            }
            if (!ok)
                break;
            events_.Push(convert::From_Proto(event));
        }

        std::lock_guard lock(impl_->state_stream_mutex);
        if (impl_->state_stream) {
            impl_->state_stream->Finish();
        }
        impl_->state_stream.reset();
        impl_->state_stream_ctx.reset();
        impl_->state_stream_stub.reset();
        state_stream_active_ = false;
    }).detach();
}

void Game_Client::Stop_State_Stream() {
    state_stream_active_ = false;
    std::lock_guard lock(impl_->state_stream_mutex);
    if (impl_->state_stream_ctx)
        impl_->state_stream_ctx->TryCancel();
}

std::optional<Game_Create_Result> Game_Client::Poll_Create() {
    return create_results_.Poll();
}

std::optional<Game_Join_Result> Game_Client::Poll_Join() {
    return join_results_.Poll();
}

std::optional<Deck_Submission_Result> Game_Client::Poll_Deck() {
    return deck_results_.Poll();
}

std::optional<Game_Snapshot> Game_Client::Poll_Snapshot() {
    return snapshots_.Poll();
}

std::optional<Game_Event> Game_Client::Poll_Event() {
    return events_.Poll();
}

std::optional<Game_Leave_Result> Game_Client::Poll_Leave() {
    return leave_results_.Poll();
}

std::optional<Game_List_Result> Game_Client::Poll_List() {
    return list_results_.Poll();
}

std::optional<Game_Rejoin_Result> Game_Client::Poll_Rejoin() {
    return rejoin_results_.Poll();
}

bool Game_Client::Stream_Active() {
    return stream_active_;
}

bool Game_Client::State_Stream_Active() {
    return state_stream_active_;
}
