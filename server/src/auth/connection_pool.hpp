#pragma once

#include <chrono>
#include <condition_variable>
#include <memory>
#include <mutex>
#include <string>
#include <vector>

#include <pqxx/pqxx>

namespace mtg::auth {

class ConnectionPool {
public:
    explicit ConnectionPool(const std::string& connection_string, int pool_size = 2);

    ConnectionPool(const ConnectionPool&) = delete;
    ConnectionPool& operator=(const ConnectionPool&) = delete;
    ConnectionPool(ConnectionPool&&) = delete;
    ConnectionPool& operator=(ConnectionPool&&) = delete;

    [[nodiscard]] auto acquire() -> pqxx::connection&;
    void release(pqxx::connection& conn);

private:
    struct PoolEntry {
        std::unique_ptr<pqxx::connection> conn;
        bool in_use{false};
    };

    std::string connection_string_;
    std::mutex pool_mutex_;
    std::condition_variable pool_cv_;
    std::vector<PoolEntry> pool_;
};

}  // namespace mtg::auth
