#pragma once

#include <memory>
#include <string>

#include "auth/jwt_manager.hpp"
#include "auth/session_store.hpp"
#include <grpcpp/grpcpp.h>
#include <grpcpp/support/server_interceptor.h>

namespace mtg::server {

class AuthInterceptor final : public grpc::experimental::Interceptor {
public:
    AuthInterceptor(grpc::experimental::ServerRpcInfo* info,
                    const mtg::auth::JwtManager& jwt_manager,
                    mtg::auth::SessionStore* session_store);

    void Intercept(grpc::experimental::InterceptorBatchMethods* methods) override;

private:
    grpc::experimental::ServerRpcInfo* info_;
    const mtg::auth::JwtManager& jwt_manager_;
    mtg::auth::SessionStore* session_store_;
    bool auth_failed_{false};
    std::string auth_error_message_;

    std::string user_id_str_;
    std::string username_str_;
    std::string token_str_;
};

class AuthInterceptorFactory : public grpc::experimental::ServerInterceptorFactoryInterface {
public:
    AuthInterceptorFactory(const mtg::auth::JwtManager& jwt_manager,
                           mtg::auth::SessionStore* session_store);

    grpc::experimental::Interceptor* CreateServerInterceptor(
        grpc::experimental::ServerRpcInfo* info) override;

private:
    const mtg::auth::JwtManager& jwt_manager_;
    mtg::auth::SessionStore* session_store_;
};

}  // namespace mtg::server
