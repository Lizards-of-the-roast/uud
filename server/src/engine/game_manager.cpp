#include "engine/game_manager.hpp"

#include <format>

#include "util/metrics.hpp"
#include <spdlog/spdlog.h>

namespace mtg::engine {

GameManager::GameManager(const CardRegistry& registry, mtg::util::MetricsRegistry* metrics,
                         int clock_seconds, int max_games)
    : registry_{registry},
      metrics_{metrics},
      clock_seconds_{clock_seconds},
      max_games_{max_games} {}

GameManager::~GameManager() {
    stop_reaper();
    stop_all_games();
}

void GameManager::stop_all_games() {
    std::lock_guard const lock{mutex_};
    for (auto& [id, game] : games_) {
        game->stop();
    }
    spdlog::info("Stopped {} active game(s)", games_.size());
}

auto GameManager::create_game() -> std::expected<std::string, std::string> {
    std::lock_guard const lock{mutex_};
    if (static_cast<int>(games_.size()) >= max_games_) {
        return std::unexpected("Server at capacity maximum number of games reached");
    }
    auto id = generate_game_id();
    auto game = std::make_shared<Game>(id, registry_, clock_seconds_);
    games_.emplace(id, game);
    if (metrics_ != nullptr) {
        metrics_->increment_games_created();
        metrics_->set_active_games(static_cast<int64_t>(games_.size()));
    }
    spdlog::info("Created game {}", id);
    return id;
}

std::shared_ptr<Game> GameManager::get_game(const std::string& game_id) {
    std::lock_guard const lock{mutex_};
    auto it = games_.find(game_id);
    return it != games_.end() ? it->second : nullptr;
}

void GameManager::remove_game(const std::string& game_id) {
    std::shared_ptr<Game> game;
    {
        std::lock_guard const lock{mutex_};
        auto it = games_.find(game_id);
        if (it == games_.end()) {
            return;
        }
        game = it->second;
        game->stop();
        games_.erase(it);
        if (metrics_ != nullptr) {
            metrics_->increment_games_finished();
            metrics_->set_active_games(static_cast<int64_t>(games_.size()));
        }
    }

    if (on_game_finish_ && game->result()) {
        GameFinishData data;
        data.game_id = game_id;
        data.winner_id = game->result()->winner_id;
        data.started_at = game->started_at();
        auto now = std::chrono::system_clock::now();
        data.duration_seconds = static_cast<int>(
            std::chrono::duration_cast<std::chrono::seconds>(now - data.started_at).count());
        for (const auto& p : game->players()) {
            data.player_ids.push_back(p.id());
            data.deck_lists.push_back(p.submitted_deck_names());
            data.final_life_totals.push_back(p.life());
        }
        on_game_finish_(data);
    }
    spdlog::info("Removed game {}", game_id);
}

size_t GameManager::active_game_count() const {
    std::lock_guard const lock{mutex_};
    return games_.size();
}

auto GameManager::list_games() const -> std::vector<GameListEntry> {
    std::lock_guard const lock{mutex_};
    std::vector<GameListEntry> result;
    for (const auto& [id, game] : games_) {
        result.push_back({id, game->state(), static_cast<int>(game->players().size())});
    }
    return result;
}

std::string GameManager::generate_game_id() {
    return std::format("game-{:06}", next_id_++);
}

void GameManager::start_reaper(int disconnect_timeout_seconds, int reap_interval_seconds) {
    disconnect_timeout_ = std::chrono::seconds(disconnect_timeout_seconds);
    reap_interval_ = std::chrono::seconds(reap_interval_seconds);
    reaper_running_ = true;
    reaper_thread_ = std::thread([this] { reaper_loop(); });
    spdlog::info("Game reaper started (disconnect timeout: {}s, interval: {}s)",
                 disconnect_timeout_seconds, reap_interval_seconds);
}

void GameManager::stop_reaper() {
    reaper_running_ = false;
    reaper_cv_.notify_one();
    if (reaper_thread_.joinable()) {
        reaper_thread_.join();
    }
}

void GameManager::reaper_loop() {
    while (reaper_running_) {
        {
            std::unique_lock lock{reaper_mutex_};
            reaper_cv_.wait_for(lock, reap_interval_, [this] { return !reaper_running_.load(); });
        }
        if (!reaper_running_) {
            break;
        }

        std::vector<std::pair<std::string, std::shared_ptr<Game>>> snapshot;
        {
            std::lock_guard const lock{mutex_};
            snapshot.reserve(games_.size());
            for (const auto& [id, game] : games_) {
                snapshot.emplace_back(id, game);
            }
        }

        std::vector<std::string> finished_ids;
        for (const auto& [id, game] : snapshot) {
            if (game->state() == GameState::Finished) {
                finished_ids.push_back(id);
                continue;
            }

            if (game->state() == GameState::InProgress || game->state() == GameState::Paused) {
                auto stale = game->stale_disconnected_players(disconnect_timeout_);
                for (uint64_t const pid : stale) {
                    game->eliminate_player(pid);
                }
            }
        }

        for (const auto& id : finished_ids) {
            remove_game(id);
        }
    }
}

}  // namespace mtg::engine
