#pragma once

#include <optional>
#include <string>
#include <unordered_map>
#include <vector>

#include <cle/abilities/activated_ability.hpp>
#include <cle/abilities/modal.hpp>
#include <cle/abilities/static_ability.hpp>
#include <cle/core/card_type.hpp>
#include <cle/core/color.hpp>
#include <cle/core/creature_stats.hpp>
#include <cle/core/internal/instance_generator.hpp>
#include <cle/mana/mana_cost.hpp>
#include <cle/triggers/trigger_type.hpp>
#include <sol/sol.hpp>

namespace cle::core {

class Card {
public:
    Card(std::string name, CardType type)
        : name_{std::move(name)},
          type_{type},
          instance_id_{internal::generate_card_instance_id()} {}

    Card(const Card &other)
        : name_{other.name_},
          type_{other.type_},
          mana_cost_{other.mana_cost_},
          colors_{other.colors_},
          oracle_text_{other.oracle_text_},
          flavor_text_{other.flavor_text_},
          subtypes_{other.subtypes_},
          keywords_{other.keywords_},
          creature_stats_{other.creature_stats_},
          instance_id_{internal::generate_card_instance_id()},
          triggers_{other.triggers_},
          activated_abilities_{other.activated_abilities_},
          static_abilities_{other.static_abilities_},
          modal_{other.modal_} {}

    Card &operator=(const Card &other) {
        if (this != &other) {
            name_ = other.name_;
            type_ = other.type_;
            mana_cost_ = other.mana_cost_;
            colors_ = other.colors_;
            oracle_text_ = other.oracle_text_;
            flavor_text_ = other.flavor_text_;
            subtypes_ = other.subtypes_;
            keywords_ = other.keywords_;
            creature_stats_ = other.creature_stats_;
            triggers_ = other.triggers_;
            activated_abilities_ = other.activated_abilities_;
            static_abilities_ = other.static_abilities_;
            modal_ = other.modal_;
            instance_id_ = internal::generate_card_instance_id();
        }
        return *this;
    }

    Card(Card &&) noexcept = default;
    Card &operator=(Card &&) noexcept = default;

    // getters
    [[nodiscard]] auto instance_id() const -> uint64_t { return instance_id_; }
    [[nodiscard]] auto name() const -> const std::string & { return name_; }
    [[nodiscard]] auto type() const -> CardType { return type_; }
    [[nodiscard]] auto mana_cost() const -> const cle::mana::ManaCost & { return mana_cost_; }
    [[nodiscard]] auto colors() const -> Color { return colors_; }
    [[nodiscard]] auto oracle_text() const -> const std::string & { return oracle_text_; }
    [[nodiscard]] auto flavor_text() const -> const std::string & { return flavor_text_; }
    [[nodiscard]] auto subtypes() const -> const std::vector<std::string> & { return subtypes_; }
    [[nodiscard]] auto keywords() const -> const std::vector<std::string> & { return keywords_; }
    [[nodiscard]] auto creature_stats() const -> std::optional<CreatureStats> {
        return creature_stats_;
    }
    [[nodiscard]] auto triggers() const
        -> const std::unordered_map<cle::triggers::TriggerType, sol::function> & {
        return triggers_;
    }
    [[nodiscard]] auto get_trigger(cle::triggers::TriggerType type) const
        -> std::optional<sol::function> {
        auto it = triggers_.find(type);
        if (it != triggers_.end()) {
            return it->second;
        }
        return std::nullopt;
    }

    // setters
    void set_mana_cost(cle::mana::ManaCost cost) { mana_cost_ = cost; }
    void set_colors(Color colors) { colors_ = colors; }
    void set_oracle_text(std::string text) { oracle_text_ = std::move(text); }
    void set_flavor_text(std::string text) { flavor_text_ = std::move(text); }
    void add_subtype(std::string subtype) { subtypes_.push_back(std::move(subtype)); }
    void add_keyword(std::string keyword) { keywords_.push_back(std::move(keyword)); }
    void set_creature_stats(CreatureStats stats) { creature_stats_ = stats; }
    void set_instance_id(uint64_t id) { instance_id_ = id; }
    void set_triggers(std::unordered_map<cle::triggers::TriggerType, sol::function> triggers) {
        triggers_ = std::move(triggers);
    }

    // activated abilities
    [[nodiscard]] auto activated_abilities() const
        -> const std::vector<cle::abilities::ActivatedAbility> & {
        return activated_abilities_;
    }
    void add_activated_ability(cle::abilities::ActivatedAbility ability) {
        activated_abilities_.push_back(std::move(ability));
    }

    // static abilities
    [[nodiscard]] auto static_abilities() const
        -> const std::vector<cle::abilities::StaticAbility> & {
        return static_abilities_;
    }
    void add_static_ability(cle::abilities::StaticAbility ability) {
        static_abilities_.push_back(std::move(ability));
    }

    // modal
    [[nodiscard]] auto modal() const -> const std::optional<cle::abilities::ModalAbility> & {
        return modal_;
    }
    void set_modal(cle::abilities::ModalAbility modal) { modal_ = std::move(modal); }

private:
    std::string name_;
    CardType type_;
    cle::mana::ManaCost mana_cost_;
    Color colors_{Color::Colorless};
    std::string oracle_text_;
    std::string flavor_text_;
    std::vector<std::string> subtypes_;
    std::vector<std::string> keywords_;
    std::optional<CreatureStats>
        creature_stats_;    // optional since not all cards are creatures obviously
    uint64_t instance_id_;  // to prevent same cards but different instance being
                            // registered as the same
    std::unordered_map<cle::triggers::TriggerType, sol::function> triggers_;
    std::vector<cle::abilities::ActivatedAbility> activated_abilities_;
    std::vector<cle::abilities::StaticAbility> static_abilities_;
    std::optional<cle::abilities::ModalAbility> modal_;
};
};  // namespace cle::core
