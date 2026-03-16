#pragma once

#include <string>
#include <vector>

#include "game/card.hpp"

struct Card_Catalog {
    void Load_Default_Cards();
    std::vector<const Game::Card *> Search(const std::string &query) const;
    const Game::Card *Find(const std::string &name) const;
    const std::vector<Game::Card> &All() const;

private:
    std::vector<Game::Card> cards_;
};

extern Card_Catalog card_catalog;
