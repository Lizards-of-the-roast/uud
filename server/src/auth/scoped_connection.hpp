#pragma once

#include <functional>

#include <pqxx/pqxx>

namespace mtg::auth {

class ScopedConnection {
public:
    ScopedConnection(pqxx::connection& conn, std::function<void(pqxx::connection&)> releaser)
        : conn_{conn}, releaser_{std::move(releaser)} {}

    ~ScopedConnection() noexcept {
        try {
            releaser_(conn_);
        } catch (...) {
        }  // NOLINT(bugprone-empty-catch)
    }

    ScopedConnection(const ScopedConnection&) = delete;
    ScopedConnection& operator=(const ScopedConnection&) = delete;
    ScopedConnection(ScopedConnection&&) = delete;
    ScopedConnection& operator=(ScopedConnection&&) = delete;

    [[nodiscard]] auto get() -> pqxx::connection& { return conn_; }

private:
    pqxx::connection& conn_;
    std::function<void(pqxx::connection&)> releaser_;
};

}  // namespace mtg::auth
