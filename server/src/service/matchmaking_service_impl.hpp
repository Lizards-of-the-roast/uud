#pragma once

#include <condition_variable>
#include <mutex>
#include <optional>
#include <unordered_map>

#include "auth/rating_store.hpp"
#include "matchmaking/matchmaker.hpp"
#include "matchmaking/queue.hpp"
#include "mtg/matchmaking_service.grpc.pb.h"
#include "util/metrics.hpp"
#include <grpcpp/grpcpp.h>

namespace mtg::service {

class MatchmakingServiceImpl final : public proto::MatchmakingService::Service {
public:
    MatchmakingServiceImpl(mtg::matchmaking::MatchmakingQueue& queue,
                           mtg::matchmaking::Matchmaker& matchmaker,
                           mtg::util::MetricsRegistry* metrics = nullptr,
                           mtg::auth::RatingStore* rating_store = nullptr,
                           int max_queue_wait_seconds = 300);

    grpc::Status JoinQueue(grpc::ServerContext* context, const proto::JoinQueueRequest* request,
                           proto::JoinQueueResponse* response) override;

    grpc::Status LeaveQueue(grpc::ServerContext* context, const proto::LeaveQueueRequest* request,
                            proto::LeaveQueueResponse* response) override;

    grpc::Status QueueStatus(grpc::ServerContext* context, const proto::QueueStatusRequest* request,
                             grpc::ServerWriter<proto::QueueStatusResponse>* writer) override;

    grpc::Status GetQueueInfo(grpc::ServerContext* context,
                              const proto::GetQueueInfoRequest* request,
                              proto::GetQueueInfoResponse* response) override;

private:
    struct MatchResult {
        bool matched{false};
        std::string game_id;
        std::chrono::steady_clock::time_point created_at{std::chrono::steady_clock::now()};
    };

    static auto extract_user_id(grpc::ServerContext* context) -> std::optional<uint64_t>;

    mtg::matchmaking::MatchmakingQueue& queue_;
    mtg::matchmaking::Matchmaker& matchmaker_;
    mtg::util::MetricsRegistry* metrics_{nullptr};
    mtg::auth::RatingStore* rating_store_{nullptr};
    int max_queue_wait_seconds_{300};

    std::mutex match_mutex_;
    std::condition_variable match_cv_;
    std::unordered_map<uint64_t, MatchResult> match_results_;
};

}  // namespace mtg::service
