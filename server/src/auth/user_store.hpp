#pragma once

#include <cstdint>
#include <expected>
#include <optional>
#include <string>

#include "auth/connection_pool.hpp"

namespace mtg::auth {

struct UserRecord {
    uint64_t id;
    std::string username;
    std::string email;
    std::string password_hash;
};

class UserStore {
public:
    explicit UserStore(const std::string& connection_string, int pool_size = 4);

    [[nodiscard]] auto register_user(const std::string& username, const std::string& email,
                                     const std::string& password_hash)
        -> std::expected<uint64_t, std::string>;

    [[nodiscard]] auto find_by_username(const std::string& username) -> std::optional<UserRecord>;

private:
    ConnectionPool pool_;
};

}  // namespace mtg::auth
