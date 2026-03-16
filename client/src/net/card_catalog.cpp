#include "card_catalog.hpp"

using namespace Game;

#include <algorithm>

Card_Catalog card_catalog;

static Card Make_Instant(const std::string &name, Mana_Cost cost, const std::string &text) {
    Card c;
    c.name = name;
    c.type = Card_Type::Instant;
    c.mana_cost = cost;
    c.oracle_text = text;
    return c;
}

static Card Make_Sorcery(const std::string &name, Mana_Cost cost, const std::string &text) {
    Card c;
    c.name = name;
    c.type = Card_Type::Sorcery;
    c.mana_cost = cost;
    c.oracle_text = text;
    return c;
}

static Card Make_Creature(const std::string &name, Mana_Cost cost,
                          const std::vector<std::string> &subtypes, const std::string &text,
                          int power, int toughness) {
    Card c;
    c.name = name;
    c.type = Card_Type::Creature;
    c.mana_cost = cost;
    c.oracle_text = text;
    c.subtypes = subtypes;
    c.creature_stats = Creature_Stats{power, toughness};
    return c;
}

static Card Make_Basic_Land(const std::string &name, const std::string &subtype,
                            const std::string &text) {
    Card c;
    c.name = name;
    c.type = Card_Type::Land;
    c.oracle_text = text;
    c.subtypes = {subtype};
    c.keywords = {"basic"};
    return c;
}

// this will be done in the backend at some point im just lazy
void Card_Catalog::Load_Default_Cards() {
    cards_ = {
        Make_Instant("Lightning Bolt", {.red = 1}, "Deal 3 damage to any target."),
        Make_Instant("Counterspell", {.blue = 2}, "Counter target spell."),
        Make_Instant("Giant Growth", {.green = 1},
                     "Target creature gets +3/+3 until end of turn."),
        Make_Instant("Dark Ritual", {.black = 1}, "Add {B}{B}{B}."),
        Make_Instant("Healing Salve", {.white = 1}, "Gain 3 life or prevent 3 damage."),
        Make_Creature("Llanowar Elves", {.green = 1}, {"Elf", "Druid"}, "Tap: Add {G}.", 1, 1),
        Make_Creature("Grizzly Bears", {.colorless = 1, .green = 1}, {"Bear"}, "", 2, 2),
        Make_Creature("Serra Angel", {.colorless = 3, .white = 2}, {"Angel"},
                      "Flying, vigilance.", 4, 4),
        Make_Creature("Shivan Dragon", {.colorless = 4, .red = 2}, {"Dragon"},
                      "Flying. {R}: +1/+0 until end of turn.", 5, 5),
        Make_Creature("Air Elemental", {.colorless = 3, .blue = 2}, {"Elemental"}, "Flying.", 4,
                      4),
        Make_Creature("Sengir Vampire", {.colorless = 3, .black = 2}, {"Vampire"},
                      "Flying. Whenever a creature dealt damage by Sengir Vampire this turn dies, "
                      "put a +1/+1 counter on Sengir Vampire.",
                      4, 4),
        Make_Creature("Savannah Lions", {.white = 1}, {"Cat"}, "", 2, 1),
        Make_Creature("Goblin Guide", {.red = 1}, {"Goblin", "Scout"}, "Haste.", 2, 2),
        Make_Creature("Tarmogoyf", {.colorless = 1, .green = 1}, {"Lhurgoyf"},
                      "Tarmogoyf's power is equal to the number of card types among cards in all "
                      "graveyards and its toughness is that number plus 1.",
                      0, 1),
        Make_Sorcery("Wrath of God", {.colorless = 2, .white = 2},
                     "Destroy all creatures. They can't be regenerated."),
        Make_Instant("Doom Blade", {.colorless = 1, .black = 1},
                     "Destroy target nonblack creature."),
        Make_Instant("Mana Leak", {.colorless = 1, .blue = 1},
                     "Counter target spell unless its controller pays {3}."),
        Make_Instant("Shock", {.red = 1}, "Deal 2 damage to any target."),
        Make_Instant("Murder", {.colorless = 1, .black = 2}, "Destroy target creature."),
        Make_Instant("Opt", {.blue = 1}, "Scry 1. Draw a card."),
        Make_Basic_Land("Plains", "Plains", "Tap: Add {W}."),
        Make_Basic_Land("Island", "Island", "Tap: Add {U}."),
        Make_Basic_Land("Swamp", "Swamp", "Tap: Add {B}."),
        Make_Basic_Land("Mountain", "Mountain", "Tap: Add {R}."),
        Make_Basic_Land("Forest", "Forest", "Tap: Add {G}."),
    };
}

std::vector<const Card *> Card_Catalog::Search(const std::string &query) const {
    std::vector<const Card *> results;
    if (query.empty()) {
        for (const auto &card : cards_)
            results.push_back(&card);
        return results;
    }

    std::string lower_query = query;
    std::transform(lower_query.begin(), lower_query.end(), lower_query.begin(), ::tolower);

    for (const auto &card : cards_) {
        std::string lower_name = card.name;
        std::transform(lower_name.begin(), lower_name.end(), lower_name.begin(), ::tolower);
        if (lower_name.find(lower_query) != std::string::npos)
            results.push_back(&card);
    }
    return results;
}

const Card *Card_Catalog::Find(const std::string &name) const {
    for (const auto &card : cards_)
        if (card.name == name)
            return &card;
    return nullptr;
}

const std::vector<Card> &Card_Catalog::All() const {
    return cards_;
}
