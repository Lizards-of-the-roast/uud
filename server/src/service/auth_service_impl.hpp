#pragma once

#include "auth/jwt_manager.hpp"
#include "auth/password_hasher.hpp"
#include "auth/session_store.hpp"
#include "auth/user_store.hpp"
#include "mtg/auth_service.grpc.pb.h"
#include <grpcpp/grpcpp.h>

namespace mtg::service {

class AuthServiceImpl final : public proto::AuthService::Service {
public:
    AuthServiceImpl(mtg::auth::JwtManager& jwt_manager, mtg::auth::PasswordHasher& password_hasher,
                    mtg::auth::UserStore& user_store, mtg::auth::SessionStore* session_store);

    grpc::Status Login(grpc::ServerContext* context, const proto::LoginRequest* request,
                       proto::LoginResponse* response) override;

    grpc::Status Register(grpc::ServerContext* context, const proto::RegisterRequest* request,
                          proto::RegisterResponse* response) override;

    grpc::Status RefreshToken(grpc::ServerContext* context,
                              const proto::RefreshTokenRequest* request,
                              proto::RefreshTokenResponse* response) override;

    grpc::Status ValidateToken(grpc::ServerContext* context,
                               const proto::ValidateTokenRequest* request,
                               proto::ValidateTokenResponse* response) override;

    grpc::Status Logout(grpc::ServerContext* context, const proto::LogoutRequest* request,
                        proto::LogoutResponse* response) override;

private:
    mtg::auth::JwtManager& jwt_manager_;
    mtg::auth::PasswordHasher& password_hasher_;
    mtg::auth::UserStore& user_store_;
    mtg::auth::SessionStore* session_store_;
};

}  // namespace mtg::service
