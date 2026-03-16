#include "service/game_service_impl.hpp"

#include <atomic>
#include <condition_variable>
#include <memory>
#include <mutex>
#include <string>
#include <thread>

#include "engine/deck_validator.hpp"
#include <spdlog/spdlog.h>

namespace mtg::service {

namespace {

constexpr int max_game_id_length = 32;
constexpr int max_card_names = 250;

auto validate_game_id(const std::string& game_id) -> std::optional<grpc::Status> {
    if (game_id.empty()) {
        return grpc::Status(grpc::StatusCode::INVALID_ARGUMENT, "Game ID is required");
    }
    if (game_id.size() > max_game_id_length) {
        return grpc::Status(grpc::StatusCode::INVALID_ARGUMENT, "Game ID too long");
    }
    return std::nullopt;
}

}  // namespace

GameServiceImpl::GameServiceImpl(mtg::engine::GameManager& game_manager,
                                 const mtg::engine::PresetDeckLoader& preset_decks,
                                 mtg::util::MetricsRegistry* metrics,
                                 mtg::auth::GameHistoryStore* history_store,
                                 mtg::auth::RatingStore* rating_store)
    : game_manager_{game_manager},
      preset_decks_{preset_decks},
      metrics_{metrics},
      history_store_{history_store},
      rating_store_{rating_store} {}

auto GameServiceImpl::extract_user_id(grpc::ServerContext* context) -> std::optional<uint64_t> {
    auto metadata = context->client_metadata();
    auto it = metadata.find("x-user-id");
    if (it != metadata.end()) {
        try {
            return std::stoull(std::string(it->second.data(), it->second.size()));
        } catch (const std::exception& e) {
            spdlog::debug("Failed to parse x-user-id: {}", e.what());
        }
    }
    return std::nullopt;
}

auto GameServiceImpl::extract_username(grpc::ServerContext* context) -> std::string {
    constexpr size_t max_username_length = 64;
    auto metadata = context->client_metadata();
    auto it = metadata.find("x-username");
    if (it != metadata.end()) {
        auto len = std::min(it->second.size(), max_username_length);
        return std::string(it->second.data(), len);
    }
    return "Anonymous";
}

grpc::Status GameServiceImpl::CreateGame(grpc::ServerContext* context,
                                         [[maybe_unused]] const proto::CreateGameRequest* request,
                                         proto::CreateGameResponse* response) {
    auto user_id = extract_user_id(context);
    if (!user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }

    auto result = game_manager_.create_game();
    if (!result) {
        return {grpc::StatusCode::RESOURCE_EXHAUSTED, result.error()};
    }
    response->set_game_id(*result);
    spdlog::info("CreateGame: game_id={}, user={}", *result, *user_id);
    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::JoinGame(grpc::ServerContext* context,
                                       const proto::JoinGameRequest* request,
                                       proto::JoinGameResponse* response) {
    auto user_id = extract_user_id(context);
    if (!user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }

    if (auto err = validate_game_id(request->game_id())) {
        return *err;
    }

    auto game = game_manager_.get_game(request->game_id());
    if (!game) {
        response->set_success(false);
        response->set_error("Game not found");
        return grpc::Status::OK;
    }

    auto username = extract_username(context);

    if (!game->add_player(*user_id, username)) {
        response->set_success(false);
        response->set_error("Cannot join game (full, already joined, or not accepting players)");
        return grpc::Status::OK;
    }

    response->set_success(true);
    spdlog::info("JoinGame: user={} ({}) joined game {}", username, *user_id, request->game_id());
    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::LeaveGame(grpc::ServerContext* context,
                                        const proto::LeaveGameRequest* request,
                                        [[maybe_unused]] proto::LeaveGameResponse* response) {
    auto user_id = extract_user_id(context);
    if (!user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }

    if (auto err = validate_game_id(request->game_id())) {
        return *err;
    }

    auto game = game_manager_.get_game(request->game_id());
    if (!game) {
        return {grpc::StatusCode::NOT_FOUND, "Game not found"};
    }

    auto* player = game->find_player(*user_id);
    if (player != nullptr) {
        if (game->state() == mtg::engine::GameState::InProgress ||
            game->state() == mtg::engine::GameState::Paused) {
            mtg::engine::ActionData concede_action;
            concede_action.player_id = *user_id;
            concede_action.action_type = "concede";
            game->submit_action(*user_id, std::move(concede_action));
        } else {
            player->eliminate();
        }
        spdlog::info("LeaveGame: user {} left game {}", *user_id, request->game_id());
    }
    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::ListGames([[maybe_unused]] grpc::ServerContext* context,
                                        [[maybe_unused]] const proto::ListGamesRequest* request,
                                        proto::ListGamesResponse* response) {
    auto games = game_manager_.list_games();
    for (const auto& entry : games) {
        auto* summary = response->add_games();
        summary->set_game_id(entry.game_id);
        summary->set_player_count(entry.player_count);
        summary->set_max_players(2);

        switch (entry.state) {
            case mtg::engine::GameState::WaitingForPlayers:
                summary->set_status("waiting_for_players");
                break;
            case mtg::engine::GameState::WaitingForDecks:
                summary->set_status("waiting_for_decks");
                break;
            case mtg::engine::GameState::InProgress:
                summary->set_status("in_progress");
                break;
            case mtg::engine::GameState::Paused:
                summary->set_status("paused");
                break;
            case mtg::engine::GameState::Finished:
                summary->set_status("finished");
                break;
            default:
                summary->set_status("unknown");
                break;
        }
    }
    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::SubmitDeck(grpc::ServerContext* context,
                                         const proto::SubmitDeckRequest* request,
                                         proto::SubmitDeckResponse* response) {
    auto user_id = extract_user_id(context);
    if (!user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }

    if (auto err = validate_game_id(request->game_id())) {
        return *err;
    }

    if (request->card_names_size() > max_card_names && request->preset_deck_name().empty()) {
        response->set_valid(false);
        response->add_errors("Too many cards in deck");
        return grpc::Status::OK;
    }

    auto game = game_manager_.get_game(request->game_id());
    if (!game) {
        response->set_valid(false);
        response->add_errors("Game not found");
        return grpc::Status::OK;
    }

    if (game->state() != mtg::engine::GameState::WaitingForDecks) {
        response->set_valid(false);
        response->add_errors("Game is not accepting decks");
        return grpc::Status::OK;
    }

    auto* player = game->find_player(*user_id);
    if (player == nullptr) {
        response->set_valid(false);
        response->add_errors("Not in this game");
        return grpc::Status::OK;
    }
    std::vector<std::string> card_names;
    if (!request->preset_deck_name().empty()) {
        auto preset = preset_decks_.get_deck(request->preset_deck_name());
        if (!preset) {
            response->set_valid(false);
            response->add_errors(preset.error());
            return grpc::Status::OK;
        }
        card_names = std::move(*preset);
    } else {
        constexpr size_t max_card_name_length = 128;
        card_names.reserve(static_cast<size_t>(request->card_names_size()));
        for (const auto& name : request->card_names()) {
            if (name.size() > max_card_name_length) {
                response->set_valid(false);
                response->add_errors("Card name too long");
                return grpc::Status::OK;
            }
            card_names.push_back(name);
        }
    }

    const mtg::engine::DeckValidator validator{game_manager_.registry()};
    auto validation = validator.validate(card_names);
    if (!validation.valid) {
        response->set_valid(false);
        for (const auto& err : validation.errors) {
            response->add_errors(err);
        }
        return grpc::Status::OK;
    }

    std::vector<std::string> sideboard_names;
    sideboard_names.reserve(static_cast<size_t>(request->sideboard_size()));
    for (const auto& name : request->sideboard()) {
        sideboard_names.push_back(name);
    }

    if (!sideboard_names.empty()) {
        auto sb_validation = validator.validate_sideboard(sideboard_names);
        if (!sb_validation.valid) {
            response->set_valid(false);
            for (const auto& err : sb_validation.errors) {
                response->add_errors(err);
            }
            return grpc::Status::OK;
        }
    }

    if (!game->submit_deck(*user_id, card_names, sideboard_names)) {
        response->set_valid(false);
        response->add_errors("Deck already submitted");
        return grpc::Status::OK;
    }

    if (game->state() == mtg::engine::GameState::WaitingForDecks) {
        game->start();
    }

    response->set_valid(true);
    spdlog::info("SubmitDeck: user {} submitted {} cards for game {}", *user_id, card_names.size(),
                 request->game_id());
    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::GetGameState(grpc::ServerContext* context,
                                           const proto::GetGameStateRequest* request,
                                           proto::GetGameStateResponse* response) {
    auto user_id = extract_user_id(context);
    if (!user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }

    if (auto err = validate_game_id(request->game_id())) {
        return *err;
    }

    auto game = game_manager_.get_game(request->game_id());
    if (!game) {
        return {grpc::StatusCode::NOT_FOUND, "Game not found"};
    }

    *response->mutable_snapshot() = game->build_snapshot(*user_id);
    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::RejoinGame(grpc::ServerContext* context,
                                         const proto::RejoinGameRequest* request,
                                         proto::RejoinGameResponse* response) {
    auto user_id = extract_user_id(context);
    if (!user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }

    if (auto err = validate_game_id(request->game_id())) {
        return *err;
    }

    auto game = game_manager_.get_game(request->game_id());
    if (!game) {
        response->set_success(false);
        response->set_error("Game not found");
        return grpc::Status::OK;
    }

    auto* player = game->find_player(*user_id);
    if (player == nullptr) {
        response->set_success(false);
        response->set_error("Not a player in this game");
        return grpc::Status::OK;
    }

    if (game->state() != mtg::engine::GameState::InProgress &&
        game->state() != mtg::engine::GameState::Paused) {
        response->set_success(false);
        response->set_error("Game is not in progress");
        return grpc::Status::OK;
    }

    game->set_player_connected(*user_id, true);
    response->set_success(true);
    *response->mutable_snapshot() = game->build_snapshot(*user_id);

    spdlog::info("RejoinGame: user {} rejoined game {}", *user_id, request->game_id());
    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::ListPresetDecks(
    [[maybe_unused]] grpc::ServerContext* context,
    [[maybe_unused]] const proto::ListPresetDecksRequest* request,
    proto::ListPresetDecksResponse* response) {
    for (const auto& name : preset_decks_.available_decks()) {
        auto deck = preset_decks_.get_deck(name);
        auto* info = response->add_decks();
        info->set_name(name);
        info->set_card_count(deck ? static_cast<int32_t>(deck->size()) : 0);
    }
    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::GetMatchHistory(grpc::ServerContext* context,
                                              const proto::GetMatchHistoryRequest* request,
                                              proto::GetMatchHistoryResponse* response) {
    auto user_id = extract_user_id(context);
    if (!user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }

    if (history_store_ == nullptr) {
        return {grpc::StatusCode::UNAVAILABLE, "Match history not available"};
    }

    const int limit = request->limit() > 0 ? request->limit() : 20;
    const int offset = request->offset() >= 0 ? request->offset() : 0;

    const auto entries = history_store_->get_player_history(*user_id, limit, offset);
    const int total = history_store_->get_player_game_count(*user_id);
    response->set_total_count(total);

    for (const auto& entry : entries) {
        auto* match = response->add_matches();
        match->set_game_id(entry.game_id);
        match->set_winner_id(entry.winner_id);
        match->set_duration_seconds(entry.duration_seconds);
        match->set_started_at(entry.started_at);
        match->set_finished_at(entry.finished_at);
        for (const auto& p : entry.players) {
            auto* player = match->add_players();
            player->set_player_id(p.player_id);
            player->set_final_life(p.final_life);
        }
    }

    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::StreamGameState(grpc::ServerContext* context,
                                              const proto::StreamRequest* request,
                                              grpc::ServerWriter<proto::GameEvent>* writer) {
    auto user_id = extract_user_id(context);
    if (!user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }

    if (auto err = validate_game_id(request->game_id())) {
        return *err;
    }

    auto game = game_manager_.get_game(request->game_id());
    if (!game) {
        return {grpc::StatusCode::NOT_FOUND, "Game not found"};
    }

    if (game->find_player(*user_id) == nullptr) {
        return {grpc::StatusCode::PERMISSION_DENIED, "Not a player in this game"};
    }

    auto past_events = game->broadcaster().get_events_since(request->last_sequence());
    for (const auto& event : past_events) {
        if (context->IsCancelled() || !writer->Write(event)) {
            return grpc::Status::OK;
        }
    }

    struct SharedState {
        std::mutex mu;
        std::condition_variable cv;
        std::vector<proto::GameEvent> pending;
        bool done{false};
    };
    auto state = std::make_shared<SharedState>();

    constexpr size_t max_pending_events = 1000;
    auto callback = std::make_shared<mtg::engine::EventBroadcaster::Callback>(
        [state](const proto::GameEvent& event) {
            std::lock_guard lock{state->mu};
            if (!state->done && state->pending.size() < max_pending_events) {
                state->pending.push_back(event);
                state->cv.notify_one();
            }
        });
    const auto sub_id = game->broadcaster().subscribe(callback);

    while (!context->IsCancelled() && game->state() != mtg::engine::GameState::Finished) {
        std::unique_lock lock{state->mu};
        state->cv.wait_for(lock, std::chrono::milliseconds(500),
                           [&] { return !state->pending.empty() || state->done; });

        auto to_send = std::move(state->pending);
        state->pending.clear();
        lock.unlock();

        for (const auto& event : to_send) {
            if (!writer->Write(event)) {
                {
                    std::lock_guard lk{state->mu};
                    state->done = true;
                }
                game->broadcaster().unsubscribe(sub_id);
                return grpc::Status::OK;
            }
        }
    }

    {
        std::lock_guard lk{state->mu};
        state->done = true;
    }
    game->broadcaster().unsubscribe(sub_id);
    return grpc::Status::OK;
}

grpc::Status GameServiceImpl::SubmitAction(
    grpc::ServerContext* context,
    grpc::ServerReaderWriter<proto::GameEvent, proto::PlayerAction>* stream) {
    auto user_id = extract_user_id(context);
    if (!user_id) {
        return {grpc::StatusCode::UNAUTHENTICATED, "Authentication required"};
    }

    proto::PlayerAction first_action;
    if (!stream->Read(&first_action)) {
        return {grpc::StatusCode::INVALID_ARGUMENT, "No action received"};
    }

    auto game = game_manager_.get_game(first_action.game_id());
    if (!game) {
        return {grpc::StatusCode::NOT_FOUND, "Game not found"};
    }

    if (game->find_player(*user_id) == nullptr) {
        return {grpc::StatusCode::PERMISSION_DENIED, "Not a player in this game"};
    }

    game->set_player_connected(*user_id, true);
    if (metrics_ != nullptr) {
        metrics_->set_connected_players(connected_player_count_.fetch_add(1) + 1);
    }
    struct MetricsGuard {
        std::atomic<int64_t>& count;
        mtg::util::MetricsRegistry* metrics;
        MetricsGuard(std::atomic<int64_t>& c, mtg::util::MetricsRegistry* m)
            : count(c), metrics(m) {}
        ~MetricsGuard() {
            if (metrics != nullptr) {
                metrics->set_connected_players(count.fetch_sub(1) - 1);
            }
        }
        MetricsGuard(const MetricsGuard&) = delete;
        MetricsGuard& operator=(const MetricsGuard&) = delete;
        MetricsGuard(MetricsGuard&&) = delete;
        MetricsGuard& operator=(MetricsGuard&&) = delete;
    } metrics_guard{connected_player_count_, metrics_};

    struct SharedState {
        std::mutex mu;
        std::condition_variable cv;
        std::vector<proto::GameEvent> pending;
        std::atomic<bool> running{true};
    };
    auto state = std::make_shared<SharedState>();

    constexpr size_t max_pending_events = 1000;
    auto callback = std::make_shared<mtg::engine::EventBroadcaster::Callback>(
        [state](const proto::GameEvent& event) {
            std::lock_guard lock{state->mu};
            if (state->running && state->pending.size() < max_pending_events) {
                state->pending.push_back(event);
                state->cv.notify_one();
            }
        });
    const auto sub_id = game->broadcaster().subscribe_player(*user_id, callback);

    proto::GameEvent snap_event;
    snap_event.set_game_id(game->game_id());
    auto* snap = snap_event.mutable_snapshot();
    *snap->mutable_snapshot() = game->build_snapshot(*user_id);
    stream->Write(snap_event);

    std::thread writer_thread([state, stream, context] {
        while (state->running && !context->IsCancelled()) {
            std::unique_lock lock{state->mu};
            state->cv.wait_for(lock, std::chrono::milliseconds(200),
                               [&] { return !state->pending.empty() || !state->running; });

            auto to_send = std::move(state->pending);
            state->pending.clear();
            lock.unlock();

            for (const auto& event : to_send) {
                if (!stream->Write(event)) {
                    state->running = false;
                    state->cv.notify_all();
                    return;
                }
            }
        }
    });

    auto convert_action = [uid =
                               *user_id](const proto::PlayerAction& pa) -> mtg::engine::ActionData {
        mtg::engine::ActionData data;
        data.player_id = uid;
        data.prompt_id = pa.prompt_id();

        if (pa.has_pass()) {
            data.action_type = "pass";
        } else if (pa.has_play_card()) {
            data.action_type = pa.play_card().flashback() ? "play_card_flashback" : "play_card";
            data.target_id = pa.play_card().card_instance_id();
            data.x_value = pa.play_card().x_value();
            data.flag = pa.play_card().kicked();
            for (auto cid : pa.play_card().convoke_ids()) {
                data.convoke_ids.push_back(cid);
            }
            data.delve_count = pa.play_card().delve_count();
        } else if (pa.has_play_land()) {
            data.action_type = "play_land";
            data.target_id = pa.play_land().card_instance_id();
        } else if (pa.has_activate_ability()) {
            data.action_type = "activate_ability";
            data.target_id = pa.activate_ability().permanent_id();
            data.indices.push_back(pa.activate_ability().ability_index());
        } else if (pa.has_select_target()) {
            data.action_type = "select_target";
            data.target_id = pa.select_target().target_id();
        } else if (pa.has_select_mode()) {
            data.action_type = "select_mode";
            for (auto m : pa.select_mode().chosen_modes()) {
                data.indices.push_back(m);
            }
        } else if (pa.has_yes_no()) {
            data.action_type = "yes_no";
            data.flag = pa.yes_no().choice();
        } else if (pa.has_discard()) {
            data.action_type = "discard";
            for (auto id : pa.discard().card_instance_ids()) {
                data.ids.push_back(id);
            }
        } else if (pa.has_select_color()) {
            data.action_type = "select_color";
            data.text = pa.select_color().color();
        } else if (pa.has_select_creature_type()) {
            data.action_type = "select_creature_type";
            data.text = pa.select_creature_type().creature_type();
        } else if (pa.has_pay_mana()) {
            data.action_type = "pay_mana";
            const auto& mp = pa.pay_mana().payment();
            data.mana_payment = {.white = mp.white(),
                                 .blue = mp.blue(),
                                 .black = mp.black(),
                                 .red = mp.red(),
                                 .green = mp.green(),
                                 .colorless = mp.colorless()};
        } else if (pa.has_concede()) {
            data.action_type = "concede_request";
        } else if (pa.has_draw_offer()) {
            data.action_type = "draw_offer";
        } else if (pa.has_draw_response()) {
            data.action_type = "draw_response";
            data.flag = pa.draw_response().accept();
        } else if (pa.has_declare_attackers()) {
            data.action_type = "declare_attackers";
            for (const auto& atk : pa.declare_attackers().attackers()) {
                data.ids.push_back(atk.creature_id());
                data.ids.push_back(atk.defending_player_id());
            }
        } else if (pa.has_declare_blockers()) {
            data.action_type = "declare_blockers";
            for (const auto& blk : pa.declare_blockers().blockers()) {
                data.ids.push_back(blk.blocker_id());
                data.ids.push_back(blk.attacker_id());
            }
        } else if (pa.has_order_blockers()) {
            data.action_type = "order_blockers";
            for (auto id : pa.order_blockers().ordered_blocker_ids()) {
                data.ids.push_back(id);
            }
        } else if (pa.has_damage_assignment()) {
            data.action_type = "damage_assignment";
            for (auto d : pa.damage_assignment().damage_to_each_blocker()) {
                data.indices.push_back(d);
            }
            data.x_value = pa.damage_assignment().damage_to_player();
        } else if (pa.has_set_auto_pass()) {
            data.action_type = "set_auto_pass";
            data.x_value = pa.set_auto_pass().mode();
        } else if (pa.has_sideboard_swap()) {
            data.action_type = "sideboard_swap";
            std::string names;
            for (const auto& name : pa.sideboard_swap().cards_in()) {
                if (!names.empty()) {
                    names += ',';
                }
                names += name;
            }
            data.text = std::move(names);
        } else if (pa.has_undo()) {
            data.action_type = "undo";
        }
        return data;
    };

    game->submit_action(*user_id, convert_action(first_action));

    proto::PlayerAction action;
    while (state->running && stream->Read(&action)) {
        game->submit_action(*user_id, convert_action(action));
    }

    state->running = false;
    state->cv.notify_all();
    writer_thread.join();
    game->broadcaster().unsubscribe(sub_id);
    game->set_player_connected(*user_id, false);

    record_game_result(game);

    return grpc::Status::OK;
}

void GameServiceImpl::record_game_result(const std::shared_ptr<mtg::engine::Game>& game) {
    if (!game || game->state() != mtg::engine::GameState::Finished) {
        return;
    }
    const auto& result = game->result();
    if (!result) {
        return;
    }

    {
        std::lock_guard const lock{recorded_games_mutex_};
        if (!recorded_game_ids_.insert(game->game_id()).second) {
            return;
        }
    }

    if (history_store_ != nullptr) {
        mtg::auth::GameHistoryRecord record;
        record.game_id = game->game_id();
        record.winner_id = result->winner_id;
        auto now = std::chrono::system_clock::now();
        record.started_at = game->started_at();
        record.duration_seconds = static_cast<int>(
            std::chrono::duration_cast<std::chrono::seconds>(now - game->started_at()).count());
        for (const auto& player : game->players()) {
            mtg::auth::GameHistoryPlayerRecord pr;
            pr.player_id = player.id();
            pr.final_life = player.life();
            pr.deck_list = player.submitted_deck_names();
            record.players.push_back(std::move(pr));
        }
        history_store_->record_game(record);
    }

    if (rating_store_ != nullptr && !result->is_draw && result->winner_id != 0) {
        uint64_t loser_id = 0;
        for (const auto& player : game->players()) {
            if (player.id() != result->winner_id) {
                loser_id = player.id();
                break;
            }
        }
        if (loser_id != 0) {
            rating_store_->update_after_game(result->winner_id, loser_id);
        }
    }
}

}  // namespace mtg::service
