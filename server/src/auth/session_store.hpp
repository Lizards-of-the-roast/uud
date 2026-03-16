#pragma once

#include <cstdint>
#include <memory>
#include <string>

#include <sw/redis++/redis++.h>

namespace mtg::auth {

class SessionStore {
public:
    explicit SessionStore(const std::string& redis_url);

    void store_session(uint64_t user_id, const std::string& token, int ttl_seconds);
    [[nodiscard]] auto is_revoked(const std::string& token) -> bool;
    [[nodiscard]] auto revoke_session(const std::string& token, int ttl_seconds) -> bool;
    [[nodiscard]] auto is_available() -> bool;

private:
    sw::redis::Redis redis_;
};

}  // namespace mtg::auth
