#include <atomic>
#include <chrono>
#include <csignal>
#include <cstdlib>
#include <filesystem>
#include <memory>
#include <thread>
#include <vector>

#include "auth/game_history_store.hpp"
#include "auth/jwt_manager.hpp"
#include "auth/migration_runner.hpp"
#include "auth/password_hasher.hpp"
#include "auth/rating_store.hpp"
#include "auth/session_store.hpp"
#include "auth/user_store.hpp"
#include "engine/card_registry.hpp"
#include "engine/game_manager.hpp"
#include "engine/preset_deck.hpp"
#include "matchmaking/matchmaker.hpp"
#include "matchmaking/queue.hpp"
#include "server/config.hpp"
#include "server/interceptors/auth_interceptor.hpp"
#include "server/interceptors/logging_interceptor.hpp"
#include "server/interceptors/rate_limit_interceptor.hpp"
#include "server/server.hpp"
#include "service/auth_service_impl.hpp"
#include "service/game_service_impl.hpp"
#include "service/matchmaking_service_impl.hpp"
#include "util/logging.hpp"
#include "util/metrics.hpp"
#include <grpcpp/ext/proto_server_reflection_plugin.h>
#include <grpcpp/grpcpp.h>
#include <pqxx/pqxx>
#include <spdlog/spdlog.h>

namespace {

std::atomic<bool> g_shutdown_requested{false};

void signal_handler([[maybe_unused]] int sig) {
    g_shutdown_requested = true;
}

void on_game_finish(mtg::auth::RatingStore& rating_store,
                    mtg::auth::GameHistoryStore& history_store,
                    const mtg::engine::GameFinishData& data) {
    if (data.winner_id != 0 && data.player_ids.size() == 2) {
        const uint64_t loser_id =
            (data.player_ids[0] == data.winner_id) ? data.player_ids[1] : data.player_ids[0];
        rating_store.update_after_game(data.winner_id, loser_id);
    }

    std::vector<mtg::auth::GameHistoryPlayerRecord> players;
    players.reserve(data.player_ids.size());
    for (size_t i = 0; i < data.player_ids.size(); ++i) {
        players.push_back({
            .player_id = data.player_ids[i],
            .deck_list =
                i < data.deck_lists.size() ? data.deck_lists[i] : std::vector<std::string>{},
            .final_life = i < data.final_life_totals.size() ? data.final_life_totals[i] : 0,
        });
    }

    history_store.record_game({
        .game_id = data.game_id,
        .winner_id = data.winner_id,
        .duration_seconds = data.duration_seconds,
        .started_at = data.started_at,
        .players = std::move(players),
    });

    spdlog::info("Game {} finished, winner={}", data.game_id, data.winner_id);
}

auto validate_config(const mtg::server::Config& config) -> std::expected<void, std::string> {
    if (config.jwt_secret.empty()) {
        return std::unexpected("JWT secret is not configured. Set MTG_JWT_SECRET.");
    }
    if (config.jwt_secret.size() < 32) {
        return std::unexpected("JWT secret is insecure. Set MTG_JWT_SECRET to at least 32 chars.");
    }
    if (config.postgres_url.empty()) {
        return std::unexpected("Postgres URL is not configured. Set MTG_POSTGRES_URL.");
    }
    if (config.redis_url.empty()) {
        return std::unexpected("Redis URL is not configured. Set MTG_REDIS_URL.");
    }
    if (config.access_token_ttl_seconds < 60) {
        return std::unexpected("access_token_ttl_seconds must be >= 60.");
    }
    if (config.refresh_token_ttl_seconds < config.access_token_ttl_seconds) {
        return std::unexpected("refresh_token_ttl_seconds must be >= access_token_ttl_seconds.");
    }
    if ((!config.tls_cert_path.empty()) != (!config.tls_key_path.empty())) {
        return std::unexpected("Both tls_cert_path and tls_key_path must be set, or neither.");
    }
    return {};
}

void install_signal_handlers() {
    struct sigaction sa{};
    sa.sa_handler = signal_handler;
    sa.sa_flags = 0;
    sigemptyset(&sa.sa_mask);
    sigaction(SIGINT, &sa, nullptr);
    sigaction(SIGTERM, &sa, nullptr);
}

auto create_interceptors(mtg::util::MetricsRegistry& metrics, mtg::auth::JwtManager& jwt_manager,
                         mtg::auth::SessionStore* session_store, int rate_limit_rps) {
    std::vector<std::unique_ptr<grpc::experimental::ServerInterceptorFactoryInterface>> v;
    v.push_back(std::make_unique<mtg::server::LoggingInterceptorFactory>(&metrics));
    v.push_back(std::make_unique<mtg::server::AuthInterceptorFactory>(jwt_manager, session_store));
    v.push_back(std::make_unique<mtg::server::RateLimitInterceptorFactory>(rate_limit_rps));
    return v;
}

}  // namespace

int main(int /*argc*/, char* argv[]) {
    // Set working directory to executable location so relative paths resolve correctly
    if (auto exe_dir = std::filesystem::path(argv[0]).parent_path(); !exe_dir.empty()) {
        std::filesystem::current_path(exe_dir);
    }

    auto config = mtg::server::load_config("config/server.yaml");
    mtg::util::init_logging(config.log_level, config.log_json);
    spdlog::info("starting...");

    if (auto result = validate_config(config); !result) {
        spdlog::critical("{}", result.error());
        return EXIT_FAILURE;
    }

    mtg::engine::CardRegistry registry;
    auto cards_loaded = registry.load_cards_from_directory(config.card_data_path);
    spdlog::info("Loaded {} card definitions from {}", cards_loaded, config.card_data_path);

    mtg::util::MetricsRegistry metrics{config.metrics_port};

    mtg::engine::PresetDeckLoader preset_decks;
    spdlog::info("Loaded {} preset decks", preset_decks.load_from_directory("config/decks"));

    mtg::engine::GameManager game_manager{registry, &metrics, config.game_clock_seconds,
                                          config.max_games};

    mtg::auth::JwtManager jwt_manager{config.jwt_secret, config.access_token_ttl_seconds,
                                      config.refresh_token_ttl_seconds};
    mtg::auth::PasswordHasher password_hasher;

    {
        pqxx::connection migration_conn{config.postgres_url};
        mtg::auth::MigrationRunner runner{migration_conn};
        [[maybe_unused]] auto migrations_applied = runner.run_pending();
    }

    mtg::auth::UserStore user_store{config.postgres_url};
    mtg::auth::RatingStore rating_store{config.postgres_url};
    mtg::auth::GameHistoryStore history_store{config.postgres_url};

    auto redis_store = std::make_unique<mtg::auth::SessionStore>(config.redis_url);
    if (!redis_store->is_available()) {
        spdlog::critical("Cannot connect to Redis at {}", config.redis_url);
        return EXIT_FAILURE;
    }
    spdlog::info("Connected to Redis at {}", config.redis_url);

    mtg::matchmaking::MatchmakingQueue mm_queue;
    mtg::matchmaking::Matchmaker matchmaker{mm_queue, game_manager};

    mtg::service::GameServiceImpl game_service{game_manager, preset_decks, &metrics,
                                               &history_store, &rating_store};
    mtg::service::AuthServiceImpl auth_service{jwt_manager, password_hasher, user_store,
                                               redis_store.get()};
    mtg::service::MatchmakingServiceImpl matchmaking_service{mm_queue, matchmaker, &metrics,
                                                             &rating_store,
                                                             config.max_queue_wait_seconds};

    grpc::EnableDefaultHealthCheckService(true);
    grpc::reflection::InitProtoReflectionServerBuilderPlugin();

    mtg::server::Server server;
    server.register_service(&game_service);
    server.register_service(&auth_service);
    server.register_service(&matchmaking_service);
    server.set_interceptors(
        create_interceptors(metrics, jwt_manager, redis_store.get(), config.rate_limit_rps));
    game_manager.set_on_game_finish([&rating_store, &history_store](const auto& data) {
        on_game_finish(rating_store, history_store, data);
    });

    server.start(config);

    game_manager.start_reaper(config.disconnect_timeout_seconds, config.reap_interval_seconds);
    matchmaker.start(config.matchmaking_poll_interval_ms);

    install_signal_handlers();
    spdlog::info("Server ready. Press Ctrl+C to stop.");

    while (!g_shutdown_requested.load()) {
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }

    spdlog::info("Shutdown signal received");
    matchmaker.stop();
    game_manager.stop_reaper();
    game_manager.stop_all_games();
    server.shutdown();
    spdlog::info("Server stopped.");
}
