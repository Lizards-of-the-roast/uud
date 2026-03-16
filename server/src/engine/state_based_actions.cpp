#include "engine/state_based_actions.hpp"

#include <algorithm>
#include <unordered_map>

#include "engine/game.hpp"
#include <cle/triggers/trigger_event.hpp>
#include <cle/triggers/trigger_type.hpp>
#include <spdlog/spdlog.h>
#include <sol/sol.hpp>

namespace mtg::engine {

bool StateBasedActionChecker::check_and_apply(Game& game) {
    bool applied = false;

    auto emit_eliminated = [&game](uint64_t player_id, const std::string& reason) {
        proto::GameEvent event;
        event.set_game_id(game.game_id());
        auto* pe = event.mutable_player_eliminated();
        pe->set_player_id(player_id);
        pe->set_reason(reason);
        game.broadcaster().emit(std::move(event));
    };

    for (auto& player : game.players()) {
        if (player.life() <= 0 && player.is_alive()) {
            player.eliminate();
            emit_eliminated(player.id(), "Life total reached zero");
            applied = true;
        }
        if (player.poison_counters() >= 10 && player.is_alive()) {
            player.eliminate();
            emit_eliminated(player.id(), "Received 10 poison counters");
            applied = true;
        }
        if (player.failed_to_draw() && player.is_alive()) {
            player.eliminate();
            emit_eliminated(player.id(), "Failed to draw from empty library");
            applied = true;
        }
    }

    auto& zones = game.zone_manager();

    std::vector<uint64_t> to_destroy;
    for (const auto& [id, perm] : zones.get_all_permanents()) {
        if (!perm.is_creature()) {
            continue;
        }
        if (perm.effective_toughness() <= 0) {
            to_destroy.push_back(id);
            continue;
        }
        if (perm.has_keyword("Indestructible")) {
            continue;
        }
        if (perm.has_lethal_damage() || perm.is_deathtouch_marked()) {
            to_destroy.push_back(id);
            continue;
        }
    }

    struct DyingCreatureInfo {
        uint64_t id;
        uint64_t controller_id;
        std::optional<sol::function> on_death_trigger;
    };
    std::vector<DyingCreatureInfo> dying_creatures;

    for (uint64_t const id : to_destroy) {
        auto* perm = zones.find_permanent(id);
        if (perm == nullptr) {
            continue;
        }
        DyingCreatureInfo info{.id = id, .controller_id = perm->controller_id(), .on_death_trigger = std::nullopt};
        auto trigger = perm->card()->get_trigger(cle::triggers::TriggerType::OnDeath);
        if (trigger && trigger->valid()) {
            info.on_death_trigger = *trigger;
        }
        dying_creatures.push_back(std::move(info));
    }

    for (auto& info : dying_creatures) {
        auto* perm_pre = zones.find_permanent(info.id);
        std::string card_name = (perm_pre != nullptr) ? perm_pre->card()->name() : "";
        auto removed = zones.remove_from_battlefield(info.id);
        if (removed) {
            if (!removed->is_token()) {
                zones.add_to_graveyard(removed->owner_id(), removed->card());
            }

            proto::GameEvent event;
            event.set_game_id(game.game_id());
            auto* pd = event.mutable_permanent_destroyed();
            pd->set_permanent_id(info.id);
            pd->set_card_name(card_name);
            game.broadcaster().emit(std::move(event));

            applied = true;
        }
    }

    auto active_id = game.turn_machine().active_player_id();
    std::stable_sort(dying_creatures.begin(), dying_creatures.end(),
                     [&](const DyingCreatureInfo& a, const DyingCreatureInfo& b) {
                         bool const a_active = (a.controller_id == active_id);
                         bool const b_active = (b.controller_id == active_id);
                         if (a_active != b_active) {
                             return a_active;
                         }
                         return false;
                     });

    for (auto& info : dying_creatures) {
        cle::triggers::TriggerEvent death_event;
        death_event.type = cle::triggers::TriggerType::OnDeath;
        death_event.source_id = info.id;
        death_event.player_id = info.controller_id;

        if (info.on_death_trigger && info.on_death_trigger->valid()) {
            try {
                (*info.on_death_trigger)(game.game_context().get(), death_event);
            } catch (const std::exception& e) {
                spdlog::warn("OnDeath trigger error for permanent {}: {}", info.id, e.what());
            }
        }
        game.fire_triggers(cle::triggers::TriggerType::OnAnotherCreatureDies, death_event);
        game.fire_delayed_triggers("on_death", info.id);
    }

    {
        struct LegendKey {
            uint64_t controller_id;
            std::string name;
            bool operator==(const LegendKey& o) const {
                return controller_id == o.controller_id && name == o.name;
            }
        };
        struct LegendKeyHash {
            size_t operator()(const LegendKey& k) const {
                return std::hash<uint64_t>{}(k.controller_id) ^
                       (std::hash<std::string>{}(k.name) << 1);
            }
        };
        std::unordered_map<LegendKey, std::vector<uint64_t>, LegendKeyHash> legends;

        for (const auto& [id, perm] : zones.get_all_permanents()) {
            if (perm.has_keyword("Legendary")) {
                legends[{perm.controller_id(), perm.card()->name()}].push_back(id);
            }
        }

        for (auto& [key, ids] : legends) {
            if (ids.size() <= 1) {
                continue;
            }

            uint64_t keep_id = ids[0];
            auto* ctx = game.game_context().get();
            if (ctx != nullptr) {
                keep_id = ctx->choose_from_legends(key.controller_id, ids);
            }

            for (auto id : ids) {
                if (id == keep_id) {
                    continue;
                }
                auto* legend_perm = zones.find_permanent(id);
                if (legend_perm == nullptr) {
                    continue;
                }
                if (legend_perm->card()->name() != key.name ||
                    legend_perm->controller_id() != key.controller_id) {
                    continue;
                }
                std::string legend_name = legend_perm->card()->name();
                auto removed = zones.remove_from_battlefield(id);
                if (removed) {
                    if (!removed->is_token()) {
                        zones.add_to_graveyard(removed->owner_id(), removed->card());
                    }

                    proto::GameEvent event;
                    event.set_game_id(game.game_id());
                    auto* pd = event.mutable_permanent_destroyed();
                    pd->set_permanent_id(id);
                    pd->set_card_name(legend_name);
                    game.broadcaster().emit(std::move(event));

                    applied = true;
                }
            }
        }
    }

    {
        std::vector<uint64_t> detached_auras;
        std::vector<uint64_t> detached_equipment;
        for (const auto& [id, perm] : zones.get_all_permanents()) {
            auto target = perm.attached_to();
            if (!target) {
                continue;
            }
            if (zones.find_permanent(*target) == nullptr) {
                if (perm.card()->type() == cle::core::CardType::Enchantment) {
                    detached_auras.push_back(id);
                } else {
                    detached_equipment.push_back(id);
                }
            }
        }
        for (uint64_t const id : detached_equipment) {
            auto* perm = zones.find_permanent(id);
            if (perm != nullptr) {
                perm->detach();
                applied = true;
            }
        }
        for (uint64_t const id : detached_auras) {
            auto* aura_perm = zones.find_permanent(id);
            std::string aura_name =
                (aura_perm != nullptr) ? aura_perm->card()->name() : "";
            auto removed = zones.remove_from_battlefield(id);
            if (removed) {
                if (!removed->is_token()) {
                    zones.add_to_graveyard(removed->owner_id(), removed->card());
                }

                proto::GameEvent event;
                event.set_game_id(game.game_id());
                auto* pd = event.mutable_permanent_destroyed();
                pd->set_permanent_id(id);
                pd->set_card_name(aura_name);
                game.broadcaster().emit(std::move(event));

                applied = true;
            }
        }
    }

    {
        for (auto& [id, perm] : zones.get_all_permanents()) {
            int const plus = perm.get_counters("+1/+1");
            int const minus = perm.get_counters("-1/-1");
            if (plus > 0 && minus > 0) {
                int const cancel = std::min(plus, minus);
                perm.remove_counter("+1/+1", cancel);
                perm.remove_counter("-1/-1", cancel);
                applied = true;
            }
        }
    }

    return applied;
}

}  // namespace mtg::engine
