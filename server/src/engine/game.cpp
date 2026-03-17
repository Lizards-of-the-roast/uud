#include "engine/game.hpp"

#include <algorithm>
#include <sstream>
#include <tuple>

#include "engine/target_filter.hpp"
#include <cle/serialization/card_serializer.hpp>
#include <cle/triggers/trigger_event.hpp>
#include <spdlog/spdlog.h>

namespace mtg::engine {

Game::Game(std::string game_id, const CardRegistry& registry, int clock_seconds)
    : game_id_{std::move(game_id)},
      card_registry_{registry},
      clock_{std::chrono::seconds{clock_seconds}} {
    game_context_ = std::make_shared<GameContextImpl>(*this);
    card_engine_.set_game_context(game_context_);

    auto& lua = card_engine_.lua_state();
    lua["dofile"] = sol::lua_nil;
    lua["loadfile"] = sol::lua_nil;
    lua["load"] = sol::lua_nil;
    lua["io"] = sol::lua_nil;
    lua["os"] = sol::lua_nil;
    lua["package"] = sol::lua_nil;
    lua["require"] = sol::lua_nil;
    lua["debug"] = sol::lua_nil;

    constexpr int max_lua_instructions = 1'000'000;
    lua_sethook(
        lua.lua_state(),
        [](lua_State* L, [[maybe_unused]] lua_Debug* ar) {
            luaL_error(L, "Lua script exceeded instruction limit");
        },
        LUA_MASKCOUNT, max_lua_instructions);
}

Game::~Game() {
    stop();

    delayed_triggers_.clear();
    stack_.clear();
    for (auto& player : players_) {
        player.clear_deck();
    }
    zones_.clear_all();
    game_context_.reset();
    card_engine_.lua_state().collect_garbage();
    card_engine_.lua_state().collect_garbage();
}

auto Game::add_player(uint64_t player_id, const std::string& username) -> bool {
    std::lock_guard const lock{mutex_};
    if (state_.load() != GameState::WaitingForPlayers) {
        spdlog::warn("Game {} not accepting players (state={})", game_id_,
                     static_cast<int>(state_.load()));
        return false;
    }
    if (players_.size() >= max_players) {
        spdlog::warn("Game {} is full, rejecting player {} ({})", game_id_, username, player_id);
        return false;
    }
    for (const auto& p : players_) {
        if (p.id() == player_id) {
            return false;
        }
    }
    players_.emplace_back(player_id, username);
    zones_.init_player(player_id);
    auto queue = std::make_shared<ActionQueue>();
    queue->set_shared_notify(shared_notify_);
    action_queues_[player_id] = std::move(queue);
    spdlog::info("Player {} ({}) joined game {}", username, player_id, game_id_);

    if (players_.size() == max_players) {
        state_.store(GameState::WaitingForDecks);
        spdlog::info("Game {} now waiting for decks", game_id_);
    }
    return true;
}

auto Game::submit_deck(uint64_t player_id, const std::vector<std::string>& card_names,
                       const std::vector<std::string>& sideboard_names) -> bool {
    std::lock_guard const lock{mutex_};
    auto* player = find_player(player_id);
    if (player == nullptr) {
        return false;
    }
    if (player->deck_submitted()) {
        return false;
    }
    player->set_submitted_deck_names(card_names);
    if (!sideboard_names.empty()) {
        player->set_submitted_sideboard_names(sideboard_names);
    }
    spdlog::info("Player {} submitted deck with {} cards + {} sideboard", player_id,
                 card_names.size(), sideboard_names.size());
    return true;
}

void Game::start() {
    std::lock_guard const lock{mutex_};
    if (running_) {
        return;
    }
    if (state_.load() != GameState::WaitingForDecks) {
        spdlog::warn("Game {}: cannot start from state {}", game_id_,
                     static_cast<int>(state_.load()));
        return;
    }
    for (const auto& p : players_) {
        if (!p.deck_submitted()) {
            spdlog::warn("Game {}: player {} has no deck, cannot start", game_id_, p.id());
            return;
        }
    }
    if (players_.size() < 2) {
        spdlog::warn("Game {}: not enough players to start", game_id_);
        return;
    }
    running_ = true;
    started_at_ = std::chrono::system_clock::now();
    state_.store(GameState::InProgress);
    game_thread_ = std::thread([this] { game_loop(); });
    spdlog::info("Game {} started", game_id_);
}

void Game::set_auto_pass(uint64_t player_id, AutoPassMode mode) {
    std::lock_guard const lock{mutex_};
    auto_pass_modes_[player_id] = mode;
    spdlog::debug("Game {}: player {} set auto-pass to {}", game_id_, player_id,
                  static_cast<int>(mode));
}

auto Game::get_auto_pass(uint64_t player_id) const -> AutoPassMode {
    std::lock_guard const lock{mutex_};
    auto it = auto_pass_modes_.find(player_id);
    if (it == auto_pass_modes_.end()) {
        return AutoPassMode::Disabled;
    }
    return it->second;
}

void Game::set_player_connected(uint64_t player_id, bool connected) {
    std::lock_guard const lock{mutex_};
    if (connected) {
        connected_players_.insert(player_id);
        disconnect_times_.erase(player_id);
        reconnect_cv_.notify_all();
        auto expected = GameState::Paused;
        if (state_.compare_exchange_strong(expected, GameState::InProgress)) {
            spdlog::info("Game {}: player {} reconnected, resuming", game_id_, player_id);
        }
    } else {
        connected_players_.erase(player_id);
        disconnect_times_[player_id] = std::chrono::steady_clock::now();
        spdlog::info("Game {}: player {} disconnected", game_id_, player_id);
    }
}

auto Game::is_player_connected(uint64_t player_id) const -> bool {
    std::lock_guard const lock{mutex_};
    return connected_players_.contains(player_id);
}

auto Game::stale_disconnected_players(std::chrono::seconds timeout) const -> std::vector<uint64_t> {
    std::lock_guard const lock{mutex_};
    std::vector<uint64_t> stale;
    auto now = std::chrono::steady_clock::now();
    for (const auto& [pid, disconnect_time] : disconnect_times_) {
        if (now - disconnect_time > timeout) {
            const auto* player = find_player(pid);
            if ((player != nullptr) && player->is_alive()) {
                stale.push_back(pid);
            }
        }
    }
    return stale;
}

void Game::eliminate_player(uint64_t player_id) {
    auto* queue = get_action_queue(player_id);
    if (queue != nullptr) {
        [[maybe_unused]] auto ok = queue->submit(ActionData{.player_id = player_id,
                                                            .prompt_id = {},
                                                            .action_type = "concede",
                                                            .target_id = 0,
                                                            .ids = {},
                                                            .indices = {},
                                                            .flag = false,
                                                            .text = "forced",
                                                            .mana_payment = {}});
        spdlog::info("Game {}: reaper submitted concede for player {}", game_id_, player_id);
    }
}

auto Game::wait_for_connected_player(uint64_t player_id) -> bool {
    std::unique_lock lock{mutex_};
    if (connected_players_.contains(player_id) || !running_) {
        return running_;
    }

    state_.store(GameState::Paused);
    spdlog::info("Game {}: pausing, waiting for player {} to reconnect", game_id_, player_id);

    bool const reconnected =
        reconnect_cv_.wait_for(lock, std::chrono::seconds(action_timeout_seconds_),
                               [&] { return connected_players_.contains(player_id) || !running_; });

    if (!reconnected || !running_) {
        auto* player = find_player(player_id);
        if ((player != nullptr) && player->is_alive()) {
            player->eliminate();
            spdlog::info("Game {}: player {} auto-conceded (disconnect timeout)", game_id_,
                         player_id);

            auto win = win_checker_.check(*this);
            if (win) {
                result_ = *win;
                state_.store(GameState::Finished);
                running_ = false;

                proto::GameEvent go_event;
                go_event.set_game_id(game_id_);
                auto* go = go_event.mutable_game_over();
                go->set_winner_id(win->winner_id);
                go->set_reason("Opponent disconnected");
                go->set_is_draw(win->is_draw);
                broadcaster_.emit(std::move(go_event));
            }
        }
        return false;
    }

    state_.store(GameState::InProgress);
    return true;
}

void Game::stop() {
    running_ = false;
    reconnect_cv_.notify_all();
    for (auto& [id, queue] : action_queues_) {
        [[maybe_unused]] auto ok = queue->submit(ActionData{.player_id = id,
                                                            .prompt_id = {},
                                                            .action_type = "shutdown",
                                                            .target_id = 0,
                                                            .ids = {},
                                                            .indices = {},
                                                            .flag = false,
                                                            .text = {},
                                                            .x_value = 0,
                                                            .mana_payment = {}});
    }
    if (game_thread_.joinable()) {
        game_thread_.join();
    }
    clear_delayed_triggers();
}

void Game::submit_action(uint64_t player_id, ActionData action) {
    std::lock_guard const lock{mutex_};
    auto it = action_queues_.find(player_id);
    if (it != action_queues_.end()) {
        [[maybe_unused]] auto ok = it->second->submit(std::move(action));
    }
}

Player* Game::find_player(uint64_t id) {
    std::lock_guard const lock{mutex_};
    for (auto& p : players_) {
        if (p.id() == id) {
            return &p;
        }
    }
    return nullptr;
}

const Player* Game::find_player(uint64_t id) const {
    std::lock_guard const lock{mutex_};
    for (const auto& p : players_) {
        if (p.id() == id) {
            return &p;
        }
    }
    return nullptr;
}

ActionQueue* Game::get_action_queue(uint64_t player_id) {
    auto it = action_queues_.find(player_id);
    return it != action_queues_.end() ? it->second.get() : nullptr;
}

void Game::add_delayed_trigger(DelayedTrigger trigger) {
    std::lock_guard const lock{mutex_};
    if (delayed_triggers_.size() >= 1000) {
        spdlog::warn("Game {}: delayed trigger limit reached, ignoring new trigger", game_id_);
        return;
    }
    delayed_triggers_.push_back(std::move(trigger));
}

void Game::clear_delayed_triggers() {
    std::lock_guard const lock{mutex_};
    delayed_triggers_.clear();
}

void Game::setup_game() {
    std::lock_guard const lock{mutex_};

    for (auto& player : players_) {
        std::vector<std::shared_ptr<cle::core::Card>> deck;
        for (const auto& name : player.submitted_deck_names()) {
            auto result = card_registry_.create_card_instance(name, card_engine_);
            if (result) {
                deck.push_back(std::move(*result));
            } else {
                spdlog::error("Failed to create card '{}': {}", name, result.error());
            }
        }
        player.set_deck(std::move(deck));

        std::vector<std::shared_ptr<cle::core::Card>> sideboard;
        for (const auto& name : player.submitted_sideboard_names()) {
            auto result = card_registry_.create_card_instance(name, card_engine_);
            if (result) {
                sideboard.push_back(std::move(*result));
            } else {
                spdlog::error("Failed to create sideboard card '{}': {}", name, result.error());
            }
        }
        player.set_sideboard(std::move(sideboard));
    }

    for (auto& player : players_) {
        zones_.set_library(player.id(), player.deck());
        zones_.shuffle_library(player.id());
    }
    for (auto& player : players_) {
        game_context_->draw_cards(player.id(), 7);
    }
    std::vector<uint64_t> order;
    for (const auto& p : players_) {
        order.push_back(p.id());
    }
    priority_.set_player_order(order);

    if (clock_.is_enabled()) {
        for (const auto& p : players_) {
            clock_.add_player(p.id());
        }
    }

    spdlog::info("Game {} setup complete. {} players.", game_id_, players_.size());
}

void Game::game_loop() {
    setup_game();
    if (!running_) {
        return;
    }

    int player_index = 0;
    while (running_) {
        uint64_t active_player_id;
        int turn_num;
        std::string active_username;
        {
            std::lock_guard const lock{mutex_};
            auto& active_player = players_[static_cast<size_t>(player_index) % players_.size()];
            turns_.new_turn(active_player.id());
            active_player.reset_for_turn();

            for (auto& [pid, mode] : auto_pass_modes_) {
                if (mode == AutoPassMode::UntilEndOfTurn) {
                    mode = AutoPassMode::Disabled;
                }
            }
            active_player_id = active_player.id();
            turn_num = turns_.turn_number();
            active_username = active_player.username();
        }

        spdlog::debug("Game {}: Turn {} ({})", game_id_, turn_num, active_username);

        {
            proto::GameEvent event;
            event.set_game_id(game_id_);
            auto* pc = event.mutable_phase_changed();
            pc->set_new_phase(static_cast<proto::Phase>(static_cast<int>(Phase::Untap)));
            pc->set_active_player_id(active_player_id);
            pc->set_turn_number(turn_num);
            broadcaster_.emit(std::move(event));
        }

        for (int phase_idx = 0; phase_idx < phase_count() && running_; ++phase_idx) {
            {
                std::lock_guard const lock{mutex_};
                Phase const phase = static_cast<Phase>(phase_idx);
                turns_.set_phase(phase);

                proto::GameEvent event;
                event.set_game_id(game_id_);
                auto* pc = event.mutable_phase_changed();
                pc->set_new_phase(static_cast<proto::Phase>(phase_idx));
                pc->set_active_player_id(active_player_id);
                pc->set_turn_number(turn_num);
                broadcaster_.emit(std::move(event));
            }

            run_phase(static_cast<Phase>(phase_idx));

            if (static_cast<Phase>(phase_idx) == Phase::Untap) {
                for (const auto& p : players_) {
                    proto::GameEvent snap_event;
                    snap_event.set_game_id(game_id_);
                    auto* se = snap_event.mutable_snapshot();
                    *se->mutable_snapshot() = build_snapshot(p.id());
                    broadcaster_.emit_to_player(p.id(), std::move(snap_event));
                }
            }

            {
                std::lock_guard const lock{mutex_};
                auto win = win_checker_.check(*this);
                if (win) {
                    result_ = *win;
                    state_.store(GameState::Finished);
                    running_ = false;

                    proto::GameEvent go_event;
                    go_event.set_game_id(game_id_);
                    auto* go = go_event.mutable_game_over();
                    go->set_winner_id(win->winner_id);
                    go->set_reason(win->reason);
                    go->set_is_draw(win->is_draw);
                    broadcaster_.emit(std::move(go_event));

                    spdlog::info("Game {} ended: {} (winner: {})", game_id_, win->reason,
                                 win->winner_id);
                    return;
                }
            }
        }

        player_index++;
    }
}

void Game::run_phase(Phase phase) {
    if (phase == Phase::Cleanup) {
        run_cleanup_phase();
        return;
    }

    {
        std::lock_guard const lock{mutex_};
        cle::triggers::TriggerEvent te;
        te.player_id = turns_.active_player_id();

        switch (phase) {
            case Phase::Upkeep:
                te.type = cle::triggers::TriggerType::BeginningOfUpkeep;
                fire_triggers(cle::triggers::TriggerType::BeginningOfUpkeep, te);
                break;
            case Phase::Draw:
                te.type = cle::triggers::TriggerType::BeginningOfDrawStep;
                fire_triggers(cle::triggers::TriggerType::BeginningOfDrawStep, te);
                break;
            case Phase::Main1:
            case Phase::Main2:
                te.type = cle::triggers::TriggerType::BeginningOfMainPhase;
                fire_triggers(cle::triggers::TriggerType::BeginningOfMainPhase, te);
                break;
            case Phase::BeginningOfCombat:
                te.type = cle::triggers::TriggerType::BeginningOfCombat;
                fire_triggers(cle::triggers::TriggerType::BeginningOfCombat, te);
                break;
            case Phase::EndOfCombat:
                te.type = cle::triggers::TriggerType::EndOfCombat;
                fire_triggers(cle::triggers::TriggerType::EndOfCombat, te);
                break;
            case Phase::EndStep:
                te.type = cle::triggers::TriggerType::BeginningOfEndStep;
                fire_triggers(cle::triggers::TriggerType::BeginningOfEndStep, te);
                fire_delayed_triggers("on_end_of_turn", 0);
                break;
            default:
                break;
        }
    }

    {
        std::unique_lock lock{mutex_};
        switch (phase) {
            case Phase::Untap:
                for (auto& [id, perm] : zones_.get_all_permanents()) {
                    if (perm.controller_id() == turns_.active_player_id()) {
                        perm.untap();
                        perm.set_summoning_sickness(false);
                    }
                }
                break;
            case Phase::Draw:
                if (turns_.turn_number() > 1) {
                    game_context_->draw_cards(turns_.active_player_id(), 1);
                }
                break;
            case Phase::BeginningOfCombat:
                combat_.begin_combat();
                break;
            case Phase::DeclareAttackers: {
                uint64_t const active_id = turns_.active_player_id();
                std::vector<uint64_t> eligible;
                for (auto perm_id : zones_.get_permanents_controlled_by(active_id)) {
                    if (combat_.validate_attacker(perm_id, zones_)) {
                        eligible.push_back(perm_id);
                    }
                }

                if (!eligible.empty()) {
                    proto::ActionPrompt atk_prompt;
                    atk_prompt.set_player_id(active_id);
                    atk_prompt.set_prompt_id(std::to_string(broadcaster_.next_sequence()));
                    auto* ap = atk_prompt.mutable_attackers();
                    for (auto id : eligible) {
                        ap->add_eligible_attackers(id);
                    }
                    for (const auto& p : players_) {
                        if (p.id() != active_id && p.is_alive()) {
                            ap->add_defending_players(p.id());
                        }
                    }

                    proto::GameEvent atk_event;
                    atk_event.set_game_id(game_id_);
                    *atk_event.mutable_action_prompt() = std::move(atk_prompt);
                    broadcaster_.emit_to_player(active_id, std::move(atk_event));

                    auto* queue = get_action_queue(active_id);
                    if (queue != nullptr) {
                        lock.unlock();
                        if (!wait_for_connected_player(active_id)) {
                            lock.lock();
                            break;
                        }
                        auto response =
                            queue->wait_for(std::chrono::seconds(action_timeout_seconds_));
                        lock.lock();
                        if (response && response->action_type == "declare_attackers" &&
                            !response->ids.empty()) {
                            std::vector<AttackDeclaration> declarations;
                            uint64_t default_defender = 0;
                            for (const auto& p : players_) {
                                if (p.id() != active_id && p.is_alive()) {
                                    default_defender = p.id();
                                    break;
                                }
                            }
                            for (size_t i = 0; i + 1 < response->ids.size(); i += 2) {
                                auto creature_id = response->ids[i];
                                auto defender_id = response->ids[i + 1];
                                auto* defender = find_player(defender_id);
                                if (defender == nullptr || !defender->is_alive() ||
                                    defender_id == active_id) {
                                    defender_id = default_defender;
                                }
                                auto* atk_perm = zones_.find_permanent(creature_id);
                                if (atk_perm == nullptr || atk_perm->controller_id() != active_id) {
                                    continue;
                                }
                                if (combat_.validate_attacker(creature_id, zones_)) {
                                    declarations.push_back({creature_id, defender_id});
                                    auto* perm = zones_.find_permanent(creature_id);
                                    if ((perm != nullptr) && !perm->has_keyword("Vigilance")) {
                                        perm->tap();
                                    }
                                }
                            }
                            combat_.set_attackers(std::move(declarations));

                            for (const auto& atk : combat_.get_attackers()) {
                                cle::triggers::TriggerEvent te;
                                te.type = cle::triggers::TriggerType::OnAttack;
                                te.source_id = atk.attacker_id;
                                te.player_id = active_id;
                                fire_triggers(cle::triggers::TriggerType::OnAttack, te);
                            }
                        }
                    }
                }
                break;
            }
            case Phase::DeclareBlockers: {
                if (combat_.get_attackers().empty()) {
                    break;
                }

                for (const auto& player : players_) {
                    if (player.id() == turns_.active_player_id()) {
                        continue;
                    }
                    if (!player.is_alive()) {
                        continue;
                    }

                    std::vector<uint64_t> eligible_blockers;
                    for (auto perm_id : zones_.get_permanents_controlled_by(player.id())) {
                        auto* perm = zones_.find_permanent(perm_id);
                        if ((perm != nullptr) && perm->is_creature() && !perm->is_tapped()) {
                            eligible_blockers.push_back(perm_id);
                        }
                    }

                    if (eligible_blockers.empty()) {
                        continue;
                    }

                    proto::ActionPrompt blk_prompt;
                    blk_prompt.set_player_id(player.id());
                    blk_prompt.set_prompt_id(std::to_string(broadcaster_.next_sequence()));
                    auto* bp = blk_prompt.mutable_blockers();
                    for (auto id : eligible_blockers) {
                        bp->add_eligible_blockers(id);
                    }
                    for (const auto& atk : combat_.get_attackers()) {
                        bp->add_attacking_creatures(atk.attacker_id);
                    }

                    proto::GameEvent blk_event;
                    blk_event.set_game_id(game_id_);
                    *blk_event.mutable_action_prompt() = std::move(blk_prompt);
                    broadcaster_.emit_to_player(player.id(), std::move(blk_event));

                    auto* queue = get_action_queue(player.id());
                    if (queue != nullptr) {
                        lock.unlock();
                        if (!wait_for_connected_player(player.id())) {
                            lock.lock();
                            break;
                        }
                        auto response =
                            queue->wait_for(std::chrono::seconds(action_timeout_seconds_));
                        lock.lock();
                        if (response && response->action_type == "declare_blockers") {
                            std::vector<BlockDeclaration> blocks;
                            std::unordered_set<uint64_t> used_blockers;
                            for (size_t i = 0; i + 1 < response->ids.size(); i += 2) {
                                uint64_t const blocker_id = response->ids[i];
                                uint64_t const attacker_id = response->ids[i + 1];
                                if (used_blockers.contains(blocker_id)) {
                                    continue;
                                }
                                auto* blk_perm = zones_.find_permanent(blocker_id);
                                if (blk_perm == nullptr ||
                                    blk_perm->controller_id() != player.id()) {
                                    continue;
                                }
                                bool attacker_exists = false;
                                for (const auto& atk : combat_.get_attackers()) {
                                    if (atk.attacker_id == attacker_id) {
                                        attacker_exists = true;
                                        break;
                                    }
                                }
                                if (!attacker_exists) {
                                    continue;
                                }
                                if (combat_.validate_blocker(blocker_id, attacker_id, zones_)) {
                                    blocks.push_back({blocker_id, attacker_id});
                                    used_blockers.insert(blocker_id);
                                }
                            }
                            if (!blocks.empty()) {
                                auto current_blockers = combat_.state().blockers;
                                current_blockers.insert(current_blockers.end(), blocks.begin(),
                                                        blocks.end());
                                combat_.set_blockers(std::move(current_blockers));
                            }
                        }
                    }
                }

                uint64_t const active_id = turns_.active_player_id();
                auto need_order = combat_.get_attackers_needing_blocker_order();
                for (uint64_t const atk_id : need_order) {
                    auto blocker_ids = combat_.get_blockers_for(atk_id);

                    proto::ActionPrompt order_prompt;
                    order_prompt.set_player_id(active_id);
                    order_prompt.set_prompt_id(std::to_string(broadcaster_.next_sequence()));
                    auto* ob = order_prompt.mutable_order_blockers();
                    ob->set_attacker_id(atk_id);
                    for (auto bid : blocker_ids) {
                        ob->add_unordered_blockers(bid);
                    }

                    proto::GameEvent order_event;
                    order_event.set_game_id(game_id_);
                    *order_event.mutable_action_prompt() = std::move(order_prompt);
                    broadcaster_.emit_to_player(active_id, std::move(order_event));

                    auto* queue = get_action_queue(active_id);
                    if (queue != nullptr) {
                        lock.unlock();
                        if (!wait_for_connected_player(active_id)) {
                            lock.lock();
                            break;
                        }
                        auto response =
                            queue->wait_for(std::chrono::seconds(action_timeout_seconds_));
                        lock.lock();
                        if (response && response->action_type == "order_blockers" &&
                            response->ids.size() == blocker_ids.size()) {
                            auto sorted_orig = blocker_ids;
                            auto sorted_resp = response->ids;
                            std::sort(sorted_orig.begin(), sorted_orig.end());
                            std::sort(sorted_resp.begin(), sorted_resp.end());
                            if (sorted_orig == sorted_resp) {
                                combat_.set_damage_order(atk_id, std::move(response->ids));
                            }
                        }
                    }
                }
                break;
            }
            case Phase::FirstStrikeDamage: {
                if (combat_.get_attackers().empty()) {
                    break;
                }

                bool has_first_strike = false;
                for (const auto& atk : combat_.get_attackers()) {
                    auto* perm = zones_.find_permanent(atk.attacker_id);
                    if ((perm != nullptr) &&
                        (perm->has_keyword("First Strike") || perm->has_keyword("Double Strike"))) {
                        has_first_strike = true;
                        break;
                    }
                }
                if (!has_first_strike) {
                    for (const auto& blk : combat_.state().blockers) {
                        auto* perm = zones_.find_permanent(blk.blocker_id);
                        if ((perm != nullptr) && (perm->has_keyword("First Strike") ||
                                                  perm->has_keyword("Double Strike"))) {
                            has_first_strike = true;
                            break;
                        }
                    }
                }

                if (has_first_strike) {
                    combat_.clear_custom_damage();
                    auto trample_atks = combat_.get_trample_attackers(zones_, true);
                    for (uint64_t const atk_id : trample_atks) {
                        prompt_trample_damage(atk_id, lock);
                    }

                    auto damage = combat_.resolve_combat_damage(zones_, true);
                    for (const auto& dmg : damage) {
                        game_context_->deal_damage(dmg.source_id, dmg.target_id, dmg.amount);
                    }
                    check_state_based_actions();
                }
                break;
            }
            case Phase::CombatDamage: {
                if (combat_.get_attackers().empty()) {
                    break;
                }

                combat_.clear_custom_damage();
                auto trample_atks = combat_.get_trample_attackers(zones_, false);
                for (uint64_t const atk_id : trample_atks) {
                    prompt_trample_damage(atk_id, lock);
                }

                auto damage = combat_.resolve_combat_damage(zones_, false);
                for (const auto& dmg : damage) {
                    game_context_->deal_damage(dmg.source_id, dmg.target_id, dmg.amount);
                }
                check_state_based_actions();
                break;
            }
            case Phase::EndOfCombat:
                combat_.end_combat();
                break;
            default:
                break;
        }
    }

    if (turns_.phase_needs_priority()) {
        run_priority_round();
    }

    {
        std::lock_guard const lock{mutex_};

        for (const auto& player : players_) {
            if (!player.mana_pool().empty() && player.is_alive()) {
                proto::GameEvent mana_event;
                mana_event.set_game_id(game_id_);
                auto* fw = mana_event.mutable_floating_mana_warning();
                fw->set_player_id(player.id());
                auto* mp = fw->mutable_mana();
                mp->set_white(player.mana_pool().white);
                mp->set_blue(player.mana_pool().blue);
                mp->set_black(player.mana_pool().black);
                mp->set_red(player.mana_pool().red);
                mp->set_green(player.mana_pool().green);
                mp->set_colorless(player.mana_pool().colorless);
                broadcaster_.emit_to_player(player.id(), std::move(mana_event));
            }
        }

        check_state_based_actions();
    }
}

void Game::run_cleanup_phase() {
    std::unique_lock lock{mutex_};
    bool repeat = true;
    while (repeat) {
        repeat = false;

        for (auto& [id, perm] : zones_.get_all_permanents()) {
            if (perm.is_animated() && perm.is_animated_until_eot()) {
                perm.remove_animation();
            }
        }

        for (auto& [id, perm] : zones_.get_all_permanents()) {
            perm.clear_damage();
        }

        auto* active = find_player(turns_.active_player_id());
        if (active != nullptr) {
            constexpr int max_hand_size = 7;
            auto& hand = zones_.get_hand(active->id());
            int const excess = static_cast<int>(hand.size()) - max_hand_size;
            if (excess > 0) {
                game_context_->discard_cards(active->id(), excess);
            }
        }

        for (auto& player : players_) {
            player.mana_pool().clear();
        }

        check_state_based_actions();

        if (!stack_.is_empty()) {
            repeat = true;
            lock.unlock();
            run_priority_round();
            lock.lock();
        }
    }
}

void Game::fire_triggers(cle::triggers::TriggerType type,
                         const cle::triggers::TriggerEvent& event) {
    struct PendingTrigger {
        uint64_t controller_id;
        uint64_t source_id;
        sol::function effect;
    };
    std::vector<PendingTrigger> pending;

    for (const auto& [id, perm] : zones_.get_all_permanents()) {
        auto trigger = perm.card()->get_trigger(type);
        if (!trigger || !trigger->valid()) {
            continue;
        }
        pending.push_back({perm.controller_id(), id, *trigger});
    }

    if (pending.empty()) {
        return;
    }

    auto active_id = turns_.active_player_id();
    std::stable_sort(pending.begin(), pending.end(),
                     [&](const PendingTrigger& a, const PendingTrigger& b) {
                         bool const a_active = (a.controller_id == active_id);
                         bool const b_active = (b.controller_id == active_id);
                         if (a_active != b_active) {
                             return a_active;
                         }
                         return a.source_id < b.source_id;
                     });

    auto push_triggers = [&](const std::vector<PendingTrigger>& trigs) {
        for (const auto& trig : trigs) {
            StackEntry entry;
            cle::triggers::TriggerEvent te = event;
            te.source_id = trig.source_id;

            entry.content = TriggeredAbilityEntry{
                .source_id = trig.source_id,
                .type = type,
                .event = te,
                .effect = trig.effect,
            };
            entry.controller_id = trig.controller_id;
            uint64_t stack_id = stack_.push(std::move(entry));

            std::string desc = "Triggered ability";
            auto* src_perm = zones_.find_permanent(trig.source_id);
            if (src_perm && src_perm->card()) {
                desc =
                    src_perm->card()->name() + " - " + cle::triggers::trigger_type_to_string(type);
            }

            proto::GameEvent ge;
            ge.set_game_id(game_id_);
            auto* tf = ge.mutable_trigger_fired();
            tf->set_source_id(trig.source_id);
            tf->set_trigger_type(cle::triggers::trigger_type_to_string(type));
            tf->set_stack_entry_id(stack_id);
            tf->set_description(desc);
            tf->set_controller_id(trig.controller_id);
            broadcaster_.emit(std::move(ge));
        }
    };

    std::unordered_map<uint64_t, std::vector<PendingTrigger>> by_controller;
    for (auto& trig : pending) {
        by_controller[trig.controller_id].push_back(std::move(trig));
    }

    auto process_controller = [&](uint64_t controller_id,
                                  std::vector<PendingTrigger>& controller_trigs) {
        if (controller_trigs.size() <= 1) {
            push_triggers(controller_trigs);
            return;
        }
        if (game_context_ != nullptr) {
            std::vector<int> indices;
            for (size_t i = 0; i < controller_trigs.size(); ++i) {
                indices.push_back(static_cast<int>(i));
            }
            auto chosen = game_context_->choose_mode(
                controller_id, static_cast<int>(indices.size()), static_cast<int>(indices.size()),
                static_cast<int>(indices.size()));
            if (chosen.size() == controller_trigs.size()) {
                std::vector<PendingTrigger> reordered;
                for (int idx : chosen) {
                    if (idx >= 0 && idx < static_cast<int>(controller_trigs.size())) {
                        reordered.push_back(std::move(controller_trigs[idx]));
                    }
                }
                if (reordered.size() == controller_trigs.size()) {
                    push_triggers(reordered);
                    return;
                }
            }
        }
        push_triggers(controller_trigs);
    };

    if (by_controller.contains(active_id)) {
        process_controller(active_id, by_controller[active_id]);
    }
    for (auto& [cid, trigs] : by_controller) {
        if (cid != active_id) {
            process_controller(cid, trigs);
        }
    }

    spdlog::debug("Game {}: {} trigger(s) pushed to stack for {}", game_id_, pending.size(),
                  phase_to_string(turns_.current_phase()));
}

void Game::fire_delayed_triggers(const std::string& event_type, uint64_t source_id) {
    auto it = delayed_triggers_.begin();
    while (it != delayed_triggers_.end()) {
        if (it->event_type != event_type ||
            (it->source_permanent_id != 0 && it->source_permanent_id != source_id)) {
            ++it;
            continue;
        }

        spdlog::debug("Game {}: firing delayed trigger '{}' from permanent {}", game_id_,
                      it->effect_description, it->source_permanent_id);

        if (it->callback && it->callback->valid()) {
            try {
                cle::triggers::TriggerEvent te;
                te.source_id = source_id;
                te.player_id = turns_.active_player_id();
                (*it->callback)(static_cast<cle::game::GameContext&>(*game_context_), te);
            } catch (const std::exception& e) {
                spdlog::error("Game {}: delayed trigger callback error: {}", game_id_, e.what());
            }
        }

        it = delayed_triggers_.erase(it);
    }
}

auto Game::send_priority_prompt(uint64_t player_id) -> PriorityPromptResult {
    auto* player = find_player(player_id);
    if (player == nullptr) {
        return {false, {}};
    }

    proto::ActionPrompt prompt;
    prompt.set_player_id(player_id);
    auto current_prompt_id = std::to_string(broadcaster_.next_sequence());
    prompt.set_prompt_id(current_prompt_id);

    auto* pp = prompt.mutable_priority();
    pp->add_legal_actions("pass");

    bool const can_land = player->can_play_land() && turns_.is_main_phase() && stack_.is_empty();
    pp->set_can_play_land(can_land);

    auto& hand = zones_.get_hand(player_id);
    for (const auto& card : hand) {
        bool const is_land = card->type() == cle::core::CardType::Land;
        if (is_land) {
            if (can_land) {
                pp->add_legal_actions("play_land");
            }
            continue;
        }
        bool const is_instant_speed =
            card->type() == cle::core::CardType::Instant ||
            std::ranges::find(card->keywords(), "Flash") != card->keywords().end();
        if (!is_instant_speed && (!turns_.is_main_phase() || !stack_.is_empty())) {
            continue;
        }

        if (!player->mana_pool().can_pay_cost(card->mana_cost())) {
            continue;
        }
        pp->add_castable_card_ids(card->instance_id());
        pp->add_legal_actions("play_card");

        if (card->adventure()) {
            bool const adv_instant = card->adventure()->type == cle::core::CardType::Instant;
            if (adv_instant || (turns_.is_main_phase() && stack_.is_empty())) {
                if (player->mana_pool().can_pay_cost(card->adventure()->mana_cost)) {
                    pp->add_legal_actions("play_card_adventure");
                }
            }
        }
    }

    for (const auto& card : zones_.get_graveyard(player_id)) {
        if (!card->flashback_cost()) {
            continue;
        }
        auto fb_cost = cle::mana::ManaCost::from_string(*card->flashback_cost());
        if (!fb_cost || !player->mana_pool().can_pay_cost(*fb_cost)) {
            continue;
        }
        bool const is_instant_speed =
            card->type() == cle::core::CardType::Instant ||
            std::ranges::find(card->keywords(), "Flash") != card->keywords().end();
        if (!is_instant_speed && (!turns_.is_main_phase() || !stack_.is_empty())) {
            continue;
        }
        pp->add_castable_card_ids(card->instance_id());
        pp->add_legal_actions("play_card_flashback");
    }

    auto perm_ids = zones_.get_permanents_controlled_by(player_id);
    for (auto perm_id : perm_ids) {
        auto* perm = zones_.find_permanent(perm_id);
        if (perm == nullptr) {
            continue;
        }
        const auto& abilities = perm->card()->activated_abilities();
        bool has_activatable = false;
        for (const auto& ability : abilities) {
            if (ability.sorcery_speed_only && !ability.is_mana_ability) {
                if (!turns_.is_main_phase() || !stack_.is_empty()) {
                    continue;
                }
            }
            bool const requires_tap = ability.cost_text.find("{T}") != std::string::npos;
            if (requires_tap) {
                if (perm->is_tapped()) {
                    continue;
                }
                if (perm->is_creature() && perm->has_summoning_sickness() &&
                    !perm->has_keyword("Haste")) {
                    continue;
                }
            }
            std::string mana_portion = ability.cost_text;
            for (auto pos = mana_portion.find("{T}"); pos != std::string::npos;
                 pos = mana_portion.find("{T}")) {
                mana_portion.erase(pos, 3);
            }
            std::erase(mana_portion, ',');
            if (!mana_portion.empty()) {
                auto parsed_cost = cle::mana::ManaCost::from_string(mana_portion);
                if (parsed_cost && !player->mana_pool().can_pay_cost(*parsed_cost)) {
                    continue;
                }
            }
            has_activatable = true;
            break;
        }
        if (has_activatable) {
            pp->add_activatable_permanent_ids(perm_id);
            pp->add_legal_actions("activate_ability");
        }
    }

    pp->add_legal_actions("concede");

    bool has_meaningful_actions =
        pp->castable_card_ids_size() > 0 || pp->activatable_permanent_ids_size() > 0 || can_land;

    {
        proto::GameEvent pce;
        pce.set_game_id(game_id_);
        pce.mutable_priority_changed()->set_player_id(player_id);
        broadcaster_.emit(std::move(pce));
    }

    proto::GameEvent event;
    event.set_game_id(game_id_);
    *event.mutable_action_prompt() = std::move(prompt);
    broadcaster_.emit_to_player(player_id, std::move(event));

    return {has_meaningful_actions, current_prompt_id};
}

void Game::process_action(uint64_t player_id, const ActionData& action) {
    if (action.action_type == "play_card") {
        auto card = zones_.remove_from_hand(player_id, action.target_id);
        if (!card) {
            spdlog::warn("Player {} tried to play card {} not in hand", player_id,
                         action.target_id);
            return;
        }

        bool const is_instant = card->type() == cle::core::CardType::Instant;
        bool const has_flash =
            std::ranges::find(card->keywords(), "Flash") != card->keywords().end();
        if (!is_instant && !has_flash) {
            if (!turns_.is_main_phase() || !stack_.is_empty()) {
                spdlog::warn("Player {} tried to cast sorcery-speed {} at illegal time", player_id,
                             card->name());
                zones_.get_hand(player_id).push_back(card);
                return;
            }
        }

        auto* player = find_player(player_id);
        if (player == nullptr) {
            zones_.get_hand(player_id).push_back(card);
            return;
        }

        auto mana_cost = card->mana_cost();
        int x_value = 0;
        if (mana_cost.x_count > 0) {
            x_value = std::max(0, action.x_value);
            mana_cost.colorless += x_value * mana_cost.x_count;
            mana_cost.x_count = 0;
        }

        bool wants_kick = action.flag && card->kicker_cost().has_value();
        if (wants_kick) {
            auto kicker_parsed = cle::mana::ManaCost::from_string(*card->kicker_cost());
            if (kicker_parsed) {
                mana_cost.white += kicker_parsed->white;
                mana_cost.blue += kicker_parsed->blue;
                mana_cost.black += kicker_parsed->black;
                mana_cost.red += kicker_parsed->red;
                mana_cost.green += kicker_parsed->green;
                mana_cost.colorless += kicker_parsed->colorless;
            } else {
                wants_kick = false;
            }
        }

        if (std::ranges::find(card->keywords(), "Convoke") != card->keywords().end()) {
            for (uint64_t cid : action.convoke_ids) {
                auto* cperm = zones_.find_permanent(cid);
                if (cperm == nullptr || !cperm->is_creature()) {
                    continue;
                }
                if (cperm->controller_id() != player_id || cperm->is_tapped()) {
                    continue;
                }
                cperm->tap();

                auto creature_colors = cperm->card()->colors();
                bool reduced_colored = false;
                if ((creature_colors & cle::core::Color::White) != cle::core::Color::Colorless &&
                    mana_cost.white > 0) {
                    --mana_cost.white;
                    reduced_colored = true;
                } else if ((creature_colors & cle::core::Color::Blue) !=
                               cle::core::Color::Colorless &&
                           mana_cost.blue > 0) {
                    --mana_cost.blue;
                    reduced_colored = true;
                } else if ((creature_colors & cle::core::Color::Black) !=
                               cle::core::Color::Colorless &&
                           mana_cost.black > 0) {
                    --mana_cost.black;
                    reduced_colored = true;
                } else if ((creature_colors & cle::core::Color::Red) !=
                               cle::core::Color::Colorless &&
                           mana_cost.red > 0) {
                    --mana_cost.red;
                    reduced_colored = true;
                } else if ((creature_colors & cle::core::Color::Green) !=
                               cle::core::Color::Colorless &&
                           mana_cost.green > 0) {
                    --mana_cost.green;
                    reduced_colored = true;
                }
                if (!reduced_colored && mana_cost.colorless > 0) {
                    --mana_cost.colorless;
                }
            }
        }

        if (std::ranges::find(card->keywords(), "Delve") != card->keywords().end() &&
            action.delve_count > 0) {
            int delved = 0;
            auto& gy = zones_.get_graveyard(player_id);
            int to_delve = std::min(action.delve_count, static_cast<int>(gy.size()));
            to_delve = std::min(to_delve, mana_cost.colorless);
            for (int i = 0; i < to_delve; ++i) {
                if (!gy.empty()) {
                    auto exiled = gy.back();
                    gy.pop_back();
                    zones_.add_to_exile(exiled, player_id);
                    ++delved;
                }
            }
            mana_cost.colorless = std::max(0, mana_cost.colorless - delved);
        }

        bool const has_cost = mana_cost.mana_value() > 0 || !mana_cost.hybrid_costs.empty();

        if (has_cost) {
            auto_tap_lands(player_id, mana_cost);
            mana_undo_stacks_[player_id].clear();

            auto auto_payment = player->mana_pool().compute_optimal_payment(mana_cost);
            if (!auto_payment) {
                zones_.get_hand(player_id).push_back(card);
                spdlog::warn("Player {} cannot pay mana for {}", player_id, card->name());
                return;
            }
            if (!player->mana_pool().pay_cost(*auto_payment)) {
                zones_.get_hand(player_id).push_back(card);
                spdlog::warn("Player {} insufficient mana for {}", player_id, card->name());
                return;
            }
            spdlog::debug("Player {} auto-paid mana for {} (X={}, kicked={})", player_id,
                          card->name(), x_value, wants_kick);
        }

        auto card_name = card->name();
        StackEntry entry;
        entry.content = SpellEntry{.card = card};
        entry.controller_id = player_id;
        entry.targets = action.ids;
        entry.x_value = x_value;
        entry.kicked = wants_kick;
        auto stack_id = stack_.push(std::move(entry));

        proto::GameEvent event;
        event.set_game_id(game_id_);
        auto* cp = event.mutable_card_played();
        cp->set_player_id(player_id);
        *cp->mutable_card() = cle::serialization::serialize_card(*card);
        cp->set_stack_entry_id(stack_id);
        broadcaster_.emit(std::move(event));

        emit_game_log(find_player(player_id)->username() + " casts " + card_name, player_id,
                      "spell");
        spdlog::debug("Player {} cast {}", player_id, card_name);
        priority_.interrupt();

    } else if (action.action_type == "play_card_flashback") {
        auto card = zones_.remove_from_graveyard(player_id, action.target_id);
        if (!card) {
            spdlog::warn("Player {} tried to flashback card {} not in graveyard", player_id,
                         action.target_id);
            return;
        }
        if (!card->flashback_cost()) {
            spdlog::warn("Player {} tried to flashback {} which has no flashback", player_id,
                         card->name());
            zones_.add_to_graveyard(player_id, card);
            return;
        }

        auto* player = find_player(player_id);
        if (player == nullptr) {
            zones_.add_to_graveyard(player_id, card);
            return;
        }

        auto fb_cost = cle::mana::ManaCost::from_string(*card->flashback_cost());
        if (!fb_cost) {
            zones_.add_to_graveyard(player_id, card);
            return;
        }
        auto_tap_lands(player_id, *fb_cost);
        mana_undo_stacks_[player_id].clear();

        auto auto_payment = player->mana_pool().compute_optimal_payment(*fb_cost);
        if (!auto_payment) {
            zones_.add_to_graveyard(player_id, card);
            spdlog::warn("Player {} cannot pay flashback cost for {}", player_id, card->name());
            return;
        }
        if (!player->mana_pool().pay_cost(*auto_payment)) {
            zones_.add_to_graveyard(player_id, card);
            return;
        }

        auto card_name = card->name();
        StackEntry entry;
        entry.content = SpellEntry{.card = card};
        entry.controller_id = player_id;
        entry.targets = action.ids;
        entry.flashback = true;
        auto stack_id = stack_.push(std::move(entry));

        proto::GameEvent event;
        event.set_game_id(game_id_);
        auto* cp = event.mutable_card_played();
        cp->set_player_id(player_id);
        *cp->mutable_card() = cle::serialization::serialize_card(*card);
        cp->set_stack_entry_id(stack_id);
        broadcaster_.emit(std::move(event));

        spdlog::debug("Player {} cast {} via flashback", player_id, card_name);
        priority_.interrupt();

    } else if (action.action_type == "play_card_adventure") {
        auto card = zones_.remove_from_hand(player_id, action.target_id);
        if (!card || !card->adventure()) {
            if (card) {
                zones_.get_hand(player_id).push_back(card);
            }
            return;
        }

        auto* player = find_player(player_id);
        if (player == nullptr) {
            zones_.get_hand(player_id).push_back(card);
            return;
        }

        auto auto_payment =
            player->mana_pool().compute_optimal_payment(card->adventure()->mana_cost);
        if (!auto_payment || !player->mana_pool().pay_cost(*auto_payment)) {
            zones_.get_hand(player_id).push_back(card);
            return;
        }

        auto card_name = card->name();
        StackEntry entry;
        entry.content = SpellEntry{.card = card};
        entry.controller_id = player_id;
        entry.targets = action.ids;
        entry.adventure = true;
        auto stack_id = stack_.push(std::move(entry));

        proto::GameEvent event;
        event.set_game_id(game_id_);
        auto* cp = event.mutable_card_played();
        cp->set_player_id(player_id);
        *cp->mutable_card() = cle::serialization::serialize_card(*card);
        cp->set_stack_entry_id(stack_id);
        broadcaster_.emit(std::move(event));

        spdlog::debug("Player {} cast {} (adventure)", player_id, card_name);
        priority_.interrupt();

    } else if (action.action_type == "play_land") {
        auto* player = find_player(player_id);
        if ((player == nullptr) || !player->can_play_land()) {
            return;
        }
        if (!turns_.is_main_phase() || !stack_.is_empty()) {
            spdlog::warn("Player {} tried to play land outside main phase or with non-empty stack",
                         player_id);
            return;
        }

        auto card = zones_.remove_from_hand(player_id, action.target_id);
        if (!card) {
            return;
        }

        if (card->type() != cle::core::CardType::Land) {
            spdlog::warn("Player {} tried to play non-land {} as land", player_id, card->name());
            zones_.get_hand(player_id).push_back(card);
            return;
        }

        auto card_name = card->name();
        auto perm_id = zones_.add_to_battlefield(card, player_id, player_id);
        player->play_land();

        auto* perm = zones_.find_permanent(perm_id);
        if (perm != nullptr) {
            perm->set_summoning_sickness(false);
        }

        proto::GameEvent event;
        event.set_game_id(game_id_);
        auto* cp = event.mutable_card_played();
        cp->set_player_id(player_id);
        *cp->mutable_card() = cle::serialization::serialize_card(*card);
        broadcaster_.emit(std::move(event));

        {
            proto::GameEvent perm_event;
            perm_event.set_game_id(game_id_);
            auto* pe = perm_event.mutable_permanent_entered();
            pe->set_permanent_id(perm_id);
            pe->set_controller_id(player_id);
            *pe->mutable_card() = cle::serialization::serialize_card(*card);
            pe->set_tapped(false);
            pe->set_is_token(false);
            broadcaster_.emit(std::move(perm_event));
        }

        {
            cle::triggers::TriggerEvent lf_event;
            lf_event.type = cle::triggers::TriggerType::OnLandfall;
            lf_event.source_id = perm_id;
            lf_event.player_id = player_id;
            fire_triggers(cle::triggers::TriggerType::OnLandfall, lf_event);
        }

        emit_game_log(find_player(player_id)->username() + " plays " + card_name, player_id,
                      "land");
        spdlog::debug("Player {} played land {}", player_id, card_name);

    } else if (action.action_type == "activate_ability") {
        auto* perm = zones_.find_permanent(action.target_id);
        if (perm == nullptr) {
            return;
        }
        if (perm->controller_id() != player_id) {
            spdlog::warn("Player {} tried to activate ability on permanent {} they don't control",
                         player_id, action.target_id);
            return;
        }

        int const ability_idx = action.indices.empty() ? 0 : action.indices[0];
        const auto& abilities = perm->card()->activated_abilities();
        if (ability_idx < 0 || static_cast<size_t>(ability_idx) >= abilities.size()) {
            return;
        }

        auto aidx = static_cast<size_t>(ability_idx);
        const auto& ability = abilities[aidx];

        if (ability.sorcery_speed_only && !ability.is_mana_ability) {
            if (!turns_.is_main_phase() || !stack_.is_empty()) {
                spdlog::warn("Player {} tried to activate sorcery-speed ability outside main phase",
                             player_id);
                return;
            }
        }

        bool const requires_tap = ability.cost_text.find("{T}") != std::string::npos;
        if (requires_tap) {
            if (perm->is_tapped()) {
                return;
            }
            if (perm->is_creature() && perm->has_summoning_sickness() &&
                !perm->has_keyword("Haste")) {
                spdlog::warn("Player {} tried to tap creature {} with summoning sickness",
                             player_id, action.target_id);
                return;
            }
        }

        auto* player = find_player(player_id);
        if (player == nullptr) {
            return;
        }
        std::string mana_portion;
        {
            std::string ct = ability.cost_text;
            for (auto pos = ct.find("{T}"); pos != std::string::npos; pos = ct.find("{T}")) {
                ct.erase(pos, 3);
            }
            std::erase(ct, ',');
            mana_portion = ct;
        }
        if (!mana_portion.empty()) {
            auto parsed_cost = cle::mana::ManaCost::from_string(mana_portion);
            if (!parsed_cost) {
                spdlog::warn("Failed to parse mana cost '{}' for ability on permanent {}",
                             mana_portion, action.target_id);
                return;
            }
            auto_tap_lands(player_id, *parsed_cost);
            mana_undo_stacks_[player_id].clear();

            auto payment = player->mana_pool().compute_optimal_payment(*parsed_cost);
            if (!payment) {
                spdlog::warn("Player {} cannot pay mana cost '{}' for ability on permanent {}",
                             player_id, mana_portion, action.target_id);
                return;
            }
            if (!player->mana_pool().pay_cost(*payment)) {
                return;
            }
        }

        if (requires_tap) {
            perm->tap();
        }

        if (ability.is_mana_ability) {
            if (ability.effect.valid()) {
                try {
                    cle::triggers::TriggerEvent evt;
                    evt.player_id = player_id;
                    evt.source_id = action.target_id;
                    ability.effect(static_cast<cle::game::GameContext&>(*game_context_), evt);
                } catch (const std::exception& e) {
                    spdlog::error("Mana ability error on permanent {}: {}", action.target_id,
                                  e.what());
                }
            }
            spdlog::debug("Mana ability resolved immediately on permanent {}", action.target_id);
            return;
        }

        StackEntry entry;
        entry.content = ActivatedAbilityEntry{
            .source_permanent_id = action.target_id,
            .ability_index = ability_idx,
            .effect = ability.effect,
        };
        entry.controller_id = player_id;
        entry.targets = action.ids;
        auto stack_id = stack_.push(std::move(entry));

        proto::GameEvent event;
        event.set_game_id(game_id_);
        auto* aa = event.mutable_ability_activated();
        aa->set_permanent_id(action.target_id);
        aa->set_ability_text(ability.effect_text);
        aa->set_stack_entry_id(stack_id);
        broadcaster_.emit(std::move(event));

        priority_.interrupt();

    } else if (action.action_type == "pay_mana") {
    } else if (action.action_type == "undo") {
        pop_mana_undo(player_id);
    } else if (action.action_type == "sideboard_swap") {
        auto* player = find_player(player_id);
        if (player == nullptr) {
            return;
        }
        auto& sb = player->sideboard();
        auto& hand = zones_.get_hand(player_id);

        std::vector<std::shared_ptr<cle::core::Card>> cards_out;
        for (uint64_t card_id : action.ids) {
            auto card = zones_.remove_from_hand(player_id, card_id);
            if (card) {
                cards_out.push_back(card);
            }
        }

        std::vector<std::string> names_in;
        {
            std::istringstream iss(action.text);
            std::string name;
            while (std::getline(iss, name, ',')) {
                if (!name.empty()) {
                    names_in.push_back(name);
                }
            }
        }

        if (names_in.size() != cards_out.size()) {
            for (auto& c : cards_out) {
                hand.push_back(c);
            }
            spdlog::warn("Player {} sideboard swap count mismatch: {} out, {} in", player_id,
                         cards_out.size(), names_in.size());
            return;
        }

        std::vector<std::shared_ptr<cle::core::Card>> cards_in;
        for (const auto& name : names_in) {
            auto it = std::ranges::find_if(sb, [&](const auto& c) { return c->name() == name; });
            if (it != sb.end()) {
                cards_in.push_back(*it);
                sb.erase(it);
            }
        }

        if (cards_in.size() != cards_out.size()) {
            for (auto& c : cards_out) {
                hand.push_back(c);
            }
            for (auto& c : cards_in) {
                sb.push_back(c);
            }
            spdlog::warn("Player {} sideboard swap failed: not all cards found", player_id);
            return;
        }

        for (auto& c : cards_out) {
            sb.push_back(c);
        }
        for (auto& c : cards_in) {
            hand.push_back(c);
        }

        spdlog::info("Player {} swapped {} cards with sideboard", player_id, cards_out.size());
    } else if (action.action_type == "yes_no") {
        if (pending_concede_.find(player_id) != pending_concede_.end()) {
            if (action.flag) {
                ActionData concede_action;
                concede_action.player_id = player_id;
                concede_action.action_type = "concede";
                process_action(player_id, concede_action);
            } else {
                pending_concede_.erase(player_id);
            }
            return;
        }
    } else if (action.action_type == "concede_request") {
        pending_concede_.insert(player_id);
        proto::GameEvent event;
        event.set_game_id(game_id_);
        auto* prompt = event.mutable_action_prompt();
        prompt->set_player_id(player_id);
        prompt->set_prompt_id(std::to_string(broadcaster_.next_sequence()));
        prompt->mutable_concede_confirm()->set_message("Are you sure you want to concede?");
        broadcaster_.emit_to_player(player_id, std::move(event));
    } else if (action.action_type == "concede_cancel") {
        pending_concede_.erase(player_id);
    } else if (action.action_type == "concede") {
        bool const forced = action.text == "forced";
        if (!forced && pending_concede_.find(player_id) == pending_concede_.end()) {
            spdlog::debug("Player {} sent concede without confirmation, ignoring", player_id);
            return;
        }
        pending_concede_.erase(player_id);
        auto* player = find_player(player_id);
        if (player != nullptr) {
            player->eliminate();
            emit_game_log(player->username() + " concedes", player_id, "concede");
            spdlog::info("Player {} ({}) conceded", player_id, player->username());

            auto win = win_checker_.check(*this);
            if (win) {
                result_ = *win;
                state_.store(GameState::Finished);
                running_ = false;

                proto::GameEvent go_event;
                go_event.set_game_id(game_id_);
                auto* go = go_event.mutable_game_over();
                go->set_winner_id(win->winner_id);
                go->set_reason(win->reason);
                go->set_is_draw(win->is_draw);
                broadcaster_.emit(std::move(go_event));

                spdlog::info("Game {} ended: {} (winner: {})", game_id_, win->reason,
                             win->winner_id);
            }
        }

    } else if (action.action_type == "draw_offer") {
        if (pending_draw_offer_from_ != 0) {
            spdlog::debug("Player {} draw offer ignored, one already pending from {}", player_id,
                          pending_draw_offer_from_);
            return;
        }
        pending_draw_offer_from_ = player_id;
        spdlog::info("Game {}: player {} offered a draw", game_id_, player_id);

        proto::GameEvent event;
        event.set_game_id(game_id_);
        auto* offer = event.mutable_draw_offer();
        offer->set_from_player_id(player_id);
        broadcaster_.emit(std::move(event));

    } else if (action.action_type == "draw_response") {
        if (pending_draw_offer_from_ == 0 || pending_draw_offer_from_ == player_id) {
            spdlog::debug("Player {} draw response ignored, no pending offer from opponent",
                          player_id);
            return;
        }

        if (action.flag) {
            result_ = GameOverResult{
                .winner_id = 0, .reason = "Draw agreed by mutual consent", .is_draw = true};
            state_.store(GameState::Finished);
            running_ = false;

            proto::GameEvent go_event;
            go_event.set_game_id(game_id_);
            auto* go = go_event.mutable_game_over();
            go->set_winner_id(0);
            go->set_reason("Draw agreed by mutual consent");
            go->set_is_draw(true);
            broadcaster_.emit(std::move(go_event));

            spdlog::info("Game {} ended in agreed draw", game_id_);
        } else {
            pending_draw_offer_from_ = 0;

            proto::GameEvent event;
            event.set_game_id(game_id_);
            auto* declined = event.mutable_draw_declined();
            declined->set_by_player_id(player_id);
            broadcaster_.emit(std::move(event));

            spdlog::info("Game {}: player {} declined draw offer", game_id_, player_id);
        }

    } else if (action.action_type == "set_auto_pass") {
        int mode_val = action.x_value;
        if (mode_val >= 0 && mode_val <= 3) {
            auto_pass_modes_[player_id] = static_cast<AutoPassMode>(mode_val);
            spdlog::debug("Game {}: player {} set auto-pass mode to {}", game_id_, player_id,
                          mode_val);
        }
    }
}

void Game::resolve_stack_entry(StackEntry entry) {
    auto entry_id = entry.id;

    if (!entry.targets.empty()) {
        std::vector<uint64_t> still_legal;
        if (!entry.target_filter.empty()) {
            auto filter = parse_filter(entry.target_filter);
            auto legal = apply_filter(filter, entry.controller_id, zones_, players_);
            for (uint64_t const t : entry.targets) {
                if (std::find(legal.begin(), legal.end(), t) != legal.end()) {
                    still_legal.push_back(t);
                }
            }
        } else {
            for (uint64_t const t : entry.targets) {
                if ((zones_.find_permanent(t) != nullptr) || (find_player(t) != nullptr)) {
                    still_legal.push_back(t);
                }
            }
        }
        if (still_legal.empty()) {
            spdlog::debug("Stack entry {} fizzled all targets illegal", entry_id);
            if (auto* spell = std::get_if<SpellEntry>(&entry.content)) {
                zones_.add_to_graveyard(entry.controller_id, spell->card);
            }
            proto::GameEvent event;
            event.set_game_id(game_id_);
            auto* sr = event.mutable_spell_resolved();
            sr->set_stack_entry_id(entry_id);
            sr->set_card_name("fizzled");
            broadcaster_.emit(std::move(event));
            return;
        }
        entry.targets = std::move(still_legal);
    }

    std::visit(
        [&](auto& content) {
            using T = std::decay_t<decltype(content)>;

            if constexpr (std::is_same_v<T, SpellEntry>) {
                auto& card = content.card;
                auto card_name = card->name();
                auto card_type = card->type();

                if (card_type == cle::core::CardType::Creature ||
                    card_type == cle::core::CardType::Enchantment ||
                    card_type == cle::core::CardType::Artifact ||
                    card_type == cle::core::CardType::Planeswalker) {
                    auto owner_id = zones_.find_card_owner(card->instance_id());
                    if (owner_id == 0) {
                        owner_id = entry.controller_id;
                    }
                    auto perm_id = zones_.add_to_battlefield(card, entry.controller_id, owner_id);
                    spdlog::debug("Spell {} resolved -> permanent {}", card_name, perm_id);

                    if (card_type == cle::core::CardType::Enchantment && !entry.targets.empty()) {
                        auto* aura_perm = zones_.find_permanent(perm_id);
                        auto* target_perm = zones_.find_permanent(entry.targets[0]);
                        if (aura_perm != nullptr && target_perm != nullptr) {
                            aura_perm->attach_to(entry.targets[0]);
                            target_perm->add_attachment(perm_id);
                        }
                    }

                    {
                        proto::GameEvent perm_event;
                        perm_event.set_game_id(game_id_);
                        auto* pe = perm_event.mutable_permanent_entered();
                        pe->set_permanent_id(perm_id);
                        pe->set_controller_id(entry.controller_id);
                        *pe->mutable_card() = cle::serialization::serialize_card(*card);
                        pe->set_tapped(false);
                        pe->set_is_token(false);
                        broadcaster_.emit(std::move(perm_event));
                    }

                    auto etb = card->get_trigger(cle::triggers::TriggerType::OnEnterBattlefield);
                    if (etb && etb->valid()) {
                        cle::triggers::TriggerEvent te;
                        te.type = cle::triggers::TriggerType::OnEnterBattlefield;
                        te.player_id = entry.controller_id;
                        te.source_id = perm_id;
                        te.amount = entry.x_value;
                        te.extra_data = entry.kicked ? "kicked" : "";

                        StackEntry etb_stack_entry;
                        etb_stack_entry.content = TriggeredAbilityEntry{
                            .source_id = perm_id,
                            .type = cle::triggers::TriggerType::OnEnterBattlefield,
                            .event = te,
                            .effect = *etb,
                        };
                        etb_stack_entry.controller_id = entry.controller_id;
                        uint64_t etb_stack_id = stack_.push(std::move(etb_stack_entry));

                        proto::GameEvent tfe;
                        tfe.set_game_id(game_id_);
                        auto* tf = tfe.mutable_trigger_fired();
                        tf->set_source_id(perm_id);
                        tf->set_trigger_type(cle::triggers::trigger_type_to_string(
                            cle::triggers::TriggerType::OnEnterBattlefield));
                        tf->set_stack_entry_id(etb_stack_id);
                        tf->set_description(card_name + " enters the battlefield");
                        tf->set_controller_id(entry.controller_id);
                        broadcaster_.emit(std::move(tfe));
                    }

                    {
                        cle::triggers::TriggerEvent etb_event;
                        etb_event.source_id = perm_id;
                        etb_event.player_id = entry.controller_id;

                        if (card_type == cle::core::CardType::Creature) {
                            etb_event.type = cle::triggers::TriggerType::OnAnotherCreatureEnters;
                            fire_triggers(cle::triggers::TriggerType::OnAnotherCreatureEnters,
                                          etb_event);
                        }
                        if (card_type == cle::core::CardType::Artifact) {
                            etb_event.type = cle::triggers::TriggerType::OnArtifactEnters;
                            fire_triggers(cle::triggers::TriggerType::OnArtifactEnters, etb_event);
                        }
                        if (card_type == cle::core::CardType::Enchantment) {
                            etb_event.type = cle::triggers::TriggerType::OnEnchantmentEnters;
                            fire_triggers(cle::triggers::TriggerType::OnEnchantmentEnters,
                                          etb_event);
                        }
                    }
                } else {
                    if (entry.adventure && card->adventure() && card->adventure()->effect.valid()) {
                        try {
                            cle::triggers::TriggerEvent te;
                            te.type = cle::triggers::TriggerType::OnCast;
                            te.player_id = entry.controller_id;
                            te.source_id = card->instance_id();
                            te.target_id = entry.targets.empty() ? 0 : entry.targets[0];
                            card->adventure()->effect(
                                static_cast<cle::game::GameContext&>(*game_context_), te);
                        } catch (const std::exception& e) {
                            spdlog::error("Adventure effect error for {}: {}", card_name, e.what());
                        }
                        zones_.add_to_exile(card, entry.controller_id);
                    } else {
                        auto on_cast = card->get_trigger(cle::triggers::TriggerType::OnCast);
                        if (on_cast && on_cast->valid()) {
                            try {
                                cle::triggers::TriggerEvent te;
                                te.type = cle::triggers::TriggerType::OnCast;
                                te.player_id = entry.controller_id;
                                te.source_id = card->instance_id();
                                te.target_id = entry.targets.empty() ? 0 : entry.targets[0];
                                te.amount = entry.x_value;
                                te.extra_data = entry.kicked ? "kicked" : "";
                                (*on_cast)(static_cast<cle::game::GameContext&>(*game_context_),
                                           te);
                            } catch (const std::exception& e) {
                                spdlog::error("Spell effect error for {}: {}", card_name, e.what());
                            }
                        }
                        if (entry.flashback) {
                            zones_.add_to_exile(card, entry.controller_id);
                        } else {
                            zones_.add_to_graveyard(entry.controller_id, card);
                        }
                    }
                }

                proto::GameEvent event;
                event.set_game_id(game_id_);
                auto* sr = event.mutable_spell_resolved();
                sr->set_stack_entry_id(entry_id);
                sr->set_card_name(card_name);
                broadcaster_.emit(std::move(event));

            } else if constexpr (std::is_same_v<T, ActivatedAbilityEntry>) {
                if (content.effect.valid()) {
                    try {
                        cle::triggers::TriggerEvent evt;
                        evt.player_id = entry.controller_id;
                        evt.source_id = content.source_permanent_id;
                        content.effect(static_cast<cle::game::GameContext&>(*game_context_), evt);
                    } catch (const std::exception& e) {
                        spdlog::error("Activated ability error: {}", e.what());
                    }
                }

                proto::GameEvent event;
                event.set_game_id(game_id_);
                auto* sr = event.mutable_spell_resolved();
                sr->set_stack_entry_id(entry_id);
                sr->set_card_name("activated ability");
                broadcaster_.emit(std::move(event));

            } else if constexpr (std::is_same_v<T, TriggeredAbilityEntry>) {
                if (content.effect.valid()) {
                    try {
                        content.effect(static_cast<cle::game::GameContext&>(*game_context_),
                                       content.event);
                    } catch (const std::exception& e) {
                        spdlog::error("Triggered ability error: {}", e.what());
                    }
                }

                proto::GameEvent event;
                event.set_game_id(game_id_);
                auto* sr = event.mutable_spell_resolved();
                sr->set_stack_entry_id(entry_id);
                sr->set_card_name("triggered ability");
                broadcaster_.emit(std::move(event));
            }
        },
        entry.content);
}

void Game::run_priority_round() {
    {
        std::lock_guard const lock{mutex_};
        priority_.begin_round(turns_.active_player_id());
    }

    for (;;) {
        while (running_ && !priority_.all_passed()) {
            uint64_t holder;
            std::shared_ptr<ActionQueue> queue;
            std::string current_prompt_id;
            {
                std::lock_guard const lock{mutex_};
                holder = priority_.current_priority_holder();
                auto it = action_queues_.find(holder);
                if (it == action_queues_.end()) {
                    priority_.pass();
                    continue;
                }
                queue = it->second;

                AutoPassMode mode = get_auto_pass(holder);
                if (mode == AutoPassMode::UntilEndOfTurn) {
                    spdlog::debug("Game {}: auto-passing for player {} (until end of turn)",
                                  game_id_, holder);
                    priority_.pass();
                    continue;
                }

                if (mode == AutoPassMode::NoActions) {
                    auto* player = find_player(holder);
                    if (player != nullptr) {
                        bool const can_land =
                            player->can_play_land() && turns_.is_main_phase() && stack_.is_empty();
                        bool has_castable = false;
                        for (const auto& card : zones_.get_hand(holder)) {
                            if (card->type() == cle::core::CardType::Land) {
                                continue;
                            }
                            bool const is_instant_speed =
                                card->type() == cle::core::CardType::Instant ||
                                std::ranges::find(card->keywords(), "Flash") !=
                                    card->keywords().end();
                            if (!is_instant_speed &&
                                (!turns_.is_main_phase() || !stack_.is_empty())) {
                                continue;
                            }
                            if (player->mana_pool().can_pay_cost(card->mana_cost())) {
                                has_castable = true;
                                break;
                            }
                        }
                        bool has_activatable = false;
                        for (auto perm_id : zones_.get_permanents_controlled_by(holder)) {
                            auto* perm = zones_.find_permanent(perm_id);
                            if (perm != nullptr && !perm->card()->activated_abilities().empty()) {
                                has_activatable = true;
                                break;
                            }
                        }
                        if (!can_land && !has_castable && !has_activatable) {
                            spdlog::debug("Game {}: auto-passing for player {} (no actions)",
                                          game_id_, holder);
                            priority_.pass();
                            continue;
                        }
                    }
                }

                if (pending_concede_.find(holder) != pending_concede_.end()) {
                    current_prompt_id.clear();
                } else {
                    auto prompt_result = send_priority_prompt(holder);
                    current_prompt_id = std::move(prompt_result.prompt_id);
                }
            }

            if (!wait_for_connected_player(holder)) {
                break;
            }

            if (clock_.is_enabled()) {
                clock_.start_clock(holder);
            }

            auto wait_timeout = std::chrono::seconds(action_timeout_seconds_);
            if (clock_.is_enabled()) {
                clock_.update();
                auto remaining = clock_.remaining_time(holder);
                if (remaining < wait_timeout) {
                    wait_timeout = std::chrono::duration_cast<std::chrono::seconds>(remaining) +
                                   std::chrono::seconds{1};
                }
            }

            bool rope_sent = false;
            constexpr auto rope_threshold = std::chrono::seconds(15);

            std::optional<ActionData> action;
            constexpr int max_stale_retries = 8;
            auto deadline = std::chrono::steady_clock::now() + wait_timeout;
            auto rope_deadline =
                std::chrono::steady_clock::now() +
                (wait_timeout > rope_threshold ? wait_timeout - rope_threshold : wait_timeout);

            for (int attempt = 0; attempt < max_stale_retries; ++attempt) {
                while (!action && running_ && std::chrono::steady_clock::now() < deadline) {
                    auto gen_before = shared_notify_->generation.load(std::memory_order_acquire);

                    action = queue->try_take();

                    if (!action) {
                        std::lock_guard const poll_lock{mutex_};
                        for (auto& [pid, pqueue] : action_queues_) {
                            if (pid == holder)
                                continue;
                            if (pending_draw_offer_from_ == 0) {
                                if (auto a = pqueue->try_take_if("draw_offer"))
                                    process_action(pid, *a);
                            }
                            if (pending_draw_offer_from_ != 0) {
                                if (auto a = pqueue->try_take_if("draw_response"))
                                    process_action(pid, *a);
                            }
                            if (auto a = pqueue->try_take_if("concede_request"))
                                process_action(pid, *a);
                            if (pending_concede_.find(pid) != pending_concede_.end()) {
                                if (auto a = pqueue->try_take_if("yes_no"))
                                    process_action(pid, *a);
                            }
                        }
                        if (!running_)
                            break;
                    }

                    if (!rope_sent && std::chrono::steady_clock::now() >= rope_deadline) {
                        rope_sent = true;
                        proto::GameEvent rope_event;
                        rope_event.set_game_id(game_id_);
                        auto* rw = rope_event.mutable_rope_warning();
                        rw->set_player_id(holder);
                        rw->set_seconds_remaining(static_cast<int32_t>(rope_threshold.count()));
                        broadcaster_.emit(std::move(rope_event));
                    }

                    if (!action && running_) {
                        auto remaining = deadline - std::chrono::steady_clock::now();
                        if (remaining <= std::chrono::milliseconds(0))
                            break;
                        std::unique_lock sn_lock{shared_notify_->mutex};
                        shared_notify_->cv.wait_for(sn_lock, remaining, [&] {
                            return shared_notify_->generation.load(std::memory_order_acquire) !=
                                   gen_before;
                        });
                    }
                }

                if (!action || !running_) {
                    break;
                }
                if (action->action_type == "concede" || action->action_type == "concede_request" ||
                    action->action_type == "shutdown" || action->action_type == "set_auto_pass" ||
                    action->action_type == "draw_offer" || action->action_type == "draw_response") {
                    break;
                }
                if (action->action_type == "yes_no" &&
                    pending_concede_.find(holder) != pending_concede_.end()) {
                    break;
                }
                if (!action->prompt_id.empty() && action->prompt_id == current_prompt_id) {
                    break;
                }
                spdlog::debug("Game {}: player {} sent stale action (prompt_id={}, expected={})",
                              game_id_, holder, action->prompt_id, current_prompt_id);
                action.reset();
            }

            if (clock_.is_enabled()) {
                clock_.stop_clock();
                if (clock_.is_expired(holder)) {
                    std::lock_guard const lock{mutex_};
                    auto* player = find_player(holder);
                    if ((player != nullptr) && player->is_alive()) {
                        player->eliminate();
                        spdlog::info("Game {}: player {} lost on time", game_id_, holder);
                        auto win = win_checker_.check(*this);
                        if (win) {
                            result_ = *win;
                            state_.store(GameState::Finished);
                            running_ = false;
                            proto::GameEvent go_event;
                            go_event.set_game_id(game_id_);
                            auto* go = go_event.mutable_game_over();
                            go->set_winner_id(win->winner_id);
                            go->set_reason("Opponent ran out of time");
                            go->set_is_draw(win->is_draw);
                            broadcaster_.emit(std::move(go_event));
                        }
                    }
                    break;
                }
            }

            {
                std::lock_guard const lock{mutex_};

                for (auto& [pid, pqueue] : action_queues_) {
                    if (pid == holder)
                        continue;

                    if (pending_draw_offer_from_ == 0) {
                        if (auto a = pqueue->try_take_if("draw_offer"))
                            process_action(pid, *a);
                    }
                    if (pending_draw_offer_from_ != 0) {
                        if (auto a = pqueue->try_take_if("draw_response"))
                            process_action(pid, *a);
                    }
                    {
                        if (auto a = pqueue->try_take_if("concede_request"))
                            process_action(pid, *a);
                    }
                    if (pending_concede_.find(pid) != pending_concede_.end()) {
                        if (auto a = pqueue->try_take_if("yes_no"))
                            process_action(pid, *a);
                    }
                }
                if (pending_concede_.find(holder) != pending_concede_.end()) {
                    if (auto a = queue->try_take_if("yes_no"))
                        process_action(holder, *a);
                }

                if (!running_) {
                    break;
                }

                if (!action || action->action_type == "pass" || action->action_type == "shutdown") {
                    priority_.pass();
                } else {
                    process_action(holder, *action);
                }

                check_state_based_actions();

                if (priority_.all_passed() && !stack_.is_empty()) {
                    auto top = stack_.resolve_top();
                    if (top) {
                        resolve_stack_entry(std::move(*top));
                    }
                    check_state_based_actions();
                    priority_.begin_round(turns_.active_player_id());
                }
            }
        }

        {
            std::lock_guard const lock{mutex_};
            if (!running_ || stack_.is_empty()) {
                break;
            }
            auto top = stack_.resolve_top();
            if (top) {
                resolve_stack_entry(std::move(*top));
            }
            check_state_based_actions();
            priority_.begin_round(turns_.active_player_id());
        }
    }
}

void Game::check_state_based_actions() {
    while (sba_checker_.check_and_apply(*this)) {
    }
}

auto Game::build_snapshot(uint64_t viewer_player_id) const -> proto::GameSnapshot {
    std::lock_guard const lock{mutex_};
    proto::GameSnapshot snap;
    snap.set_game_id(game_id_);
    snap.set_current_phase(static_cast<proto::Phase>(static_cast<int>(turns_.current_phase())));
    snap.set_active_player_id(turns_.active_player_id());
    snap.set_priority_player_id(priority_.current_priority_holder());
    snap.set_turn_number(turns_.turn_number());

    for (const auto& player : players_) {
        auto* ps = snap.add_players();
        ps->set_player_id(player.id());
        ps->set_username(player.username());
        ps->set_life_total(player.life());
        ps->set_poison_counters(player.poison_counters());

        auto* mp = ps->mutable_mana_pool();
        mp->set_white(player.mana_pool().white);
        mp->set_blue(player.mana_pool().blue);
        mp->set_black(player.mana_pool().black);
        mp->set_red(player.mana_pool().red);
        mp->set_green(player.mana_pool().green);
        mp->set_colorless(player.mana_pool().colorless);

        ps->set_library_count(zones_.library_size(player.id()));
        ps->set_lands_played_this_turn(player.lands_played_this_turn());
        if (clock_.is_enabled()) {
            ps->set_clock_remaining_ms(
                static_cast<int32_t>(clock_.remaining_time(player.id()).count()));
        }

        const auto& hand = zones_.get_hand(player.id());
        ps->set_hand_count(static_cast<int>(hand.size()));
        if (player.id() == viewer_player_id) {
            for (const auto& card : hand) {
                *ps->add_hand() = cle::serialization::serialize_card(*card);
            }
        }

        auto perm_ids = zones_.get_permanents_controlled_by(player.id());
        for (auto perm_id : perm_ids) {
            const auto* perm = zones_.find_permanent(perm_id);
            if (perm == nullptr) {
                continue;
            }
            auto* pstate = ps->add_battlefield();
            pstate->set_permanent_id(perm_id);
            *pstate->mutable_card() = cle::serialization::serialize_card(*perm->card());
            pstate->set_controller_id(perm->controller_id());
            pstate->set_owner_id(perm->owner_id());
            pstate->set_tapped(perm->is_tapped());
            pstate->set_damage_marked(perm->damage_marked());
            pstate->set_summoning_sick(perm->has_summoning_sickness());
            pstate->set_power_modifier(perm->power_modifier());
            pstate->set_toughness_modifier(perm->toughness_modifier());
            pstate->set_is_token(perm->is_token());
            if (perm->attached_to()) {
                pstate->set_attached_to(*perm->attached_to());
            }
            for (auto att_id : perm->attachments()) {
                pstate->add_attachments(att_id);
            }
            for (const auto& kw : perm->all_keywords()) {
                pstate->add_granted_keywords(kw);
            }
            for (const auto& [ctype, ccount] : perm->all_counters()) {
                auto* c = pstate->add_counters();
                c->set_type(ctype);
                c->set_count(ccount);
            }
        }

        for (const auto& card : zones_.get_graveyard(player.id())) {
            *ps->add_graveyard() = cle::serialization::serialize_card(*card);
        }

        for (const auto* exiled : zones_.get_exile_for_player(player.id())) {
            if (!exiled->face_down || player.id() == viewer_player_id) {
                *ps->add_exile() = cle::serialization::serialize_card(*exiled->card);
            }
        }
    }

    for (const auto& entry : stack_.entries()) {
        auto* se = snap.add_stack();
        se->set_entry_id(entry.id);
        se->set_controller_id(entry.controller_id);
        for (auto t : entry.targets) {
            se->add_targets(t);
        }
        std::visit(
            [&](const auto& content) {
                using T = std::decay_t<decltype(content)>;
                if constexpr (std::is_same_v<T, SpellEntry>) {
                    *se->mutable_spell() = cle::serialization::serialize_card(*content.card);
                } else if constexpr (std::is_same_v<T, ActivatedAbilityEntry>) {
                    se->set_ability_description("Activated ability");
                } else if constexpr (std::is_same_v<T, TriggeredAbilityEntry>) {
                    se->set_ability_description("Triggered ability");
                }
            },
            entry.content);
    }

    return snap;
}

void Game::prompt_trample_damage(uint64_t attacker_id,
                                 std::unique_lock<std::recursive_mutex>& lock) {
    uint64_t const active_id = turns_.active_player_id();
    auto* attacker = zones_.find_permanent(attacker_id);
    if (attacker == nullptr) {
        return;
    }

    auto blocker_ids = combat_.get_blockers_for(attacker_id);
    if (blocker_ids.empty()) {
        return;
    }

    int const total_damage = attacker->effective_power();
    if (total_damage <= 0) {
        return;
    }

    uint64_t defending_player_id = 0;
    for (const auto& atk : combat_.get_attackers()) {
        if (atk.attacker_id == attacker_id) {
            defending_player_id = atk.defending_player_id;
            break;
        }
    }

    proto::ActionPrompt dmg_prompt;
    dmg_prompt.set_player_id(active_id);
    dmg_prompt.set_prompt_id(std::to_string(broadcaster_.next_sequence()));
    auto* da = dmg_prompt.mutable_damage_assignment();
    da->set_attacker_id(attacker_id);
    for (auto bid : blocker_ids) {
        da->add_ordered_blockers(bid);
    }
    da->set_total_damage(total_damage);
    da->set_defending_player_id(defending_player_id);

    proto::GameEvent dmg_event;
    dmg_event.set_game_id(game_id_);
    *dmg_event.mutable_action_prompt() = std::move(dmg_prompt);
    broadcaster_.emit_to_player(active_id, std::move(dmg_event));

    auto* queue = get_action_queue(active_id);
    if (queue == nullptr) {
        return;
    }

    lock.unlock();
    if (!wait_for_connected_player(active_id)) {
        lock.lock();
        return;
    }
    auto response = queue->wait_for(std::chrono::seconds(action_timeout_seconds_));
    lock.lock();
    if (!response || response->action_type != "damage_assignment") {
        return;
    }

    if (static_cast<size_t>(response->indices.size()) != blocker_ids.size()) {
        return;
    }

    int sum = 0;
    bool const has_deathtouch = attacker->has_keyword("Deathtouch");
    for (size_t i = 0; i < blocker_ids.size(); ++i) {
        int const assigned = response->indices[i];
        if (assigned < 0) {
            return;
        }

        auto* blocker = zones_.find_permanent(blocker_ids[i]);
        if (blocker != nullptr) {
            int lethal =
                has_deathtouch ? 1 : blocker->effective_toughness() - blocker->damage_marked();
            if (lethal < 0) {
                lethal = 0;
            }
            if (assigned < lethal) {
                return;
            }
        }
        sum += assigned;
    }

    int const player_dmg = response->x_value;
    if (player_dmg < 0) {
        return;
    }
    sum += player_dmg;

    if (sum != total_damage) {
        return;
    }

    combat_.set_custom_damage(attacker_id,
                              std::vector<int>(response->indices.begin(), response->indices.end()),
                              player_dmg);
}

void Game::emit_game_log(const std::string& text, uint64_t player_id, const std::string& category) {
    proto::GameEvent event;
    event.set_game_id(game_id_);
    auto* log = event.mutable_game_log_entry();
    log->set_sequence_number(broadcaster_.next_sequence());
    log->set_text(text);
    log->set_player_id(player_id);
    log->set_category(category);
    broadcaster_.emit(std::move(event));
}

void Game::auto_tap_lands(uint64_t player_id, const cle::mana::ManaCost& cost) {
    auto* player = find_player(player_id);
    if (player == nullptr) {
        return;
    }

    if (player->mana_pool().can_pay_cost(cost)) {
        return;
    }

    struct LandInfo {
        uint64_t permanent_id;
        std::string produces;
    };
    std::vector<LandInfo> untapped_lands;

    for (auto& [id, perm] : zones_.get_all_permanents()) {
        if (perm.controller_id() != player_id) {
            continue;
        }
        if (perm.card()->type() != cle::core::CardType::Land) {
            continue;
        }
        if (perm.is_tapped()) {
            continue;
        }

        const auto& abilities = perm.card()->activated_abilities();
        for (const auto& ability : abilities) {
            if (!ability.is_mana_ability) {
                continue;
            }
            std::string color;
            auto& text = ability.effect_text;
            if (text.find("{W}") != std::string::npos) {
                color = "W";
            } else if (text.find("{U}") != std::string::npos) {
                color = "U";
            } else if (text.find("{B}") != std::string::npos) {
                color = "B";
            } else if (text.find("{R}") != std::string::npos) {
                color = "R";
            } else if (text.find("{G}") != std::string::npos) {
                color = "G";
            } else {
                color = "C";
            }
            untapped_lands.push_back({id, color});
            break;
        }
    }

    auto needed = [&](const std::string& c) -> bool {
        if (c == "W")
            return cost.white > 0;
        if (c == "U")
            return cost.blue > 0;
        if (c == "B")
            return cost.black > 0;
        if (c == "R")
            return cost.red > 0;
        if (c == "G")
            return cost.green > 0;
        return false;
    };

    std::ranges::sort(untapped_lands, [&](const LandInfo& a, const LandInfo& b) {
        bool a_needed = needed(a.produces);
        bool b_needed = needed(b.produces);
        if (a_needed != b_needed) {
            return a_needed > b_needed;
        }
        return false;
    });

    for (const auto& land : untapped_lands) {
        if (player->mana_pool().can_pay_cost(cost)) {
            break;
        }
        auto* perm = zones_.find_permanent(land.permanent_id);
        if (perm == nullptr || perm->is_tapped()) {
            continue;
        }

        const auto& abilities = perm->card()->activated_abilities();
        for (const auto& ability : abilities) {
            if (!ability.is_mana_ability) {
                continue;
            }
            perm->tap();

            if (ability.effect.valid()) {
                try {
                    cle::triggers::TriggerEvent evt;
                    evt.player_id = player_id;
                    evt.source_id = land.permanent_id;
                    ability.effect(static_cast<cle::game::GameContext&>(*game_context_), evt);
                } catch (const std::exception& e) {
                    spdlog::error("Auto-tap mana ability error on {}: {}", land.permanent_id,
                                  e.what());
                }
            }

            push_mana_undo(player_id, {land.permanent_id, land.produces, 1});
            break;
        }
    }
}

void Game::push_mana_undo(uint64_t player_id, ManaUndoEntry entry) {
    auto& stack = mana_undo_stacks_[player_id];
    if (stack.size() >= 64) {
        stack.erase(stack.begin());
    }
    stack.push_back(std::move(entry));
}

void Game::pop_mana_undo(uint64_t player_id) {
    auto it = mana_undo_stacks_.find(player_id);
    if (it == mana_undo_stacks_.end() || it->second.empty()) {
        return;
    }

    auto entry = it->second.back();
    it->second.pop_back();

    auto* perm = zones_.find_permanent(entry.permanent_id);
    if (perm != nullptr) {
        perm->untap();
    }

    auto* player = find_player(player_id);
    if (player != nullptr) {
        std::ignore = player->mana_pool().spend(entry.color, entry.amount);
    }

    spdlog::debug("Player {} undid mana tap on permanent {}", player_id, entry.permanent_id);
}

}  // namespace mtg::engine
