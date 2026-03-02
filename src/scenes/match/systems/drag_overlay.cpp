#include "drag_overlay.hpp"

#include "core/state.hpp"
#include "ui/context/ui_context.hpp"
#include <SDL3/SDL.h>

void Render_Drag_Overlay(UI_Context &ui, SDL_Texture *card_texture) {
    UI_Box *active = ui.Get_Box(ui.active);
    if (!active)
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
