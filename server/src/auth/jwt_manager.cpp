#include "auth/jwt_manager.hpp"

#include <chrono>

#include <jwt-cpp/jwt.h>
#include <spdlog/spdlog.h>

namespace mtg::auth {

JwtManager::JwtManager(std::string secret, int access_ttl, int refresh_ttl)
    : secret_{std::move(secret)}, access_ttl_{access_ttl}, refresh_ttl_{refresh_ttl} {}

std::string JwtManager::create_access_token(uint64_t user_id, const std::string& username) {
    auto now = std::chrono::system_clock::now();
    return jwt::create()
        .set_issuer("mtg-server")
        .set_subject(std::to_string(user_id))
        .set_type("access")
        .set_payload_claim("username", jwt::claim(username))
        .set_issued_at(now)
        .set_expires_at(now + std::chrono::seconds(access_ttl_))
        .sign(jwt::algorithm::hs256{secret_});
}

std::string JwtManager::create_refresh_token(uint64_t user_id) {
    auto now = std::chrono::system_clock::now();
    return jwt::create()
        .set_issuer("mtg-server")
        .set_subject(std::to_string(user_id))
        .set_type("refresh")
        .set_issued_at(now)
        .set_expires_at(now + std::chrono::seconds(refresh_ttl_))
        .sign(jwt::algorithm::hs256{secret_});
}

auto JwtManager::validate_token(const std::string& token) const
    -> std::expected<TokenPayload, std::string> {
    try {
        auto decoded = jwt::decode(token);

        auto verifier =
            jwt::verify().allow_algorithm(jwt::algorithm::hs256{secret_}).with_issuer("mtg-server");

        verifier.verify(decoded);

        TokenPayload payload;
        payload.user_id = std::stoull(decoded.get_subject());
        if (decoded.has_payload_claim("username")) {
            payload.username = decoded.get_payload_claim("username").as_string();
        }

        return payload;
    } catch (const std::exception& e) {
        spdlog::debug("JWT validation failed: {}", e.what());
        return std::unexpected(std::string("Invalid token: ") + e.what());
    }
}

auto JwtManager::validate_access_token(const std::string& token) const
    -> std::expected<TokenPayload, std::string> {
    auto result = validate_token(token);
    if (!result)
        return result;

    try {
        auto decoded = jwt::decode(token);
        if (!decoded.has_header_claim("typ")) {
            return std::unexpected(std::string("Missing token type header"));
        }
        auto typ = decoded.get_header_claim("typ").as_string();
        if (typ != "access") {
            return std::unexpected(std::string("Expected access token, got ") + typ);
        }
    } catch (const std::exception& e) {
        return std::unexpected(std::string("Token type check failed: ") + e.what());
    }

    return result;
}

auto JwtManager::validate_refresh_token(const std::string& token) const
    -> std::expected<TokenPayload, std::string> {
    auto result = validate_token(token);
    if (!result)
        return result;

    try {
        auto decoded = jwt::decode(token);
        if (!decoded.has_header_claim("typ")) {
            return std::unexpected(std::string("Missing token type header"));
        }
        auto typ = decoded.get_header_claim("typ").as_string();
        if (typ != "refresh") {
            return std::unexpected(std::string("Expected refresh token, got ") + typ);
        }
    } catch (const std::exception& e) {
        return std::unexpected(std::string("Token type check failed: ") + e.what());
    }

    return result;
}

}  // namespace mtg::auth
