#include "engine/permanent.hpp"

#include <algorithm>
#include <array>

namespace mtg::engine {

Permanent::Permanent(std::shared_ptr<cle::core::Card> card, uint64_t controller_id,
                     uint64_t owner_id)
    : card_{std::move(card)}, controller_id_{controller_id}, owner_id_{owner_id} {}

void Permanent::modify_power_toughness(int power_mod, int toughness_mod) {
    power_mod_ += power_mod;
    toughness_mod_ += toughness_mod;
}

int Permanent::effective_power() const {
    int const counter_mod = get_counters("+1/+1") - get_counters("-1/-1");
    if (animated_) {
        return animated_power_ + power_mod_ + counter_mod;
    }
    auto stats = card_->creature_stats();
    int const base = stats ? stats->power : 0;
    return base + power_mod_ + counter_mod;
}

int Permanent::effective_toughness() const {
    int const counter_mod = get_counters("+1/+1") - get_counters("-1/-1");
    if (animated_) {
        return animated_toughness_ + toughness_mod_ + counter_mod;
    }
    auto stats = card_->creature_stats();
    int const base = stats ? stats->toughness : 0;
    return base + toughness_mod_ + counter_mod;
}

bool Permanent::has_lethal_damage() const {
    if (!is_creature()) {
        return false;
    }
    return damage_marked_ >= effective_toughness();
}

void Permanent::grant_keyword(const std::string& keyword) {
    if (!has_keyword(keyword)) {
        granted_keywords_.push_back(keyword);
    }
}

void Permanent::remove_keyword(const std::string& keyword) {
    std::erase(granted_keywords_, keyword);
}

bool Permanent::has_keyword(const std::string& keyword) const {
    if (std::ranges::find(granted_keywords_, keyword) != granted_keywords_.end()) {
        return true;
    }
    const auto& kws = card_->keywords();
    return std::ranges::find(kws, keyword) != kws.end();
}

std::vector<std::string> Permanent::all_keywords() const {
    std::vector<std::string> result = card_->keywords();
    for (const auto& kw : granted_keywords_) {
        if (std::ranges::find(result, kw) == result.end()) {
            result.push_back(kw);
        }
    }
    return result;
}

void Permanent::add_counter(const std::string& type, int amount) {
    counters_[type] += amount;
}

void Permanent::remove_counter(const std::string& type, int amount) {
    auto it = counters_.find(type);
    if (it != counters_.end()) {
        it->second = std::max(0, it->second - amount);
        if (it->second == 0) {
            counters_.erase(it);
        }
    }
}

int Permanent::get_counters(const std::string& type) const {
    auto it = counters_.find(type);
    return it != counters_.end() ? it->second : 0;
}

void Permanent::add_attachment(uint64_t equipment_id) {
    attachments_.push_back(equipment_id);
}

void Permanent::remove_attachment(uint64_t equipment_id) {
    std::erase(attachments_, equipment_id);
}

void Permanent::animate(int power, int toughness, bool until_eot) {
    animated_ = true;
    animated_until_eot_ = until_eot;
    animated_power_ = power;
    animated_toughness_ = toughness;
}

void Permanent::remove_animation() {
    animated_ = false;
    animated_until_eot_ = false;
    animated_power_ = 0;
    animated_toughness_ = 0;
}

bool Permanent::is_creature() const {
    return card_->type() == cle::core::CardType::Creature || animated_;
}

bool Permanent::is_land() const {
    return card_->type() == cle::core::CardType::Land;
}

void Permanent::add_keyword_counter(const std::string& keyword) {
    std::string counter_name = keyword + " counter";
    add_counter(counter_name, 1);
    grant_keyword(keyword);
}

void Permanent::remove_keyword_counter(const std::string& keyword) {
    std::string counter_name = keyword + " counter";
    int const current = get_counters(counter_name);
    if (current > 0) {
        remove_counter(counter_name, 1);
        if (get_counters(counter_name) == 0) {
            remove_keyword(keyword);
        }
    }
}

bool Permanent::has_protection_from_color(const std::string& color) const {
    std::string keyword = "Protection from " + color;
    return has_keyword(keyword);
}

bool Permanent::has_protection_from(const Permanent& source) const {
    auto source_colors = source.card()->colors();
    if (source_colors == cle::core::Color::Colorless) {
        return false;
    }
    static const std::array<std::pair<cle::core::Color, std::string>, 5> color_map = {{
        {cle::core::Color::White, "white"},
        {cle::core::Color::Blue, "blue"},
        {cle::core::Color::Black, "black"},
        {cle::core::Color::Red, "red"},
        {cle::core::Color::Green, "green"},
    }};
    return std::ranges::any_of(color_map, [&](const auto& entry) {
        return (source_colors & entry.first) != cle::core::Color::Colorless &&
               has_protection_from_color(entry.second);
    });
}

}  // namespace mtg::engine
