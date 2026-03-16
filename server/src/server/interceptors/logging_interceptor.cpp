#include "server/interceptors/logging_interceptor.hpp"

#include <chrono>

#include "util/metrics.hpp"
#include <spdlog/spdlog.h>

namespace mtg::server {

LoggingInterceptor::LoggingInterceptor(grpc::experimental::ServerRpcInfo* info,
                                       mtg::util::MetricsRegistry* metrics)
    : info_(info), metrics_(metrics), start_time_(std::chrono::steady_clock::now()) {}

void LoggingInterceptor::Intercept(grpc::experimental::InterceptorBatchMethods* methods) {
    if (methods->QueryInterceptionHookPoint(
            grpc::experimental::InterceptionHookPoints::POST_RECV_INITIAL_METADATA)) {
        start_time_ = std::chrono::steady_clock::now();
        spdlog::info("RPC started: {}", info_->method());
    }

    if (methods->QueryInterceptionHookPoint(
            grpc::experimental::InterceptionHookPoints::PRE_SEND_STATUS)) {
        auto elapsed = std::chrono::steady_clock::now() - start_time_;
        auto ms = static_cast<double>(
                      std::chrono::duration_cast<std::chrono::microseconds>(elapsed).count()) /
                  1000.0;

        auto status = methods->GetSendStatus();
        auto code = static_cast<int>(status.error_code());
        spdlog::info("RPC completed: {} ({:.1f}ms, status={})", info_->method(), ms, code);

        if (metrics_ != nullptr) {
            metrics_->record_rpc(info_->method(), ms, code);
        }
    }

    methods->Proceed();
}

LoggingInterceptorFactory::LoggingInterceptorFactory(mtg::util::MetricsRegistry* metrics)
    : metrics_(metrics) {}

grpc::experimental::Interceptor* LoggingInterceptorFactory::CreateServerInterceptor(
    grpc::experimental::ServerRpcInfo* info) {
    return new LoggingInterceptor(info, metrics_);
}

}  // namespace mtg::server
