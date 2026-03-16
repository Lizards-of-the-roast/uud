#include "instances.hpp"

namespace Game {

Instances instances;

const Card *Instances::Find(Card_ID id) const {
    if (auto it = this->cards_.find(id); it != this->cards_.end())
        return &it->second;
    return nullptr;
}
const Permanent_State *Instances::Find(Permanent_ID id) const {
    if (auto it = this->permanents_.find(id); it != this->permanents_.end())
        return &it->second;
    return nullptr;
}
const Card *Instances::Add(Card card)
{
    //100% a better way to do this but idk how cpp itterators work (:
    this->cards_.insert(std::make_pair(card.instance_id, card));
    return this->Find(card.instance_id);
}
const Permanent_State *Instances::Add(Permanent_State permanent)
{
    //100% a better way to do this but idk how cpp itterators work (:
    this->permanents_.insert({permanent.permanent_id, permanent});
    return this->Find(permanent.permanent_id);
}

void Instances::Upsert(Card card) {
    cards_.insert_or_assign(card.instance_id, std::move(card));
}

void Instances::Upsert(Permanent_State permanent) {
    permanents_.insert_or_assign(permanent.permanent_id, std::move(permanent));
}

void Instances::Clear() {
    cards_.clear();
    permanents_.clear();
}

} //namespace Game
