#pragma once

#include <chrono>
#include <cstdint>
#include <mutex>
#include <string>
#include <unordered_map>

#include <grpcpp/grpcpp.h>
#include <grpcpp/support/server_interceptor.h>

namespace mtg::server {

struct TokenBucket {
    double tokens{0.0};
    std::chrono::steady_clock::time_point last_refill;
};

class RateLimitInterceptor final : public grpc::experimental::Interceptor {
public:
    RateLimitInterceptor(grpc::experimental::ServerRpcInfo* info, int max_rps,
                         std::unordered_map<std::string, TokenBucket>& rate_state,
                         std::mutex& state_mutex);

    void Intercept(grpc::experimental::InterceptorBatchMethods* methods) override;

private:
    grpc::experimental::ServerRpcInfo* info_;
    int max_rps_;
    std::unordered_map<std::string, TokenBucket>& rate_state_;
    std::mutex& state_mutex_;
    bool rate_limited_{false};
};

class RateLimitInterceptorFactory : public grpc::experimental::ServerInterceptorFactoryInterface {
public:
    explicit RateLimitInterceptorFactory(int max_rps = 100);

    grpc::experimental::Interceptor* CreateServerInterceptor(
        grpc::experimental::ServerRpcInfo* info) override;

private:
    int max_rps_;
    std::unordered_map<std::string, TokenBucket> rate_state_;
    std::mutex state_mutex_;
};

}  // namespace mtg::server
