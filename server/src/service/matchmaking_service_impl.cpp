#include "service/matchmaking_service_impl.hpp"

#include <chrono>

#include <spdlog/spdlog.h>

namespace mtg::service {

MatchmakingServiceImpl::MatchmakingServiceImpl(mtg::matchmaking::MatchmakingQueue& queue,
                                               mtg::matchmaking::Matchmaker& matchmaker,
                                               mtg::util::MetricsRegistry* metrics,
                                               mtg::auth::RatingStore* rating_store,
                                               int max_queue_wait_seconds)
    : queue_{queue},
      matchmaker_{matchmaker},
      metrics_{metrics},
      rating_store_{rating_store},
      max_queue_wait_seconds_{max_queue_wait_seconds} {
    matchmaker_.set_match_callback(
        [this](uint64_t player_a, uint64_t player_b, const std::string& game_id) {
            const std::lock_guard lock{match_mutex_};

            auto now = std::chrono::steady_clock::now();
            std::erase_if(match_results_, [now](const auto& entry) {
                return now - entry.second.created_at > std::chrono::minutes(5);
            });

            match_results_[player_a] = MatchResult{.matched = true, .game_id = game_id};
            match_results_[player_b] = MatchResult{.matched = true, .game_id = game_id};
            match_cv_.notify_all();
        });
}

auto MatchmakingServiceImpl::extract_user_id(grpc::ServerContext* context)
    -> std::optional<uint64_t> {
    auto metadata = context->client_metadata();
    auto it = metadata.find("x-user-id");
    if (it != metadata.end()) {
        try {
            return std::stoull(std::string(it->second.data(), it->second.size()));
        } catch (const std::exception& e) {
            spdlog::debug("Failed to parse x-user-id: {}", e.what());
        }
    }
    return std::nullopt;
}

grpc::Status MatchmakingServiceImpl::JoinQueue(grpc::ServerContext* context,
                                               const proto::JoinQueueRequest* request,
                                               proto::JoinQueueResponse* response) {
    auto opt_user_id = extract_user_id(context);
    if (!opt_user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }
    auto user_id = *opt_user_id;

    if (request->deck_id().size() > 64) {
        return {grpc::StatusCode::INVALID_ARGUMENT, "deck_id too long"};
    }

    std::string username = "Player";
    {
        auto metadata = context->client_metadata();
        auto it = metadata.find("x-username");
        if (it != metadata.end()) {
            username = std::string(it->second.data(), it->second.size());
        }
    }

    mtg::matchmaking::QueueEntry entry;
    entry.player_id = user_id;
    entry.username = std::move(username);
    entry.deck_id = request->deck_id();
    entry.enqueued_at = std::chrono::steady_clock::now();

    if (rating_store_ != nullptr) {
        entry.elo = rating_store_->get_rating(user_id).elo;
    }

    if (!queue_.add(std::move(entry))) {
        return {grpc::StatusCode::RESOURCE_EXHAUSTED,
                "Matchmaking queue is full, please try again later"};
    }
    if (metrics_ != nullptr) {
        metrics_->set_queue_size(static_cast<int64_t>(queue_.size()));
    }

    auto ticket = std::to_string(user_id);
    response->set_queue_ticket(ticket);

    spdlog::info("JoinQueue: user={}", user_id);
    return grpc::Status::OK;
}

grpc::Status MatchmakingServiceImpl::LeaveQueue(
    grpc::ServerContext* context, const proto::LeaveQueueRequest* request,
    [[maybe_unused]] proto::LeaveQueueResponse* response) {
    auto opt_caller_id = extract_user_id(context);
    if (!opt_caller_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }
    auto caller_id = *opt_caller_id;

    try {
        auto player_id = std::stoull(request->queue_ticket());
        if (player_id != caller_id) {
            return {grpc::StatusCode::PERMISSION_DENIED, "Cannot cancel another player's ticket"};
        }
        queue_.remove(player_id);
        if (metrics_ != nullptr) {
            metrics_->set_queue_size(static_cast<int64_t>(queue_.size()));
        }
        spdlog::info("LeaveQueue: player={}", player_id);
    } catch (const std::exception& e) {
        spdlog::debug("LeaveQueue: invalid queue ticket: {}", e.what());
        return {grpc::StatusCode::INVALID_ARGUMENT, "Invalid queue ticket"};
    }

    return grpc::Status::OK;
}

grpc::Status MatchmakingServiceImpl::QueueStatus(
    grpc::ServerContext* context, const proto::QueueStatusRequest* request,
    grpc::ServerWriter<proto::QueueStatusResponse>* writer) {
    auto opt_caller_id = extract_user_id(context);
    if (!opt_caller_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }
    auto caller_id = *opt_caller_id;

    uint64_t player_id = 0;
    try {
        player_id = std::stoull(request->queue_ticket());
    } catch (...) {
        return {grpc::StatusCode::INVALID_ARGUMENT, "Invalid queue ticket"};
    }

    if (player_id != caller_id) {
        return {grpc::StatusCode::PERMISSION_DENIED, "Cannot query another player's ticket"};
    }

    auto start = std::chrono::steady_clock::now();
    auto const max_wait = std::chrono::seconds(max_queue_wait_seconds_);

    while (!context->IsCancelled()) {
        auto elapsed_dur = std::chrono::steady_clock::now() - start;
        if (elapsed_dur >= max_wait) {
            queue_.remove(player_id);
            return {grpc::StatusCode::DEADLINE_EXCEEDED, "Queue wait time exceeded"};
        }

        int32_t queue_position = 0;
        {
            std::unique_lock lock{match_mutex_};
            match_cv_.wait_for(lock, std::chrono::seconds(2));

            auto it = match_results_.find(player_id);
            if (it != match_results_.end() && it->second.matched) {
                proto::QueueStatusResponse resp;
                resp.set_matched(true);
                resp.set_game_id(it->second.game_id);
                resp.set_queue_position(0);
                resp.set_estimated_wait_seconds(0);
                writer->Write(resp);
                match_results_.erase(it);
                return grpc::Status::OK;
            }
            queue_position = static_cast<int32_t>(queue_.position(player_id));
        }

        auto elapsed = std::chrono::duration_cast<std::chrono::seconds>(elapsed_dur).count();

        proto::QueueStatusResponse resp;
        resp.set_matched(false);
        resp.set_queue_position(queue_position);
        auto remaining = std::max(0LL, static_cast<long long>(max_queue_wait_seconds_) - elapsed);
        resp.set_estimated_wait_seconds(static_cast<int32_t>(remaining));

        if (rating_store_ != nullptr) {
            resp.set_elo(rating_store_->get_rating(player_id).elo);
        }
        if (!writer->Write(resp)) {
            return grpc::Status::OK;
        }
    }

    queue_.remove(player_id);
    return grpc::Status::OK;
}

grpc::Status MatchmakingServiceImpl::GetQueueInfo(
    [[maybe_unused]] grpc::ServerContext* context,
    [[maybe_unused]] const proto::GetQueueInfoRequest* request,
    proto::GetQueueInfoResponse* response) {
    auto queued = static_cast<int32_t>(queue_.size());
    response->set_total_queued(queued);
    response->set_queued_players(queued);

    return grpc::Status::OK;
}

}  // namespace mtg::service
