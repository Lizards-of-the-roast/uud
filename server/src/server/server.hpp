#pragma once

#include <chrono>
#include <memory>
#include <vector>

#include "server/config.hpp"
#include <grpcpp/grpcpp.h>
#include <grpcpp/support/server_interceptor.h>

namespace mtg::server {

class Server {
public:
    Server();
    ~Server();

    Server(const Server&) = delete;
    Server& operator=(const Server&) = delete;
    Server(Server&&) noexcept;
    Server& operator=(Server&&) noexcept;

    void start(const Config& config);
    void register_service(grpc::Service* service);
    void set_interceptors(
        std::vector<std::unique_ptr<grpc::experimental::ServerInterceptorFactoryInterface>>
            interceptors);
    void wait();
    void shutdown();
    void shutdown(std::chrono::system_clock::time_point deadline);

private:
    struct Impl;
    std::unique_ptr<Impl> impl_;
};

}  // namespace mtg::server
