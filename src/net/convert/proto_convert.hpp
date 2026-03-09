#pragma once

#include "cle/card.pb.h"
#include "game/actions.hpp"
#include "game/card.hpp"
#include "game/counter.hpp"
#include "game/events.hpp"
#include "game/game_snapshot.hpp"
#include "game/mana_pool.hpp"
#include "game/matchmaking.hpp"
#include "game/permanent.hpp"
#include "game/phase.hpp"
#include "game/player.hpp"
#include "game/prompts.hpp"
#include "game/stack_entry.hpp"
#include "game/zone.hpp"
#include "mtg/common.pb.h"
#include "mtg/game_service.pb.h"
#include "mtg/game_state.pb.h"
#include "mtg/matchmaking_service.pb.h"

namespace convert {

Card_Type From_Proto(cle::proto::CardType proto);
Mana_Color From_Proto(cle::proto::ManaColor proto);
Phase From_Proto(mtg::proto::Phase proto);
Zone_Type From_Proto(mtg::proto::ZoneType proto);

Mana_Cost From_Proto(const cle::proto::ManaCost &proto);
Mana_Pool From_Proto(const mtg::proto::ManaPoolProto &proto);
Counter From_Proto(const mtg::proto::Counter &proto);
Card From_Proto(const cle::proto::CardData &proto);
Permanent_State From_Proto(const mtg::proto::PermanentState &proto);
Stack_Entry From_Proto(const mtg::proto::StackEntry &proto);
Player_State From_Proto(const mtg::proto::PlayerState &proto);
Game_Snapshot From_Proto(const mtg::proto::GameSnapshot &proto);
Action_Prompt From_Proto(const mtg::proto::ActionPrompt &proto);
Game_Event From_Proto(const mtg::proto::GameEvent &proto);
Queue_Status From_Proto(const mtg::proto::QueueStatusResponse &proto);

mtg::proto::PlayerAction To_Proto(const Player_Action &action);
mtg::proto::ManaPoolProto To_Proto(const Mana_Pool &pool);

}  // namespace convert
