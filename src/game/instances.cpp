#include "instances.hpp"

namespace Game {

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

} //namespace Game
