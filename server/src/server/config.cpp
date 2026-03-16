#include "server/config.hpp"

#include <cstdlib>
#include <stdexcept>

#include <spdlog/spdlog.h>
#include <yaml-cpp/yaml.h>

namespace mtg::server {

namespace {

std::optional<std::string> env(const char* name) {
    const char* val = std::getenv(name);
    if (val != nullptr) {
        return std::string(val);
    }
    return std::nullopt;
}

template <typename T>
T yaml_get(const YAML::Node& node, const std::string& key, const T& default_val) {
    if (node[key]) {
        return node[key].as<T>();
    }
    return default_val;
}

int env_stoi(const std::string& value, int default_val, const char* var_name) {
    try {
        return std::stoi(value);
    } catch (const std::exception& e) {
        spdlog::warn("Invalid integer for {}: '{}' ({}), using default {}", var_name, value,
                     e.what(), default_val);
        return default_val;
    }
}

}  // namespace

Config load_config(const std::filesystem::path& path) {
    Config cfg;

    if (std::filesystem::exists(path)) {
        spdlog::info("Loading config from {}", path.string());
        YAML::Node root = YAML::LoadFile(path.string());

        auto server = root["server"];
        if (server) {
            cfg.listen_address =
                yaml_get<std::string>(server, "listen_address", cfg.listen_address);
            cfg.max_games = yaml_get<int>(server, "max_games", cfg.max_games);
            cfg.action_timeout_seconds =
                yaml_get<int>(server, "action_timeout_seconds", cfg.action_timeout_seconds);
            cfg.max_concurrent_streams =
                yaml_get<int>(server, "max_concurrent_streams", cfg.max_concurrent_streams);
            cfg.keepalive_time_ms =
                yaml_get<int>(server, "keepalive_time_ms", cfg.keepalive_time_ms);
            cfg.keepalive_timeout_ms =
                yaml_get<int>(server, "keepalive_timeout_ms", cfg.keepalive_timeout_ms);
        }

        auto database = root["database"];
        if (database) {
            cfg.postgres_url = yaml_get<std::string>(database, "postgres_url", cfg.postgres_url);
            cfg.redis_url = yaml_get<std::string>(database, "redis_url", cfg.redis_url);
        }

        auto auth = root["auth"];
        if (auth) {
            cfg.jwt_secret = yaml_get<std::string>(auth, "jwt_secret", cfg.jwt_secret);
            cfg.access_token_ttl_seconds =
                yaml_get<int>(auth, "access_token_ttl_seconds", cfg.access_token_ttl_seconds);
            cfg.refresh_token_ttl_seconds =
                yaml_get<int>(auth, "refresh_token_ttl_seconds", cfg.refresh_token_ttl_seconds);
        }

        auto rate_limit = root["rate_limit"];
        if (rate_limit) {
            cfg.rate_limit_rps = yaml_get<int>(rate_limit, "rps", cfg.rate_limit_rps);
        }

        auto game = root["game"];
        if (game) {
            cfg.game_clock_seconds = yaml_get<int>(game, "clock_seconds", cfg.game_clock_seconds);
        }

        auto cleanup = root["cleanup"];
        if (cleanup) {
            cfg.disconnect_timeout_seconds = yaml_get<int>(cleanup, "disconnect_timeout_seconds",
                                                           cfg.disconnect_timeout_seconds);
            cfg.reap_interval_seconds =
                yaml_get<int>(cleanup, "reap_interval_seconds", cfg.reap_interval_seconds);
        }

        auto matchmaking = root["matchmaking"];
        if (matchmaking) {
            cfg.matchmaking_poll_interval_ms =
                yaml_get<int>(matchmaking, "poll_interval_ms", cfg.matchmaking_poll_interval_ms);
            cfg.max_queue_wait_seconds =
                yaml_get<int>(matchmaking, "max_queue_wait_seconds", cfg.max_queue_wait_seconds);
        }

        auto logging = root["logging"];
        if (logging) {
            cfg.log_level = yaml_get<std::string>(logging, "level", cfg.log_level);
            cfg.log_json = yaml_get<bool>(logging, "json", cfg.log_json);
        }

        auto metrics = root["metrics"];
        if (metrics) {
            cfg.metrics_port = yaml_get<int>(metrics, "port", cfg.metrics_port);
        }

        auto cards = root["cards"];
        if (cards) {
            cfg.card_data_path = yaml_get<std::string>(cards, "data_path", cfg.card_data_path);
            cfg.lua_scripts_path =
                yaml_get<std::string>(cards, "lua_scripts_path", cfg.lua_scripts_path);
        }
    } else {
        spdlog::warn("Config file {} not found, using defaults", path.string());
    }

    if (auto v = env("MTG_LISTEN_ADDRESS")) {
        cfg.listen_address = *v;
    }
    if (auto v = env("MTG_MAX_GAMES")) {
        cfg.max_games = env_stoi(*v, cfg.max_games, "MTG_MAX_GAMES");
    }
    if (auto v = env("MTG_ACTION_TIMEOUT_SECONDS")) {
        cfg.action_timeout_seconds =
            env_stoi(*v, cfg.action_timeout_seconds, "MTG_ACTION_TIMEOUT_SECONDS");
    }
    if (auto v = env("MTG_POSTGRES_URL")) {
        cfg.postgres_url = *v;
    }
    if (auto v = env("MTG_REDIS_URL")) {
        cfg.redis_url = *v;
    }
    if (auto v = env("MTG_JWT_SECRET")) {
        cfg.jwt_secret = *v;
    }
    if (auto v = env("MTG_LOG_LEVEL")) {
        cfg.log_level = *v;
    }
    if (auto v = env("MTG_LOG_JSON")) {
        cfg.log_json = (*v == "true" || *v == "1");
    }
    if (auto v = env("MTG_METRICS_PORT")) {
        cfg.metrics_port = env_stoi(*v, cfg.metrics_port, "MTG_METRICS_PORT");
    }
    if (auto v = env("MTG_RATE_LIMIT_RPS")) {
        cfg.rate_limit_rps = env_stoi(*v, cfg.rate_limit_rps, "MTG_RATE_LIMIT_RPS");
    }
    if (auto v = env("MTG_GAME_CLOCK_SECONDS")) {
        cfg.game_clock_seconds = env_stoi(*v, cfg.game_clock_seconds, "MTG_GAME_CLOCK_SECONDS");
    }
    if (auto v = env("MTG_DISCONNECT_TIMEOUT_SECONDS")) {
        cfg.disconnect_timeout_seconds =
            env_stoi(*v, cfg.disconnect_timeout_seconds, "MTG_DISCONNECT_TIMEOUT_SECONDS");
    }
    if (auto v = env("MTG_REAP_INTERVAL_SECONDS")) {
        cfg.reap_interval_seconds =
            env_stoi(*v, cfg.reap_interval_seconds, "MTG_REAP_INTERVAL_SECONDS");
    }
    if (auto v = env("MTG_CARD_DATA_PATH")) {
        cfg.card_data_path = *v;
    }
    if (auto v = env("MTG_LUA_SCRIPTS_PATH")) {
        cfg.lua_scripts_path = *v;
    }
    if (auto v = env("MTG_KEEPALIVE_TIME_MS")) {
        cfg.keepalive_time_ms = env_stoi(*v, cfg.keepalive_time_ms, "MTG_KEEPALIVE_TIME_MS");
    }
    if (auto v = env("MTG_KEEPALIVE_TIMEOUT_MS")) {
        cfg.keepalive_timeout_ms =
            env_stoi(*v, cfg.keepalive_timeout_ms, "MTG_KEEPALIVE_TIMEOUT_MS");
    }
    if (auto v = env("MTG_TLS_CERT_PATH")) {
        cfg.tls_cert_path = *v;
    }
    if (auto v = env("MTG_TLS_KEY_PATH")) {
        cfg.tls_key_path = *v;
    }

    if (cfg.rate_limit_rps <= 0) {
        spdlog::warn("rate_limit_rps must be positive, clamping to 1");
        cfg.rate_limit_rps = 1;
    }
    if (cfg.action_timeout_seconds <= 0) {
        spdlog::warn("action_timeout_seconds must be positive, clamping to 1");
        cfg.action_timeout_seconds = 1;
    }
    if (cfg.reap_interval_seconds <= 0) {
        spdlog::warn("reap_interval_seconds must be positive, clamping to 1");
        cfg.reap_interval_seconds = 1;
    }
    if (cfg.disconnect_timeout_seconds <= 0) {
        spdlog::warn("disconnect_timeout_seconds must be positive, clamping to 1");
        cfg.disconnect_timeout_seconds = 1;
    }
    if (cfg.metrics_port < 1 || cfg.metrics_port > 65535) {
        spdlog::warn("metrics_port {} out of range [1, 65535], resetting to 9090",
                     cfg.metrics_port);
        cfg.metrics_port = 9090;
    }
    if (cfg.max_games <= 0) {
        spdlog::warn("max_games must be positive, clamping to 1");
        cfg.max_games = 1;
    }
    if (cfg.max_concurrent_streams <= 0) {
        spdlog::warn("max_concurrent_streams must be positive, clamping to 1");
        cfg.max_concurrent_streams = 1;
    }
    if (cfg.keepalive_time_ms < 1000) {
        spdlog::warn("keepalive_time_ms too low, clamping to 1000");
        cfg.keepalive_time_ms = 1000;
    }
    if (cfg.keepalive_timeout_ms < 500) {
        spdlog::warn("keepalive_timeout_ms too low, clamping to 500");
        cfg.keepalive_timeout_ms = 500;
    }
    if (cfg.matchmaking_poll_interval_ms <= 0) {
        spdlog::warn("matchmaking_poll_interval_ms must be positive, clamping to 100");
        cfg.matchmaking_poll_interval_ms = 100;
    }

    return cfg;
}

}  // namespace mtg::server
