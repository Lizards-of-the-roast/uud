#pragma once

#include "game/game_snapshot.hpp"
#include "game_state_local.hpp"
#include "net/game_client.hpp"
#include "ui/widgets/widgets.hpp"
#include <SDL3/SDL.h>
#include <SDL3_ttf/SDL_ttf.h>

void Battlefield_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                    Game::Player_State *opp, Combat_UI_State *combat = nullptr,
                    bool is_local = true);
void Hand_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player, bool is_local = true);
void Side_Zones_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                   SDL_Texture *library_texture, TTF_Font *font,
                   const Game::Game_Snapshot *snapshot = nullptr, bool *leave_pressed = nullptr,
                   bool is_local = true);
void Opp_Side_Zones_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                       SDL_Texture *library_texture, TTF_Font *font, bool is_local = true);
void Drag_Overlay_UI(UI_Context &ui);
void Combat_Lines_UI(SDL_Renderer *renderer, Combat_UI_State *combat);

void Opp_Hand_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player);

void Game_UI(Widget_Context &w, UI_Context &ui, Game::Game_Snapshot &snapshot,
             SDL_Texture *library_texture, TTF_Font *font, uint64_t my_user_id = 0,
             bool *leave_pressed = nullptr, Combat_UI_State *combat = nullptr,
             bool is_local = true);

void Track_Card_Position(uint64_t id, const SDL_FRect &rect);
const SDL_FRect *Last_Card_Position(uint64_t id);
void Clear_Card_Positions();

extern SDL_FRect player_library_rect;
extern SDL_FRect opp_library_rect;

struct Drag_Play {
    uint64_t card_id = 0;
    bool pending = false;
};
Drag_Play &Pending_Drag_Play();

struct Drag_Attack {
    uint64_t permanent_id = 0;
    bool pending = false;
};
Drag_Attack &Pending_Drag_Attack();

struct Drag_Block {
    uint64_t blocker_id = 0;
    uint64_t attacker_id = 0;
    bool pending = false;
};
Drag_Block &Pending_Drag_Block();

struct Drag_Activate {
    uint64_t permanent_id = 0;
    bool pending = false;
};
Drag_Activate &Pending_Drag_Activate();

void Draw_Pending_Card_Overlays(SDL_Renderer *renderer, TTF_Font *font);
