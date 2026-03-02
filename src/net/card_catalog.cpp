#include "card_catalog.hpp"

#include <algorithm>
#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iostream>

Card_Catalog card_catalog;

int Deck::Total_Cards() const {
    int total = 0;
    for (const auto &e : cards)
        total += e.count;
    return total;
}

std::vector<std::string> Deck::To_Card_Names() const {
    std::vector<std::string> names;
    for (const auto &e : cards)
        for (int i = 0; i < e.count; i++)
            names.push_back(e.card_name);
    return names;
}

// this will be done in the backend at some point im just lazy
void Card_Catalog::Load_Default_Cards() {
    cards_ = {
        {"Lightning Bolt", "{R}", "Instant", "Deal 3 damage to any target.", 0, 0, false, false,
         false},
        {"Counterspell", "{U}{U}", "Instant", "Counter target spell.", 0, 0, false, false, false},
        {"Giant Growth", "{G}", "Instant", "Target creature gets +3/+3 until end of turn.", 0, 0,
         false, false, false},
        {"Dark Ritual", "{B}", "Instant", "Add {B}{B}{B}.", 0, 0, false, false, false},
        {"Healing Salve", "{W}", "Instant", "Gain 3 life or prevent 3 damage.", 0, 0, false, false,
         false},
        {"Llanowar Elves", "{G}", "Creature - Elf Druid", "Tap: Add {G}.", 1, 1, true, false,
         false},
        {"Grizzly Bears", "{1}{G}", "Creature - Bear", "", 2, 2, true, false, false},
        {"Serra Angel", "{3}{W}{W}", "Creature - Angel", "Flying, vigilance.", 4, 4, true, false,
         false},
        {"Shivan Dragon", "{4}{R}{R}", "Creature - Dragon", "Flying. {R}: +1/+0 until end of turn.",
         5, 5, true, false, false},
        {"Air Elemental", "{3}{U}{U}", "Creature - Elemental", "Flying.", 4, 4, true, false, false},
        {"Sengir Vampire", "{3}{B}{B}", "Creature - Vampire",
         "Flying. Whenever a creature dealt damage by Sengir Vampire this turn dies, put a +1/+1 "
         "counter on Sengir Vampire.",
         4, 4, true, false, false},
        {"Savannah Lions", "{W}", "Creature - Cat", "", 2, 1, true, false, false},
        {"Goblin Guide", "{R}", "Creature - Goblin Scout", "Haste.", 2, 2, true, false, false},
        {"Tarmogoyf", "{1}{G}", "Creature - Lhurgoyf",
         "Tarmogoyf's power is equal to the number of card types among cards in all graveyards and "
         "its toughness is that number plus 1.",
         0, 1, true, false, false},
        {"Wrath of God", "{2}{W}{W}", "Sorcery",
         "Destroy all creatures. They can't be regenerated.", 0, 0, false, false, false},
        {"Doom Blade", "{1}{B}", "Instant", "Destroy target nonblack creature.", 0, 0, false, false,
         false},
        {"Mana Leak", "{1}{U}", "Instant", "Counter target spell unless its controller pays {3}.",
         0, 0, false, false, false},
        {"Shock", "{R}", "Instant", "Deal 2 damage to any target.", 0, 0, false, false, false},
        {"Murder", "{1}{B}{B}", "Instant", "Destroy target creature.", 0, 0, false, false, false},
        {"Opt", "{U}", "Instant", "Scry 1. Draw a card.", 0, 0, false, false, false},
        {"Plains", "", "Basic Land - Plains", "Tap: Add {W}.", 0, 0, false, true, true},
        {"Island", "", "Basic Land - Island", "Tap: Add {U}.", 0, 0, false, true, true},
        {"Swamp", "", "Basic Land - Swamp", "Tap: Add {B}.", 0, 0, false, true, true},
        {"Mountain", "", "Basic Land - Mountain", "Tap: Add {R}.", 0, 0, false, true, true},
        {"Forest", "", "Basic Land - Forest", "Tap: Add {G}.", 0, 0, false, true, true},
    };
}

std::vector<const Card_Entry *> Card_Catalog::Search(const std::string &query) const {
    std::vector<const Card_Entry *> results;
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

const Card_Entry *Card_Catalog::Find(const std::string &name) const {
    for (const auto &card : cards_)
        if (card.name == name)
            return &card;
    return nullptr;
}

const std::vector<Card_Entry> &Card_Catalog::All() const {
    return cards_;
}

std::string Deck_Store::Get_Dir() {
    const char *home = std::getenv("HOME");
    if (!home)
        home = ".";
    return std::string(home) + "/.uud/decks";
}

bool Deck_Store::Save(const Deck &deck) {
    if (deck.name.empty())
        return false;

    std::string dir = Get_Dir();
    std::filesystem::create_directories(dir);

    std::ofstream file(dir + "/" + deck.name + ".deck", std::ios::trunc);
    if (!file.is_open())
        return false;

    for (const auto &entry : deck.cards)
        file << entry.count << ' ' << entry.card_name << '\n';

    return file.good();
}

std::vector<std::string> Deck_Store::List_Decks() {
    std::vector<std::string> names;
    std::string dir = Get_Dir();

    if (!std::filesystem::exists(dir))
        return names;

    for (const auto &entry : std::filesystem::directory_iterator(dir)) {
        if (entry.path().extension() == ".deck")
            names.push_back(entry.path().stem().string());
    }

    std::sort(names.begin(), names.end());
    return names;
}

std::optional<Deck> Deck_Store::Load(const std::string &name) {
    std::string path = Get_Dir() + "/" + name + ".deck";
    std::ifstream file(path);
    if (!file.is_open())
        return std::nullopt;

    Deck deck;
    deck.name = name;

    std::string line;
    while (std::getline(file, line)) {
        if (line.empty())
            continue;
        size_t space = line.find(' ');
        if (space == std::string::npos)
            continue;

        Deck_Entry entry;
        entry.count = std::stoi(line.substr(0, space));
        entry.card_name = line.substr(space + 1);
        deck.cards.push_back(entry);
    }

    return deck;
}

void Deck_Store::Delete(const std::string &name) {
    std::string path = Get_Dir() + "/" + name + ".deck";
    std::filesystem::remove(path);
}
