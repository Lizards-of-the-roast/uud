#pragma once

#include <cstdint>
#include <optional>
#include <string>
#include <vector>

namespace Game {

enum class Card_Type {
    Creature,
    Instant,
    Sorcery,
    Enchantment,
    Artifact,
    Planeswalker,
    Land,
};

enum class Mana_Color {
    Colorless,
    White,
    Blue,
    Black,
    Red,
    Green,
};

struct Hybrid_Cost {
    Mana_Color primary;
    Mana_Color secondary;
};

struct Mana_Cost {
    int colorless = 0;
    int white = 0;
    int blue = 0;
    int black = 0;
    int red = 0;
    int green = 0;
    int x_count = 0;
    std::vector<Hybrid_Cost> hybrid_costs;

    int cmc() const { return colorless + white + blue + black + red + green + (int)hybrid_costs.size(); }
};

struct Creature_Stats {
    int power;
    int toughness;
};

struct Activated_Ability {
    std::string cost_text;
    std::string effect_text;
    bool sorcery_speed_only = false;
};

struct Static_Ability {
    std::string description;
};

struct Modal_Choice {
    std::string text;
};

struct Modal_Ability {
    int min_choices = 0;
    int max_choices = 0;
    std::vector<Modal_Choice> modes;
};

typedef uint32_t Card_ID;
struct Card {
    std::string name;
    Card_Type type;
    Mana_Cost mana_cost;
    uint32_t colors = 0;
    std::string oracle_text;
    std::string flavor_text;
    std::vector<std::string> subtypes;
    std::vector<std::string> keywords;
    std::optional<Creature_Stats> creature_stats;
    Card_ID instance_id = 0;
    std::vector<int> trigger_types;
    std::vector<Activated_Ability> activated_abilities;
    std::vector<Static_Ability> static_abilities;
    std::optional<Modal_Ability> modal;
};

} //namespace Game
