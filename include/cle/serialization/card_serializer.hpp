#pragma once

#include <expected>
#include <filesystem>
#include <string>

#include <cle/core/card.hpp>
#include <cle/core/card_type.hpp>
#include <cle/core/color.hpp>
#include <cle/lua/engine.hpp>
#include <cle/card.pb.h>

namespace cle::serialization {

inline auto to_proto(core::CardType t) -> proto::CardType {
    switch (t) {
        case core::CardType::Creature:
            return proto::CARD_TYPE_CREATURE;
        case core::CardType::Instant:
            return proto::CARD_TYPE_INSTANT;
        case core::CardType::Sorcery:
            return proto::CARD_TYPE_SORCERY;
        case core::CardType::Enchantment:
            return proto::CARD_TYPE_ENCHANTMENT;
        case core::CardType::Artifact:
            return proto::CARD_TYPE_ARTIFACT;
        case core::CardType::Planeswalker:
            return proto::CARD_TYPE_PLANESWALKER;
        case core::CardType::Land:
            return proto::CARD_TYPE_LAND;
    }
    return proto::CARD_TYPE_CREATURE;
}

inline auto from_proto(proto::CardType t) -> core::CardType {
    switch (t) {
        case proto::CARD_TYPE_CREATURE:
            return core::CardType::Creature;
        case proto::CARD_TYPE_INSTANT:
            return core::CardType::Instant;
        case proto::CARD_TYPE_SORCERY:
            return core::CardType::Sorcery;
        case proto::CARD_TYPE_ENCHANTMENT:
            return core::CardType::Enchantment;
        case proto::CARD_TYPE_ARTIFACT:
            return core::CardType::Artifact;
        case proto::CARD_TYPE_PLANESWALKER:
            return core::CardType::Planeswalker;
        case proto::CARD_TYPE_LAND:
            return core::CardType::Land;
        default:
            return core::CardType::Creature;
    }
}

inline auto to_proto(mana::ManaColor c) -> proto::ManaColor {
    switch (c) {
        case mana::ManaColor::White:
            return proto::MANA_COLOR_WHITE;
        case mana::ManaColor::Blue:
            return proto::MANA_COLOR_BLUE;
        case mana::ManaColor::Black:
            return proto::MANA_COLOR_BLACK;
        case mana::ManaColor::Red:
            return proto::MANA_COLOR_RED;
        case mana::ManaColor::Green:
            return proto::MANA_COLOR_GREEN;
        case mana::ManaColor::Colorless:
            return proto::MANA_COLOR_COLORLESS;
    }
    return proto::MANA_COLOR_COLORLESS;
}

inline auto from_proto(proto::ManaColor c) -> mana::ManaColor {
    switch (c) {
        case proto::MANA_COLOR_WHITE:
            return mana::ManaColor::White;
        case proto::MANA_COLOR_BLUE:
            return mana::ManaColor::Blue;
        case proto::MANA_COLOR_BLACK:
            return mana::ManaColor::Black;
        case proto::MANA_COLOR_RED:
            return mana::ManaColor::Red;
        case proto::MANA_COLOR_GREEN:
            return mana::ManaColor::Green;
        default:
            return mana::ManaColor::Colorless;
    }
}

inline auto serialize_card(const core::Card& card, const std::string& lua_source_path = {})
    -> proto::CardData {
    proto::CardData data;

    data.set_name(card.name());
    data.set_type(to_proto(card.type()));

    // mana cost
    auto* mc = data.mutable_mana_cost();
    const auto& cost = card.mana_cost();
    mc->set_colorless(cost.colorless);
    mc->set_white(cost.white);
    mc->set_blue(cost.blue);
    mc->set_black(cost.black);
    mc->set_red(cost.red);
    mc->set_green(cost.green);
    mc->set_x_count(cost.x_count);
    for (const auto& hybrid : cost.hybrid_costs) {
        auto* h = mc->add_hybrid_costs();
        h->set_primary(to_proto(hybrid.primary));
        h->set_secondary(to_proto(hybrid.secondary));
    }

    data.set_colors(static_cast<uint32_t>(card.colors()));
    data.set_oracle_text(card.oracle_text());
    data.set_flavor_text(card.flavor_text());

    for (const auto& st : card.subtypes()) {
        data.add_subtypes(st);
    }
    for (const auto& kw : card.keywords()) {
        data.add_keywords(kw);
    }

    if (auto stats = card.creature_stats()) {
        auto* cs = data.mutable_creature_stats();
        cs->set_power(stats->power);
        cs->set_toughness(stats->toughness);
    }

    data.set_instance_id(card.instance_id());

    for (const auto& [type, _] : card.triggers()) {
        data.add_trigger_types(static_cast<int32_t>(type));
    }

    for (const auto& ab : card.activated_abilities()) {
        auto* a = data.add_activated_abilities();
        a->set_cost_text(ab.cost_text);
        a->set_effect_text(ab.effect_text);
        a->set_sorcery_speed_only(ab.sorcery_speed_only);
    }

    for (const auto& sa : card.static_abilities()) {
        auto* s = data.add_static_abilities();
        s->set_description(sa.description);
    }

    if (auto& modal = card.modal()) {
        auto* m = data.mutable_modal();
        m->set_min_choices(modal->min_choices);
        m->set_max_choices(modal->max_choices);
        for (const auto& mode : modal->modes) {
            auto* mc = m->add_modes();
            mc->set_text(mode.text);
        }
    }

    if (!lua_source_path.empty()) {
        data.set_lua_source_path(lua_source_path);
    }

    return data;
}

inline auto deserialize_card(const proto::CardData& data, lua::CardEngine& engine)
    -> std::expected<std::shared_ptr<core::Card>, std::string> {
    std::shared_ptr<core::Card> card;

    if (!data.lua_source_path().empty()) {
        auto result = engine.load_card_from_file(data.lua_source_path());
        if (!result) {
            return std::unexpected("Failed to reload Lua source: " + result.error().detail);
        }
        card = *result;
    } else {
        card = std::make_shared<core::Card>(data.name(), from_proto(data.type()));
    }

    // mana cost
    mana::ManaCost cost;
    cost.colorless = data.mana_cost().colorless();
    cost.white = data.mana_cost().white();
    cost.blue = data.mana_cost().blue();
    cost.black = data.mana_cost().black();
    cost.red = data.mana_cost().red();
    cost.green = data.mana_cost().green();
    cost.x_count = data.mana_cost().x_count();
    for (const auto& h : data.mana_cost().hybrid_costs()) {
        cost.hybrid_costs.push_back({from_proto(h.primary()), from_proto(h.secondary())});
    }
    card->set_mana_cost(cost);

    card->set_colors(static_cast<core::Color>(data.colors()));
    card->set_oracle_text(data.oracle_text());
    card->set_flavor_text(data.flavor_text());

    if (data.lua_source_path().empty()) {
        for (const auto& st : data.subtypes()) {
            card->add_subtype(st);
        }
        for (const auto& kw : data.keywords()) {
            card->add_keyword(kw);
        }
    }

    if (data.has_creature_stats()) {
        card->set_creature_stats(
            core::CreatureStats{data.creature_stats().power(), data.creature_stats().toughness()});
    }

    // keep the same instance id
    if (data.instance_id() != 0) {
        card->set_instance_id(data.instance_id());
    }

    return card;
}

}  // namespace cle::serialization
