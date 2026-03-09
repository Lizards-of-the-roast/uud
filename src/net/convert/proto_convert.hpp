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

Game::Card_Type From_Proto(cle::proto::CardType proto);
Game::Mana_Color From_Proto(cle::proto::ManaColor proto);
Game::Phase From_Proto(mtg::proto::Phase proto);
Game::Zone_Type From_Proto(mtg::proto::ZoneType proto);

Game::Mana_Cost From_Proto(const cle::proto::ManaCost &proto);
Game::Mana_Pool From_Proto(const mtg::proto::ManaPoolProto &proto);
Game::Counter From_Proto(const mtg::proto::Counter &proto);
Game::Card From_Proto(const cle::proto::CardData &proto);
Game::Permanent_State From_Proto(const mtg::proto::PermanentState &proto);
Game::Stack_Entry From_Proto(const mtg::proto::StackEntry &proto);
Game::Player_State From_Proto(const mtg::proto::PlayerState &proto);
Game::Game_Snapshot From_Proto(const mtg::proto::GameSnapshot &proto);
Game::Action_Prompt From_Proto(const mtg::proto::ActionPrompt &proto);
Game::Game_Event From_Proto(const mtg::proto::GameEvent &proto);
Game::Queue_Status From_Proto(const mtg::proto::QueueStatusResponse &proto);

mtg::proto::PlayerAction To_Proto(const Game::Player_Action &action);
mtg::proto::ManaPoolProto To_Proto(const Game::Mana_Pool &pool);

}  // namespace convert
