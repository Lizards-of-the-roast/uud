#pragma once

#include <atomic>
#include <mutex>
#include <optional>
#include <string>
#include <unordered_set>

#include "auth/game_history_store.hpp"
#include "auth/rating_store.hpp"
#include "engine/game_manager.hpp"
#include "engine/preset_deck.hpp"
#include "mtg/game_service.grpc.pb.h"
#include "mtg/game_state.pb.h"
#include "util/metrics.hpp"
#include <grpcpp/grpcpp.h>

namespace mtg::service {

class GameServiceImpl final : public proto::GameService::Service {
public:
    GameServiceImpl(mtg::engine::GameManager& game_manager,
                    const mtg::engine::PresetDeckLoader& preset_decks,
                    mtg::util::MetricsRegistry* metrics = nullptr,
                    mtg::auth::GameHistoryStore* history_store = nullptr,
                    mtg::auth::RatingStore* rating_store = nullptr);

    grpc::Status CreateGame(grpc::ServerContext* context, const proto::CreateGameRequest* request,
                            proto::CreateGameResponse* response) override;

    grpc::Status JoinGame(grpc::ServerContext* context, const proto::JoinGameRequest* request,
                          proto::JoinGameResponse* response) override;

    grpc::Status LeaveGame(grpc::ServerContext* context, const proto::LeaveGameRequest* request,
                           proto::LeaveGameResponse* response) override;

    grpc::Status ListGames(grpc::ServerContext* context, const proto::ListGamesRequest* request,
                           proto::ListGamesResponse* response) override;

    grpc::Status SubmitDeck(grpc::ServerContext* context, const proto::SubmitDeckRequest* request,
                            proto::SubmitDeckResponse* response) override;

    grpc::Status GetGameState(grpc::ServerContext* context,
                              const proto::GetGameStateRequest* request,
                              proto::GetGameStateResponse* response) override;

    grpc::Status RejoinGame(grpc::ServerContext* context, const proto::RejoinGameRequest* request,
                            proto::RejoinGameResponse* response) override;

    grpc::Status ListPresetDecks(grpc::ServerContext* context,
                                 const proto::ListPresetDecksRequest* request,
                                 proto::ListPresetDecksResponse* response) override;

    grpc::Status GetMatchHistory(grpc::ServerContext* context,
                                 const proto::GetMatchHistoryRequest* request,
                                 proto::GetMatchHistoryResponse* response) override;

    grpc::Status StreamGameState(grpc::ServerContext* context, const proto::StreamRequest* request,
                                 grpc::ServerWriter<proto::GameEvent>* writer) override;

    grpc::Status SubmitAction(
        grpc::ServerContext* context,
        grpc::ServerReaderWriter<proto::GameEvent, proto::PlayerAction>* stream) override;

private:
    [[nodiscard]] static auto extract_user_id(grpc::ServerContext* context) -> std::optional<uint64_t>;
    static auto extract_username(grpc::ServerContext* context) -> std::string;

    mtg::engine::GameManager& game_manager_;
    const mtg::engine::PresetDeckLoader& preset_decks_;
    mtg::util::MetricsRegistry* metrics_{nullptr};
    mtg::auth::GameHistoryStore* history_store_{nullptr};
    mtg::auth::RatingStore* rating_store_{nullptr};
    std::atomic<int64_t> connected_player_count_{0};

    std::mutex recorded_games_mutex_;
    std::unordered_set<std::string> recorded_game_ids_;
    void record_game_result(const std::shared_ptr<mtg::engine::Game>& game);
};

}  // namespace mtg::service
