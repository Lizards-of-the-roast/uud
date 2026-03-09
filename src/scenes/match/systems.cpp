#include "systems.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"

static const std::array<std::string, 7> card_ids = {
    "card 0", "card 1", "card 2", "card 3", "card 4", "card 5", "card 6",
};
const float card_width = 143.0f;
const float card_height = 200.0f;
const float card_offset = 40.0f;
const float div_width = (card_width * 7.0f - card_offset * 5);

void Hand_UI(Widget_Context &w, UI_Context &ui, SDL_Texture *card_texture) {
    DIV_O(&w, Rect{state.window_width * 0.5f - div_width * 0.5f,
                   state.window_height - card_height / 3.0f, div_width, card_height}) {
        UI_Box *div = ui.leafs.back();
        div->flags &= ~UI_BOX_FLAG_CLIP;
        div->offset.y = 0;

        ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
        defer(ui.sizes.pop());
        int hovered = INT32_MAX;
        for (int i = 0; i < 7; i++) {
            UI_Signal button = w.Card(card_texture, {}, card_ids[i]);
            if (button.flags & (UI_SIG_HOVERING | UI_SIG_LEFT_DOWN)) {
                hovered = i;
                div->offset.y = -card_height / 3 * 2;
            }
            button.box->offset.x = -card_offset * ((!hovered || i < hovered) ? i : i - 1);
        }
    }

    /*
    w.Card(card_texture,
           Rect{100.0f, state.window_height * 0.66f - card_height, card_width, card_height});
    */
}

void Library_UI(Widget_Context &w, UI_Context &ui, SDL_Texture *card_texture) {
    w.Card(card_texture,
           Rect{100.0f, state.window_height * 0.66f - card_height, card_width, card_height});
}

void Drag_Overlay_UI(UI_Context &ui, SDL_Texture *card_texture) {
    UI_Box *active = ui.Get_Box(ui.active);
    if (!active || ((~active->signal_last.flags) & UI_SIG_LEFT_DOWN))
        return;

    // TODO: add checks for if the ui element should be draggable

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
