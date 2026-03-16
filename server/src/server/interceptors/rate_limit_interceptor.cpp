#include "server/interceptors/rate_limit_interceptor.hpp"

#include <algorithm>

#include <spdlog/spdlog.h>

namespace mtg::server {

RateLimitInterceptor::RateLimitInterceptor(grpc::experimental::ServerRpcInfo* info, int max_rps,
                                           std::unordered_map<std::string, TokenBucket>& rate_state,
                                           std::mutex& state_mutex)
    : info_(info), max_rps_(max_rps), rate_state_(rate_state), state_mutex_(state_mutex) {}

void RateLimitInterceptor::Intercept(grpc::experimental::InterceptorBatchMethods* methods) {
    if (methods->QueryInterceptionHookPoint(
            grpc::experimental::InterceptionHookPoints::POST_RECV_INITIAL_METADATA)) {
        auto* ctx = info_->server_context();
        std::string peer_key = "unknown";
        if (ctx != nullptr) {
            auto metadata = ctx->client_metadata();
            auto it = metadata.find("x-user-id");
            if (it != metadata.end()) {
                peer_key = "user:" + std::string(it->second.data(), it->second.size());
            } else {
                peer_key = ctx->peer();
            }
        }

        const std::lock_guard lock(state_mutex_);
        auto now = std::chrono::steady_clock::now();

        constexpr size_t cleanup_threshold = 10000;
        constexpr auto stale_threshold = std::chrono::minutes(5);
        if (rate_state_.size() > cleanup_threshold) {
            std::erase_if(rate_state_, [now, stale_threshold](const auto& entry) {
                return (now - entry.second.last_refill) > stale_threshold;
            });
        }

        auto [it, inserted] = rate_state_.try_emplace(
            peer_key, TokenBucket{.tokens = static_cast<double>(max_rps_), .last_refill = now});

        auto& bucket = it->second;

        auto elapsed = std::chrono::duration<double>(now - bucket.last_refill).count();
        bucket.tokens =
            std::min(static_cast<double>(max_rps_), bucket.tokens + (elapsed * max_rps_));
        bucket.last_refill = now;

        if (bucket.tokens >= 1.0) {
            bucket.tokens -= 1.0;
        } else {
            spdlog::warn("RateLimitInterceptor: rate limit exceeded for {}", peer_key);
            rate_limited_ = true;
        }
    }

    if (methods->QueryInterceptionHookPoint(
            grpc::experimental::InterceptionHookPoints::PRE_SEND_STATUS)) {
        if (rate_limited_) {
            methods->ModifySendStatus(
                grpc::Status(grpc::StatusCode::RESOURCE_EXHAUSTED, "Rate limit exceeded"));
        }
    }

    methods->Proceed();
}

RateLimitInterceptorFactory::RateLimitInterceptorFactory(int max_rps) : max_rps_(max_rps) {}

grpc::experimental::Interceptor* RateLimitInterceptorFactory::CreateServerInterceptor(
    grpc::experimental::ServerRpcInfo* info) {
    return new RateLimitInterceptor(info, max_rps_, rate_state_, state_mutex_);
}

}  // namespace mtg::server
