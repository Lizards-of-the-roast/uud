#include "server/server.hpp"

#include <fstream>
#include <sstream>
#include <vector>

#include <grpcpp/grpcpp.h>
#include <grpcpp/security/server_credentials.h>
#include <spdlog/spdlog.h>

namespace mtg::server {

struct Server::Impl {
    std::unique_ptr<grpc::Server> grpc_server;
    std::vector<grpc::Service*> services;
    std::vector<std::unique_ptr<grpc::experimental::ServerInterceptorFactoryInterface>>
        interceptors;
};

Server::Server() : impl_(std::make_unique<Impl>()) {}

Server::~Server() {
    if (impl_ && impl_->grpc_server) {
        shutdown();
    }
}

Server::Server(Server&&) noexcept = default;
Server& Server::operator=(Server&&) noexcept = default;

void Server::register_service(grpc::Service* service) {
    impl_->services.push_back(service);
}

void Server::set_interceptors(
    std::vector<std::unique_ptr<grpc::experimental::ServerInterceptorFactoryInterface>>
        interceptors) {
    impl_->interceptors = std::move(interceptors);
}

namespace {
auto read_file(const std::string& path) -> std::string {
    const std::ifstream f(path);
    if (!f) {
        throw std::runtime_error("Failed to read file: " + path);
    }
    std::ostringstream ss;
    ss << f.rdbuf();
    return ss.str();
}
}  // namespace

void Server::start(const Config& config) {
    grpc::ServerBuilder builder;

    if (!config.tls_cert_path.empty() && !config.tls_key_path.empty()) {
        auto cert = read_file(config.tls_cert_path);
        auto key = read_file(config.tls_key_path);

        grpc::SslServerCredentialsOptions ssl_opts;
        ssl_opts.pem_key_cert_pairs.push_back({key, cert});
        auto creds = grpc::SslServerCredentials(ssl_opts);

        builder.AddListeningPort(config.listen_address, creds);
        spdlog::debug("TLS enabled with cert={} key={}", config.tls_cert_path, config.tls_key_path);
    } else {
        builder.AddListeningPort(config.listen_address, grpc::InsecureServerCredentials());
        spdlog::warn(
            "TLS not configured using insecure plaintext transport. "
            "Set MTG_TLS_CERT_PATH and MTG_TLS_KEY_PATH for production.");
    }

    builder.SetMaxReceiveMessageSize(16 * 1024 * 1024);
    builder.SetMaxSendMessageSize(16 * 1024 * 1024);

    builder.AddChannelArgument(GRPC_ARG_KEEPALIVE_TIME_MS, config.keepalive_time_ms);
    builder.AddChannelArgument(GRPC_ARG_KEEPALIVE_TIMEOUT_MS, config.keepalive_timeout_ms);
    builder.AddChannelArgument(GRPC_ARG_KEEPALIVE_PERMIT_WITHOUT_CALLS, 1);
    builder.AddChannelArgument(GRPC_ARG_HTTP2_MAX_PINGS_WITHOUT_DATA, 0);
    builder.AddChannelArgument(GRPC_ARG_MAX_CONNECTION_IDLE_MS, 300000);
    builder.AddChannelArgument(GRPC_ARG_MAX_CONCURRENT_STREAMS, config.max_concurrent_streams);

    for (auto* svc : impl_->services) {
        builder.RegisterService(svc);
    }

    if (!impl_->interceptors.empty()) {
        builder.experimental().SetInterceptorCreators(std::move(impl_->interceptors));
    }

    impl_->grpc_server = builder.BuildAndStart();

    if (!impl_->grpc_server) {
        spdlog::critical("Failed to start gRPC server on {}", config.listen_address);
        throw std::runtime_error("Failed to start gRPC server");
    }

    spdlog::info("gRPC server listening on {}", config.listen_address);
}

void Server::wait() {
    if (impl_->grpc_server) {
        impl_->grpc_server->Wait();
    }
}

void Server::shutdown() {
    if (impl_->grpc_server) {
        spdlog::info("Shutting down gRPC server...");
        impl_->grpc_server->Shutdown();
        impl_->grpc_server.reset();
        spdlog::info("gRPC server shut down");
    }
}

void Server::shutdown(std::chrono::system_clock::time_point deadline) {
    if (impl_->grpc_server) {
        spdlog::info("Shutting down gRPC server with deadline...");
        impl_->grpc_server->Shutdown(deadline);
        impl_->grpc_server.reset();
        spdlog::info("gRPC server shut down");
    }
}

}  // namespace mtg::server
