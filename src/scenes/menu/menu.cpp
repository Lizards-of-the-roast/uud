#include "menu.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "menu_types.hpp"
#include "pages/about/page.hpp"
#include "pages/credits/page.hpp"
#include "pages/main/page.hpp"
#include "pages/match/page.hpp"
#include "pages/settings/page.hpp"
#include "scenes/common/scene_helpers.hpp"
#include "ui/widgets/widgets.hpp"
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>

bool Scene_Menu(void) {
    TTF_TextEngine *text_engine = TTF_CreateRendererTextEngine(state.renderer);
    defer(TTF_DestroyRendererTextEngine(text_engine););
    UI_Context ui = UI_Context(state.window, text_engine);

    Widget_Context w = Widget_Context(state.renderer, &ui);
    for (Widget_Style &s : w.default_style)
        s.background.a = 0xFF * 0.9;

    SDL_Texture *bg = state.texture[paths::bg_texture];
    TTF_Font *menu_font = state.font[paths::beleren_bold];
    TTF_SetFontSize(menu_font, 30);

    Menu_Tab tab = Menu_Tab::None;

    for (;;) {
        state.Update_Delta_Time();
        for (SDL_Event event; SDL_PollEvent(&event);) {
            ui.Pass_Event(event);
            if (Handle_Window_Event(event))
                return true;
        }

        SDL_SetRenderDrawColor(state.renderer, 0x00, 0x18, 0x18, 0xFF);
        SDL_RenderClear(state.renderer);

        if (bg) {
            float bg_w = 0.0f;
            float bg_h = 0.0f;
            if (SDL_GetTextureSize(bg, &bg_w, &bg_h)) {
                SDL_FRect dst = {(float)state.window_width / 2.0f - bg_w / 2.0f,
                                 (float)state.window_height / 2.0f - bg_h / 2.0f, bg_w, bg_h};

                SDL_RenderTexture(state.renderer, bg, NULL, &dst);
            } else {
                SDL_Log("failed to get bg size: %s", SDL_GetError());
            }
        }

        ui.Begin();

        ui.root.elem_align = UI_ALIGN_CENTER;

        ui.sizes.push({UI_Size_Parent(0.4), UI_Size_Parent(0.9)});
        defer(ui.sizes.pop());
        DIV(&w) {
            UI_Box *div = ui.leafs.back();
            div->child_layout_axis = 1;

            ui.sizes.push({UI_Size_Fit(), UI_Size_Fit()});
            defer(ui.sizes.pop());

            ui.fonts.push(menu_font);
            defer(ui.fonts.pop());

            const float val = 20.0f;
            ui.margins.push({.left = val, .right = val, .top = val / 2.0f, .bottom = val / 2.0f});
            defer(ui.margins.pop());

            ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});

            if (Menu_Main_Page(w, ui, tab))
                return true;

            ui.label_alignments.pop();
        }
        switch (tab) {
            case Menu_Tab::Match:
                if (Menu_Match_Page(w, ui, tab))
                    return true;
                break;
            case Menu_Tab::Settings:
                Menu_Settings_Page(w, ui);
                break;
            case Menu_Tab::About:
                Menu_About_Page(w, ui);
                break;
            case Menu_Tab::Credits:
                Menu_Credits_Page(w, ui);
                break;
            case Menu_Tab::None:
                break;
        }

        Present_Frame(ui, w);
    }
    state.scene = Scene::Exit;
    return true;
}
