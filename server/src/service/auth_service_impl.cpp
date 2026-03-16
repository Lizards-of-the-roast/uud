#include "service/auth_service_impl.hpp"

#include <cctype>

#include <spdlog/spdlog.h>

namespace mtg::service {

AuthServiceImpl::AuthServiceImpl(mtg::auth::JwtManager& jwt_manager,
                                 mtg::auth::PasswordHasher& password_hasher,
                                 mtg::auth::UserStore& user_store,
                                 mtg::auth::SessionStore* session_store)
    : jwt_manager_{jwt_manager},
      password_hasher_{password_hasher},
      user_store_{user_store},
      session_store_{session_store} {}

grpc::Status AuthServiceImpl::Login([[maybe_unused]] grpc::ServerContext* context,
                                    const proto::LoginRequest* request,
                                    proto::LoginResponse* response) {
    if (request->username().size() > 64) {
        return {grpc::StatusCode::INVALID_ARGUMENT, "Username too long"};
    }
    if (request->password().size() > 128) {
        return {grpc::StatusCode::INVALID_ARGUMENT, "Password too long"};
    }

    auto user = user_store_.find_by_username(request->username());
    if (!user) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Invalid username or password"};
    }

    if (!password_hasher_.verify(request->password(), user->password_hash)) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Invalid username or password"};
    }

    auto access_token = jwt_manager_.create_access_token(user->id, user->username);
    if (session_store_ != nullptr) {
        session_store_->store_session(user->id, access_token, jwt_manager_.access_ttl());
    }

    response->set_access_token(access_token);
    response->set_refresh_token(jwt_manager_.create_refresh_token(user->id));
    response->set_user_id(user->id);

    spdlog::info("Login: user={} ({})", user->username, user->id);
    return grpc::Status::OK;
}

grpc::Status AuthServiceImpl::Register([[maybe_unused]] grpc::ServerContext* context,
                                       const proto::RegisterRequest* request,
                                       proto::RegisterResponse* response) {
    if (request->username().empty() || request->password().empty()) {
        return {grpc::StatusCode::INVALID_ARGUMENT, "Username and password are required"};
    }

    if (request->username().size() > 64) {
        return {grpc::StatusCode::INVALID_ARGUMENT, "Username must be at most 64 characters"};
    }

    for (const char c : request->username()) {
        if (std::isalnum(static_cast<unsigned char>(c)) == 0 && c != '_') {
            return {grpc::StatusCode::INVALID_ARGUMENT,
                    "Username may only contain letters, digits, and underscores"};
        }
    }

    if (request->password().size() < 8) {
        return {grpc::StatusCode::INVALID_ARGUMENT, "Password must be at least 8 characters"};
    }
    if (request->password().size() > 128) {
        return {grpc::StatusCode::INVALID_ARGUMENT, "Password must be at most 128 characters"};
    }

    if (!request->email().empty()) {
        if (request->email().size() > 256) {
            return {grpc::StatusCode::INVALID_ARGUMENT, "Email too long"};
        }
        if (request->email().find('@') == std::string::npos) {
            return {grpc::StatusCode::INVALID_ARGUMENT, "Invalid email address"};
        }
    }

    auto password_hash = password_hasher_.hash(request->password());
    auto result = user_store_.register_user(request->username(), request->email(), password_hash);

    if (!result) {
        return {grpc::StatusCode::ALREADY_EXISTS, result.error()};
    }

    response->set_user_id(*result);
    spdlog::info("Register: user={} ({})", request->username(), *result);
    return grpc::Status::OK;
}

grpc::Status AuthServiceImpl::RefreshToken([[maybe_unused]] grpc::ServerContext* context,
                                           const proto::RefreshTokenRequest* request,
                                           proto::RefreshTokenResponse* response) {
    auto payload = jwt_manager_.validate_refresh_token(request->refresh_token());
    if (!payload) {
        return {grpc::StatusCode::UNAUTHENTICATED, payload.error()};
    }

    if (session_store_ != nullptr &&
        session_store_->is_revoked(request->refresh_token())) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Refresh token has been revoked"};
    }

    auto user = user_store_.find_by_username(payload->username);
    const std::string username = user ? user->username : payload->username;

    response->set_access_token(jwt_manager_.create_access_token(payload->user_id, username));
    response->set_refresh_token(jwt_manager_.create_refresh_token(payload->user_id));

    spdlog::info("RefreshToken: user_id={}", payload->user_id);
    return grpc::Status::OK;
}

grpc::Status AuthServiceImpl::ValidateToken([[maybe_unused]] grpc::ServerContext* context,
                                            const proto::ValidateTokenRequest* request,
                                            proto::ValidateTokenResponse* response) {
    auto payload = jwt_manager_.validate_token(request->token());
    if (!payload) {
        response->set_valid(false);
        return grpc::Status::OK;
    }

    if (session_store_ != nullptr && session_store_->is_revoked(request->token())) {
        response->set_valid(false);
        return grpc::Status::OK;
    }

    response->set_valid(true);
    response->set_user_id(payload->user_id);
    response->set_username(payload->username);
    return grpc::Status::OK;
}

grpc::Status AuthServiceImpl::Logout(grpc::ServerContext* context,
                                     [[maybe_unused]] const proto::LogoutRequest* request,
                                     proto::LogoutResponse* response) {
    auto metadata = context->client_metadata();
    auto it = metadata.find("x-auth-token");
    if (it == metadata.end()) {
        return {grpc::StatusCode::UNAUTHENTICATED, "No token found"};
    }
    const std::string token(it->second.data(), it->second.size());

    if (session_store_ != nullptr) {
        if (!session_store_->revoke_session(token, jwt_manager_.access_ttl())) {
            spdlog::warn("Logout: failed to revoke token in session store");
            response->set_success(false);
            return grpc::Status::OK;
        }
    }

    response->set_success(true);
    spdlog::info("Logout: token revoked");
    return grpc::Status::OK;
}

}  // namespace mtg::service
