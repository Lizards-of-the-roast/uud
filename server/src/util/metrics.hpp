#pragma once

#include <cstdint>
#include <memory>
#include <string>

namespace mtg::util {

class MetricsRegistry {
public:
    explicit MetricsRegistry(int port);
    ~MetricsRegistry();

    MetricsRegistry(const MetricsRegistry&) = delete;
    MetricsRegistry& operator=(const MetricsRegistry&) = delete;
    MetricsRegistry(MetricsRegistry&&) = delete;
    MetricsRegistry& operator=(MetricsRegistry&&) = delete;

    void increment_games_created();
    void increment_games_finished();

    void set_active_games(int64_t count);
    void set_connected_players(int64_t count);
    void set_queue_size(int64_t count);

    void record_rpc(const std::string& method, double duration_ms, int status_code);
    void record_db_query(const std::string& operation, double duration_ms);
    void increment_auth_failures(const std::string& reason);
    void increment_rate_limit_hits();

    [[nodiscard]] auto games_created() const -> int64_t;
    [[nodiscard]] auto games_finished() const -> int64_t;
    [[nodiscard]] auto active_games() const -> int64_t;
    [[nodiscard]] auto connected_players() const -> int64_t;
    [[nodiscard]] auto queue_size() const -> int64_t;

private:
    struct Impl;
    std::unique_ptr<Impl> impl_;
};

}  // namespace mtg::util
