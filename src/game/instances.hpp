#pragma once

#include <unordered_map>
#include "card.hpp"
#include "permanent.hpp"

namespace Game {

struct Instances {
    const Card *Find(Card_ID id) const;
    const Permanent_State *Find(Permanent_ID id) const;
    const Card *Add(Card card);
    const Permanent_State *Add(Permanent_State permanent);
private:
    std::unordered_map<Card_ID, Card> cards_;
    std::unordered_map<Permanent_ID, Permanent_State> permanents_;
};

extern Instances instances;

} //namespace Game
