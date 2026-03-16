#pragma once

#include "game/card.hpp"
#include "game/permanent.hpp"
#include <SDL3/SDL.h>
#include <SDL3_ttf/SDL_ttf.h>

SDL_Color Card_Frame_Color(const Game::Card &card);
void Draw_Card_Overlay(SDL_Renderer *renderer, const SDL_FRect &rect, const Game::Card &card,
                       const Game::Permanent_State *perm, TTF_Font *font);
