#pragma once

#include <SDL3/SDL.h>
// #include "ui/context/ui_context.hpp"
#include "ui/widgets/widgets.hpp"

#include "net/game_client.hpp"

void Hand_UI(Widget_Context &w, UI_Context &ui, Player_State player);
void Library_UI(Widget_Context &w, UI_Context &ui, SDL_Texture *card_texture);
void Drag_Overlay_UI(UI_Context &ui, SDL_Texture *card_texture);
