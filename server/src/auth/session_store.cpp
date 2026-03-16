#include "auth/session_store.hpp"

#include <spdlog/spdlog.h>

namespace mtg::auth {

SessionStore::SessionStore(const std::string& redis_url) : redis_{redis_url} {
    auto safe_url = redis_url;
    auto at_pos = safe_url.find('@');
    if (at_pos != std::string::npos) {
        auto scheme_end = safe_url.find("://");
        if (scheme_end != std::string::npos) {
            safe_url = safe_url.substr(0, scheme_end + 3) + "***" + safe_url.substr(at_pos);
        }
    }
    spdlog::info("SessionStore: connected to {}", safe_url);
}

void SessionStore::store_session(uint64_t user_id, const std::string& token, int ttl_seconds) {
    try {
        auto key = "session:" + token;
        redis_.setex(key, std::chrono::seconds(ttl_seconds), std::to_string(user_id));
    } catch (const std::exception& e) {
        spdlog::warn("SessionStore: store_session failed: {}", e.what());
    }
}

auto SessionStore::is_revoked(const std::string& token) -> bool {
    try {
        auto key = "revoked:" + token;
        return redis_.exists(key) > 0;
    } catch (const std::exception& e) {
        spdlog::error("SessionStore: is_revoked failed (denying request, Redis unavailable): {}",
                      e.what());
        return true;
    }
}

auto SessionStore::revoke_session(const std::string& token, int ttl_seconds) -> bool {
    try {
        auto key = "revoked:" + token;
        redis_.setex(key, std::chrono::seconds(ttl_seconds), "1");
        redis_.del("session:" + token);
        return true;
    } catch (const std::exception& e) {
        spdlog::error("SessionStore: revoke_session failed: {}", e.what());
        return false;
    }
}

auto SessionStore::is_available() -> bool {
    try {
        redis_.ping();
        return true;
    } catch (...) {
        return false;
    }
}

}  // namespace mtg::auth
