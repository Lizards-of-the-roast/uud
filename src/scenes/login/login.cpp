#include "login.hpp"

#include <cstring>
#include <variant>

#include "core/defer.hpp"
#include "core/state.hpp"
#include "net/auth_client.hpp"
#include "net/net_client.hpp"
#include "net/token_store.hpp"
#include "scenes/common/scene_helpers.hpp"
#include "scenes/common/ui_theme.hpp"
#include "ui/widgets/widgets.hpp"
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>

enum class Login_Mode {
    Login,
    Register,
};

static void Styled_Message(Widget_Context &w, const std::string &text, SDL_Color color) {
    auto style = theme::Label_Body();
    for (auto &s : style)
        s.text = color;
    w.styles.push(style);
    w.Label(text);
    w.styles.pop();
}

bool Scene_Login(void) {
    TTF_TextEngine *text_engine = TTF_CreateRendererTextEngine(state.renderer);
    defer(TTF_DestroyRendererTextEngine(text_engine););
    UI_Context ui = UI_Context(state.window, text_engine);

    Widget_Context w = Widget_Context(state.renderer, &ui);
    w.default_style = theme::Button_Primary();

    SDL_Texture *bg = state.texture[paths::bg_texture];
    TTF_Font *font = state.font[paths::beleren_bold];

    Login_Mode mode = Login_Mode::Login;
    std::string error_text;
    std::string success_text;
    bool auto_login_attempted = false;

    SDL_Log("Scene_Login: entering render loop");
    for (;;) {
        state.Update_Delta_Time();
        for (SDL_Event event; SDL_PollEvent(&event);) {
            ui.Pass_Event(event);
            if (Handle_Window_Event(event))
                return true;
        }

        if (!auto_login_attempted) {
            auto_login_attempted = true;
            auto tokens = Token_Store::Load();
            if (tokens.has_value()) {
                state.server_address = tokens->server_address;
                auth_client.Validate(tokens->server_address, tokens->access_token);
                success_text = "Validating saved session...";
            }
        }

        if (auto result = auth_client.Poll()) {
            std::visit(
                [&](auto &r) {
                    using T = std::decay_t<decltype(r)>;
                    if constexpr (std::is_same_v<T, Auth_Login_Result>) {
                        if (r.success) {
                            state.user_id = r.user_id;
                            state.username = r.username;
                            state.offline = false;
                            state.scene = Scene::Main_Menu;
                            return;
                        }
                        error_text = r.error.empty() ? "Login failed" : r.error;
                        success_text.clear();
                    } else if constexpr (std::is_same_v<T, Auth_Register_Result>) {
                        if (r.success) {
                            success_text = "Registration successful! Please log in.";
                            error_text.clear();
                            mode = Login_Mode::Login;
                        } else {
                            error_text = r.error.empty() ? "Registration failed" : r.error;
                            success_text.clear();
                        }
                    } else if constexpr (std::is_same_v<T, Auth_Validate_Result>) {
                        if (r.valid) {
                            state.user_id = r.user_id;
                            state.username = r.username;
                            state.offline = false;
                            state.scene = Scene::Main_Menu;
                            return;
                        }
                        error_text.clear();
                        success_text.clear();
                    }
                },
                *result);

            if (state.scene == Scene::Main_Menu)
                return true;
        }

        SDL_SetRenderDrawColor(state.renderer, theme::SCENE_BG.r, theme::SCENE_BG.g,
                               theme::SCENE_BG.b, theme::SCENE_BG.a);
        SDL_RenderClear(state.renderer);
        if (bg) {
            float bg_w = 0.0f, bg_h = 0.0f;
            if (SDL_GetTextureSize(bg, &bg_w, &bg_h)) {
                SDL_FRect dst = {(float)state.window_width / 2.0f - bg_w / 2.0f,
                                 (float)state.window_height / 2.0f - bg_h / 2.0f, bg_w, bg_h};
                SDL_RenderTexture(state.renderer, bg, NULL, &dst);
            }
        }

        ui.Begin();
        ui.root.elem_align = UI_ALIGN_CENTER;

        ui.sizes.push({UI_Size_Parent(0.35), UI_Size_Child(0.0f)});
        defer(ui.sizes.pop());

        DIV(&w) {
            UI_Box *root_div = ui.leafs.back();
            root_div->child_layout_axis = 1;
            root_div->elem_align = UI_ALIGN_CENTER;
            theme::Apply_Panel(root_div, theme::Panel());

            const float m = 10.0f;
            ui.margins.push({.left = m, .right = m, .top = m / 2.0f, .bottom = m / 2.0f});
            defer(ui.margins.pop());

            ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
            defer(ui.label_alignments.pop());

            ui.fonts.push(font);
            defer(ui.fonts.pop());

            TTF_SetFontSize(font, 28);
            w.styles.push(theme::Label_Title());
            ui.sizes.push({UI_Size_Parent(0.9), UI_Size_Text(10)});
            w.Label("Untap Upkeep Draw");
            ui.sizes.pop();
            w.styles.pop();

            w.Spacer(UI_Size_Pixels(8));

            TTF_SetFontSize(font, 16);

            ui.sizes.push({UI_Size_Parent(0.85), UI_Size_Text(8)});
            defer(ui.sizes.pop());

            if (mode == Login_Mode::Login) {
                w.styles.push(theme::Label_Body());
                w.Label("Username");
                w.styles.pop();
                w.styles.push(theme::Textbox());
                UI_Signal username_sig = w.Textbox("", {}, std::string("login_username"));
                w.styles.pop();

                w.styles.push(theme::Label_Body());
                w.Label("Password");
                w.styles.pop();
                w.styles.push(theme::Textbox());
                UI_Signal password_sig = w.Textbox("", {}, std::string("login_password"));
                w.styles.pop();

                w.styles.push(theme::Label_Body());
                w.Label("Server Address");
                w.styles.pop();
                w.styles.push(theme::Textbox());
                UI_Signal server_sig =
                    w.Textbox(state.server_address, {}, std::string("login_server"));
                w.styles.pop();

                if (!error_text.empty())
                    Styled_Message(w, error_text, theme::TEXT_ERROR);
                if (!success_text.empty())
                    Styled_Message(w, success_text, theme::TEXT_SUCCESS);

                w.Spacer(UI_Size_Pixels(6));

                ui.sizes.push({UI_Size_Fit(), UI_Size_Text(8)});
                defer(ui.sizes.pop());

                TTF_SetFontSize(font, 18);

                if (auth_client.In_Flight()) {
                    w.styles.push(theme::Label_Body());
                    w.Label("Connecting...");
                    w.styles.pop();
                } else {
                    UI_Signal login_btn = w.Button("Login");
                    if (login_btn.flags & UI_SIG_LEFT_RELEASED) {
                        const char *user =
                            username_sig.box->label ? username_sig.box->label->text : "";
                        const char *pass =
                            password_sig.box->label ? password_sig.box->label->text : "";
                        const char *srv = server_sig.box->label ? server_sig.box->label->text : "";

                        if (user && pass && srv && std::strlen(user) > 0 && std::strlen(pass) > 0 &&
                            std::strlen(srv) > 0) {
                            state.server_address = srv;
                            error_text.clear();
                            success_text.clear();
                            auth_client.Login(srv, user, pass);
                        } else {
                            error_text = "Please fill in all fields";
                        }
                    }

                    w.styles.push(theme::Button_Secondary());
                    UI_Signal reg_btn = w.Button("Register Instead");
                    w.styles.pop();
                    if (reg_btn.flags & UI_SIG_LEFT_RELEASED) {
                        mode = Login_Mode::Register;
                        error_text.clear();
                        success_text.clear();
                    }
                }
            } else {
                w.styles.push(theme::Label_Body());
                w.Label("Username");
                w.styles.pop();
                w.styles.push(theme::Textbox());
                UI_Signal username_sig = w.Textbox("", {}, std::string("reg_username"));
                w.styles.pop();

                w.styles.push(theme::Label_Body());
                w.Label("Email");
                w.styles.pop();
                w.styles.push(theme::Textbox());
                UI_Signal email_sig = w.Textbox("", {}, std::string("reg_email"));
                w.styles.pop();

                w.styles.push(theme::Label_Body());
                w.Label("Password (8+ chars)");
                w.styles.pop();
                w.styles.push(theme::Textbox());
                UI_Signal password_sig = w.Textbox("", {}, std::string("reg_password"));
                w.styles.pop();

                w.styles.push(theme::Label_Body());
                w.Label("Confirm Password");
                w.styles.pop();
                w.styles.push(theme::Textbox());
                UI_Signal confirm_sig = w.Textbox("", {}, std::string("reg_confirm"));
                w.styles.pop();

                w.styles.push(theme::Label_Body());
                w.Label("Server Address");
                w.styles.pop();
                w.styles.push(theme::Textbox());
                UI_Signal server_sig =
                    w.Textbox(state.server_address, {}, std::string("reg_server"));
                w.styles.pop();

                if (!error_text.empty())
                    Styled_Message(w, error_text, theme::TEXT_ERROR);
                if (!success_text.empty())
                    Styled_Message(w, success_text, theme::TEXT_SUCCESS);

                w.Spacer(UI_Size_Pixels(6));

                ui.sizes.push({UI_Size_Fit(), UI_Size_Text(8)});
                defer(ui.sizes.pop());

                TTF_SetFontSize(font, 18);

                if (auth_client.In_Flight()) {
                    w.styles.push(theme::Label_Body());
                    w.Label("Registering...");
                    w.styles.pop();
                } else {
                    UI_Signal reg_btn = w.Button("Register");
                    if (reg_btn.flags & UI_SIG_LEFT_RELEASED) {
                        const char *user =
                            username_sig.box->label ? username_sig.box->label->text : "";
                        const char *email = email_sig.box->label ? email_sig.box->label->text : "";
                        const char *pass =
                            password_sig.box->label ? password_sig.box->label->text : "";
                        const char *confirm =
                            confirm_sig.box->label ? confirm_sig.box->label->text : "";
                        const char *srv = server_sig.box->label ? server_sig.box->label->text : "";

                        if (!user || !pass || !email || !confirm || !srv ||
                            std::strlen(user) == 0 || std::strlen(pass) == 0 ||
                            std::strlen(email) == 0 || std::strlen(srv) == 0) {
                            error_text = "Please fill in all fields";
                        } else if (std::strcmp(pass, confirm) != 0) {
                            error_text = "Passwords do not match";
                        } else {
                            state.server_address = srv;
                            error_text.clear();
                            success_text.clear();
                            auth_client.Register(srv, user, email, pass);
                        }
                    }

                    w.styles.push(theme::Button_Secondary());
                    UI_Signal back_btn = w.Button("Login Instead");
                    w.styles.pop();
                    if (back_btn.flags & UI_SIG_LEFT_RELEASED) {
                        mode = Login_Mode::Login;
                        error_text.clear();
                        success_text.clear();
                    }
                }
            }

            w.Spacer(UI_Size_Pixels(6));

            ui.sizes.push({UI_Size_Fit(), UI_Size_Text(8)});
            w.styles.push(theme::Button_Danger());
            UI_Signal quit = w.Button("Quit");
            w.styles.pop();
            ui.sizes.pop();
            if (quit.flags & UI_SIG_LEFT_RELEASED) {
                state.scene = Scene::Exit;
                return true;
            }
        }

        Present_Frame(ui, w);
    }
    return true;
}
