#include "auth/rating_store.hpp"

#include <cmath>

#include "auth/scoped_connection.hpp"
#include <spdlog/spdlog.h>

namespace mtg::auth {

RatingStore::RatingStore(const std::string& connection_string, int pool_size)
    : pool_{connection_string, pool_size} {
    spdlog::info("RatingStore: initialized with {} connections", pool_size);
}

auto RatingStore::get_rating(uint64_t user_id) -> RatingRecord {
    ScopedConnection sc{pool_.acquire(), [this](pqxx::connection& c) { pool_.release(c); }};
    try {
        pqxx::work txn{sc.get()};
        txn.exec_params(
            "INSERT INTO ratings (user_id, format) VALUES ($1, 'default') ON CONFLICT DO NOTHING",
            static_cast<int64_t>(user_id));
        auto result = txn.exec_params(
            "SELECT user_id, elo, wins, losses FROM ratings "
            "WHERE user_id = $1 AND format = 'default'",
            static_cast<int64_t>(user_id));
        txn.commit();

        if (result.empty()) {
            return RatingRecord{.user_id = user_id};
        }
        return RatingRecord{
            .user_id = result[0][0].as<uint64_t>(),
            .elo = result[0][1].as<int>(),
            .wins = result[0][2].as<int>(),
            .losses = result[0][3].as<int>(),
        };
    } catch (const std::exception& e) {
        spdlog::error("RatingStore: get_rating failed: {}", e.what());
        return RatingRecord{.user_id = user_id};
    }
}

void RatingStore::update_after_game(uint64_t winner_id, uint64_t loser_id) {
    ScopedConnection sc{pool_.acquire(), [this](pqxx::connection& c) { pool_.release(c); }};
    try {
        pqxx::work txn{sc.get()};

        txn.exec_params(
            "INSERT INTO ratings (user_id, format) VALUES ($1, 'default') ON CONFLICT DO NOTHING",
            static_cast<int64_t>(winner_id));
        txn.exec_params(
            "INSERT INTO ratings (user_id, format) VALUES ($1, 'default') ON CONFLICT DO NOTHING",
            static_cast<int64_t>(loser_id));

        auto w_row =
            txn.exec_params1("SELECT elo FROM ratings WHERE user_id = $1 AND format = 'default'",
                             static_cast<int64_t>(winner_id));
        auto l_row =
            txn.exec_params1("SELECT elo FROM ratings WHERE user_id = $1 AND format = 'default'",
                             static_cast<int64_t>(loser_id));

        auto elo_w = w_row[0].as<double>();
        auto elo_l = l_row[0].as<double>();

        constexpr double k_factor = 32.0;
        const double expected_w = 1.0 / (1.0 + std::pow(10.0, (elo_l - elo_w) / 400.0));
        const double expected_l = 1.0 - expected_w;

        int new_elo_w =
            std::max(100, static_cast<int>(std::round(elo_w + (k_factor * (1.0 - expected_w)))));
        int new_elo_l =
            std::max(100, static_cast<int>(std::round(elo_l + (k_factor * (0.0 - expected_l)))));

        txn.exec_params(
            "UPDATE ratings SET elo = $1, wins = wins + 1 "
            "WHERE user_id = $2 AND format = 'default'",
            new_elo_w, static_cast<int64_t>(winner_id));

        txn.exec_params(
            "UPDATE ratings SET elo = $1, losses = losses + 1 "
            "WHERE user_id = $2 AND format = 'default'",
            new_elo_l, static_cast<int64_t>(loser_id));

        txn.commit();

        spdlog::info(
            "RatingStore: updated ratings for game (winner={} {} -> {}, loser={} {} -> {})",
            winner_id, static_cast<int>(elo_w), new_elo_w, loser_id, static_cast<int>(elo_l),
            new_elo_l);
    } catch (const std::exception& e) {
        spdlog::error("RatingStore: update_after_game failed: {}", e.what());
    }
}

}  // namespace mtg::auth
