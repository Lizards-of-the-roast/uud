#include "match.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "game_state_local.hpp"
#include "net/game_client.hpp"
#include "scenes/common/scene_helpers.hpp"
#include "scenes/common/ui_theme.hpp"
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
    //TTF_Font *match_font = state.font[paths::beleren_bold];

    Local_Game_State game_state;
    bool is_local = (state.current_game_id == "local");

    bool joined = false;
    bool stream_started = false;

    if (is_local) {
        mtg::proto::GameSnapshot mock;
        mock.set_game_id("local");
        mock.set_current_phase(mtg::proto::PHASE_MAIN_1);
        mock.set_turn_number(1);
        mock.set_active_player_id(1);
        mock.set_priority_player_id(1);

        auto *p1 = mock.add_players();
        p1->set_player_id(1);
        p1->set_username("You");
        p1->set_life_total(20);
        p1->set_hand_count(7);
        p1->set_library_count(53);

        auto *p2 = mock.add_players();
        p2->set_player_id(2);
        p2->set_username("Opponent");
        p2->set_life_total(20);
        p2->set_hand_count(7);
        p2->set_library_count(53);

        game_state.Apply_Snapshot(mock);
        joined = true;
    } else if (!state.current_game_id.empty()) {
        game_client.Join_Game(state.current_game_id);
    }

    for (;;) {
        state.Update_Delta_Time();
        for (SDL_Event event; SDL_PollEvent(&event);) {
            ui.Pass_Event(event);
            if (Handle_Window_Event(event))
                return true;
        }

        if (!is_local) {
            if (auto join = game_client.Poll_Join()) {
                joined = join->success;
                if (joined && !stream_started) {
                    game_client.Start_Action_Stream(state.current_game_id);
                    game_client.Get_State(state.current_game_id);
                    stream_started = true;
                }
            }

            if (auto snapshot = game_client.Poll_Snapshot())
                game_state.Apply_Snapshot(*snapshot);

            while (auto event = game_client.Poll_Event())
                game_state.Apply_Event(*event);
        }

        if (game_state.Is_Game_Over()) {
            if (!is_local)
                game_client.Stop_Action_Stream();
            state.current_game_id.clear();
            state.scene = Scene::Main_Menu;
            return true;
        }

        if (crack_texture)
            SDL_RenderTextureTiled(state.renderer, crack_texture, NULL, 1.0f, NULL);

        ui.Begin();

        /*
        ui.fonts.push(match_font);
        defer(ui.fonts.pop());
        */

        ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
        defer(ui.label_alignments.pop());

        //TTF_SetFontSize(match_font, 14);

        std::array<Widget_Style, WIDGET_STYLE_COUNT> hud_style;
        for (auto &s : hud_style) {
            s.background = {0x0A, 0x0A, 0x14, 0xCC};
            s.border = {0x8B, 0x6F, 0x2E, 0x44};
            s.text.color = SDL_Color{0xEE, 0xDD, 0xBB, 0xFF};
        }

        ui.sizes.push({UI_Size_Parent(1.0), UI_Size_Text(4)});
        w.styles.push(hud_style);

        if (game_state.Has_Snapshot()) {
            uint64_t my_id = is_local ? 1 : state.user_id;
            std::string hud_line = "Phase: " + game_state.Phase_Name() + " | Turn " +
                                   std::to_string(game_state.Snapshot().turn_number());

            const auto *me = game_state.My_State(my_id);
            if (me)
                hud_line += " | Life: " + std::to_string(me->life_total());

            const auto *opp = game_state.Opponent_State(my_id);
            if (opp)
                hud_line += " | Opp: " + opp->username() + " " + std::to_string(opp->life_total()) +
                            " HP, " + std::to_string(opp->hand_count()) + " cards";

            w.Label(hud_line);
        } else {
            w.Label("Waiting for game state...");
        }

        w.styles.pop();
        ui.sizes.pop();

        //TTF_SetFontSize(match_font, 30);

        Render_Hand(w, ui, card_texture);

        //TTF_Font *font_btn = state.font[paths::matrix_bold];
        //TTF_SetFontSize(font_btn, 14);
        //ui.fonts.push(font_btn);
        ui.sizes.push({UI_Size_Fit(), UI_Size_Text(6)});
        w.styles.push(theme::Button_Danger());
        bool leaving = w.Button("Leave Game").flags & UI_SIG_LEFT_RELEASED;
        w.styles.pop();
        ui.sizes.pop();
        //ui.fonts.pop();
        //TTF_SetFontSize(match_font, 30);

        if (leaving) {
            if (!is_local)
                game_client.Stop_Action_Stream();
            state.current_game_id.clear();
            state.scene = Scene::Main_Menu;
            return true;
        }

        ui.End();

        w.Draw();

        Render_Drag_Overlay(ui, card_texture);

        SDL_RenderPresent(state.renderer);
    }
    state.scene = Scene::Exit;
    return true;
}
