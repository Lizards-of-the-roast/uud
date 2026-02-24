#pragma once

#include <optional>
#include <stdexcept>
#include <string>
#include <unordered_map>

#include <cle/abilities/modal.hpp>
#include <cle/core/card.hpp>
#include <cle/core/keywords.hpp>
#include <cle/mana/mana_cost.hpp>
#include <cle/triggers/trigger_type.hpp>
#include <sol/sol.hpp>

namespace cle::lua {

class CardBuilder {
public:
    explicit CardBuilder(std::string name, cle::core::CardType type)
        : card_{std::make_shared<cle::core::Card>(std::move(name), type)} {}

    auto mana_cost(const std::string& mana_str) -> CardBuilder& {
        throw_if_built();
        auto cost = cle::mana::ManaCost::from_string(mana_str);
        if (!cost)
            throw std::runtime_error("Invalid mana cost: " + std::string(mana_str));
        card_->set_mana_cost(*cost);
        return *this;
    }

    auto colors(std::vector<std::string> color_strings) -> CardBuilder& {
        throw_if_built();
        cle::core::Color result = cle::core::Color::Colorless;
        for (const auto& c : color_strings) {
            auto color = cle::core::color_from_string(c);
            if (!color)
                throw std::runtime_error("Invalid color: " + c);
            result = result | *color;
        }
        card_->set_colors(result);
        return *this;
    }

    auto oracle_text(std::string text) -> CardBuilder& {
        throw_if_built();
        card_->set_oracle_text(std::move(text));
        return *this;
    }

    auto flavor_text(std::string text) -> CardBuilder& {
        throw_if_built();
        card_->set_flavor_text(std::move(text));
        return *this;
    }

    auto subtypes(std::vector<std::string> types) -> CardBuilder& {
        throw_if_built();
        for (auto& t : types) {
            card_->add_subtype(std::move(t));
        }
        return *this;
    }

    auto keywords(std::vector<std::string> kws) -> CardBuilder& {
        throw_if_built();
        for (auto& keyword : kws) {
            card_->add_keyword(std::move(keyword));
        }
        return *this;
    }

    auto power_toughness(int power, int toughness) -> CardBuilder& {
        throw_if_built();
        card_->set_creature_stats(cle::core::CreatureStats{power, toughness});
        return *this;
    }

#define X(name, enum_val)                                                \
    auto name(sol::function fn) -> CardBuilder& {                        \
        throw_if_built();                                                \
        triggers_[cle::triggers::TriggerType::enum_val] = std::move(fn); \
        return *this;                                                    \
    }
    CARD_TRIGGER_LIST(X)
#undef X

#define X(name, str)              \
    auto name() -> CardBuilder& { \
        throw_if_built();         \
        card_->add_keyword(str);  \
        return *this;             \
    }
    CARD_KEYWORD_LIST(X)
#undef X

    auto activated_ability(std::string cost, std::string desc, sol::function fn,
                           bool sorcery_only = false) -> CardBuilder& {
        throw_if_built();
        card_->add_activated_ability(
            {std::move(cost), std::move(desc), std::move(fn), sorcery_only});
        return *this;
    }

    auto static_ability(std::string desc, sol::function fn) -> CardBuilder& {
        throw_if_built();
        card_->add_static_ability({std::move(desc), std::move(fn)});
        return *this;
    }

    auto modal(int min_choices, int max_choices) -> CardBuilder& {
        throw_if_built();
        modal_ = cle::abilities::ModalAbility{min_choices, max_choices, {}};
        return *this;
    }

    auto mode(std::string text, sol::function fn) -> CardBuilder& {
        throw_if_built();
        if (!modal_) {
            throw std::runtime_error("mode() called without first calling modal()");
        }
        modal_->modes.push_back({std::move(text), std::move(fn)});
        return *this;
    }

    auto build() -> std::shared_ptr<cle::core::Card> {
        throw_if_built();
        built_ = true;
        card_->set_triggers(std::move(triggers_));
        if (modal_) {
            card_->set_modal(std::move(*modal_));
        }
        return card_;
    }

    [[nodiscard]] auto get_triggers() const
        -> const std::unordered_map<cle::triggers::TriggerType, sol::function>& {
        return triggers_;
    }

private:
    void throw_if_built() const {
        if (built_) {
            throw std::runtime_error("CardBuilder used after build() was called");
        }
    }

    std::shared_ptr<cle::core::Card> card_;
    std::unordered_map<cle::triggers::TriggerType, sol::function> triggers_;
    std::optional<cle::abilities::ModalAbility> modal_;
    bool built_{false};
};

}  // namespace cle::lua
