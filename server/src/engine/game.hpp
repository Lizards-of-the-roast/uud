#pragma once

#include <atomic>
#include <chrono>
#include <condition_variable>
#include <cstdint>
#include <functional>
#include <memory>
#include <mutex>
#include <optional>
#include <string>
#include <thread>
#include <unordered_map>
#include <unordered_set>
#include <vector>

#include "engine/action_queue.hpp"
#include "engine/card_registry.hpp"
#include "engine/combat_manager.hpp"
#include "engine/event_broadcaster.hpp"
#include "engine/game_clock.hpp"
#include "engine/game_context_impl.hpp"
#include "engine/player.hpp"
#include "engine/continuous_effect.hpp"
#include "engine/priority_system.hpp"
#include "engine/replacement_effect.hpp"
#include "engine/state_based_actions.hpp"
#include "engine/the_stack.hpp"
#include "engine/turn_machine.hpp"
#include "engine/win_condition.hpp"
#include "engine/zone_manager.hpp"
#include "mtg/game_state.pb.h"
#include <cle/lua/engine.hpp>
#include <cle/triggers/trigger_event.hpp>
#include <cle/triggers/trigger_type.hpp>
#include <sol/sol.hpp>

namespace mtg::engine {

enum class GameState : std::uint8_t {
    WaitingForPlayers,
    WaitingForDecks,
    InProgress,
    Paused,
    Finished
};

enum class AutoPassMode : std::uint8_t {
    Disabled = 0,
    NoActions = 1,
    UntilEndOfTurn = 2,
    FullControl = 3
};

class Game {
public:
    static constexpr size_t max_players = 2;

    explicit Game(std::string game_id, const CardRegistry& registry, int clock_seconds = 0);
    ~Game();

    Game(const Game&) = delete;
    Game& operator=(const Game&) = delete;
    Game(Game&&) = delete;
    Game& operator=(Game&&) = delete;

    [[nodiscard]] auto add_player(uint64_t player_id, const std::string& username) -> bool;
    [[nodiscard]] auto submit_deck(uint64_t player_id, const std::vector<std::string>& card_names,
                                   const std::vector<std::string>& sideboard_names = {}) -> bool;
    void start();
    void stop();

    void submit_action(uint64_t player_id, ActionData action);

    [[nodiscard]] auto game_id() const -> const std::string& { return game_id_; }
    [[nodiscard]] auto state() const -> GameState { return state_.load(); }
    [[nodiscard]] auto started_at() const -> std::chrono::system_clock::time_point {
        return started_at_;
    }
    [[nodiscard]] auto players() -> std::vector<Player>& { return players_; }
    [[nodiscard]] auto players() const -> const std::vector<Player>& { return players_; }
    [[nodiscard]] auto find_player(uint64_t id) -> Player*;
    [[nodiscard]] auto find_player(uint64_t id) const -> const Player*;
    [[nodiscard]] auto zone_manager() -> ZoneManager& { return zones_; }
    [[nodiscard]] auto zone_manager() const -> const ZoneManager& { return zones_; }
    [[nodiscard]] auto replacement_effects() -> ReplacementEffectRegistry& {
        return replacement_effects_;
    }
    [[nodiscard]] auto continuous_effects() -> ContinuousEffectManager& {
        return continuous_effects_;
    }
    [[nodiscard]] auto turn_machine() -> TurnMachine& { return turns_; }
    [[nodiscard]] auto turn_machine() const -> const TurnMachine& { return turns_; }
    [[nodiscard]] auto the_stack() -> TheStack& { return stack_; }
    [[nodiscard]] auto combat_manager() -> CombatManager& { return combat_; }
    [[nodiscard]] auto broadcaster() -> EventBroadcaster& { return broadcaster_; }
    [[nodiscard]] auto result() const -> const std::optional<GameOverResult>& { return result_; }
    [[nodiscard]] auto get_action_queue(uint64_t player_id) -> ActionQueue*;
    [[nodiscard]] auto card_engine() -> cle::lua::CardEngine& { return card_engine_; }
    [[nodiscard]] auto game_context() -> std::shared_ptr<GameContextImpl>& { return game_context_; }
    [[nodiscard]] auto action_timeout() const -> int { return action_timeout_seconds_; }
    friend class GameContextImpl;
    [[nodiscard]] auto clock() const -> const GameClock& { return clock_; }

    struct DelayedTrigger {
        uint64_t source_permanent_id;
        std::string event_type;
        std::string effect_description;
        std::optional<sol::function> callback;
    };
    void add_delayed_trigger(DelayedTrigger trigger);

    void fire_triggers(cle::triggers::TriggerType type, const cle::triggers::TriggerEvent& event);
    void fire_delayed_triggers(const std::string& event_type, uint64_t source_id);
    [[nodiscard]] auto delayed_triggers() const -> const std::vector<DelayedTrigger>& {
        return delayed_triggers_;
    }
    void clear_delayed_triggers();

    void set_auto_pass(uint64_t player_id, AutoPassMode mode);
    [[nodiscard]] auto get_auto_pass(uint64_t player_id) const -> AutoPassMode;

    [[nodiscard]] auto pending_draw_offer_from() const -> uint64_t {
        return pending_draw_offer_from_;
    }

    void set_player_connected(uint64_t player_id, bool connected);
    [[nodiscard]] auto is_player_connected(uint64_t player_id) const -> bool;

    [[nodiscard]] auto stale_disconnected_players(std::chrono::seconds timeout) const
        -> std::vector<uint64_t>;
    void eliminate_player(uint64_t player_id);

    [[nodiscard]] auto build_snapshot(uint64_t viewer_player_id) const -> proto::GameSnapshot;

private:
    void game_loop();
    void setup_game();

    void run_phase(Phase phase);
    void run_cleanup_phase();
    void run_priority_round();
    void check_state_based_actions();
    void resolve_stack_entry(StackEntry entry);
    void process_action(uint64_t player_id, const ActionData& action);
    struct PriorityPromptResult {
        bool has_meaningful_actions;
        std::string prompt_id;
    };
    auto send_priority_prompt(uint64_t player_id) -> PriorityPromptResult;
    void prompt_trample_damage(uint64_t attacker_id, std::unique_lock<std::recursive_mutex>& lock);
    void emit_game_log(const std::string& text, uint64_t player_id = 0,
                       const std::string& category = "action");
    auto wait_for_connected_player(uint64_t player_id) -> bool;
    void auto_tap_lands(uint64_t player_id, const cle::mana::ManaCost& cost);

    struct ManaUndoEntry {
        uint64_t permanent_id;
        std::string color;
        int amount;
    };
    void push_mana_undo(uint64_t player_id, ManaUndoEntry entry);
    void pop_mana_undo(uint64_t player_id);

    std::string game_id_;
    std::atomic<GameState> state_{GameState::WaitingForPlayers};

    mutable std::recursive_mutex mutex_;
    std::vector<Player> players_;
    ZoneManager zones_;
    TurnMachine turns_;
    PrioritySystem priority_;
    TheStack stack_;
    CombatManager combat_;
    ReplacementEffectRegistry replacement_effects_;
    ContinuousEffectManager continuous_effects_;
    StateBasedActionChecker sba_checker_;
    WinConditionChecker win_checker_;
    EventBroadcaster broadcaster_;
    std::optional<GameOverResult> result_;
    std::vector<DelayedTrigger> delayed_triggers_;

    cle::lua::CardEngine card_engine_;
    std::shared_ptr<GameContextImpl> game_context_;
    const CardRegistry& card_registry_;

    std::unordered_map<uint64_t, std::shared_ptr<ActionQueue>> action_queues_;
    std::shared_ptr<SharedNotify> shared_notify_ = std::make_shared<SharedNotify>();

    std::chrono::system_clock::time_point started_at_;

    std::unordered_set<uint64_t> connected_players_;
    std::unordered_map<uint64_t, std::chrono::steady_clock::time_point> disconnect_times_;
    std::condition_variable_any reconnect_cv_;

    GameClock clock_;
    uint64_t pending_draw_offer_from_{0};
    std::unordered_map<uint64_t, AutoPassMode> auto_pass_modes_;
    std::unordered_set<uint64_t> pending_concede_;
    std::unordered_map<uint64_t, std::vector<ManaUndoEntry>> mana_undo_stacks_;

    std::thread game_thread_;
    std::atomic<bool> running_{false};
    int action_timeout_seconds_{60};
};

}  // namespace mtg::engine
