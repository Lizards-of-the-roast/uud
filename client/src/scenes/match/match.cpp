#include "match.hpp"

#include <algorithm>
#include <iostream>

#include "core/defer.hpp"
#include "core/state.hpp"
#include "game/actions.hpp"
#include "game/instances.hpp"
#include "game/textures.hpp"
#include "game_state_local.hpp"
#include "net/game_client.hpp"
#include "scenes/common/scene_helpers.hpp"
#include "scenes/common/ui_theme.hpp"
#include "systems.hpp"
#include "ui/widgets/widgets.hpp"
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>

static bool Send_Action(const std::string &game_id, const std::string &prompt_id,
                        auto action_value) {
    Game::Player_Action action;
    action.game_id = game_id;
    action.prompt_id = prompt_id;
    action.action = std::move(action_value);
    return game_client.Send_Action(action);
}

static bool Send_And_Clear(Local_Game_State &gs, const std::string &game_id,
                           const std::string &prompt_id, auto action_value) {
    if (Send_Action(game_id, prompt_id, std::move(action_value))) {
        gs.Clear_Prompt();
        return true;
    }
    return false;
}

static std::string Format_Mana_Cost(const Game::Mana_Cost &cost) {
    std::string s;
    if (cost.x_count > 0) {
        for (int i = 0; i < cost.x_count; i++)
            s += "X";
    }
    if (cost.colorless > 0)
        s += std::to_string(cost.colorless);
    for (int i = 0; i < cost.white; i++)
        s += "W";
    for (int i = 0; i < cost.blue; i++)
        s += "U";
    for (int i = 0; i < cost.black; i++)
        s += "B";
    for (int i = 0; i < cost.red; i++)
        s += "R";
    for (int i = 0; i < cost.green; i++)
        s += "G";
    if (s.empty())
        s = "0";
    return s;
}

bool Scene_Match(void) {
    TTF_TextEngine *text_engine = TTF_CreateRendererTextEngine(state.renderer);
    defer(TTF_DestroyRendererTextEngine(text_engine););
    UI_Context ui = UI_Context(state.window, text_engine);

    Widget_Context w = Widget_Context(state.renderer, &ui);

    SDL_Texture *crack_texture = state.texture[paths::crack_texture];
    SDL_Texture *card_texture = state.texture[paths::card_texture];
    Game::card_textures.default_texture = card_texture;
    Game::card_textures.Set_Renderer(state.renderer);
    Game::card_textures.Scan_Card_Directory("./res/cards");
    SDL_Texture *test_card_texture = state.texture["./res/textures/ankle-biter.png"];
    SDL_Texture *library_texture = state.texture["./res/textures/library.png"];
    TTF_Font *match_font = state.font[paths::beleren_bold];

    Local_Game_State game_state;
    game_state.Set_Local_User_Id(state.user_id);
    bool is_local = (state.current_game_id == "local");
    uint64_t last_event_sequence = 0;
    Game::instances.Clear();
    Clear_Card_Positions();

    bool joined = false;
    bool stream_started = false;
    bool deck_submitted = false;
    bool join_failed = false;
    bool reconnecting = false;
    int reconnect_attempt = 0;
    Uint64 reconnect_backoff_ms = 1000;
    Uint64 last_reconnect_ms = 0;
    bool game_over_streams_stopped = false;
    Uint64 error_return_at = 0;
    std::string deck_error;
    std::string match_error;
    std::string join_error_detail;

    std::vector<int> damage_split;
    std::vector<uint64_t> selected_discards;
    std::vector<int> selected_modes;
    uint64_t selected_target = 0;
    std::string last_prompt_id;
    int auto_pass_mode = 0;
    int last_auto_pass_turn = 0;
    Combat_UI_State combat_ui;
    Uint64 last_priority_tick = 0;
    bool had_priority_last_frame = false;
    uint64_t hover_card_id = 0;
    Uint64 hover_start_ms = 0;
    bool showing_detail = false;
    std::vector<Damage_Float> damage_floats;
    std::vector<Life_Pulse> life_pulses;
    Phase_Flash phase_flash;
    Uint64 turn_banner_start_ms = 0;
    bool turn_banner_is_ours = false;

    if (!is_local)
        game_client.Drain_All();

    if (is_local) {
        Game::Game_Snapshot mock;
        mock.game_id = "local";
        mock.current_phase = Game::Phase::Main_1;
        mock.turn_number = 1;
        mock.active_player_id = 1;
        mock.priority_player_id = 1;

        Game::Player_State p1;
        p1.player_id = 1;
        p1.username = "You";
        p1.life_total = 20;
        p1.hand_count = 7;
        for (int i = 0; i < p1.hand_count; i++) {
            Game::Card c;
            c.instance_id = i;
            c.name = "Card " + std::to_string(i);
            if (i == 3)
                Game::card_textures.Set(c.name, test_card_texture);
            Game::instances.Add(c);
            p1.hand.push_back(c.instance_id);
        }
        p1.library_count = 53;
        mock.players.push_back(p1);

        Game::Player_State p2;
        p2.player_id = 2;
        p2.username = "Opponent";
        p2.life_total = 20;
        p2.hand_count = 7;
        for (int i = 0; i < p2.hand_count; i++) {
            Game::Card c;
            c.instance_id = p1.hand_count + i;
            Game::instances.Add(c);
            p2.hand.push_back(c.instance_id);
        }
        p2.library_count = 53;
        mock.players.push_back(p2);

        game_state.Apply_Snapshot(mock);
        joined = true;
    } else if (!state.current_game_id.empty()) {
        if (state.joined_via_matchmaking) {
            joined = true;
            state.joined_via_matchmaking = false;
        } else {
            game_client.Join_Game(state.current_game_id);
        }
    }

    for (;;) {
        if (state.scene == Scene::Exit)
            return true;
        (void)SDL_GetTicksNS();
        state.Update_Delta_Time();
        for (SDL_Event event; SDL_PollEvent(&event);) {
            ui.Pass_Event(event);
            if (Handle_Window_Event(event))
                return true;

            if (!is_local && event.type == SDL_EVENT_KEY_DOWN) {
                const std::string &gid = state.current_game_id;
                if (event.key.key == SDLK_SPACE || event.key.key == SDLK_RETURN) {
                    if (game_state.Has_Prompt()) {
                        const auto &prompt = game_state.Pending_Prompt();
                        if (std::holds_alternative<Game::Priority_Prompt>(prompt.prompt)) {
                            Send_And_Clear(game_state, gid, prompt.prompt_id,
                                           Game::Pass_Priority_Action{});
                        }
                    }
                } else if (event.key.key == SDLK_F2) {
                    auto_pass_mode = (auto_pass_mode == 1) ? 0 : 1;
                    Send_Action(gid, "", Game::Set_Auto_Pass_Action{auto_pass_mode});
                } else if (event.key.key == SDLK_F3) {
                    auto_pass_mode = 2;
                    Send_Action(gid, "", Game::Set_Auto_Pass_Action{2});
                } else if (event.key.key == SDLK_F4) {
                    auto_pass_mode = 0;
                    Send_Action(gid, "", Game::Set_Auto_Pass_Action{0});
                }
            }
        }

        if (!is_local) {
            if (auto join = game_client.Poll_Join()) {
                if (join->success) {
                    joined = true;
                    match_error.clear();
                    std::cerr << "match: join succeeded\n";
                } else {
                    join_error_detail = join->error;
                    std::cerr << "match: join failed: " << join_error_detail << '\n';
                    join_failed = true;
                    match_error = "Join failed: " + join_error_detail;
                    error_return_at = SDL_GetTicks() + 5000;
                }
            }

            if (auto rejoin = game_client.Poll_Rejoin()) {
                if (rejoin->success) {
                    joined = true;
                    match_error.clear();
                    if (rejoin->snapshot)
                        game_state.Apply_Snapshot(*rejoin->snapshot);
                    std::cerr << "match: rejoin succeeded\n";
                } else {
                    match_error = "Rejoin failed: " + rejoin->error;
                    std::cerr << "match: " << match_error << '\n';
                }
            }

            if (joined && !stream_started) {
                stream_started = true;

                if (!state.selected_deck_name.empty() && !deck_submitted) {
                    game_client.Submit_Preset_Deck(state.current_game_id, state.selected_deck_name);
                    deck_submitted = true;
                }

                game_client.Start_Action_Stream(state.current_game_id);
            }

            if (joined && stream_started && !game_client.Stream_Active() &&
                !game_state.Is_Game_Over()) {
                if (!reconnecting) {
                    reconnecting = true;
                    reconnect_attempt = 0;
                    reconnect_backoff_ms = 1000;
                    last_reconnect_ms = SDL_GetTicks();
                    std::cerr << "match: stream dropped, reconnecting...\n";
                } else if (SDL_GetTicks() - last_reconnect_ms >= reconnect_backoff_ms) {
                    reconnect_attempt++;
                    if (reconnect_attempt > 10) {
                        match_error = "Disconnected, could not reconnect";
                        std::cerr << "match: max reconnect attempts reached\n";
                    } else {
                        std::cerr << "match: reconnect attempt " << reconnect_attempt << '\n';
                        game_client.Rejoin_Game(state.current_game_id);
                        game_client.Start_Action_Stream(state.current_game_id);
                        game_client.Start_State_Stream(state.current_game_id, last_event_sequence);
                        reconnect_backoff_ms =
                            std::min(static_cast<Uint64>(30000), static_cast<Uint64>(1000)
                                                                     << reconnect_attempt);
                        last_reconnect_ms = SDL_GetTicks();
                    }
                }
            } else if (reconnecting && game_client.Stream_Active()) {
                reconnecting = false;
                reconnect_attempt = 0;
                match_error.clear();
                std::cerr << "match: reconnected successfully\n";
            }

            if (auto deck_result = game_client.Poll_Deck()) {
                if (!deck_result->valid) {
                    deck_error = "Deck invalid -";
                    for (const auto &err : deck_result->errors)
                        deck_error += " " + err;
                    deck_error += " (Leave Game to pick a different deck)";
                    std::cerr << "match: " << deck_error << '\n';
                } else {
                    deck_error.clear();
                }
            }

            while (auto event = game_client.Poll_Event()) {
                if (event->sequence_number > last_event_sequence)
                    last_event_sequence = event->sequence_number;
                game_state.Apply_Event(*event);
            }

            if (join_failed && error_return_at > 0 && SDL_GetTicks() >= error_return_at) {
                std::cerr << "match: join failed, returning to menu\n";
                state.current_game_id.clear();
                state.selected_deck_name.clear();
                state.scene = Scene::Main_Menu;
                return true;
            }

            if (stream_started && !game_state.Has_Snapshot() && !game_state.Is_Game_Over()) {
                bool streams_dead = !game_client.Stream_Active();
                if (streams_dead && !reconnecting) {
                    if (error_return_at == 0) {
                        error_return_at = SDL_GetTicks() + 5000;
                        std::cerr
                            << "match: streams dead, no snapshot, will return to menu in 5s\n";
                    } else if (SDL_GetTicks() >= error_return_at) {
                        std::cerr << "match: giving up, returning to menu\n";
                        game_client.Stop_Action_Stream();
                        game_client.Stop_State_Stream();
                        state.current_game_id.clear();
                        state.selected_deck_name.clear();
                        state.scene = Scene::Main_Menu;
                        return true;
                    }
                }
            } else if (!join_failed) {
                error_return_at = 0;
                if (game_state.Has_Snapshot()) {
                    match_error.clear();
                    join_error_detail.clear();
                }
            }
        }

        if (game_state.Is_Game_Over()) {
            if (!game_over_streams_stopped) {
                game_over_streams_stopped = true;
                if (!is_local) {
                    game_client.Stop_Action_Stream();
                    game_client.Stop_State_Stream();
                }
            }
        }

        if (game_state.Has_Snapshot()) {
            auto *me = game_state.My_State_Mut(is_local ? 1 : state.user_id);
            if (me && me->has_priority) {
                if (!had_priority_last_frame) {
                    last_priority_tick = SDL_GetTicks();
                    had_priority_last_frame = true;
                }
                Uint64 now = SDL_GetTicks();
                Uint64 elapsed = now - last_priority_tick;
                if (me->clock_remaining_ms > static_cast<int>(elapsed))
                    me->clock_remaining_ms -= static_cast<int>(elapsed);
                else
                    me->clock_remaining_ms = 0;
                last_priority_tick = now;
            } else {
                had_priority_last_frame = false;
            }
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

        uint64_t my_id = is_local ? 1 : state.user_id;
        bool leaving = false;

        if (game_state.Has_Snapshot()) {
            ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Parent(1.0f)});
            DIV(&w) {
                UI_Box *board = ui.leafs.back();

                ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Parent(1.0f)});
                DIV(&w) {
                    UI_Box *inner = ui.leafs.back();
                    inner->flags |= UI_BOX_FLAG_FLOATING;
                    inner->fixed_position = V2{0, 0};
                    Game_UI(w, ui, game_state.Snapshot(), library_texture, match_font, my_id,
                            &leaving, &combat_ui, is_local);
                }
                ui.sizes.pop();

                {
                    auto &dp = Pending_Drag_Play();
                    if (dp.pending && !is_local && game_state.Has_Prompt()) {
                        const auto &prompt = game_state.Pending_Prompt();
                        const std::string &pid = prompt.prompt_id;
                        const std::string &gid = state.current_game_id;
                        if (auto *pp = std::get_if<Game::Priority_Prompt>(&prompt.prompt)) {
                            uint64_t card_id = dp.card_id;
                            const auto *card =
                                Game::instances.Find(static_cast<Game::Card_ID>(card_id));
                            bool sent = false;
                            for (auto cid : pp->castable_card_ids) {
                                if (cid == card_id) {
                                    if (Send_And_Clear(game_state, gid, pid,
                                                       Game::Play_Card_Action{card_id}))
                                        sent = true;
                                    break;
                                }
                            }
                            if (!sent && pp->can_play_land && card &&
                                card->type == Game::Card_Type::Land) {
                                Send_And_Clear(game_state, gid, pid,
                                               Game::Play_Land_Action{card_id});
                            }
                        }
                    }
                    dp.pending = false;
                }

                {
                    auto &da = Pending_Drag_Activate();
                    if (da.pending && !is_local && game_state.Has_Prompt()) {
                        const auto &prompt = game_state.Pending_Prompt();
                        if (auto *pp = std::get_if<Game::Priority_Prompt>(&prompt.prompt)) {
                            uint64_t pid_num = da.permanent_id;
                            for (auto act_id : pp->activatable_permanent_ids) {
                                if (act_id == pid_num && !game_state.Already_Activated(pid_num)) {
                                    game_state.Mark_Activated(pid_num);
                                    Send_And_Clear(game_state, state.current_game_id,
                                                   prompt.prompt_id,
                                                   Game::Activate_Ability_Action{pid_num, 0});
                                    break;
                                }
                            }
                        }
                    }
                    da.pending = false;
                }

                if (combat_ui.clicked_target != 0 && !is_local && game_state.Has_Prompt()) {
                    const auto &prompt = game_state.Pending_Prompt();
                    if (std::holds_alternative<Game::Target_Prompt>(prompt.prompt)) {
                        if (Send_And_Clear(game_state, state.current_game_id, prompt.prompt_id,
                                           Game::Select_Target_Action{combat_ui.clicked_target}))
                            selected_target = 0;
                    }
                    combat_ui.clicked_target = 0;
                }

                if (!match_error.empty() || !deck_error.empty()) {
                    ui.sizes.push({UI_Size_Parent(0.6f), UI_Size_Child()});
                    DIV(&w) {
                        UI_Box *err_box = ui.leafs.back();
                        err_box->flags |= UI_BOX_FLAG_FLOATING;
                        err_box->fixed_position = V2{board->layout_box.w * 0.2f, 4};
                        err_box->child_layout_axis = 1;
                        theme::Apply_Panel(err_box, theme::Panel());

                        ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Text(2)});
                        auto err_style = theme::Label_Body(match_font);
                        for (auto &s : err_style)
                            s.text.color = theme::TEXT_ERROR;
                        w.styles.push(err_style);
                        if (!match_error.empty())
                            w.Label(match_error);
                        if (!deck_error.empty())
                            w.Label(deck_error);
                        w.styles.pop();
                        ui.sizes.pop();
                    }
                    ui.sizes.pop();
                }

                if (reconnecting) {
                    const float rc_w = 300.0f, rc_h = 40.0f;
                    ui.sizes.push({UI_Size_Pixels(rc_w), UI_Size_Pixels(rc_h)});
                    DIV(&w) {
                        UI_Box *rc_box = ui.leafs.back();
                        rc_box->flags |= UI_BOX_FLAG_FLOATING;
                        rc_box->fixed_position = V2{(board->layout_box.w - rc_w) / 2, 4};
                        rc_box->elem_align = {UI_ALIGN_CENTER, UI_ALIGN_CENTER};
                        theme::Apply_Panel(rc_box, theme::Panel());

                        ui.sizes.push({UI_Size_Fit(), UI_Size_Fit()});
                        auto rc_style = theme::Label_Body(match_font);
                        for (auto &s : rc_style)
                            s.text.color = theme::TEXT_ERROR;
                        w.styles.push(rc_style);
                        w.Label("Reconnecting... (attempt " + std::to_string(reconnect_attempt) +
                                ")");
                        w.styles.pop();
                        ui.sizes.pop();
                    }
                    ui.sizes.pop();
                }

                if (!is_local && !game_state.Is_Game_Over()) {
                    const float btn_w = 160.0f, btn_h = 32.0f;
                    float hand_top = board->layout_box.h - 100.0f;

                    ui.sizes.push({UI_Size_Pixels(btn_w), UI_Size_Pixels(btn_h)});
                    DIV(&w) {
                        UI_Box *box = ui.leafs.back();
                        box->flags |= UI_BOX_FLAG_FLOATING;
                        box->fixed_position = V2{10, hand_top - btn_h - 5};
                        box->elem_align = {UI_ALIGN_CENTER, UI_ALIGN_CENTER};
                        theme::Apply_Panel(box, theme::Panel());

                        ui.sizes.push({UI_Size_Fit(), UI_Size_Fit()});
                        w.styles.push(theme::Button_Danger());
                        if (w.Button("Concede").flags & UI_SIG_LEFT_RELEASED) {
                            Send_Action(state.current_game_id, "", Game::Concede_Action{});
                        }
                        w.styles.pop();
                        ui.sizes.pop();
                    }
                    ui.sizes.pop();

                    ui.sizes.push({UI_Size_Pixels(btn_w), UI_Size_Pixels(btn_h)});
                    DIV(&w) {
                        UI_Box *box = ui.leafs.back();
                        box->flags |= UI_BOX_FLAG_FLOATING;
                        box->fixed_position = V2{10, hand_top - btn_h * 2 - 10};
                        box->elem_align = {UI_ALIGN_CENTER, UI_ALIGN_CENTER};
                        theme::Apply_Panel(box, theme::Panel());

                        ui.sizes.push({UI_Size_Fit(), UI_Size_Fit()});
                        if (game_state.Draw_Offered_By_Us()) {
                            w.styles.push(theme::Label_Body(match_font));
                            w.Label("Draw offered...");
                            w.styles.pop();
                        } else if (!game_state.Has_Draw_Offer()) {
                            w.styles.push(theme::Button_Secondary());
                            if (w.Button("Offer Draw").flags & UI_SIG_LEFT_RELEASED) {
                                Send_Action(state.current_game_id, "", Game::Draw_Offer_Action{});
                            }
                            w.styles.pop();
                        }
                        ui.sizes.pop();
                    }
                    ui.sizes.pop();
                }

                if (!is_local && game_state.Has_Draw_Offer() &&
                    game_state.Draw_Offer_From() != state.user_id) {
                    const float do_w = 300.0f, do_h = 80.0f;
                    ui.sizes.push({UI_Size_Pixels(do_w), UI_Size_Pixels(do_h)});
                    DIV(&w) {
                        UI_Box *do_box = ui.leafs.back();
                        do_box->flags |= UI_BOX_FLAG_FLOATING;
                        do_box->fixed_position =
                            V2{(board->layout_box.w - do_w) / 2, (board->layout_box.h - do_h) / 2};
                        do_box->child_layout_axis = 1;
                        do_box->elem_align = {UI_ALIGN_CENTER, UI_ALIGN_CENTER};
                        theme::Apply_Panel(do_box, theme::Panel());

                        ui.sizes.push({UI_Size_Fit(), UI_Size_Fit()});
                        const float m = 4.0f;
                        ui.margins.push({m, m, m, m});

                        w.styles.push(theme::Label_Title(match_font));
                        w.Label("Draw Offered!");
                        w.styles.pop();

                        const std::string &gid = state.current_game_id;
                        w.styles.push(theme::Button_Primary());
                        if (w.Button("Accept").flags & UI_SIG_LEFT_RELEASED) {
                            if (Send_Action(gid, "", Game::Draw_Response_Action{true}))
                                game_state.Clear_Draw_Offer();
                        }
                        w.styles.pop();

                        w.styles.push(theme::Button_Danger());
                        if (w.Button("Decline").flags & UI_SIG_LEFT_RELEASED) {
                            if (Send_Action(gid, "", Game::Draw_Response_Action{false}))
                                game_state.Clear_Draw_Offer();
                        }
                        w.styles.pop();
                        ui.margins.pop();
                        ui.sizes.pop();
                    }
                    ui.sizes.pop();
                }

                if (!is_local && auto_pass_mode != 0) {
                    if (game_state.Has_Snapshot()) {
                        int turn = game_state.Snapshot().turn_number;
                        if (auto_pass_mode == 2 && turn != last_auto_pass_turn &&
                            last_auto_pass_turn != 0)
                            auto_pass_mode = 0;
                        last_auto_pass_turn = turn;
                    }
                    if (auto_pass_mode != 0) {
                        const float ap_w = 280.0f, ap_h = 30.0f;
                        ui.sizes.push({UI_Size_Pixels(ap_w), UI_Size_Pixels(ap_h)});
                        DIV(&w) {
                            UI_Box *ap_box = ui.leafs.back();
                            ap_box->flags |= UI_BOX_FLAG_FLOATING;
                            ap_box->fixed_position = V2{board->layout_box.w - ap_w - 10,
                                                        board->layout_box.h - ap_h - 10};
                            ap_box->elem_align = {UI_ALIGN_CENTER, UI_ALIGN_CENTER};
                            theme::Apply_Panel(ap_box, theme::Panel());

                            ui.sizes.push({UI_Size_Fit(), UI_Size_Fit()});
                            auto ap_style = theme::Label_Body(match_font);
                            for (auto &s : ap_style)
                                s.text.color = theme::TEXT_GOLD;
                            w.styles.push(ap_style);
                            if (auto_pass_mode == 1)
                                w.Label("[Auto-Pass: No Actions] F4=off");
                            else
                                w.Label("[Auto-Pass: Until EOT] F4=off");
                            w.styles.pop();
                            ui.sizes.pop();
                        }
                        ui.sizes.pop();
                    }
                }

                if (!is_local && game_state.Has_Prompt()) {
                    const auto &prompt = game_state.Pending_Prompt();
                    const std::string &pid = prompt.prompt_id;
                    const std::string &gid = state.current_game_id;

                    if (pid != last_prompt_id) {
                        last_prompt_id = pid;
                        damage_split.clear();
                        selected_discards.clear();
                        selected_modes.clear();
                        selected_target = 0;
                        combat_ui.Clear();

                        if (auto *ap = std::get_if<Game::Attacker_Prompt>(&prompt.prompt)) {
                            combat_ui.attacker_prompt_active = true;
                            combat_ui.eligible_attackers.insert(ap->eligible_attackers.begin(),
                                                                ap->eligible_attackers.end());
                        } else if (auto *bp = std::get_if<Game::Blocker_Prompt>(&prompt.prompt)) {
                            combat_ui.blocker_prompt_active = true;
                            combat_ui.eligible_blockers.insert(bp->eligible_blockers.begin(),
                                                               bp->eligible_blockers.end());
                            combat_ui.attacking_creatures = bp->attacking_creatures;
                            for (uint64_t atk_id : bp->attacking_creatures) {
                                auto *perm = const_cast<Game::Permanent_State *>(
                                    Game::instances.Find(atk_id));
                                if (perm)
                                    perm->attacking = true;
                            }
                        } else if (auto *tp = std::get_if<Game::Target_Prompt>(&prompt.prompt)) {
                            combat_ui.legal_targets.insert(tp->legal_targets.begin(),
                                                           tp->legal_targets.end());
                        }
                    }

                    const float prompt_w = 220.0f;
                    const float prompt_h = 60.0f;
                    const float side_w = 163.0f;
                    ui.sizes.push({UI_Size_Pixels(prompt_w), UI_Size_Pixels(prompt_h)});
                    DIV(&w) {
                        UI_Box *bar = ui.leafs.back();
                        bar->flags |= UI_BOX_FLAG_FLOATING;
                        float hand_top = board->layout_box.h - 100.0f;
                        bar->fixed_position = V2{board->layout_box.w - side_w - prompt_w - 10,
                                                 hand_top - prompt_h - 5};
                        theme::Apply_Panel(bar, theme::Panel());
                        bar->child_layout_axis = 1;
                        bar->elem_align = {UI_ALIGN_CENTER, UI_ALIGN_CENTER};

                        ui.sizes.push({UI_Size_Fit(), UI_Size_Fit()});
                        defer(ui.sizes.pop());

                        const float btn_margin = 4.0f;
                        ui.margins.push({btn_margin, btn_margin, btn_margin / 2, btn_margin / 2});
                        defer(ui.margins.pop());

                        std::visit(
                            [&](const auto &p) {
                                using T = std::decay_t<decltype(p)>;

                                if constexpr (std::is_same_v<T, Game::Priority_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label("Priority");
                                    w.styles.pop();

                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});

                                    if (p.can_play_land) {
                                        const auto *me = game_state.My_State(state.user_id);
                                        if (me) {
                                            w.styles.push(theme::Button_Secondary());
                                            for (auto card_id : me->hand) {
                                                const auto *card = Game::instances.Find(card_id);
                                                if (card && card->type == Game::Card_Type::Land) {
                                                    std::string label =
                                                        "Land: " + (card->name.empty()
                                                                        ? std::to_string(card_id)
                                                                        : card->name);
                                                    if (w.Button(label).flags &
                                                        UI_SIG_LEFT_RELEASED) {
                                                        Send_And_Clear(
                                                            game_state, gid, pid,
                                                            Game::Play_Land_Action{card_id});
                                                    }
                                                }
                                            }
                                            w.styles.pop();
                                        }
                                    }

                                    if (!p.castable_card_ids.empty()) {
                                        w.styles.push(theme::Button_Secondary());
                                        for (auto card_id : p.castable_card_ids) {
                                            const auto *card = Game::instances.Find(
                                                static_cast<Game::Card_ID>(card_id));
                                            std::string label = "Cast: ";
                                            if (card && !card->name.empty()) {
                                                label += card->name + " (" +
                                                         Format_Mana_Cost(card->mana_cost) + ")";
                                            } else {
                                                label += std::to_string(card_id);
                                            }
                                            if (w.Button(label).flags & UI_SIG_LEFT_RELEASED) {
                                                Send_And_Clear(game_state, gid, pid,
                                                               Game::Play_Card_Action{card_id});
                                            }
                                        }
                                        w.styles.pop();
                                    }

                                    if (!p.activatable_permanent_ids.empty()) {
                                        w.styles.push(theme::Button_Secondary());
                                        for (auto perm_id : p.activatable_permanent_ids) {
                                            if (game_state.Already_Activated(perm_id))
                                                continue;
                                            const auto *perm = Game::instances.Find(
                                                static_cast<Game::Permanent_ID>(perm_id));
                                            const Game::Card *card =
                                                perm ? Game::instances.Find(perm->card) : nullptr;
                                            std::string perm_name = card && !card->name.empty()
                                                                        ? card->name
                                                                        : std::to_string(perm_id);

                                            int num_abilities =
                                                card ? static_cast<int>(
                                                           card->activated_abilities.size())
                                                     : 0;

                                            if (num_abilities <= 1) {
                                                std::string label = "Activate: " + perm_name;
                                                if (num_abilities == 1)
                                                    label +=
                                                        " (" +
                                                        card->activated_abilities[0].cost_text +
                                                        ": " +
                                                        card->activated_abilities[0].effect_text +
                                                        ")";
                                                if (w.Button(label).flags & UI_SIG_LEFT_RELEASED) {
                                                    game_state.Mark_Activated(perm_id);
                                                    Send_And_Clear(
                                                        game_state, gid, pid,
                                                        Game::Activate_Ability_Action{perm_id, 0});
                                                }
                                            } else {
                                                for (int i = 0; i < num_abilities; i++) {
                                                    const auto &ability =
                                                        card->activated_abilities[i];
                                                    std::string label = perm_name + " [" +
                                                                        ability.cost_text + ": " +
                                                                        ability.effect_text + "]";
                                                    if (w.Button(label).flags &
                                                        UI_SIG_LEFT_RELEASED) {
                                                        game_state.Mark_Activated(perm_id);
                                                        Send_And_Clear(
                                                            game_state, gid, pid,
                                                            Game::Activate_Ability_Action{perm_id,
                                                                                          i});
                                                    }
                                                }
                                            }
                                        }
                                        w.styles.pop();
                                    }

                                    w.Spacer(UI_Size_Pixels(4));

                                    w.styles.push(theme::Button_Primary());
                                    if (w.Button("Pass").flags & UI_SIG_LEFT_RELEASED) {
                                        Send_And_Clear(game_state, gid, pid,
                                                       Game::Pass_Priority_Action{});
                                    }
                                    w.styles.pop();

                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T, Game::Yes_No_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label(p.question.empty() ? "Choose" : p.question);
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    w.styles.push(theme::Button_Primary());
                                    if (w.Button("Yes").flags & UI_SIG_LEFT_RELEASED) {
                                        Send_And_Clear(game_state, gid, pid,
                                                       Game::Yes_No_Action{true});
                                    }
                                    w.styles.pop();
                                    w.Spacer(UI_Size_Pixels(4));
                                    w.styles.push(theme::Button_Secondary());
                                    if (w.Button("No").flags & UI_SIG_LEFT_RELEASED) {
                                        Send_And_Clear(game_state, gid, pid,
                                                       Game::Yes_No_Action{false});
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T, Game::Attacker_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label("Declare Attackers (" +
                                            std::to_string(combat_ui.selected_attackers.size()) +
                                            ")");
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});

                                    w.styles.push(theme::Button_Secondary());
                                    if (w.Button("All Attack").flags & UI_SIG_LEFT_RELEASED) {
                                        combat_ui.selected_attackers.assign(
                                            p.eligible_attackers.begin(),
                                            p.eligible_attackers.end());
                                    }
                                    w.styles.pop();

                                    w.Spacer(UI_Size_Pixels(4));
                                    w.styles.push(theme::Button_Primary());
                                    if (w.Button("Confirm Attackers").flags &
                                        UI_SIG_LEFT_RELEASED) {
                                        Game::Declare_Attackers_Action act;
                                        uint64_t def_player = p.defending_players.empty()
                                                                  ? 0
                                                                  : p.defending_players[0];
                                        for (auto id : combat_ui.selected_attackers)
                                            act.attackers.push_back({id, def_player});
                                        if (Send_And_Clear(game_state, gid, pid, std::move(act)))
                                            combat_ui.Clear();
                                    }
                                    w.styles.pop();
                                    w.Spacer(UI_Size_Pixels(4));
                                    w.styles.push(theme::Button_Secondary());
                                    if (w.Button("No Attacks").flags & UI_SIG_LEFT_RELEASED) {
                                        if (Send_And_Clear(game_state, gid, pid,
                                                           Game::Declare_Attackers_Action{}))
                                            combat_ui.Clear();
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T, Game::Blocker_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    std::string blk_label =
                                        "Declare Blockers (" +
                                        std::to_string(combat_ui.selected_blockers.size()) + ")";
                                    if (combat_ui.pending_blocker != 0)
                                        blk_label += " - Click an attacker";
                                    w.Label(blk_label);
                                    w.styles.pop();

                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});

                                    w.styles.push(theme::Button_Primary());
                                    if (w.Button("Confirm Blockers").flags & UI_SIG_LEFT_RELEASED) {
                                        Game::Declare_Blockers_Action act;
                                        for (const auto &[b, a] : combat_ui.selected_blockers)
                                            act.blockers.push_back({b, a});
                                        if (Send_And_Clear(game_state, gid, pid, std::move(act)))
                                            combat_ui.Clear();
                                    }
                                    w.styles.pop();
                                    w.Spacer(UI_Size_Pixels(4));
                                    w.styles.push(theme::Button_Secondary());
                                    if (w.Button("No Blocks").flags & UI_SIG_LEFT_RELEASED) {
                                        if (Send_And_Clear(game_state, gid, pid,
                                                           Game::Declare_Blockers_Action{}))
                                            combat_ui.Clear();
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T,
                                                                    Game::Order_Blockers_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label("Order Blockers");
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    w.styles.push(theme::Button_Primary());
                                    Game::Order_Blockers_Action act;
                                    act.ordered_blocker_ids = p.unordered_blockers;
                                    if (w.Button("Confirm Order").flags & UI_SIG_LEFT_RELEASED) {
                                        Send_And_Clear(game_state, gid, pid, std::move(act));
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<
                                                         T, Game::Damage_Assignment_Prompt>) {
                                    int num_blockers = static_cast<int>(p.ordered_blockers.size());
                                    if (static_cast<int>(damage_split.size()) != num_blockers)
                                        damage_split.assign(num_blockers, 0);

                                    int assigned = 0;
                                    for (int d : damage_split)
                                        assigned += d;
                                    int remaining = p.total_damage - assigned;

                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label("Assign Damage (" + std::to_string(remaining) +
                                            " remaining)");
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});

                                    for (int i = 0; i < num_blockers; i++) {
                                        const auto *perm = Game::instances.Find(
                                            static_cast<Game::Permanent_ID>(p.ordered_blockers[i]));
                                        const Game::Card *card =
                                            perm ? Game::instances.Find(perm->card) : nullptr;
                                        std::string name =
                                            card && !card->name.empty()
                                                ? card->name
                                                : std::to_string(p.ordered_blockers[i]);

                                        w.styles.push(theme::Label_Body(match_font));
                                        w.Label(name + ": " + std::to_string(damage_split[i]));
                                        w.styles.pop();
                                        w.styles.push(theme::Button_Secondary());
                                        if (w.Button(name + " +").flags & UI_SIG_LEFT_RELEASED) {
                                            if (remaining > 0)
                                                damage_split[i]++;
                                        }
                                        if (w.Button(name + " -").flags & UI_SIG_LEFT_RELEASED) {
                                            if (damage_split[i] > 0)
                                                damage_split[i]--;
                                        }
                                        w.styles.pop();
                                    }

                                    w.styles.push(theme::Label_Body(match_font));
                                    w.Label("Player: " + std::to_string(remaining));
                                    w.styles.pop();

                                    w.Spacer(UI_Size_Pixels(4));
                                    w.styles.push(theme::Button_Primary());
                                    if (w.Button("Confirm Damage").flags & UI_SIG_LEFT_RELEASED) {
                                        Game::Damage_Assignment_Action act;
                                        act.damage_to_each_blocker = damage_split;
                                        int used = 0;
                                        for (int d : damage_split)
                                            used += d;
                                        act.damage_to_player = p.total_damage - used;
                                        if (Send_And_Clear(game_state, gid, pid, std::move(act)))
                                            damage_split.clear();
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T, Game::Target_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label(p.filter.empty() ? "Select Target" : p.filter);
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    w.styles.push(theme::Button_Secondary());
                                    for (auto tid : p.legal_targets) {
                                        const auto *perm = Game::instances.Find(
                                            static_cast<Game::Permanent_ID>(tid));
                                        const Game::Card *card =
                                            perm ? Game::instances.Find(perm->card)
                                                 : Game::instances.Find(
                                                       static_cast<Game::Card_ID>(tid));
                                        std::string label = card && !card->name.empty()
                                                                ? card->name
                                                                : "Target " + std::to_string(tid);
                                        bool is_sel = selected_target == tid;
                                        if (is_sel)
                                            label = "[*] " + label;
                                        if (w.Button(label).flags & UI_SIG_LEFT_RELEASED)
                                            selected_target = tid;
                                    }
                                    w.styles.pop();
                                    w.Spacer(UI_Size_Pixels(4));
                                    w.styles.push(theme::Button_Primary());
                                    {
                                        uint64_t t = selected_target ? selected_target
                                                                     : (p.legal_targets.empty()
                                                                            ? 0
                                                                            : p.legal_targets[0]);
                                        bool can_confirm = (t != 0);
                                        if (can_confirm && w.Button("Confirm Target").flags &
                                                               UI_SIG_LEFT_RELEASED) {
                                            if (Send_And_Clear(game_state, gid, pid,
                                                               Game::Select_Target_Action{t}))
                                                selected_target = 0;
                                        } else if (!can_confirm) {
                                            w.Label("No valid targets");
                                        }
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T, Game::Discard_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label("Discard " + std::to_string(p.count) + " card(s) (" +
                                            std::to_string(selected_discards.size()) +
                                            " selected)");
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    for (const auto &card : p.hand) {
                                        bool is_sel =
                                            std::find(selected_discards.begin(),
                                                      selected_discards.end(),
                                                      card.instance_id) != selected_discards.end();
                                        std::string label =
                                            (card.name.empty() ? std::to_string(card.instance_id)
                                                               : card.name);
                                        if (is_sel)
                                            label = "[D] " + label;
                                        w.styles.push(is_sel ? theme::Button_Primary()
                                                             : theme::Button_Secondary());
                                        if (w.Button(label).flags & UI_SIG_LEFT_RELEASED) {
                                            if (is_sel)
                                                selected_discards.erase(
                                                    std::remove(selected_discards.begin(),
                                                                selected_discards.end(),
                                                                card.instance_id),
                                                    selected_discards.end());
                                            else if (static_cast<int>(selected_discards.size()) <
                                                     p.count)
                                                selected_discards.push_back(card.instance_id);
                                        }
                                        w.styles.pop();
                                    }
                                    w.Spacer(UI_Size_Pixels(4));
                                    w.styles.push(theme::Button_Primary());
                                    if (w.Button("Confirm Discard").flags & UI_SIG_LEFT_RELEASED) {
                                        if (Send_And_Clear(game_state, gid, pid,
                                                           Game::Discard_Action{selected_discards}))
                                            selected_discards.clear();
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T, Game::Mana_Payment_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label("Pay Mana: " + p.cost_description);
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    w.styles.push(theme::Label_Body(match_font));
                                    w.Label("Pool: W" + std::to_string(p.available.white) + " U" +
                                            std::to_string(p.available.blue) + " B" +
                                            std::to_string(p.available.black) + " R" +
                                            std::to_string(p.available.red) + " G" +
                                            std::to_string(p.available.green) + " C" +
                                            std::to_string(p.available.colorless));
                                    w.styles.pop();
                                    w.styles.push(theme::Button_Primary());
                                    if (w.Button("Auto-Pay").flags & UI_SIG_LEFT_RELEASED) {
                                        Send_And_Clear(game_state, gid, pid,
                                                       Game::Pay_Mana_Action{p.available});
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T, Game::Mode_Choice_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label("Choose Mode(s) (" + std::to_string(p.min_choices) +
                                            "-" + std::to_string(p.max_choices) + ") [" +
                                            std::to_string(selected_modes.size()) + " selected]");
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    for (int i = 0; i < static_cast<int>(p.modes.size()); i++) {
                                        bool is_sel =
                                            std::find(selected_modes.begin(), selected_modes.end(),
                                                      i) != selected_modes.end();
                                        std::string label = p.modes[i];
                                        if (is_sel)
                                            label = "[*] " + label;
                                        w.styles.push(is_sel ? theme::Button_Primary()
                                                             : theme::Button_Secondary());
                                        if (w.Button(label).flags & UI_SIG_LEFT_RELEASED) {
                                            if (is_sel) {
                                                selected_modes.erase(
                                                    std::remove(selected_modes.begin(),
                                                                selected_modes.end(), i),
                                                    selected_modes.end());
                                            } else if (static_cast<int>(selected_modes.size()) <
                                                       p.max_choices) {
                                                selected_modes.push_back(i);
                                            }
                                        }
                                        w.styles.pop();
                                    }
                                    w.Spacer(UI_Size_Pixels(4));
                                    w.styles.push(theme::Button_Primary());
                                    if (static_cast<int>(selected_modes.size()) >= p.min_choices &&
                                        w.Button("Confirm Modes").flags & UI_SIG_LEFT_RELEASED) {
                                        if (Send_And_Clear(
                                                game_state, gid, pid,
                                                Game::Select_Mode_Action{selected_modes}))
                                            selected_modes.clear();
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T, Game::Color_Choice_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label(p.reason.empty() ? "Choose Color" : p.reason);
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    w.styles.push(theme::Button_Secondary());
                                    if (p.legal_colors.empty()) {
                                        auto color_btn = [&](const char *name, Game::Mana_Color c) {
                                            if (w.Button(name).flags & UI_SIG_LEFT_RELEASED) {
                                                Send_And_Clear(game_state, gid, pid,
                                                               Game::Select_Color_Action{c});
                                            }
                                        };
                                        color_btn("White", Game::Mana_Color::White);
                                        color_btn("Blue", Game::Mana_Color::Blue);
                                        color_btn("Black", Game::Mana_Color::Black);
                                        color_btn("Red", Game::Mana_Color::Red);
                                        color_btn("Green", Game::Mana_Color::Green);
                                    } else {
                                        for (const auto &c : p.legal_colors) {
                                            Game::Mana_Color mc = Game::Mana_Color::Colorless;
                                            if (c == "White")
                                                mc = Game::Mana_Color::White;
                                            else if (c == "Blue")
                                                mc = Game::Mana_Color::Blue;
                                            else if (c == "Black")
                                                mc = Game::Mana_Color::Black;
                                            else if (c == "Red")
                                                mc = Game::Mana_Color::Red;
                                            else if (c == "Green")
                                                mc = Game::Mana_Color::Green;
                                            if (w.Button(c).flags & UI_SIG_LEFT_RELEASED) {
                                                Send_And_Clear(game_state, gid, pid,
                                                               Game::Select_Color_Action{mc});
                                            }
                                        }
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T,
                                                                    Game::Creature_Type_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label(p.reason.empty() ? "Choose Creature Type" : p.reason);
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    w.styles.push(theme::Button_Secondary());
                                    std::vector<std::string> types;
                                    if (!p.suggestions.empty()) {
                                        types = p.suggestions;
                                    } else {
                                        types = {"Human",   "Elf",    "Goblin", "Zombie",
                                                 "Soldier", "Wizard", "Dragon", "Angel"};
                                    }
                                    for (const auto &ct : types) {
                                        if (w.Button(ct).flags & UI_SIG_LEFT_RELEASED) {
                                            Send_And_Clear(game_state, gid, pid,
                                                           Game::Select_Creature_Type_Action{ct});
                                        }
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T, Game::X_Cost_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label("Choose X for " + p.card_name);
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    w.styles.push(theme::Button_Secondary());
                                    for (int x = 0; x <= p.max_x; x++) {
                                        std::string label = "X = " + std::to_string(x);
                                        if (w.Button(label).flags & UI_SIG_LEFT_RELEASED) {
                                            Game::Play_Card_Action act;
                                            act.x_value = x;
                                            Send_And_Clear(game_state, gid, pid, std::move(act));
                                        }
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                } else if constexpr (std::is_same_v<T,
                                                                    Game::Concede_Confirm_Prompt>) {
                                    w.styles.push(theme::Label_Title(match_font));
                                    w.Label(p.message.empty() ? "Concede?" : p.message);
                                    w.styles.pop();
                                    ui.sizes.push({UI_Size_Text(2), UI_Size_Text(2)});
                                    w.styles.push(theme::Button_Danger());
                                    if (w.Button("Yes, Concede").flags & UI_SIG_LEFT_RELEASED) {
                                        Send_And_Clear(game_state, gid, pid,
                                                       Game::Yes_No_Action{true});
                                    }
                                    w.styles.pop();
                                    w.styles.push(theme::Button_Secondary());
                                    if (w.Button("Cancel").flags & UI_SIG_LEFT_RELEASED) {
                                        Send_And_Clear(game_state, gid, pid,
                                                       Game::Yes_No_Action{false});
                                    }
                                    w.styles.pop();
                                    ui.sizes.pop();
                                }
                            },
                            prompt.prompt);
                    }
                    ui.sizes.pop();
                } else {
                    if (combat_ui.attacker_prompt_active || combat_ui.blocker_prompt_active)
                        combat_ui.Clear();
                }

                if (game_state.Is_Game_Over()) {
                    const float go_w = 360.0f;
                    const float go_h = 200.0f;
                    ui.sizes.push({UI_Size_Pixels(go_w), UI_Size_Pixels(go_h)});
                    DIV(&w) {
                        UI_Box *go_box = ui.leafs.back();
                        go_box->flags |= UI_BOX_FLAG_FLOATING;
                        go_box->fixed_position =
                            V2{(board->layout_box.w - go_w) / 2, (board->layout_box.h - go_h) / 2};
                        go_box->child_layout_axis = 1;
                        go_box->elem_align = {UI_ALIGN_CENTER, UI_ALIGN_CENTER};
                        std::array<Widget_Style, WIDGET_STYLE_COUNT> go_panel;
                        for (auto &s : go_panel) {
                            s.background = {0x1A, 0x0A, 0x0A, 0xDD};
                            s.border = {0xD4, 0xAF, 0x37, 0xFF};
                        }
                        theme::Apply_Panel(go_box, go_panel);

                        ui.sizes.push({UI_Size_Parent(1.0), UI_Size_Fit()});
                        const float m = 6.0f;
                        ui.margins.push({m, m, m, m});

                        std::string headline;
                        SDL_Color headline_color;
                        if (game_state.Game_Over_Is_Draw()) {
                            headline = "Draw";
                            headline_color = {0xD4, 0xAF, 0x37, 0xFF};
                        } else if (game_state.Game_Over_Winner() == state.user_id) {
                            headline = "Victory";
                            headline_color = {0x40, 0xFF, 0x40, 0xFF};
                        } else {
                            headline = "Defeat";
                            headline_color = {0xFF, 0x40, 0x40, 0xFF};
                        }
                        auto title_style = theme::Label_Title(match_font);
                        for (auto &s : title_style)
                            s.text.color = headline_color;
                        w.styles.push(title_style);
                        w.Label(headline);
                        w.styles.pop();

                        if (!game_state.Game_Over_Reason().empty()) {
                            w.styles.push(theme::Label_Body(match_font));
                            w.Label(game_state.Game_Over_Reason());
                            w.styles.pop();
                        }

                        w.Spacer(UI_Size_Pixels(4));

                        if (game_state.Has_Snapshot()) {
                            auto body = theme::Label_Body(match_font);
                            for (const auto &p : game_state.Snapshot().players) {
                                std::string name = p.username.empty()
                                                       ? "Player " + std::to_string(p.player_id)
                                                       : p.username;
                                std::string line =
                                    name + "  -  " + std::to_string(p.life_total) + " HP";
                                bool is_winner = (p.player_id == game_state.Game_Over_Winner());
                                auto line_style = body;
                                if (is_winner && !game_state.Game_Over_Is_Draw())
                                    for (auto &s : line_style)
                                        s.text.color = {0xD4, 0xAF, 0x37, 0xFF};
                                w.styles.push(line_style);
                                w.Label(line);
                                w.styles.pop();
                            }
                        }

                        w.Spacer(UI_Size_Pixels(8));

                        w.styles.push(theme::Button_Danger());
                        ui.sizes.push({UI_Size_Parent(0.8), UI_Size_Text(2)});
                        bool ret = w.Button("Return to Menu").flags & UI_SIG_LEFT_RELEASED;
                        ui.sizes.pop();
                        w.styles.pop();
                        ui.margins.pop();
                        ui.sizes.pop();

                        if (ret) {
                            if (!is_local) {
                                game_client.Leave_Game(state.current_game_id);
                                game_client.Stop_Action_Stream();
                                game_client.Stop_State_Stream();
                            }
                            state.current_game_id.clear();
                            state.selected_deck_name.clear();
                            state.scene = Scene::Main_Menu;
                            return true;
                        }
                    }
                    ui.sizes.pop();
                }
            }
            ui.sizes.pop();
        } else {
            ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Parent(1.0f)});
            DIV(&w) {
                UI_Box *wait_box = ui.leafs.back();
                wait_box->child_layout_axis = 1;
                wait_box->elem_align = {UI_ALIGN_CENTER, UI_ALIGN_CENTER};

                ui.sizes.push({UI_Size_Fit(), UI_Size_Fit()});
                w.styles.push(theme::Label_Body(match_font));
                w.Label("Waiting for game state...");
                w.styles.pop();

                if (!match_error.empty()) {
                    auto err_style = theme::Label_Body(match_font);
                    for (auto &s : err_style)
                        s.text.color = theme::TEXT_ERROR;
                    w.styles.push(err_style);
                    w.Label(match_error);
                    w.styles.pop();
                }

                w.styles.push(theme::Button_Danger(match_font));
                leaving = w.Button("Leave").flags & UI_SIG_LEFT_RELEASED;
                w.styles.pop();
                ui.sizes.pop();
            }
            ui.sizes.pop();
        }

        if (leaving) {
            if (!is_local) {
                game_client.Leave_Game(state.current_game_id);
                game_client.Stop_Action_Stream();
                game_client.Stop_State_Stream();
            }
            state.current_game_id.clear();
            state.selected_deck_name.clear();
            state.scene = Scene::Main_Menu;
            return true;
        }

        ui.End();

        w.Draw();
        Draw_Pending_Card_Overlays(state.renderer, match_font);

        Drag_Overlay_UI(ui);
        Combat_Lines_UI(state.renderer, &combat_ui);

        {
            uint64_t cur_hover = combat_ui.hovered_card_id;
            combat_ui.hovered_card_id = 0;
            if (cur_hover != 0) {
                if (cur_hover != hover_card_id) {
                    hover_card_id = cur_hover;
                    hover_start_ms = SDL_GetTicks();
                    showing_detail = false;
                } else if (!showing_detail && SDL_GetTicks() - hover_start_ms > 500) {
                    showing_detail = true;
                }
            } else {
                hover_card_id = 0;
                showing_detail = false;
            }

            if (showing_detail && hover_card_id != 0) {
                const Game::Card *card =
                    Game::instances.Find(static_cast<Game::Card_ID>(hover_card_id));
                if (card) {
                    int w_px = 0, h_px = 0;
                    SDL_GetWindowSize(state.window, &w_px, &h_px);
                    const float detail_w = 280, detail_h = 400;
                    float x = static_cast<float>(w_px) - detail_w - 20;
                    float y = (static_cast<float>(h_px) - detail_h) / 2;

                    SDL_FRect bg = {x - 4, y - 4, detail_w + 8, detail_h + 8};
                    SDL_SetRenderDrawColor(state.renderer, 0x1A, 0x0A, 0x0A, 0xEE);
                    SDL_RenderFillRect(state.renderer, &bg);
                    SDL_SetRenderDrawColor(state.renderer, 0xD4, 0xAF, 0x37, 0xFF);
                    SDL_RenderRect(state.renderer, &bg);

                    SDL_Texture *tex = Game::card_textures.Get(card->name);
                    if (tex) {
                        SDL_FRect art = {x, y, detail_w, detail_h * 0.55f};
                        SDL_RenderTexture(state.renderer, tex, NULL, &art);
                    }

                    float text_y = y + detail_h * 0.58f;
                    auto render_line = [&](const std::string &text, SDL_Color col) {
                        if (text.empty())
                            return;
                        TTF_Text *ttf =
                            TTF_CreateText(text_engine, match_font, text.c_str(), text.length());
                        if (ttf) {
                            SDL_SetRenderDrawColor(state.renderer, col.r, col.g, col.b, col.a);
                            TTF_DrawRendererText(ttf, x + 8, text_y);
                            TTF_DestroyText(ttf);
                        }
                        text_y += 20;
                    };
                    render_line(card->name, {0xFF, 0xFF, 0xFF, 0xFF});
                    render_line(Format_Mana_Cost(card->mana_cost), {0xD4, 0xAF, 0x37, 0xFF});
                    if (!card->oracle_text.empty())
                        render_line(card->oracle_text, {0xCC, 0xCC, 0xCC, 0xFF});
                    if (card->creature_stats.has_value())
                        render_line(std::to_string(card->creature_stats->power) + "/" +
                                        std::to_string(card->creature_stats->toughness),
                                    {0xFF, 0xFF, 0xFF, 0xFF});
                }
            }
        }

        for (auto &df : game_state.pending_damage_floats)
            damage_floats.push_back(df);
        game_state.pending_damage_floats.clear();
        for (auto &lp : game_state.pending_life_pulses)
            life_pulses.push_back(lp);
        game_state.pending_life_pulses.clear();
        game_state.pending_card_anims.clear();
        if (game_state.phase_just_changed) {
            phase_flash.start_ms = SDL_GetTicks();
            game_state.phase_just_changed = false;
        }
        if (game_state.turn_just_changed) {
            turn_banner_start_ms = SDL_GetTicks();
            if (game_state.Has_Snapshot()) {
                turn_banner_is_ours = (game_state.Snapshot().active_player_id == my_id);
            }
            game_state.turn_just_changed = false;
        }

        {
            int w_px = 0, h_px = 0;
            SDL_GetWindowSize(state.window, &w_px, &h_px);
            Uint64 now = SDL_GetTicks();
            for (auto it = damage_floats.begin(); it != damage_floats.end();) {
                float elapsed = static_cast<float>(now - it->start_ms);
                if (elapsed > 1200.0f) {
                    it = damage_floats.erase(it);
                    continue;
                }
                if (!it->pos_resolved) {
                    auto rect_it = combat_ui.permanent_rects.find(it->target_id);
                    if (rect_it != combat_ui.permanent_rects.end()) {
                        it->pos = {rect_it->second.x + rect_it->second.w / 2, rect_it->second.y};
                    } else {
                        it->pos = {static_cast<float>(w_px) / 2, static_cast<float>(h_px) / 2};
                    }
                    it->pos_resolved = true;
                }

                float t = elapsed / 1200.0f;
                float ease_t = 1.0f - (1.0f - t) * (1.0f - t);
                float x = it->pos.x;
                float y = it->pos.y - 60.0f * ease_t;
                Uint8 alpha = static_cast<Uint8>(255 * (1.0f - t));

                std::string dmg_text = "-" + std::to_string(it->amount);
                TTF_Text *ttf =
                    TTF_CreateText(text_engine, match_font, dmg_text.c_str(), dmg_text.length());
                if (ttf) {
                    SDL_SetRenderDrawColor(state.renderer, 0xFF, 0x40, 0x40, alpha);
                    TTF_DrawRendererText(ttf, x, y);
                    TTF_DestroyText(ttf);
                }
                ++it;
            }
        }

        {
            int w_px = 0, h_px = 0;
            SDL_GetWindowSize(state.window, &w_px, &h_px);
            Uint64 now = SDL_GetTicks();
            for (auto it = life_pulses.begin(); it != life_pulses.end();) {
                float elapsed = static_cast<float>(now - it->start_ms);
                if (elapsed > 800.0f) {
                    it = life_pulses.erase(it);
                    continue;
                }
                float t = elapsed / 800.0f;
                float ease_t = 1.0f - (1.0f - t) * (1.0f - t);
                Uint8 alpha = static_cast<Uint8>(255 * (1.0f - t));

                bool is_me = (it->player_id == my_id);
                float x = is_me ? 20.0f : static_cast<float>(w_px) - 80.0f;
                float base_y =
                    is_me ? static_cast<float>(h_px) * 0.4f : static_cast<float>(h_px) * 0.15f;
                float y = base_y - 30.0f * ease_t;

                std::string txt = (it->delta > 0 ? "+" : "") + std::to_string(it->delta);
                Uint8 r = it->delta > 0 ? 0x40 : 0xFF;
                Uint8 g = it->delta > 0 ? 0xFF : 0x40;
                Uint8 b = 0x40;

                TTF_Text *ttf = TTF_CreateText(text_engine, match_font, txt.c_str(), txt.length());
                if (ttf) {
                    SDL_SetRenderDrawColor(state.renderer, r, g, b, alpha);
                    TTF_DrawRendererText(ttf, x, y);
                    TTF_DestroyText(ttf);
                }
                ++it;
            }
        }

        if (phase_flash.start_ms > 0) {
            Uint64 now = SDL_GetTicks();
            float elapsed = static_cast<float>(now - phase_flash.start_ms);
            if (elapsed < 600.0f) {
                float t = elapsed / 600.0f;
                Uint8 alpha = static_cast<Uint8>(180 * (1.0f - t));
                int w_px = 0, h_px = 0;
                SDL_GetWindowSize(state.window, &w_px, &h_px);
                SDL_FRect flash_bar = {0, static_cast<float>(h_px) * 0.48f,
                                       static_cast<float>(w_px), 4.0f};
                SDL_SetRenderDrawBlendMode(state.renderer, SDL_BLENDMODE_BLEND);
                SDL_SetRenderDrawColor(state.renderer, 0xFF, 0xD7, 0x00, alpha);
                SDL_RenderFillRect(state.renderer, &flash_bar);
            } else {
                phase_flash.start_ms = 0;
            }
        }

        if (turn_banner_start_ms > 0) {
            Uint64 now = SDL_GetTicks();
            float elapsed = static_cast<float>(now - turn_banner_start_ms);
            const float banner_duration = 1500.0f;
            if (elapsed < banner_duration) {
                int w_px = 0, h_px = 0;
                SDL_GetWindowSize(state.window, &w_px, &h_px);
                float t = elapsed / banner_duration;
                float alpha_t;
                if (t < 0.2f)
                    alpha_t = t / 0.2f;
                else if (t < 0.7f)
                    alpha_t = 1.0f;
                else
                    alpha_t = (1.0f - t) / 0.3f;
                Uint8 alpha = static_cast<Uint8>(220 * std::clamp(alpha_t, 0.0f, 1.0f));

                float bar_h = 40.0f;
                float bar_y = static_cast<float>(h_px) * 0.45f - bar_h / 2;
                SDL_FRect bar = {0, bar_y, static_cast<float>(w_px), bar_h};
                SDL_SetRenderDrawBlendMode(state.renderer, SDL_BLENDMODE_BLEND);
                SDL_SetRenderDrawColor(state.renderer, 0x00, 0x00, 0x00, alpha);
                SDL_RenderFillRect(state.renderer, &bar);

                const char *banner_text = turn_banner_is_ours ? "YOUR TURN" : "OPPONENT'S TURN";
                SDL_Color text_color = turn_banner_is_ours ? SDL_Color{0x40, 0xFF, 0x40, alpha}
                                                           : SDL_Color{0xFF, 0x80, 0x40, alpha};
                TTF_Text *ttf =
                    TTF_CreateText(text_engine, match_font, banner_text, SDL_strlen(banner_text));
                if (ttf) {
                    int tw = 0, th = 0;
                    TTF_GetTextSize(ttf, &tw, &th);
                    float tx = (static_cast<float>(w_px) - tw) / 2;
                    float ty = bar_y + (bar_h - th) / 2;
                    SDL_SetRenderDrawColor(state.renderer, text_color.r, text_color.g, text_color.b,
                                           text_color.a);
                    TTF_DrawRendererText(ttf, tx, ty);
                    TTF_DestroyText(ttf);
                }
            } else {
                turn_banner_start_ms = 0;
            }
        }

        if (game_state.Has_Snapshot()) {
            const auto *me = game_state.My_State(is_local ? 1 : state.user_id);
            if (me && me->has_priority && me->clock_remaining_ms > 0) {
                int w_px = 0, h_px = 0;
                SDL_GetWindowSize(state.window, &w_px, &h_px);
                float max_time = 600000.0f;
                float fraction =
                    std::clamp(static_cast<float>(me->clock_remaining_ms) / max_time, 0.0f, 1.0f);
                float bar_w = static_cast<float>(w_px) * 0.5f;
                float bar_h = 4.0f;
                float bar_x = (static_cast<float>(w_px) - bar_w) / 2;
                float bar_y = static_cast<float>(h_px) - 12.0f;

                SDL_FRect bar = {bar_x, bar_y, bar_w * fraction, bar_h};
                Uint8 r, g, b;
                if (fraction > 0.5f) {
                    r = 0x40;
                    g = 0xFF;
                    b = 0x40;
                } else if (fraction > 0.2f) {
                    r = 0xFF;
                    g = 0xFF;
                    b = 0x40;
                } else {
                    r = 0xFF;
                    g = 0x40;
                    b = 0x40;
                }
                SDL_SetRenderDrawColor(state.renderer, r, g, b, 0xFF);
                SDL_RenderFillRect(state.renderer, &bar);
            }
        }

        SDL_RenderPresent(state.renderer);
    }

    state.scene = Scene::Exit;
    return true;
}
