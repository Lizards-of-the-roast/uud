#pragma once

#include <cstdint>
#include <expected>
#include <string>

namespace mtg::auth {

struct TokenPayload {
    uint64_t user_id{0};
    std::string username;
};

class JwtManager {
public:
    explicit JwtManager(std::string secret, int access_ttl = 3600, int refresh_ttl = 86400 * 30);

    [[nodiscard]] auto create_access_token(uint64_t user_id, const std::string& username)
        -> std::string;
    [[nodiscard]] auto create_refresh_token(uint64_t user_id) -> std::string;
    [[nodiscard]] auto validate_access_token(const std::string& token) const
        -> std::expected<TokenPayload, std::string>;
    [[nodiscard]] auto validate_refresh_token(const std::string& token) const
        -> std::expected<TokenPayload, std::string>;
    [[nodiscard]] auto validate_token(const std::string& token) const
        -> std::expected<TokenPayload, std::string>;
    [[nodiscard]] auto access_ttl() const -> int { return access_ttl_; }

private:
    std::string secret_;
    int access_ttl_;
    int refresh_ttl_;
};

}  // namespace mtg::auth
