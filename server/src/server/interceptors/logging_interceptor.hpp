#pragma once

#include <grpcpp/grpcpp.h>
#include <grpcpp/support/server_interceptor.h>

namespace mtg::util {
class MetricsRegistry;
}  // namespace mtg::util

namespace mtg::server {

class LoggingInterceptor final : public grpc::experimental::Interceptor {
public:
    LoggingInterceptor(grpc::experimental::ServerRpcInfo* info,
                       mtg::util::MetricsRegistry* metrics);

    void Intercept(grpc::experimental::InterceptorBatchMethods* methods) override;

private:
    grpc::experimental::ServerRpcInfo* info_;
    mtg::util::MetricsRegistry* metrics_;
    std::chrono::steady_clock::time_point start_time_;
};

class LoggingInterceptorFactory : public grpc::experimental::ServerInterceptorFactoryInterface {
public:
    explicit LoggingInterceptorFactory(mtg::util::MetricsRegistry* metrics = nullptr);

    grpc::experimental::Interceptor* CreateServerInterceptor(
        grpc::experimental::ServerRpcInfo* info) override;

private:
    mtg::util::MetricsRegistry* metrics_;
};

}  // namespace mtg::server
