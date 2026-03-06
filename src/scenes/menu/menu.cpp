#include "menu.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "menu_types.hpp"
#include "pages/about/page.hpp"
#include "pages/credits/page.hpp"
#include "pages/deck_builder/page.hpp"
#include "pages/main/page.hpp"
#include "pages/matchmaking/page.hpp"
#include "pages/settings/page.hpp"
#include "scenes/common/scene_helpers.hpp"
#include "scenes/common/ui_theme.hpp"
#include "ui/widgets/widgets.hpp"
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>

bool Scene_Menu(void) {
    TTF_TextEngine *text_engine = TTF_CreateRendererTextEngine(state.renderer);
    defer(TTF_DestroyRendererTextEngine(text_engine););
    UI_Context ui = UI_Context(state.window, text_engine);

    Widget_Context w = Widget_Context(state.renderer, &ui);
    w.default_style = theme::Button_Primary();

    SDL_Texture *bg = state.texture[paths::bg_texture];
    TTF_Font *font_button = state.font[paths::matrix_bold];

    Menu_Tab tab = Menu_Tab::None;

    for (;;) {
        TTF_SetFontSize(font_button, 20);
        state.Update_Delta_Time();
        for (SDL_Event event; SDL_PollEvent(&event);) {
            ui.Pass_Event(event);
            if (Handle_Window_Event(event))
                return true;
        }

        if (state.scene != Scene::Main_Menu)
            return true;

        SDL_SetRenderDrawColor(state.renderer, theme::SCENE_BG.r, theme::SCENE_BG.g,
                               theme::SCENE_BG.b, theme::SCENE_BG.a);
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
        DIV(&w) {
            UI_Box *div = ui.leafs.back();
            div->child_layout_axis = 1;
            theme::Apply_Panel(div, theme::Panel());

            ui.sizes.push({UI_Size_Fit(), UI_Size_Fit()});
            defer(ui.sizes.pop());

            /*
            ui.fonts.push(font_button);
            defer(ui.fonts.pop());
            */

            const float val = 16.0f;
            ui.margins.push({.left = val, .right = val, .top = val / 2.0f, .bottom = val / 2.0f});
            defer(ui.margins.pop());

            ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
            defer(ui.label_alignments.pop());

            if (Menu_Main_Page(w, ui, tab))
                return true;
        }
        ui.sizes.pop();

        ui.sizes.push({UI_Size_Parent(0.55), UI_Size_Parent(0.9)});
        switch (tab) {
            case Menu_Tab::Matchmaking:
                if (Menu_Matchmaking_Page(w, ui, tab))
                    return true;
                break;
            case Menu_Tab::Deck_Builder:
                if (Menu_Deck_Builder_Page(w, ui, tab))
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
        ui.sizes.pop();

        Present_Frame(ui, w);
    }
    state.scene = Scene::Exit;
    return true;
}
