#include "util/metrics.hpp"

#include <string>

#include <prometheus/counter.h>
#include <prometheus/exposer.h>
#include <prometheus/gauge.h>
#include <prometheus/histogram.h>
#include <prometheus/registry.h>
#include <spdlog/spdlog.h>

namespace mtg::util {

struct MetricsRegistry::Impl {
    std::shared_ptr<prometheus::Registry> registry;
    std::unique_ptr<prometheus::Exposer> exposer;

    prometheus::Counter& games_created;
    prometheus::Counter& games_finished;
    prometheus::Gauge& active_games;
    prometheus::Gauge& connected_players;
    prometheus::Gauge& queue_size;
    prometheus::Family<prometheus::Counter>& rpc_requests_family;
    prometheus::Family<prometheus::Histogram>& rpc_duration_family;
    prometheus::Family<prometheus::Histogram>& db_duration_family;
    prometheus::Family<prometheus::Counter>& auth_failures_family;
    prometheus::Counter& rate_limit_hits;

    explicit Impl(int port)
        : registry{std::make_shared<prometheus::Registry>()},
          games_created{prometheus::BuildCounter()
                            .Name("mtg_games_created_total")
                            .Help("Total number of games created.")
                            .Register(*registry)
                            .Add({})},
          games_finished{prometheus::BuildCounter()
                             .Name("mtg_games_finished_total")
                             .Help("Total number of games finished.")
                             .Register(*registry)
                             .Add({})},
          active_games{prometheus::BuildGauge()
                           .Name("mtg_active_games")
                           .Help("Current number of active games.")
                           .Register(*registry)
                           .Add({})},
          connected_players{prometheus::BuildGauge()
                                .Name("mtg_connected_players")
                                .Help("Current number of connected players.")
                                .Register(*registry)
                                .Add({})},
          queue_size{prometheus::BuildGauge()
                         .Name("mtg_queue_size")
                         .Help("Current matchmaking queue size.")
                         .Register(*registry)
                         .Add({})},
          rpc_requests_family{prometheus::BuildCounter()
                                  .Name("mtg_rpc_requests_total")
                                  .Help("Total RPC requests by method and status.")
                                  .Register(*registry)},
          rpc_duration_family{prometheus::BuildHistogram()
                                  .Name("mtg_rpc_duration_seconds")
                                  .Help("RPC duration in seconds by method.")
                                  .Register(*registry)},
          db_duration_family{prometheus::BuildHistogram()
                                 .Name("mtg_db_query_duration_seconds")
                                 .Help("Database query duration in seconds by operation.")
                                 .Register(*registry)},
          auth_failures_family{prometheus::BuildCounter()
                                   .Name("mtg_auth_failures_total")
                                   .Help("Total authentication failures by reason.")
                                   .Register(*registry)},
          rate_limit_hits{prometheus::BuildCounter()
                              .Name("mtg_rate_limit_hits_total")
                              .Help("Total number of rate-limited requests.")
                              .Register(*registry)
                              .Add({})} {
        if (port > 0) {
            try {
                exposer = std::make_unique<prometheus::Exposer>("0.0.0.0:" + std::to_string(port));
                exposer->RegisterCollectable(registry);
                spdlog::info("Metrics endpoint listening on :{}/metrics", port);
            } catch (const std::exception& e) {
                spdlog::error("Metrics: failed to start exposer on port {}: {}", port, e.what());
            }
        }
    }
};

MetricsRegistry::MetricsRegistry(int port) : impl_{std::make_unique<Impl>(port)} {}

MetricsRegistry::~MetricsRegistry() = default;

void MetricsRegistry::increment_games_created() {
    impl_->games_created.Increment();
}

void MetricsRegistry::increment_games_finished() {
    impl_->games_finished.Increment();
}

void MetricsRegistry::set_active_games(int64_t count) {
    impl_->active_games.Set(static_cast<double>(count));
}

void MetricsRegistry::set_connected_players(int64_t count) {
    impl_->connected_players.Set(static_cast<double>(count));
}

void MetricsRegistry::set_queue_size(int64_t count) {
    impl_->queue_size.Set(static_cast<double>(count));
}

void MetricsRegistry::record_rpc(const std::string& method, double duration_ms, int status_code) {
    auto status_str = std::to_string(status_code);
    impl_->rpc_requests_family.Add({{"method", method}, {"status", status_str}}).Increment();
    impl_->rpc_duration_family
        .Add({{"method", method}},
             prometheus::Histogram::BucketBoundaries{1, 5, 10, 25, 50, 100, 250, 500, 1000, 5000})
        .Observe(duration_ms / 1000.0);
}

void MetricsRegistry::record_db_query(const std::string& operation, double duration_ms) {
    impl_->db_duration_family
        .Add({{"operation", operation}},
             prometheus::Histogram::BucketBoundaries{1, 5, 10, 25, 50, 100, 250, 500, 1000})
        .Observe(duration_ms / 1000.0);
}

void MetricsRegistry::increment_auth_failures(const std::string& reason) {
    impl_->auth_failures_family.Add({{"reason", reason}}).Increment();
}

void MetricsRegistry::increment_rate_limit_hits() {
    impl_->rate_limit_hits.Increment();
}

auto MetricsRegistry::games_created() const -> int64_t {
    return static_cast<int64_t>(impl_->games_created.Value());
}

auto MetricsRegistry::games_finished() const -> int64_t {
    return static_cast<int64_t>(impl_->games_finished.Value());
}

auto MetricsRegistry::active_games() const -> int64_t {
    return static_cast<int64_t>(impl_->active_games.Value());
}

auto MetricsRegistry::connected_players() const -> int64_t {
    return static_cast<int64_t>(impl_->connected_players.Value());
}

auto MetricsRegistry::queue_size() const -> int64_t {
    return static_cast<int64_t>(impl_->queue_size.Value());
}

}  // namespace mtg::util
