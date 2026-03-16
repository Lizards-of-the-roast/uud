#include "server/interceptors/auth_interceptor.hpp"

#include <string_view>

#include <spdlog/spdlog.h>

namespace mtg::server {

namespace {

auto is_auth_exempt(std::string_view method) -> bool {
    return method == "/mtg.proto.AuthService/Login" ||
           method == "/mtg.proto.AuthService/Register";
}

}  // namespace

AuthInterceptor::AuthInterceptor(grpc::experimental::ServerRpcInfo* info,
                                 const mtg::auth::JwtManager& jwt_manager,
                                 mtg::auth::SessionStore* session_store)
    : info_(info), jwt_manager_(jwt_manager), session_store_(session_store) {}

void AuthInterceptor::Intercept(grpc::experimental::InterceptorBatchMethods* methods) {
    if (methods->QueryInterceptionHookPoint(
            grpc::experimental::InterceptionHookPoints::POST_RECV_INITIAL_METADATA)) {
        std::string_view method_name = info_->method();

        if (is_auth_exempt(method_name)) {
            methods->Proceed();
            return;
        }

        auto* metadata = methods->GetRecvInitialMetadata();

        auto it = metadata->find("authorization");
        if (it == metadata->end()) {
            spdlog::debug("AuthInterceptor: missing auth header for {}", method_name);
            auth_failed_ = true;
            auth_error_message_ = "Missing authorization header";
            metadata->insert({"x-auth-failed", "1"});
            methods->Proceed();
            return;
        }

        const std::string auth_value(it->second.data(), it->second.size());
        constexpr std::string_view bearer_prefix = "Bearer ";

        if (!auth_value.starts_with(bearer_prefix)) {
            spdlog::debug("AuthInterceptor: malformed auth header for {}", method_name);
            auth_failed_ = true;
            auth_error_message_ = "Malformed authorization header";
            metadata->insert({"x-auth-failed", "1"});
            methods->Proceed();
            return;
        }

        auto token = auth_value.substr(bearer_prefix.size());
        auto payload = jwt_manager_.validate_access_token(token);

        if (payload) {
            if (session_store_ != nullptr && session_store_->is_revoked(token)) {
                spdlog::debug("AuthInterceptor: revoked token for {}", method_name);
                auth_failed_ = true;
                auth_error_message_ = "Token has been revoked";
                metadata->insert({"x-auth-failed", "1"});
                methods->Proceed();
                return;
            }

            user_id_str_ = std::to_string(payload->user_id);
            username_str_ = payload->username;
            token_str_ = std::string(token);
            metadata->insert({"x-user-id", user_id_str_});
            metadata->insert({"x-username", username_str_});
            metadata->insert({"x-auth-token", token_str_});
            spdlog::debug("AuthInterceptor: authenticated user {} for {}", payload->username,
                          method_name);
        } else {
            spdlog::debug("AuthInterceptor: invalid token for {}: {}", method_name,
                          payload.error());
            auth_failed_ = true;
            auth_error_message_ = "Invalid or expired token";
            metadata->insert({"x-auth-failed", "1"});
            methods->Proceed();
            return;
        }
    }

    if (auth_failed_ && methods->QueryInterceptionHookPoint(
                            grpc::experimental::InterceptionHookPoints::PRE_SEND_STATUS)) {
        methods->ModifySendStatus(
            grpc::Status(grpc::StatusCode::UNAUTHENTICATED, auth_error_message_));
    }

    methods->Proceed();
}

AuthInterceptorFactory::AuthInterceptorFactory(const mtg::auth::JwtManager& jwt_manager,
                                               mtg::auth::SessionStore* session_store)
    : jwt_manager_(jwt_manager), session_store_(session_store) {}

grpc::experimental::Interceptor* AuthInterceptorFactory::CreateServerInterceptor(
    grpc::experimental::ServerRpcInfo* info) {
    return new AuthInterceptor(info, jwt_manager_, session_store_);
}

}  // namespace mtg::server
