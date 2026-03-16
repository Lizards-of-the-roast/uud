#include "engine/game_context_impl.hpp"

#include <algorithm>
#include <array>

#include "engine/game.hpp"
#include "engine/target_filter.hpp"
#include <cle/serialization/card_serializer.hpp>
#include <cle/triggers/trigger_event.hpp>
#include <cle/triggers/trigger_type.hpp>
#include <spdlog/spdlog.h>

namespace mtg::engine {

GameContextImpl::GameContextImpl(Game& game) : game_{game} {}

auto GameContextImpl::send_prompt_and_wait(uint64_t player_id, proto::ActionPrompt prompt)
    -> ActionData {
    prompt.set_player_id(player_id);
    auto prompt_id = std::to_string(game_.broadcaster().next_sequence());
    prompt.set_prompt_id(prompt_id);

    proto::GameEvent event;
    event.set_game_id(game_.game_id());
    *event.mutable_action_prompt() = std::move(prompt);
    game_.broadcaster().emit_to_player(player_id, std::move(event));

    auto* queue = game_.get_action_queue(player_id);
    if (queue == nullptr) {
        spdlog::warn("No action queue for player {}", player_id);
        return ActionData{.player_id = player_id,
                          .prompt_id = {},
                          .action_type = "timeout",
                          .target_id = 0,
                          .ids = {},
                          .indices = {},
                          .flag = false,
                          .text = {},
                          .x_value = 0,
                          .mana_payment = {}};
    }

    std::unique_lock<std::recursive_mutex> ul{game_.mutex_, std::adopt_lock};
    ul.unlock();

    constexpr int max_stale_retries = 8;
    std::optional<ActionData> result;
    for (int attempt = 0; attempt < max_stale_retries; ++attempt) {
        result = queue->wait_for(std::chrono::seconds(game_.action_timeout()));
        if (!result) {
            break;
        }
        if (result->action_type == "concede" || result->action_type == "shutdown") {
            break;
        }
        if (!result->prompt_id.empty() && result->prompt_id == prompt_id) {
            break;
        }
        spdlog::debug("Player {} sent stale response (prompt_id={}, expected={}), discarding",
                      player_id, result->prompt_id, prompt_id);
        result.reset();
    }

    ul.lock();
    ul.release();
    if (!result) {
        spdlog::warn("Player {} timed out on prompt {}", player_id, prompt_id);
        return ActionData{.player_id = player_id,
                          .prompt_id = {},
                          .action_type = "timeout",
                          .target_id = 0,
                          .ids = {},
                          .indices = {},
                          .flag = false,
                          .text = {},
                          .x_value = 0,
                          .mana_payment = {}};
    }
    return std::move(*result);
}

void GameContextImpl::emit_zone_transfer(uint64_t card_id, proto::ZoneType from, proto::ZoneType to,
                                         uint64_t player_id) {
    proto::GameEvent event;
    event.set_game_id(game_.game_id());
    auto* zt = event.mutable_zone_transfer();
    zt->set_card_id(card_id);
    zt->set_from_zone(from);
    zt->set_to_zone(to);
    zt->set_player_id(player_id);
    game_.broadcaster().emit(std::move(event));
}

void GameContextImpl::draw_cards(uint64_t player_id, int count) {
    for (int i = 0; i < count; ++i) {
        auto card = game_.zone_manager().draw_card(player_id);
        if (!card) {
            spdlog::info("Player {} tried to draw from empty library", player_id);
            auto* player = game_.find_player(player_id);
            if (player != nullptr) {
                player->set_failed_to_draw();
            }
            break;
        }
        proto::GameEvent event;
        event.set_game_id(game_.game_id());
        auto* drawn = event.mutable_card_drawn();
        drawn->set_player_id(player_id);
        *drawn->mutable_card() = cle::serialization::serialize_card(**card);
        game_.broadcaster().emit_to_player(player_id, std::move(event));

        for (const auto& p : game_.players()) {
            if (p.id() == player_id)
                continue;
            proto::GameEvent zt_event;
            zt_event.set_game_id(game_.game_id());
            auto* zt = zt_event.mutable_zone_transfer();
            zt->set_card_id((*card)->instance_id());
            zt->set_from_zone(proto::ZONE_LIBRARY);
            zt->set_to_zone(proto::ZONE_HAND);
            zt->set_player_id(player_id);
            game_.broadcaster().emit_to_player(p.id(), std::move(zt_event));
        }
    }
}

void GameContextImpl::discard_cards(uint64_t player_id, int count) {
    auto& hand = game_.zone_manager().get_hand(player_id);
    int const actual_count = std::min(count, static_cast<int>(hand.size()));
    if (actual_count <= 0) {
        return;
    }

    if (actual_count >= static_cast<int>(hand.size())) {
        while (!hand.empty()) {
            auto card = std::move(hand.back());
            auto card_id = card->instance_id();
            hand.pop_back();
            game_.zone_manager().add_to_graveyard(player_id, std::move(card));
            emit_zone_transfer(card_id, proto::ZONE_HAND, proto::ZONE_GRAVEYARD, player_id);
        }
        return;
    }

    proto::ActionPrompt prompt;
    auto* discard = prompt.mutable_discard();
    discard->set_count(actual_count);
    for (const auto& card : hand) {
        *discard->add_hand() = cle::serialization::serialize_card(*card);
    }

    auto response = send_prompt_and_wait(player_id, std::move(prompt));

    int discarded = 0;
    if (!response.ids.empty() && static_cast<int>(response.ids.size()) == actual_count) {
        for (auto card_id : response.ids) {
            auto card = game_.zone_manager().remove_from_hand(player_id, card_id);
            if (card) {
                game_.zone_manager().add_to_graveyard(player_id, std::move(card));
                emit_zone_transfer(card_id, proto::ZONE_HAND, proto::ZONE_GRAVEYARD, player_id);
                ++discarded;
            }
        }
    }
    for (int i = discarded; i < actual_count && !hand.empty(); ++i) {
        auto card = std::move(hand.back());
        auto card_id = card->instance_id();
        hand.pop_back();
        game_.zone_manager().add_to_graveyard(player_id, std::move(card));
        emit_zone_transfer(card_id, proto::ZONE_HAND, proto::ZONE_GRAVEYARD, player_id);
    }
}

void GameContextImpl::gain_life(uint64_t player_id, int amount) {
    auto* player = game_.find_player(player_id);
    if (player == nullptr) {
        return;
    }
    player->gain_life(amount);

    proto::GameEvent event;
    event.set_game_id(game_.game_id());
    auto* lc = event.mutable_life_changed();
    lc->set_player_id(player_id);
    lc->set_new_total(player->life());
    lc->set_delta(amount);
    game_.broadcaster().emit(std::move(event));
}

void GameContextImpl::lose_life(uint64_t player_id, int amount) {
    auto* player = game_.find_player(player_id);
    if (player == nullptr) {
        return;
    }
    player->lose_life(amount);

    proto::GameEvent event;
    event.set_game_id(game_.game_id());
    auto* lc = event.mutable_life_changed();
    lc->set_player_id(player_id);
    lc->set_new_total(player->life());
    lc->set_delta(-amount);
    game_.broadcaster().emit(std::move(event));
}

void GameContextImpl::add_mana(uint64_t player_id, const std::string& color, int amount) {
    auto* player = game_.find_player(player_id);
    if (player == nullptr) {
        return;
    }
    player->mana_pool().add(color, amount);

    proto::GameEvent event;
    event.set_game_id(game_.game_id());
    auto* ma = event.mutable_mana_added();
    ma->set_player_id(player_id);
    ma->set_color(color);
    ma->set_amount(amount);
    game_.broadcaster().emit_to_player(player_id, std::move(event));
}

void GameContextImpl::deal_damage(uint64_t source_id, uint64_t target_id, int amount) {
    if (amount <= 0) {
        return;
    }

    auto* perm = game_.zone_manager().find_permanent(target_id);
    if (perm != nullptr) {
        auto* source = game_.zone_manager().find_permanent(source_id);
        if (source != nullptr && perm->has_protection_from(*source)) {
            return;
        }

        proto::GameEvent dmg_event;
        dmg_event.set_game_id(game_.game_id());
        auto* dd = dmg_event.mutable_damage_dealt();
        dd->set_source_id(source_id);
        dd->set_target_id(target_id);
        dd->set_amount(amount);
        game_.broadcaster().emit(std::move(dmg_event));

        perm->mark_damage(amount);
        if ((source != nullptr) && source->has_keyword("Deathtouch") && amount > 0) {
            perm->mark_deathtouch();
        }
        if ((source != nullptr) && source->has_keyword("Lifelink")) {
            auto* controller = game_.find_player(source->controller_id());
            if (controller != nullptr) {
                controller->gain_life(amount);

                proto::GameEvent ll_event;
                ll_event.set_game_id(game_.game_id());
                auto* ll = ll_event.mutable_life_changed();
                ll->set_player_id(source->controller_id());
                ll->set_new_total(controller->life());
                ll->set_delta(amount);
                game_.broadcaster().emit(std::move(ll_event));
            }
        }
    } else {
        auto* player = game_.find_player(target_id);
        if (player != nullptr) {
            proto::GameEvent dmg_event;
            dmg_event.set_game_id(game_.game_id());
            auto* dd = dmg_event.mutable_damage_dealt();
            dd->set_source_id(source_id);
            dd->set_target_id(target_id);
            dd->set_amount(amount);
            game_.broadcaster().emit(std::move(dmg_event));

            player->lose_life(amount);

            proto::GameEvent lc_event;
            lc_event.set_game_id(game_.game_id());
            auto* lc = lc_event.mutable_life_changed();
            lc->set_player_id(target_id);
            lc->set_new_total(player->life());
            lc->set_delta(-amount);
            game_.broadcaster().emit(std::move(lc_event));

            auto* source = game_.zone_manager().find_permanent(source_id);
            if ((source != nullptr) && source->has_keyword("Lifelink")) {
                auto* controller = game_.find_player(source->controller_id());
                if (controller != nullptr) {
                    controller->gain_life(amount);

                    proto::GameEvent ll_event;
                    ll_event.set_game_id(game_.game_id());
                    auto* ll = ll_event.mutable_life_changed();
                    ll->set_player_id(source->controller_id());
                    ll->set_new_total(controller->life());
                    ll->set_delta(amount);
                    game_.broadcaster().emit(std::move(ll_event));
                }
            }
        }
    }
}

void GameContextImpl::destroy_permanent(uint64_t permanent_id) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    if (perm == nullptr) {
        return;
    }
    if (perm->has_keyword("Indestructible")) {
        return;
    }
    auto card_name = perm->card()->name();
    bool const was_creature = perm->is_creature();
    auto controller_id = perm->controller_id();
    auto removed = game_.zone_manager().remove_from_battlefield(permanent_id);
    if (removed) {
        game_.zone_manager().add_to_graveyard(removed->owner_id(), removed->card());

        proto::GameEvent event;
        event.set_game_id(game_.game_id());
        auto* pd = event.mutable_permanent_destroyed();
        pd->set_permanent_id(permanent_id);
        pd->set_card_name(card_name);
        game_.broadcaster().emit(std::move(event));

        if (was_creature) {
            fire_death_triggers(permanent_id, controller_id);
        }
    }
}

void GameContextImpl::exile_card(uint64_t card_id) {
    auto removed = game_.zone_manager().remove_from_battlefield(card_id);
    if (removed) {
        auto owner_id = removed->owner_id();
        if (!removed->is_token()) {
            game_.zone_manager().add_to_exile(removed->card(), owner_id);
        }
        emit_zone_transfer(card_id, proto::ZONE_BATTLEFIELD, proto::ZONE_EXILE, owner_id);
        return;
    }

    for (const auto& player : game_.players()) {
        auto card = game_.zone_manager().remove_from_graveyard(player.id(), card_id);
        if (card) {
            auto owner_id = game_.zone_manager().find_card_owner(card_id);
            if (owner_id == 0) {
                owner_id = player.id();
            }
            game_.zone_manager().add_to_exile(std::move(card), owner_id);
            emit_zone_transfer(card_id, proto::ZONE_GRAVEYARD, proto::ZONE_EXILE, owner_id);
            return;
        }
    }

    for (const auto& player : game_.players()) {
        auto card = game_.zone_manager().remove_from_hand(player.id(), card_id);
        if (card) {
            auto owner_id = game_.zone_manager().find_card_owner(card_id);
            if (owner_id == 0) {
                owner_id = player.id();
            }
            game_.zone_manager().add_to_exile(std::move(card), owner_id);
            emit_zone_transfer(card_id, proto::ZONE_HAND, proto::ZONE_EXILE, owner_id);
            return;
        }
    }
}

void GameContextImpl::return_to_hand(uint64_t card_id) {
    auto removed = game_.zone_manager().remove_from_battlefield(card_id);
    if (removed) {
        auto owner_id = removed->owner_id();
        game_.zone_manager().get_hand(owner_id).push_back(removed->card());
        emit_zone_transfer(card_id, proto::ZONE_BATTLEFIELD, proto::ZONE_HAND, owner_id);
        return;
    }

    for (const auto& player : game_.players()) {
        auto card = game_.zone_manager().remove_from_graveyard(player.id(), card_id);
        if (card) {
            auto owner_id = game_.zone_manager().find_card_owner(card_id);
            if (owner_id == 0) {
                owner_id = player.id();
            }
            game_.zone_manager().get_hand(owner_id).push_back(std::move(card));
            emit_zone_transfer(card_id, proto::ZONE_GRAVEYARD, proto::ZONE_HAND, owner_id);
            return;
        }
    }

    auto exiled = game_.zone_manager().remove_from_exile(card_id);
    if (exiled) {
        auto owner_id = exiled->owner_id;
        game_.zone_manager().get_hand(owner_id).push_back(std::move(exiled->card));
        emit_zone_transfer(card_id, proto::ZONE_EXILE, proto::ZONE_HAND, owner_id);
    }
}

void GameContextImpl::tap_permanent(uint64_t permanent_id) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    if (perm != nullptr) {
        perm->tap();
    }
}

void GameContextImpl::untap_permanent(uint64_t permanent_id) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    if (perm != nullptr) {
        perm->untap();
    }
}

uint64_t GameContextImpl::create_token(const std::string& name, cle::core::CardType type, int power,
                                       int toughness) {
    auto card = std::make_shared<cle::core::Card>(name, type);
    card->set_creature_stats(cle::core::CreatureStats{power, toughness});
    uint64_t const controller = game_.turn_machine().active_player_id();
    uint64_t const id = game_.zone_manager().add_to_battlefield(card, controller, controller);
    auto* perm = game_.zone_manager().find_permanent(id);
    if (perm != nullptr) {
        perm->set_token(true);
    }

    proto::GameEvent perm_event;
    perm_event.set_game_id(game_.game_id());
    auto* pe = perm_event.mutable_permanent_entered();
    pe->set_permanent_id(id);
    pe->set_controller_id(controller);
    *pe->mutable_card() = cle::serialization::serialize_card(*card);
    pe->set_tapped(false);
    pe->set_is_token(true);
    game_.broadcaster().emit(std::move(perm_event));

    return id;
}

uint64_t GameContextImpl::create_token_tapped(const std::string& name, cle::core::CardType type,
                                              int power, int toughness) {
    auto id = create_token(name, type, power, toughness);
    auto* perm = game_.zone_manager().find_permanent(id);
    if (perm != nullptr) {
        perm->tap();
    }
    return id;
}

auto GameContextImpl::get_permanents_with_type(uint64_t player_id, cle::core::CardType type) const
    -> std::vector<uint64_t> {
    return game_.zone_manager().get_permanents_with_type(player_id, type);
}

auto GameContextImpl::get_cards_in_graveyard(uint64_t player_id) const -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    for (const auto& card : game_.zone_manager().get_graveyard(player_id)) {
        result.push_back(card->instance_id());
    }
    return result;
}

void GameContextImpl::add_counter(uint64_t permanent_id, const std::string& counter_type,
                                  int amount) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    if (perm == nullptr) {
        return;
    }
    perm->add_counter(counter_type, amount);

    proto::GameEvent event;
    event.set_game_id(game_.game_id());
    auto* cc = event.mutable_counter_changed();
    cc->set_permanent_id(permanent_id);
    cc->set_counter_type(counter_type);
    cc->set_new_count(perm->get_counters(counter_type));
    cc->set_delta(amount);
    game_.broadcaster().emit(std::move(event));
}

void GameContextImpl::remove_counter(uint64_t permanent_id, const std::string& counter_type,
                                     int amount) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    if (perm == nullptr) {
        return;
    }
    perm->remove_counter(counter_type, amount);

    proto::GameEvent event;
    event.set_game_id(game_.game_id());
    auto* cc = event.mutable_counter_changed();
    cc->set_permanent_id(permanent_id);
    cc->set_counter_type(counter_type);
    cc->set_new_count(perm->get_counters(counter_type));
    cc->set_delta(-amount);
    game_.broadcaster().emit(std::move(event));
}

uint64_t GameContextImpl::choose_target(uint64_t player_id, const std::string& filter) {
    auto parsed = parse_filter(filter);
    auto legal = apply_filter(parsed, player_id, game_.zone_manager(), game_.players());

    if (legal.empty()) {
        spdlog::debug("No legal targets for filter '{}' (player {})", filter, player_id);
        return 0;
    }

    if (legal.size() == 1) {
        return legal[0];
    }

    auto build_prompt = [&]() {
        proto::ActionPrompt prompt;
        auto* tp = prompt.mutable_target();
        tp->set_filter(filter);
        for (uint64_t const id : legal) {
            tp->add_legal_targets(id);
        }
        return prompt;
    };

    auto response = send_prompt_and_wait(player_id, build_prompt());

    if (response.target_id != 0 &&
        std::find(legal.begin(), legal.end(), response.target_id) == legal.end()) {
        spdlog::warn("Player {} chose illegal target {} for filter '{}', re-prompting", player_id,
                     response.target_id, filter);
        response = send_prompt_and_wait(player_id, build_prompt());
        if (std::find(legal.begin(), legal.end(), response.target_id) == legal.end()) {
            return 0;
        }
    }

    return response.target_id;
}

uint64_t GameContextImpl::get_controller(uint64_t permanent_id) const {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    return (perm != nullptr) ? perm->controller_id() : 0;
}

uint64_t GameContextImpl::get_owner(uint64_t card_id) const {
    return game_.zone_manager().find_card_owner(card_id);
}

uint64_t GameContextImpl::get_permanent_id(uint64_t card_instance_id) const {
    return card_instance_id;
}

std::string GameContextImpl::get_card_zone(uint64_t card_id) const {
    return game_.zone_manager().find_card_zone(card_id);
}

void GameContextImpl::scry(uint64_t player_id, int count) {
    std::vector<std::shared_ptr<cle::core::Card>> peeked;
    for (int i = 0; i < count; ++i) {
        auto card = game_.zone_manager().remove_top(player_id);
        if (!card) {
            break;
        }
        peeked.push_back(std::move(*card));
    }
    if (peeked.empty()) {
        return;
    }

    proto::ActionPrompt prompt;
    auto* dp = prompt.mutable_discard();
    dp->set_count(0);
    for (const auto& card : peeked) {
        *dp->add_hand() = cle::serialization::serialize_card(*card);
    }

    auto response = send_prompt_and_wait(player_id, std::move(prompt));

    std::unordered_set<uint64_t> to_bottom(response.ids.begin(), response.ids.end());
    if (to_bottom.empty() && response.target_id != 0) {
        to_bottom.insert(response.target_id);
    }
    for (auto it = peeked.rbegin(); it != peeked.rend(); ++it) {
        if (!to_bottom.contains((*it)->instance_id())) {
            game_.zone_manager().put_on_top(player_id, *it);
        }
    }
    for (const auto& card : peeked) {
        if (to_bottom.contains(card->instance_id())) {
            game_.zone_manager().put_on_bottom(player_id, card);
        }
    }
}

void GameContextImpl::surveil(uint64_t player_id, int count) {
    std::vector<std::shared_ptr<cle::core::Card>> peeked;
    for (int i = 0; i < count; ++i) {
        auto card = game_.zone_manager().remove_top(player_id);
        if (!card) {
            break;
        }
        peeked.push_back(std::move(*card));
    }
    if (peeked.empty()) {
        return;
    }

    proto::ActionPrompt prompt;
    auto* dp = prompt.mutable_discard();
    dp->set_count(0);
    for (const auto& card : peeked) {
        *dp->add_hand() = cle::serialization::serialize_card(*card);
    }

    auto response = send_prompt_and_wait(player_id, std::move(prompt));

    std::unordered_set<uint64_t> to_graveyard(response.ids.begin(), response.ids.end());
    if (to_graveyard.empty() && response.target_id != 0) {
        to_graveyard.insert(response.target_id);
    }
    for (auto it = peeked.rbegin(); it != peeked.rend(); ++it) {
        if (to_graveyard.count((*it)->instance_id()) != 0u) {
            game_.zone_manager().add_to_graveyard(player_id, *it);
            emit_zone_transfer((*it)->instance_id(), proto::ZONE_LIBRARY, proto::ZONE_GRAVEYARD,
                               player_id);
        } else {
            game_.zone_manager().put_on_top(player_id, *it);
        }
    }
}

uint64_t GameContextImpl::search_library(uint64_t player_id, const std::string& filter) {
    proto::ActionPrompt prompt;
    auto* tp = prompt.mutable_target();
    tp->set_filter(filter);

    auto parsed = parse_filter(filter);

    auto lib_size = game_.zone_manager().library_size(player_id);
    std::vector<std::shared_ptr<cle::core::Card>> lib_cards;
    for (int i = 0; i < lib_size; ++i) {
        auto card = game_.zone_manager().remove_top(player_id);
        if (card) {
            lib_cards.push_back(std::move(*card));
        }
    }
    std::vector<uint64_t> legal_ids;
    for (const auto& card : lib_cards) {
        if (parsed.card_type && card->type() != *parsed.card_type) {
            continue;
        }
        if (parsed.subtype) {
            bool found = false;
            for (const auto& st : card->subtypes()) {
                std::string lower_st = st;
                std::transform(lower_st.begin(), lower_st.end(), lower_st.begin(),
                               [](unsigned char c) { return std::tolower(c); });
                if (lower_st == *parsed.subtype) {
                    found = true;
                    break;
                }
            }
            if (!found) {
                continue;
            }
        }
        if (parsed.max_mana_value && card->mana_cost().mana_value() > *parsed.max_mana_value) {
            continue;
        }
        tp->add_legal_targets(card->instance_id());
        legal_ids.push_back(card->instance_id());
    }
    for (auto it = lib_cards.rbegin(); it != lib_cards.rend(); ++it) {
        game_.zone_manager().put_on_top(player_id, *it);
    }

    auto response = send_prompt_and_wait(player_id, std::move(prompt));
    game_.zone_manager().shuffle_library(player_id);
    if (response.target_id != 0) {
        if (std::find(legal_ids.begin(), legal_ids.end(), response.target_id) == legal_ids.end()) {
            return 0;
        }
    }
    return response.target_id;
}

void GameContextImpl::return_from_graveyard(uint64_t player_id, uint64_t card_id) {
    auto card = game_.zone_manager().remove_from_graveyard(player_id, card_id);
    if (card) {
        game_.zone_manager().get_hand(player_id).push_back(std::move(card));
        emit_zone_transfer(card_id, proto::ZONE_GRAVEYARD, proto::ZONE_HAND, player_id);
    }
}

void GameContextImpl::mill(uint64_t player_id, int count) {
    for (int i = 0; i < count; ++i) {
        auto card = game_.zone_manager().remove_top(player_id);
        if (!card) {
            spdlog::info("Player {} has no more cards to mill", player_id);
            break;
        }
        auto card_id = (*card)->instance_id();
        game_.zone_manager().add_to_graveyard(player_id, std::move(*card));
        emit_zone_transfer(card_id, proto::ZONE_LIBRARY, proto::ZONE_GRAVEYARD, player_id);
    }
}

void GameContextImpl::fight(uint64_t creature_a, uint64_t creature_b) {
    auto* a = game_.zone_manager().find_permanent(creature_a);
    auto* b = game_.zone_manager().find_permanent(creature_b);
    if (a == nullptr || b == nullptr) {
        return;
    }
    if (!a->is_creature() || !b->is_creature()) {
        spdlog::warn("Fight: one or both permanents are not creatures ({}, {})", creature_a,
                     creature_b);
        return;
    }
    int const a_power = a->effective_power();
    int const b_power = b->effective_power();
    deal_damage(creature_a, creature_b, a_power);
    deal_damage(creature_b, creature_a, b_power);
}

void GameContextImpl::modify_power_toughness(uint64_t permanent_id, int power_mod,
                                             int toughness_mod) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    if (perm != nullptr) {
        perm->modify_power_toughness(power_mod, toughness_mod);
    }
}

int GameContextImpl::get_power(uint64_t permanent_id) const {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    return (perm != nullptr) ? perm->effective_power() : 0;
}

int GameContextImpl::get_toughness(uint64_t permanent_id) const {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    return (perm != nullptr) ? perm->effective_toughness() : 0;
}

void GameContextImpl::grant_keyword(uint64_t permanent_id, const std::string& keyword) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    if (perm != nullptr) {
        perm->grant_keyword(keyword);
    }
}

void GameContextImpl::remove_keyword(uint64_t permanent_id, const std::string& keyword) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    if (perm != nullptr) {
        perm->remove_keyword(keyword);
    }
}

void GameContextImpl::attach(uint64_t equipment_id, uint64_t target_id) {
    auto* equipment = game_.zone_manager().find_permanent(equipment_id);
    auto* target = game_.zone_manager().find_permanent(target_id);
    if ((equipment == nullptr) || (target == nullptr)) {
        return;
    }
    if (auto old_target = equipment->attached_to()) {
        auto* old = game_.zone_manager().find_permanent(*old_target);
        if (old != nullptr) {
            old->remove_attachment(equipment_id);
        }
    }
    equipment->attach_to(target_id);
    target->add_attachment(equipment_id);
}

auto GameContextImpl::get_opponents(uint64_t player_id) const -> std::vector<uint64_t> {
    std::vector<uint64_t> result;
    for (const auto& p : game_.players()) {
        if (p.id() != player_id && p.is_alive()) {
            result.push_back(p.id());
        }
    }
    return result;
}

auto GameContextImpl::get_all_creatures() const -> std::vector<uint64_t> {
    return game_.zone_manager().get_all_creatures();
}

int GameContextImpl::get_life_total(uint64_t player_id) const {
    auto* player = game_.find_player(player_id);
    return (player != nullptr) ? player->life() : 0;
}

int GameContextImpl::get_counters(uint64_t permanent_id, const std::string& counter_type) const {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    return (perm != nullptr) ? perm->get_counters(counter_type) : 0;
}

void GameContextImpl::blight(uint64_t player_id, int count) {
    auto creatures = get_permanents_with_type(player_id, cle::core::CardType::Creature);
    for (size_t i = 0; i < static_cast<size_t>(count) && i < creatures.size(); ++i) {
        add_counter(creatures[i], "-1/-1", 1);
    }
}

auto GameContextImpl::choose_mode(uint64_t player_id, int min_choices, int max_choices,
                                  int total_modes) -> std::vector<int> {
    proto::ActionPrompt prompt;
    auto* mc = prompt.mutable_mode_choice();
    mc->set_min_choices(min_choices);
    mc->set_max_choices(max_choices);
    for (int i = 0; i < total_modes; ++i) {
        mc->add_modes("Mode " + std::to_string(i + 1));
    }

    auto response = send_prompt_and_wait(player_id, std::move(prompt));

    if (!response.indices.empty()) {
        return response.indices;
    }
    std::vector<int> result;
    for (int i = 0; i < min_choices && i < total_modes; ++i) {
        result.push_back(i);
    }
    return result;
}

std::string GameContextImpl::get_current_phase() const {
    return phase_to_string(game_.turn_machine().current_phase());
}

auto GameContextImpl::get_permanents_with_subtype(uint64_t player_id,
                                                  const std::string& subtype) const
    -> std::vector<uint64_t> {
    return game_.zone_manager().get_permanents_with_subtype(player_id, subtype);
}

auto GameContextImpl::get_graveyard_cards_with_subtype(uint64_t player_id,
                                                       const std::string& subtype) const
    -> std::vector<uint64_t> {
    return game_.zone_manager().get_graveyard_cards_with_subtype(player_id, subtype);
}

std::string GameContextImpl::choose_creature_type(uint64_t player_id) {
    proto::ActionPrompt prompt;
    auto* ct = prompt.mutable_creature_type();
    ct->set_reason("Choose a creature type");
    static const std::array<const char*, 20> common_types = {
        "Human",   "Elf",     "Goblin",  "Zombie",  "Vampire", "Angel",  "Dragon",
        "Beast",   "Elemental", "Wizard", "Knight",  "Soldier", "Spirit", "Demon",
        "Merfolk", "Warrior", "Rogue",   "Cleric",  "Cat",     "Bird",
    };
    for (const auto* type : common_types) {
        ct->add_suggestions(type);
    }

    auto response = send_prompt_and_wait(player_id, std::move(prompt));
    if (!response.text.empty()) {
        return response.text;
    }
    return "Human";
}

bool GameContextImpl::player_may(uint64_t player_id, const std::string& question) {
    proto::ActionPrompt prompt;
    prompt.mutable_yes_no()->set_question(question);

    auto response = send_prompt_and_wait(player_id, std::move(prompt));
    return response.flag;
}

std::string GameContextImpl::choose_color(uint64_t player_id) {
    proto::ActionPrompt prompt;
    auto* cc = prompt.mutable_color_choice();
    cc->set_reason("Choose a color");
    cc->add_legal_colors("White");
    cc->add_legal_colors("Blue");
    cc->add_legal_colors("Black");
    cc->add_legal_colors("Red");
    cc->add_legal_colors("Green");

    auto response = send_prompt_and_wait(player_id, std::move(prompt));
    if (!response.text.empty()) {
        return response.text;
    }
    return "White";
}

uint64_t GameContextImpl::search_library_to_battlefield(uint64_t player_id,
                                                        const std::string& filter, bool tapped) {
    auto card_id = search_library(player_id, filter);
    if (card_id == 0) {
        return 0;
    }

    auto lib_size = game_.zone_manager().library_size(player_id);
    std::shared_ptr<cle::core::Card> found_card;
    std::vector<std::shared_ptr<cle::core::Card>> remaining;
    for (int i = 0; i < lib_size; ++i) {
        auto card = game_.zone_manager().remove_top(player_id);
        if (!card) {
            break;
        }
        if ((*card)->instance_id() == card_id && !found_card) {
            found_card = std::move(*card);
        } else {
            remaining.push_back(std::move(*card));
        }
    }
    for (auto it = remaining.rbegin(); it != remaining.rend(); ++it) {
        game_.zone_manager().put_on_top(player_id, *it);
    }
    game_.zone_manager().shuffle_library(player_id);

    if (!found_card) {
        return 0;
    }

    auto perm_id = game_.zone_manager().add_to_battlefield(found_card, player_id, player_id);
    if (tapped) {
        auto* perm = game_.zone_manager().find_permanent(perm_id);
        if (perm != nullptr) {
            perm->tap();
        }
    }
    emit_zone_transfer(card_id, proto::ZONE_LIBRARY, proto::ZONE_BATTLEFIELD, player_id);
    return perm_id;
}

void GameContextImpl::register_delayed_trigger(uint64_t permanent_id, const std::string& event_type,
                                               const std::string& effect_description) {
    register_delayed_trigger(permanent_id, event_type, effect_description, sol::nil);
}

void GameContextImpl::register_delayed_trigger(uint64_t permanent_id, const std::string& event_type,
                                               const std::string& effect_description,
                                               sol::function callback) {
    std::optional<sol::function> cb;
    if (callback.valid()) {
        cb = std::move(callback);
    }
    bool const has_callback = cb.has_value();
    game_.add_delayed_trigger(Game::DelayedTrigger{
        .source_permanent_id = permanent_id,
        .event_type = event_type,
        .effect_description = effect_description,
        .callback = std::move(cb),
    });
    spdlog::debug("Registered delayed trigger from permanent {} on {}{}", permanent_id, event_type,
                  has_callback ? " (with callback)" : "");
}

void GameContextImpl::animate(uint64_t permanent_id, int power, int toughness, bool until_eot) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    if (perm != nullptr) {
        perm->animate(power, toughness, until_eot);
    }
}

auto GameContextImpl::get_subtypes(uint64_t permanent_id) const -> std::vector<std::string> {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    return (perm != nullptr) ? perm->card()->subtypes() : std::vector<std::string>{};
}

int GameContextImpl::get_mana_value(uint64_t permanent_id) const {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    return (perm != nullptr) ? perm->card()->mana_cost().mana_value() : 0;
}

void GameContextImpl::sacrifice([[maybe_unused]] uint64_t player_id, uint64_t permanent_id) {
    auto* perm = game_.zone_manager().find_permanent(permanent_id);
    bool const was_creature = (perm != nullptr) && perm->is_creature();
    auto controller_id = (perm != nullptr) ? perm->controller_id()
                                           : game_.turn_machine().active_player_id();
    auto removed = game_.zone_manager().remove_from_battlefield(permanent_id);
    if (removed) {
        auto card_name = removed->card()->name();
        game_.zone_manager().add_to_graveyard(removed->owner_id(), removed->card());
        emit_zone_transfer(permanent_id, proto::ZONE_BATTLEFIELD, proto::ZONE_GRAVEYARD,
                           removed->owner_id());

        proto::GameEvent event;
        event.set_game_id(game_.game_id());
        auto* pd = event.mutable_permanent_destroyed();
        pd->set_permanent_id(permanent_id);
        pd->set_card_name(card_name);
        game_.broadcaster().emit(std::move(event));

        if (was_creature) {
            fire_death_triggers(permanent_id, controller_id);
        }
    }
}

void GameContextImpl::counter_spell(uint64_t stack_entry_id) {
    auto removed = game_.the_stack().remove(stack_entry_id);
    if (!removed) {
        spdlog::warn("counter_spell: stack entry {} not found", stack_entry_id);
        return;
    }
    if (auto* spell = std::get_if<SpellEntry>(&removed->content)) {
        game_.zone_manager().add_to_graveyard(removed->controller_id, spell->card);

        proto::GameEvent event;
        event.set_game_id(game_.game_id());
        auto* sr = event.mutable_spell_resolved();
        sr->set_stack_entry_id(stack_entry_id);
        sr->set_card_name(spell->card->name());
        game_.broadcaster().emit(std::move(event));
    }
    spdlog::debug("Stack entry {} was countered", stack_entry_id);
}

auto GameContextImpl::choose_sacrifice(uint64_t player_id, const std::string& filter)
    -> uint64_t {
    auto parsed = parse_filter(filter);
    parsed.controller = TargetFilter::Controller::You;
    if (parsed.zone == TargetFilter::Zone::Any) {
        parsed.zone = TargetFilter::Zone::Battlefield;
    }
    auto legal = apply_filter(parsed, player_id, game_.zone_manager(), game_.players());
    if (legal.empty()) {
        return 0;
    }
    if (legal.size() == 1) {
        sacrifice(player_id, legal[0]);
        return legal[0];
    }

    proto::ActionPrompt prompt;
    auto* tp = prompt.mutable_target();
    tp->set_filter("sacrifice_" + filter);
    for (auto id : legal) {
        tp->add_legal_targets(id);
    }

    auto response = send_prompt_and_wait(player_id, std::move(prompt));
    uint64_t chosen = response.target_id;
    if (std::ranges::find(legal, chosen) == legal.end()) {
        chosen = legal[0];
    }
    sacrifice(player_id, chosen);
    return chosen;
}

auto GameContextImpl::choose_from_legends(uint64_t player_id,
                                           const std::vector<uint64_t>& legend_ids) -> uint64_t {
    proto::ActionPrompt prompt;
    auto* tp = prompt.mutable_target();
    tp->set_filter("legend_rule_choose");
    for (auto id : legend_ids) {
        tp->add_legal_targets(id);
    }
    auto response = send_prompt_and_wait(player_id, std::move(prompt));
    if (response.target_id != 0 &&
        std::ranges::find(legend_ids, response.target_id) != legend_ids.end()) {
        return response.target_id;
    }
    return legend_ids[0];
}

void GameContextImpl::fire_death_triggers(uint64_t permanent_id, uint64_t controller_id) {
    cle::triggers::TriggerEvent death_event;
    death_event.type = cle::triggers::TriggerType::OnDeath;
    death_event.source_id = permanent_id;
    death_event.player_id = controller_id;

    game_.fire_triggers(cle::triggers::TriggerType::OnAnotherCreatureDies, death_event);
    game_.fire_delayed_triggers("on_death", permanent_id);
}

}  // namespace mtg::engine
