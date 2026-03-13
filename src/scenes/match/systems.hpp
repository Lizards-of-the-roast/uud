#pragma once

#include <SDL3/SDL.h>
// #include "ui/context/ui_context.hpp"
#include "ui/widgets/widgets.hpp"

#include "net/game_client.hpp"

void Battlefield_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player, Game::Player_State *opp);
void Hand_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player);
void Side_Zones_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player, SDL_Texture *library_texture);
void Drag_Overlay_UI(UI_Context &ui);

void Opp_Hand_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player);

void Game_UI(Widget_Context &w, UI_Context &ui, const Game::Game_Snapshot &snapshot, SDL_Texture *library_texture);
