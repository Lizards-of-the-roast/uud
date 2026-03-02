#include "match.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "scenes/common/scene_helpers.hpp"
#include "systems/drag_overlay.hpp"
#include "systems/hand.hpp"
#include "ui/widgets/widgets.hpp"
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>

bool Scene_Match(void) {
    TTF_TextEngine *text_engine = TTF_CreateRendererTextEngine(state.renderer);
    defer(TTF_DestroyRendererTextEngine(text_engine););
    UI_Context ui = UI_Context(state.window, text_engine);

    Widget_Context w = Widget_Context(state.renderer, &ui);

    SDL_Texture *crack_texture = state.texture[paths::crack_texture];
    SDL_Texture *card_texture = state.texture[paths::card_texture];
    TTF_Font *match_font = state.font[paths::beleren_bold];
    TTF_SetFontSize(match_font, 30);

    for (;;) {
        state.Update_Delta_Time();
        for (SDL_Event event; SDL_PollEvent(&event);) {
            ui.Pass_Event(event);
            if (Handle_Window_Event(event))
                return true;
        }

        if (crack_texture)
            SDL_RenderTextureTiled(state.renderer, crack_texture, NULL, 1.0f, NULL);

        ui.Begin();

        ui.fonts.push(match_font);
        defer(ui.fonts.pop());

        Render_Hand(w, ui, card_texture);

        ui.End();

        w.Draw();

        Render_Drag_Overlay(ui, card_texture);

        SDL_RenderPresent(state.renderer);
    }
    state.scene = Scene::Exit;
    return true;
}
