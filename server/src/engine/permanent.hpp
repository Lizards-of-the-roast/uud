#pragma once

#include <cstdint>
#include <memory>
#include <optional>
#include <string>
#include <unordered_map>
#include <vector>

#include <cle/core/card.hpp>

namespace mtg::engine {

class Permanent {
public:
    Permanent(std::shared_ptr<cle::core::Card> card, uint64_t controller_id, uint64_t owner_id);

    [[nodiscard]] auto id() const -> uint64_t { return card_->instance_id(); }
    [[nodiscard]] auto card() const -> const std::shared_ptr<cle::core::Card>& { return card_; }
    [[nodiscard]] auto controller_id() const -> uint64_t { return controller_id_; }
    [[nodiscard]] auto owner_id() const -> uint64_t { return owner_id_; }
    [[nodiscard]] auto is_tapped() const -> bool { return tapped_; }
    [[nodiscard]] auto has_summoning_sickness() const -> bool { return summoning_sick_; }
    [[nodiscard]] auto damage_marked() const -> int { return damage_marked_; }
    [[nodiscard]] auto is_token() const -> bool { return is_token_; }

    void set_controller(uint64_t id) { controller_id_ = id; }
    void tap() { tapped_ = true; }
    void untap() { tapped_ = false; }
    void set_summoning_sickness(bool sick) { summoning_sick_ = sick; }
    void mark_damage(int amount) { damage_marked_ += amount; }
    void clear_damage() { damage_marked_ = 0; deathtouch_marked_ = false; }
    void mark_deathtouch() { deathtouch_marked_ = true; }
    [[nodiscard]] auto is_deathtouch_marked() const -> bool { return deathtouch_marked_; }
    void set_token(bool token) { is_token_ = token; }

    [[nodiscard]] auto power_modifier() const -> int { return power_mod_; }
    [[nodiscard]] auto toughness_modifier() const -> int { return toughness_mod_; }
    void modify_power_toughness(int power_mod, int toughness_mod);
    [[nodiscard]] auto effective_power() const -> int;
    [[nodiscard]] auto effective_toughness() const -> int;
    [[nodiscard]] auto has_lethal_damage() const -> bool;

    void grant_keyword(const std::string& keyword);
    void remove_keyword(const std::string& keyword);
    [[nodiscard]] auto has_keyword(const std::string& keyword) const -> bool;
    [[nodiscard]] auto all_keywords() const -> std::vector<std::string>;

    void add_counter(const std::string& type, int amount);
    void remove_counter(const std::string& type, int amount);
    [[nodiscard]] auto get_counters(const std::string& type) const -> int;
    [[nodiscard]] auto all_counters() const -> const std::unordered_map<std::string, int>& {
        return counters_;
    }

    [[nodiscard]] auto attached_to() const -> std::optional<uint64_t> { return attached_to_; }
    [[nodiscard]] auto attachments() const -> const std::vector<uint64_t>& { return attachments_; }
    void attach_to(uint64_t target_id) { attached_to_ = target_id; }
    void detach() { attached_to_ = std::nullopt; }
    void add_attachment(uint64_t equipment_id);
    void remove_attachment(uint64_t equipment_id);

    [[nodiscard]] auto is_animated() const -> bool { return animated_; }
    [[nodiscard]] auto is_animated_until_eot() const -> bool { return animated_until_eot_; }
    void animate(int power, int toughness, bool until_eot = true);
    void remove_animation();

    [[nodiscard]] auto is_creature() const -> bool;
    [[nodiscard]] auto is_land() const -> bool;
    [[nodiscard]] auto has_protection_from(const Permanent& source) const -> bool;
    [[nodiscard]] auto has_protection_from_color(const std::string& color) const -> bool;

    void add_keyword_counter(const std::string& keyword);
    void remove_keyword_counter(const std::string& keyword);

private:
    std::shared_ptr<cle::core::Card> card_;
    uint64_t controller_id_;
    uint64_t owner_id_;
    bool tapped_{false};
    bool summoning_sick_{true};
    int damage_marked_{0};
    bool deathtouch_marked_{false};
    bool is_token_{false};
    int power_mod_{0};
    int toughness_mod_{0};
    std::vector<std::string> granted_keywords_;
    std::unordered_map<std::string, int> counters_;
    std::optional<uint64_t> attached_to_;
    std::vector<uint64_t> attachments_;
    bool animated_{false};
    bool animated_until_eot_{false};
    int animated_power_{0};
    int animated_toughness_{0};
};

}  // namespace mtg::engine
