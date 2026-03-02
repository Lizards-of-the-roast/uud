#pragma once

#include <optional>
#include <string>
#include <vector>

struct Card_Entry {
    std::string name;
    std::string mana_cost;
    std::string type_line;
    std::string text;
    int power = 0;
    int toughness = 0;
    bool is_creature = false;
    bool is_land = false;
    bool is_basic_land = false;
};

struct Deck_Entry {
    std::string card_name;
    int count = 1;
};

struct Deck {
    std::string name;
    std::vector<Deck_Entry> cards;

    int Total_Cards() const;
    std::vector<std::string> To_Card_Names() const;
};

struct Card_Catalog {
    void Load_Default_Cards();
    std::vector<const Card_Entry *> Search(const std::string &query) const;
    const Card_Entry *Find(const std::string &name) const;
    const std::vector<Card_Entry> &All() const;

   private:
    std::vector<Card_Entry> cards_;
};

struct Deck_Store {
    static bool Save(const Deck &deck);
    static std::vector<std::string> List_Decks();
    static std::optional<Deck> Load(const std::string &name);
    static void Delete(const std::string &name);

   private:
    static std::string Get_Dir();
};

extern Card_Catalog card_catalog;
