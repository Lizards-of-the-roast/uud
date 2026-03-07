#include "page.hpp"

#include <string>

#include "core/defer.hpp"
#include "core/state.hpp"
#include "net/matchmaking_client.hpp"
#include "scenes/common/ui_theme.hpp"
#include "scenes/menu/menu_types.hpp"
#include "ui/widgets/widgets.hpp"

static void Styled_Message(Widget_Context &w, const std::string &text, SDL_Color color) {
    auto style = theme::Label_Body();
    for (auto &s : style)
        s.text.color = color;
    w.styles.push(style);
    w.Label(text);
    w.styles.pop();
}

bool Menu_Matchmaking_Page(Widget_Context &w, UI_Context &ui, Menu_Tab &tab) {
    DIV(&w) {
        UI_Box *div = ui.leafs.back();
        div->child_layout_axis = 1;
        div->elem_align = UI_ALIGN_CENTER;
        theme::Apply_Panel(div, theme::Panel());

        //TTF_Font *font = state.font[paths::beleren_bold];

        /*
        ui.fonts.push(font);
        defer(ui.fonts.pop());
        */

        ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
        defer(ui.label_alignments.pop());

        ui.sizes.push({UI_Size_Parent(0.9), UI_Size_Text(10)});
        defer(ui.sizes.pop());

        static std::string status_text;
        static std::string error_text;
        static bool was_active = false;

        if (!was_active) {
            if (state.offline || !matchmaking_client.In_Queue()) {
                status_text.clear();
                error_text.clear();
            }
            was_active = true;
        }

        if (!state.offline) {
            if (auto join = matchmaking_client.Poll_Join()) {
                if (join->success) {
                    status_text = "In queue...";
                    error_text.clear();
                } else {
                    error_text = join->error.empty() ? "Failed to join queue" : join->error;
                    status_text.clear();
                }
            }

            if (auto update = matchmaking_client.Poll_Update()) {
                if (update->error) {
                    error_text = update->error_message;
                    status_text.clear();
                } else if (update->matched) {
                    state.current_game_id = update->game_id;
                    state.scene = Scene::Match;
                    status_text.clear();
                    return true;
                } else {
                    status_text =
                        "Position: " + std::to_string(update->queue_position) + " | Wait: ~" +
                        std::to_string(update->estimated_wait_seconds) + "s";
                }
            }
        }

        //TTF_SetFontSize(font, 24);
        w.styles.push(theme::Label_Title());
        w.Label("Matchmaking");
        w.styles.pop();

        if (!error_text.empty())
            Styled_Message(w, error_text, theme::TEXT_ERROR);
        if (!status_text.empty())
            Styled_Message(w, status_text, theme::TEXT_INFO);

        //TTF_SetFontSize(font, 18);

        ui.sizes.push({UI_Size_Fit(), UI_Size_Text(10)});
        defer(ui.sizes.pop());

        if (w.Button("Play Local").flags & UI_SIG_LEFT_RELEASED) {
            state.current_game_id = "local";
            state.scene = Scene::Match;
            status_text.clear();
            error_text.clear();
            was_active = false;
            return true;
        }

        if (!state.offline) {
            w.Spacer(UI_Size_Pixels(8));

            if (matchmaking_client.In_Queue()) {
                w.styles.push(theme::Button_Secondary());
                if (w.Button("Cancel Queue").flags & UI_SIG_LEFT_RELEASED) {
                    matchmaking_client.Leave_Queue();
                    status_text.clear();
                    error_text.clear();
                }
                w.styles.pop();
            } else {
                if (w.Button("Find Match").flags & UI_SIG_LEFT_RELEASED) {
                    error_text.clear();
                    status_text = "Joining queue...";
                    matchmaking_client.Join_Queue("standard", "default");
                }
            }
        }

        w.styles.push(theme::Button_Secondary());
        if (w.Button("Back").flags & UI_SIG_LEFT_RELEASED) {
            if (matchmaking_client.In_Queue())
                matchmaking_client.Leave_Queue();
            status_text.clear();
            error_text.clear();
            was_active = false;
            tab = Menu_Tab::None;
        }
        w.styles.pop();
    }
    return false;
}
