#include "auth/user_store.hpp"

#include "auth/scoped_connection.hpp"
#include <spdlog/spdlog.h>

namespace mtg::auth {

UserStore::UserStore(const std::string& connection_string, int pool_size)
    : pool_{connection_string, pool_size} {
    spdlog::info("UserStore: initialized with {} connections", pool_size);
}

auto UserStore::register_user(const std::string& username, const std::string& email,
                              const std::string& password_hash)
    -> std::expected<uint64_t, std::string> {
    ScopedConnection sc{pool_.acquire(), [this](pqxx::connection& c) { pool_.release(c); }};
    try {
        pqxx::work txn{sc.get()};
        auto result = txn.exec_params(
            "INSERT INTO users (username, email, password_hash) VALUES ($1, $2, $3) "
            "RETURNING id",
            username, email, password_hash);
        txn.commit();

        if (result.empty()) {
            return std::unexpected("Failed to insert user");
        }
        return result[0][0].as<uint64_t>();
    } catch (const pqxx::unique_violation&) {
        return std::unexpected("Username already exists");
    } catch (const std::exception& e) {
        return std::unexpected(std::string("Database error: ") + e.what());
    }
}

auto UserStore::find_by_username(const std::string& username) -> std::optional<UserRecord> {
    ScopedConnection sc{pool_.acquire(), [this](pqxx::connection& c) { pool_.release(c); }};
    try {
        pqxx::work txn{sc.get()};
        auto result = txn.exec_params(
            "SELECT id, username, email, password_hash FROM users WHERE username = $1", username);
        txn.commit();

        if (result.empty()) {
            return std::nullopt;
        }

        return UserRecord{
            .id = result[0][0].as<uint64_t>(),
            .username = result[0][1].as<std::string>(),
            .email = result[0][2].as<std::string>(),
            .password_hash = result[0][3].as<std::string>(),
        };
    } catch (const std::exception& e) {
        spdlog::error("UserStore: find_by_username failed: {}", e.what());
        return std::nullopt;
    }
}

}  // namespace mtg::auth
