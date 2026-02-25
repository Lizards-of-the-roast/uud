#include <iostream>
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>

#include "menu.hpp"
#include "state.hpp"
#include "defer.hpp"
#include "simp_ui.hpp"
#include "widgets.hpp"

#include <cassert>

enum Menu_Tab
{
    TAB_NONE     = 0,
    TAB_MATCH    = 1,
    TAB_SETTINGS = 2,
    TAB_ABOUT    = 3,
    TAB_CREDITS  = 4,
};

static void Offset_If_Hovered(UI_Signal sig, V2 offset)
{
    bool hovering = sig.flags & (UI_SIG_HOVERING | UI_SIG_LEFT_DOWN);
    V2 mouse_rel = (V2)(sig.mouse_pos - sig.box->area.pos() - sig.box->offset);
    bool mouse_over_offset = mouse_rel.x > offset.x && mouse_rel.y > offset.y;
    if ( hovering
    && ( mouse_over_offset || sig.box->offset.x)
    )
        sig.box->offset = offset;
    else
        sig.box->offset = V2{};
}

bool Main_Menu(void)
{
    TTF_TextEngine *text_engine = TTF_CreateRendererTextEngine(state.renderer);
    defer (TTF_DestroyRendererTextEngine(text_engine););
    UI_Context ui = UI_Context(state.window, text_engine);

    Widget_Context w = Widget_Context(state.renderer, &ui);
    for (Widget_Style &s : w.default_style)
        s.background.a = 0xFF*0.9;

    Menu_Tab tab = TAB_NONE;

    for (;;)
    {
        Get_Delta_Time(&state.delta_time, &state.tick);
        for (SDL_Event event; SDL_PollEvent(&event); )
        {
            ui.Pass_Event(event);
            switch (event.type)
            {
                case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                case SDL_EVENT_QUIT:
                    state.game_state = GAME_STATE_EXIT;
                    return true;
                case SDL_EVENT_WINDOW_RESIZED:
                    SDL_GetWindowSize(state.window, &state.window_width, &state.window_height);
                    break;
            }
        }

        SDL_SetRenderDrawColor(state.renderer, 0x00, 0x18, 0x18, 0xFF);
        SDL_RenderClear(state.renderer);

        SDL_Texture *bg = state.texture[TEXTURE_BG_PATH];
        SDL_FRect dst = {(float)state.window_width / 2.0f - (float)bg->w / 2.0f,(float)state.window_height / 2.0f - (float)bg->h / 2.0f, (float)bg->w, (float)bg->h};
        SDL_RenderTexture(state.renderer, bg, NULL, &dst);

        ui.Begin();

        ui.root.elem_align = UI_ALIGN_CENTER;

        ui.sizes.push({ UI_Size_Parent(0.4), UI_Size_Parent(0.9) });
        defer (ui.sizes.pop());
        DIV(&w)
        {
            UI_Box *div = ui.leafs.back();
            div->child_layout_axis = 1;

            ui.sizes.push({ UI_Size_Fit(), UI_Size_Fit() });
            defer (ui.sizes.pop());

            ui.fonts.push(state.font[FONT_BELEREN_BOLD_PATH]);
            TTF_SetFontSize(state.font[FONT_BELEREN_BOLD_PATH], 30);
            defer (ui.fonts.pop());

            const float val = 20.0f;
            ui.margins.push({
                .left   = val,
                .right  = val,
                .top    = val/2.0f,
                .bottom = val/2.0f
            });
            defer (ui.margins.pop());

            ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});

            UI_Signal start = w.Button("Start Game");
            Offset_If_Hovered(start, {val, 0});
            if (start.flags & UI_SIG_LEFT_RELEASED)
            {
                tab = (tab != TAB_MATCH) ? TAB_MATCH : TAB_NONE;
                //state.game_state = GAME_STATE_MATCH;
                //return true;
            }

            UI_Signal settings = w.Button("Settings");
            Offset_If_Hovered(settings, {val, 0});
            if (settings.flags & UI_SIG_LEFT_RELEASED)
                tab = (tab != TAB_SETTINGS) ? TAB_SETTINGS : TAB_NONE;

            UI_Signal about = w.Button("About");
            Offset_If_Hovered(about, {val, 0});
            if (about.flags & UI_SIG_LEFT_RELEASED)
                tab = (tab != TAB_ABOUT) ? TAB_ABOUT : TAB_NONE;

            UI_Signal credits = w.Button("Credits");
            Offset_If_Hovered(credits, {val, 0});
            if (credits.flags & UI_SIG_LEFT_RELEASED)
                tab = (tab != TAB_CREDITS) ? TAB_CREDITS : TAB_NONE;

            UI_Signal quit = w.Button("Quit");
            Offset_If_Hovered(quit, {val, 0});
            if (quit.flags & UI_SIG_LEFT_RELEASED)
            {
                state.game_state = GAME_STATE_EXIT;
                return true;
            }

            ui.label_alignments.pop();
        }
        switch (tab)
        {
            case TAB_MATCH:
                DIV(&w)
                {
                    UI_Box *div = ui.leafs.back();
                    div->child_layout_axis = 1;
                    div->elem_align = UI_ALIGN_CENTER;

                    ui.fonts.push(state.font[FONT_BELEREN_BOLD_PATH]);
                    TTF_SetFontSize(state.font[FONT_BELEREN_BOLD_PATH], 30);
                    defer (ui.fonts.pop());

                    ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
                    defer (ui.label_alignments.pop());

                    ui.sizes.push({ UI_Size_Parent(0.9), UI_Size_Text(40) });
                    defer (ui.sizes.pop());

                    UI_Signal label = w.Label("Enter Server IP");
                    w.Textbox().box->size.y = UI_Size_Pixels(label.box->layout_box.h);
                    //w.Textbox().box->size = {UI_Size_Parent(1.0f), UI_Size_Pixels(label.box->layout_box.h)};
                    DIV(&w)
                    {
                        ui.sizes.push({ UI_Size_Fit(), UI_Size_Text(40) });
                        defer (ui.sizes.pop());
                        if (w.Button("Cancel").flags & UI_SIG_LEFT_RELEASED )
                            tab = TAB_NONE;
                        if (w.Button("Connect").flags & UI_SIG_LEFT_RELEASED )
                        {
                            state.game_state = GAME_STATE_MATCH;
                            return true;
                        }
                    }
                }
                break;
            case TAB_SETTINGS:
                DIV(&w)
                {
                    UI_Box *div = ui.leafs.back();
                    div->child_layout_axis = 1;
                    div->elem_align = UI_ALIGN_CENTER;

                    ui.sizes.push({ UI_Size_Text(50), UI_Size_Text(50) });
                    defer (ui.sizes.pop());

                    ui.fonts.push(state.font[FONT_BELEREN_BOLD_PATH]);
                    TTF_SetFontSize(state.font[FONT_BELEREN_BOLD_PATH], 30);
                    defer (ui.fonts.pop());

                    UI_Signal label = w.Label(
                        "WIP"
                    );
                    label.box->label_alignment = UI_ALIGN_CENTER;
                }
                break;
            case TAB_ABOUT:
                DIV(&w)
                {
                    UI_Box *div = ui.leafs.back();
                    div->child_layout_axis = 1;
                    div->elem_align = UI_ALIGN_CENTER;

                    ui.sizes.push({ UI_Size_Text(50), UI_Size_Text(50) });
                    defer (ui.sizes.pop());

                    ui.fonts.push(state.font[FONT_BELEREN_BOLD_PATH]);
                    TTF_SetFontSize(state.font[FONT_BELEREN_BOLD_PATH], 30);
                    defer (ui.fonts.pop());

                    UI_Signal label = w.Label(
                        "WIP"
                    );
                    label.box->label_alignment = UI_ALIGN_CENTER;
                }
                break;
            case TAB_CREDITS:
                DIV(&w)
                {
                    UI_Box *div = ui.leafs.back();
                    div->child_layout_axis = 1;
                    div->elem_align = UI_ALIGN_CENTER;

                    ui.sizes.push({ UI_Size_Text(50), UI_Size_Text(50) });
                    defer (ui.sizes.pop());

                    ui.fonts.push(state.font[FONT_BELEREN_BOLD_PATH]);
                    TTF_SetFontSize(state.font[FONT_BELEREN_BOLD_PATH], 30);
                    defer (ui.fonts.pop());

                    UI_Signal label = w.Label(
                        "Lizards Of the Roast™\n"
                        "Engine:\n"
                        "    Ian Fogarty\n"
                        "Client:\n"
                        "    Matthew Conroy\n"
                        "Card Scripting and Game Design:\n"
                        "    Brendan Egan\n"
                        "    Thibault Wysocinski\n"
                    );
                    label.box->label_alignment = UI_ALIGN_CENTER;
                }
                break;
            case TAB_NONE:
                break;
        }

        ui.End();

        w.Draw();

        SDL_RenderPresent(state.renderer);
    }
    state.game_state = GAME_STATE_EXIT;
    return true;
}
