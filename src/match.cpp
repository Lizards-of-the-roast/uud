#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>

#include "state.hpp"
#include "defer.hpp"
#include "simp_ui.hpp"
#include "widgets.hpp"

bool Match(void)
{
    TTF_TextEngine *text_engine = TTF_CreateRendererTextEngine(state.renderer);
    defer (TTF_DestroyRendererTextEngine(text_engine););
    UI_Context ui = UI_Context(state.window, text_engine);

    Widget_Context w = Widget_Context(state.renderer, &ui);
    /*
    for (Widget_Style &s : w.default_style)
        s.background.a = 0xFF*0.9;
    */

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

        SDL_RenderTextureTiled(state.renderer, state.texture[TEXTURE_PATH"gimp_crack.png"], NULL, 1.0f, NULL);

        ui.Begin();


        ui.fonts.push(state.font[FONT_BELEREN_BOLD_PATH]);
        TTF_SetFontSize(state.font[FONT_BELEREN_BOLD_PATH], 30);
        defer (ui.fonts.pop());

        //hand
        const float card_width = 143.0f;
        const float card_height = 200.0f;
        const float card_offset = 40.0f;
        const float div_width = (card_width*7.0f - card_offset*5);
        //ui.root.child_layout_axis = 1;
        //DIV_O(&w, Rect{state.window_width*0.5f - (120.0f*7.0f)*0.5f, state.window_height-200.0f/3.0f, 120*7, 200})
        DIV_O(&w, Rect{state.window_width*0.5f - div_width*0.5f, state.window_height-card_height/3.0f, div_width, card_height})
        {
            UI_Box *div = ui.leafs.back();
            div->flags &= ~UI_BOX_FLAG_CLIP;
            div->offset.y = 0;

            ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
            defer (ui.sizes.pop());
            int hovered = INT32_MAX;
            for (int i = 0; i < 7; i++)
            {
                std::string name = "card "+std::to_string(i);
                UI_Signal button = w.Card(state.texture[TEXTURE_CARD_PATH], {}, name);
                if (button.flags & (UI_SIG_HOVERING | UI_SIG_LEFT_DOWN) )
                {
                    hovered = i;
                    div->offset.y = -card_height/3*2;
                }
                button.box->offset.x = -card_offset*((!hovered || i < hovered) ? i : i-1);
            }
        }

        //w.Button("library", Rect{100.0f, state.window_height*0.66f - 200.0f, 120, 200});
        w.Card(state.texture[TEXTURE_CARD_PATH], Rect{100.0f, state.window_height*0.66f - card_height, card_width, card_height});

        ui.End();

        w.Draw();

        UI_Box *active = ui.Get_Box(ui.active);
        if (active)
        {

            V2 box_pos = (V2)(active->area.pos() + active->area.size()/2);

            V2 v = (V2)(box_pos - ui.mouse_pos);
            V2 v_perp_a = (V2)(V2{-v.y, v.x} / v.Lenght());
            V2 v_perp_b = (V2)(V2{v.y, -v.x} / v.Lenght());

            float width = active->area.w/2;

            SDL_SetRenderDrawColor( state.renderer, 0xFF, 0x00, 0x00, 0xFF);
            SDL_RenderLine( state.renderer,
                            box_pos.x + v_perp_a.x*width,
                            box_pos.y + v_perp_a.y*width,
                            ui.mouse_pos.x,
                            ui.mouse_pos.y
            );
            SDL_RenderLine( state.renderer,
                            box_pos.x + v_perp_b.x*width,
                            box_pos.y + v_perp_b.y*width,
                            ui.mouse_pos.x,
                            ui.mouse_pos.y
            );

            V2 drop_size = active->area.size();
            V2 drop_pos = (V2)(ui.mouse_pos - active->area.size()/2);
            SDL_FRect drop_rect = {
                drop_pos.x,
                drop_pos.y,
                drop_size.x,
                drop_size.y
            };
            //SDL_RenderRect( state.renderer, &drop_rect);
            SDL_SetTextureAlphaMod(state.texture[TEXTURE_CARD_PATH], 0xFF*0.7);
            SDL_RenderTexture( state.renderer, state.texture[TEXTURE_CARD_PATH], NULL, &drop_rect);
            SDL_SetTextureAlphaMod(state.texture[TEXTURE_CARD_PATH], 0xFF);
        }

        SDL_RenderPresent(state.renderer);
    }
    state.game_state = GAME_STATE_EXIT;
    return true;
}
