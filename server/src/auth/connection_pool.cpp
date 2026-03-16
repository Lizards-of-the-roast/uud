#include "auth/connection_pool.hpp"

#include <algorithm>

#include <spdlog/spdlog.h>

namespace mtg::auth {

ConnectionPool::ConnectionPool(const std::string& connection_string, int pool_size)
    : connection_string_{connection_string} {
    pool_.reserve(static_cast<size_t>(pool_size));
    for (int i = 0; i < pool_size; ++i) {
        pool_.push_back(PoolEntry{
            .conn = std::make_unique<pqxx::connection>(connection_string),
            .in_use = false,
        });
    }
}

auto ConnectionPool::acquire() -> pqxx::connection& {
    constexpr auto acquire_timeout = std::chrono::seconds(10);
    std::unique_lock lock{pool_mutex_};
    if (!pool_cv_.wait_for(lock, acquire_timeout, [this] {
            return std::ranges::any_of(pool_, [](const PoolEntry& e) { return !e.in_use; });
        })) {
        throw std::runtime_error("ConnectionPool: acquire timeout all connections busy");
    }
    for (auto& entry : pool_) {
        if (!entry.in_use) {
            entry.in_use = true;
            if (!entry.conn->is_open()) {
                spdlog::warn("ConnectionPool: reconnecting stale DB connection");
                entry.conn = std::make_unique<pqxx::connection>(connection_string_);
            }
            return *entry.conn;
        }
    }
    throw std::runtime_error("ConnectionPool: no available connections");
}

void ConnectionPool::release(pqxx::connection& conn) {
    const std::lock_guard lock{pool_mutex_};
    for (auto& entry : pool_) {
        if (entry.conn.get() == &conn) {
            entry.in_use = false;
            pool_cv_.notify_one();
            return;
        }
    }
}

}  // namespace mtg::auth
