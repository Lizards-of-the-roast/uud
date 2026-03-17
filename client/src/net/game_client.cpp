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

void Game_Client::Create_Game() {
    std::thread([this]() {
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

void Game_Client::Submit_Preset_Deck(const std::string &game_id,
                                      const std::string &preset_deck_name) {
    std::cerr << "game: Submit_Preset_Deck game_id=" << game_id
              << " deck=" << preset_deck_name << '\n';
    std::thread([this, game_id, preset_deck_name]() {
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
        req.set_preset_deck_name(preset_deck_name);

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

void Game_Client::List_Games() {
    std::thread([this]() {
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

        mtg::proto::ListGamesResponse resp;
        grpc::Status status = stub->ListGames(&ctx, req, &resp);

        if (status.ok()) {
            result.success = true;
            for (const auto &g : resp.games()) {
                result.games.push_back({
                    .game_id = g.game_id(),
                    .player_count = g.player_count(),
                    .max_players = g.max_players(),
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
    std::cerr << "game: Get_State game_id=" << game_id << '\n';
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

        if (status.ok() && resp.has_snapshot()) {
            std::cerr << "game: Get_State got snapshot with "
                      << resp.snapshot().players_size() << " players\n";
            snapshots_.Push(convert::From_Proto(resp.snapshot()));
        } else {
            std::cerr << "game: Get_State failed - ok=" << status.ok()
                      << " has_snapshot=" << resp.has_snapshot()
                      << " msg=" << status.error_message() << '\n';
        }
    }).detach();
}

void Game_Client::Get_Active_Game() {
    std::thread([this]() {
        auto stub = net.Game();
        Active_Game_Result result;
        if (!stub) {
            active_game_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::GetActiveGameRequest req;
        mtg::proto::GetActiveGameResponse resp;
        grpc::Status status = stub->GetActiveGame(&ctx, req, &resp);

        if (status.ok() && resp.has_active_game()) {
            result.has_active_game = true;
            result.game_id = resp.game_id();
        }
        active_game_results_.Push(result);
    }).detach();
}

void Game_Client::List_Preset_Decks() {
    std::thread([this]() {
        auto stub = net.Game();
        Preset_Decks_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Not connected to server";
            preset_decks_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::ListPresetDecksRequest req;

        mtg::proto::ListPresetDecksResponse resp;
        grpc::Status status = stub->ListPresetDecks(&ctx, req, &resp);

        if (status.ok()) {
            result.success = true;
            for (const auto &d : resp.decks()) {
                result.decks.push_back({
                    .name = d.name(),
                    .card_count = d.card_count(),
                });
            }
        } else {
            result.success = false;
            result.error = status.error_message();
        }
        preset_decks_results_.Push(result);
    }).detach();
}

void Game_Client::Get_Match_History(int limit, int offset) {
    std::thread([this, limit, offset]() {
        auto stub = net.Game();
        Match_History_Result result;
        if (!stub) {
            result.success = false;
            result.error = "Not connected to server";
            match_history_results_.Push(result);
            return;
        }

        grpc::ClientContext ctx;
        net.Attach_Auth(ctx);
        mtg::proto::GetMatchHistoryRequest req;
        req.set_limit(limit);
        req.set_offset(offset);

        mtg::proto::GetMatchHistoryResponse resp;
        grpc::Status status = stub->GetMatchHistory(&ctx, req, &resp);

        if (status.ok()) {
            result.success = true;
            result.total_count = resp.total_count();
            for (const auto &m : resp.matches()) {
                Game::Match_History_Entry entry;
                entry.game_id = m.game_id();
                entry.winner_id = m.winner_id();
                entry.duration_seconds = m.duration_seconds();
                entry.started_at = m.started_at();
                entry.finished_at = m.finished_at();
                for (const auto &p : m.players()) {
                    entry.players.push_back({
                        .player_id = p.player_id(),
                        .final_life = p.final_life(),
                    });
                }
                result.matches.push_back(std::move(entry));
            }
        } else {
            result.success = false;
            result.error = status.error_message();
        }
        match_history_results_.Push(result);
    }).detach();
}

void Game_Client::Start_Action_Stream(const std::string &game_id) {
    std::cerr << "game: Start_Action_Stream game_id=" << game_id << '\n';
    if (stream_active_.exchange(true)) {
        std::cerr << "game: action stream already active, skipping\n";
        return;
    }

    {
        std::lock_guard lock(impl_->stream_mutex);
        impl_->stream_stub = net.Game();
        if (!impl_->stream_stub) {
            std::cerr << "game: action stream, no stub (not connected)\n";
            stream_active_ = false;
            return;
        }
        impl_->stream_ctx = std::make_unique<grpc::ClientContext>();
        net.Attach_Auth(*impl_->stream_ctx);
        impl_->stream = impl_->stream_stub->SubmitAction(impl_->stream_ctx.get());
        std::cerr << "game: action stream opened, sending handshake\n";

        mtg::proto::PlayerAction handshake;
        handshake.set_game_id(game_id);
        handshake.mutable_pass();
        if (!impl_->stream->Write(handshake)) {
            std::cerr << "game: handshake write failed, tearing down stream\n";
            impl_->stream->WritesDone();
            impl_->stream->Finish();
            impl_->stream.reset();
            impl_->stream_ctx.reset();
            impl_->stream_stub.reset();
            stream_active_ = false;
            return;
        }
        std::cerr << "game: handshake sent OK\n";
    }

    std::thread([this]() {
        std::cerr << "game: action stream read loop started\n";
        int event_count = 0;
        mtg::proto::GameEvent event;
        while (stream_active_) {
            grpc::ClientReaderWriter<mtg::proto::PlayerAction, mtg::proto::GameEvent> *raw =
                nullptr;
            {
                std::lock_guard lock(impl_->stream_mutex);
                if (!impl_->stream)
                    break;
                raw = impl_->stream.get();
            }
            bool ok = raw->Read(&event);
            if (!ok) {
                std::cerr << "game: action stream Read() returned false after " << event_count
                          << " events\n";
                break;
            }
            event_count++;
            std::cerr << "game: action stream event #" << event_count
                      << " type=" << event.event_case() << '\n';
            events_.Push(convert::From_Proto(event));
        }

        grpc::ClientReaderWriter<mtg::proto::PlayerAction, mtg::proto::GameEvent> *raw = nullptr;
        {
            std::lock_guard lock(impl_->stream_mutex);
            if (impl_->stream)
                raw = impl_->stream.get();
        }
        if (raw) {
            grpc::Status status = raw->Finish();
            std::cerr << "game: action stream finished: ok=" << status.ok()
                      << " code=" << status.error_code() << " msg=" << status.error_message()
                      << '\n';
        }
        {
            std::lock_guard lock(impl_->stream_mutex);
            impl_->stream.reset();
            impl_->stream_ctx.reset();
            impl_->stream_stub.reset();
        }
        stream_active_ = false;
    }).detach();
}

bool Game_Client::Send_Action(const Player_Action &action) {
    std::lock_guard lock(impl_->stream_mutex);
    if (impl_->stream && stream_active_) {
        std::cerr << "game: Send_Action prompt=" << action.prompt_id << '\n';
        bool ok = impl_->stream->Write(convert::To_Proto(action));
        if (!ok)
            std::cerr << "game: Send_Action Write() returned false\n";
        return ok;
    } else {
        std::cerr << "game: Send_Action DROPPED, stream not active\n";
        return false;
    }
}

void Game_Client::Stop_Action_Stream() {
    stream_active_ = false;
    std::lock_guard lock(impl_->stream_mutex);
    if (impl_->stream_ctx)
        impl_->stream_ctx->TryCancel();
    if (impl_->stream)
        impl_->stream->WritesDone();
}

void Game_Client::Start_State_Stream(const std::string &game_id, uint64_t last_sequence) {
    std::cerr << "game: Start_State_Stream game_id=" << game_id << " last_seq=" << last_sequence
              << '\n';
    if (state_stream_active_.exchange(true)) {
        std::cerr << "game: state stream already active, skipping\n";
        return;
    }

    {
        std::lock_guard lock(impl_->state_stream_mutex);
        impl_->state_stream_stub = net.Game();
        if (!impl_->state_stream_stub) {
            std::cerr << "game: state stream, no stub (not connected)\n";
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
        std::cerr << "game: state stream opened\n";
    }

    std::thread([this]() {
        std::cerr << "game: state stream read loop started\n";
        int event_count = 0;
        mtg::proto::GameEvent event;
        while (state_stream_active_) {
            grpc::ClientReader<mtg::proto::GameEvent> *raw = nullptr;
            {
                std::lock_guard lock(impl_->state_stream_mutex);
                if (!impl_->state_stream)
                    break;
                raw = impl_->state_stream.get();
            }
            bool ok = raw->Read(&event);
            if (!ok) {
                std::cerr << "game: state stream Read() returned false after " << event_count
                          << " events\n";
                break;
            }
            event_count++;
            std::cerr << "game: state stream event #" << event_count
                      << " type=" << event.event_case() << '\n';
            events_.Push(convert::From_Proto(event));
        }

        grpc::ClientReader<mtg::proto::GameEvent> *raw = nullptr;
        {
            std::lock_guard lock(impl_->state_stream_mutex);
            if (impl_->state_stream)
                raw = impl_->state_stream.get();
        }
        if (raw) {
            grpc::Status status = raw->Finish();
            std::cerr << "game: state stream finished: ok=" << status.ok()
                      << " code=" << status.error_code() << " msg=" << status.error_message()
                      << '\n';
        }
        {
            std::lock_guard lock(impl_->state_stream_mutex);
            impl_->state_stream.reset();
            impl_->state_stream_ctx.reset();
            impl_->state_stream_stub.reset();
        }
        state_stream_active_ = false;
    }).detach();
}

void Game_Client::Stop_State_Stream() {
    state_stream_active_ = false;
    std::lock_guard lock(impl_->state_stream_mutex);
    if (impl_->state_stream_ctx)
        impl_->state_stream_ctx->TryCancel();
}

void Game_Client::Drain_All() {
    create_results_.Clear();
    join_results_.Clear();
    leave_results_.Clear();
    list_results_.Clear();
    rejoin_results_.Clear();
    active_game_results_.Clear();
    preset_decks_results_.Clear();
    match_history_results_.Clear();
    deck_results_.Clear();
    snapshots_.Clear();
    events_.Clear();
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

std::optional<Preset_Decks_Result> Game_Client::Poll_Preset_Decks() {
    return preset_decks_results_.Poll();
}

std::optional<Match_History_Result> Game_Client::Poll_Match_History() {
    return match_history_results_.Poll();
}

std::optional<Active_Game_Result> Game_Client::Poll_Active_Game() {
    return active_game_results_.Poll();
}

bool Game_Client::Stream_Active() {
    return stream_active_;
}

bool Game_Client::State_Stream_Active() {
    return state_stream_active_;
}
