#include "systems.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"

#include "game/textures.hpp"
#include "game/instances.hpp"
#include "game/permanent.hpp"

static const std::array<std::string, 7> card_ids = {
    "card 0", "card 1", "card 2", "card 3", "card 4", "card 5", "card 6",
};
const float card_width = 143.0f;
const float card_height = 200.0f;
const float card_offset = 40.0f;
const float card_grow_amount = 60.0f;
const float div_width = (card_width * 7.0f - card_offset * 5);

/*
I assume this is the servers job but i dont have the server yet
*/
static Game::Permanent_ID Card_To_Permanent(Game::Card_ID card, Game::Player_State *player)
{
    Game::Permanent_State p = {0};
    p.card = card;
    p.permanent_id = (Game::Permanent_ID)SDL_GetTicksNS(); /*tmp*/
    p.controller_id = p.owner_id
                    = player->player_id;
    Game::instances.Add(p);
    return p.permanent_id;
}

void Battlefield_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player) {
    Rect dst = {};
    dst.w = (float)state.window_width * 0.5f;
    dst.h = (float)state.window_height * 0.5f;
    dst.x = (float)state.window_width * 0.5f - dst.w/2;
    dst.y = (float)state.window_height * 0.5f - dst.h/2;
    w.Button(std::to_string(player->battlefield.size()), dst, UI_BOX_FLAG_DROPPABLE);
}

void Hand_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player) {
    DIV_O(&w, Rect{state.window_width * 0.5f - div_width * 0.5f,
                   state.window_height - card_height / 3.0f, div_width, card_height}) {
        UI_Box *div = ui.leafs.back();
        div->flags &= ~UI_BOX_FLAG_CLIP;
        div->offset.y = 0;

        ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
        defer(ui.sizes.pop());
        size_t hovered = INT32_MAX;
        for (size_t i = 0; i < player->hand.size(); i++) {
            const Game::Card *c = Game::instances.Find(player->hand[i]);
            UI_Signal button = w.Card(*c, {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP | UI_BOX_FLAG_DRAGGABLE, "Card[" + std::to_string(c->instance_id));
            if (button.flags & UI_SIG_DROPPED_OUT)
            {
                //TODO: this is probably where you tell the server that you are trying to
                //      play a card, rather then just editing state
                player->hand_count--;
                player->hand.erase(player->hand.begin() + i);
                player->battlefield.push_back(Card_To_Permanent(c->instance_id, player));
            }
            if (button.flags & (UI_SIG_HOVERING | UI_SIG_LEFT_DOWN)) {
                hovered = i;
                div->offset.y = -card_height / 3 * 2;
                button.box->margin.top = button.box->margin.right
                                       = -card_grow_amount;
            }
            else
            {
                button.box->margin.top = 0;
                button.box->margin.right = 0;
            }
            button.box->offset.x = -card_offset * ((!hovered || i < hovered) ? i : i - 1)
                                                + ((i > hovered) ? card_grow_amount : 0);
        }
    }
}

void Library_UI(Widget_Context &w, UI_Context &ui, SDL_Texture *card_texture) {
    /*
    w.Card(card_texture,
           Rect{100.0f, state.window_height * 0.66f - card_height, card_width, card_height});
    */
}

void Drag_Overlay_UI(UI_Context &ui, SDL_Texture *card_texture) {
    UI_Box *active = ui.Get_Box(ui.active);
    if (!active || ((~active->signal_last.flags) & UI_SIG_LEFT_DOWN)
    ||  ((~active->flags) & UI_BOX_FLAG_DRAGGABLE) )
        return;


    V2 box_pos = (V2)(active->area.pos() + active->area.size() / 2);

    V2 v = (V2)(box_pos - ui.mouse_pos);
    float len = v.Length();

    if (len > 0.001f) {
        V2 v_perp_a = (V2)(V2{-v.y, v.x} / len);
        V2 v_perp_b = (V2)(V2{v.y, -v.x} / len);

        float width = active->area.w / 2;

        SDL_SetRenderDrawColor(state.renderer, 0xFF, 0x00, 0x00, 0xFF);
        SDL_RenderLine(state.renderer, box_pos.x + v_perp_a.x * width,
                       box_pos.y + v_perp_a.y * width, ui.mouse_pos.x, ui.mouse_pos.y);
        SDL_RenderLine(state.renderer, box_pos.x + v_perp_b.x * width,
                       box_pos.y + v_perp_b.y * width, ui.mouse_pos.x, ui.mouse_pos.y);
    }

    V2 drop_size = active->area.size();
    V2 drop_pos = (V2)(ui.mouse_pos - active->area.size() / 2);
    SDL_FRect drop_rect = {drop_pos.x, drop_pos.y, drop_size.x, drop_size.y};

    if (card_texture) {
        SDL_SetTextureAlphaMod(card_texture, 0xFF * 0.7);
        SDL_RenderTexture(state.renderer, card_texture, NULL, &drop_rect);
        SDL_SetTextureAlphaMod(card_texture, 0xFF);
    }
}
