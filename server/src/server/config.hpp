#pragma once

#include <cstdint>
#include <filesystem>
#include <string>

namespace mtg::server {

struct Config {
    std::string listen_address = "0.0.0.0:50051";
    int max_games = 100;
    int action_timeout_seconds = 60;
    int max_concurrent_streams = 200;
    int keepalive_time_ms = 30000;
    int keepalive_timeout_ms = 5000;

    std::string postgres_url = "postgresql://localhost:5432/mtg";

    std::string redis_url = "redis://localhost:6379";

    std::string jwt_secret;

    std::string tls_cert_path;
    std::string tls_key_path;
    int access_token_ttl_seconds = 3600;
    int refresh_token_ttl_seconds = 86400 * 30;

    int matchmaking_poll_interval_ms = 500;
    int max_queue_wait_seconds = 300;

    std::string log_level = "info";
    bool log_json = false;

    int rate_limit_rps = 100;

    int game_clock_seconds = 0;

    int disconnect_timeout_seconds = 120;
    int reap_interval_seconds = 10;

    int metrics_port = 9090;

    std::string card_data_path = "./cards";
    std::string lua_scripts_path = "./cards/lua";
};

Config load_config(const std::filesystem::path& path);

}  // namespace mtg::server
