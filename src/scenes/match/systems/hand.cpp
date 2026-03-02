#include "hand.hpp"

#include <array>

#include "core/defer.hpp"
#include "core/state.hpp"
#include "ui/widgets/widgets.hpp"

static const std::array<std::string, 7> card_ids = {
    "card 0", "card 1", "card 2", "card 3", "card 4", "card 5", "card 6",
};

void Render_Hand(Widget_Context &w, UI_Context &ui, SDL_Texture *card_texture) {
    const float card_width = 143.0f;
    const float card_height = 200.0f;
    const float card_offset = 40.0f;
    const float div_width = (card_width * 7.0f - card_offset * 5);

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

    w.Card(card_texture,
           Rect{100.0f, state.window_height * 0.66f - card_height, card_width, card_height});
}
