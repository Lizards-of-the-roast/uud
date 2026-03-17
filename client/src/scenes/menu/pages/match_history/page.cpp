#include "page.hpp"

#include <string>
#include <vector>

#include "core/defer.hpp"
#include "core/state.hpp"
#include "net/game_client.hpp"
#include "scenes/common/ui_theme.hpp"
#include "scenes/menu/menu_types.hpp"
#include "ui/widgets/widgets.hpp"

bool Menu_Match_History_Page(Widget_Context &w, UI_Context &ui, Menu_Tab &tab) {
    DIV(&w) {
        UI_Box *div = ui.leafs.back();
        div->child_layout_axis = 1;
        div->elem_align = UI_ALIGN_CENTER;
        theme::Apply_Panel(div, theme::Panel());

        ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
        defer(ui.label_alignments.pop());

        ui.sizes.push({UI_Size_Parent(0.9), UI_Size_Text(10)});
        defer(ui.sizes.pop());

        static std::vector<Game::Match_History_Entry> history;
        static bool fetched = false;
        static bool was_active = false;

        if (!was_active) {
            if (!state.offline && !fetched) {
                game_client.Get_Match_History(20);
            }
            was_active = true;
        }

        if (!state.offline) {
            if (auto result = game_client.Poll_Match_History()) {
                if (result->success) {
                    history = result->matches;
                    fetched = true;
                }
            }
        }

        w.styles.push(theme::Label_Title());
        w.Label("Match History");
        w.styles.pop();

        ui.sizes.push({UI_Size_Fit(), UI_Size_Text(6)});
        defer(ui.sizes.pop());

        if (!fetched) {
            w.styles.push(theme::Label_Body());
            w.Label(state.offline ? "Not available offline" : "Loading...");
            w.styles.pop();
        } else if (history.empty()) {
            w.styles.push(theme::Label_Body());
            w.Label("No matches played yet");
            w.styles.pop();
        } else {
            //NOTE: theres some weird behaviour with child sizing
            //      and scroll widgets, thus 0.5 parent
            w.styles.push(theme::Label_Body());
            ui.sizes.push({UI_Size_Parent(1.0), UI_Size_Parent(0.5)});
            defer(ui.sizes.pop());
            SCROLL_O(&w, 1, {}, UI_BOX_FLAG_CLIP)
            {
                ui.sizes.push({UI_Size_Parent(1.0), UI_Size_Text(6)});
                defer(ui.sizes.pop());
                for (size_t i = 0; i < history.size(); i++) {
                    const auto &entry = history[i];
                    std::string line;
                    if (entry.winner_id == 0)
                        line = "DRAW";
                    else if (entry.winner_id == state.user_id)
                        line = "WIN";
                    else
                        line = "LOSS";
                    line += " | " + std::to_string(entry.duration_seconds) + "s";
                    if (!entry.started_at.empty())
                        line += " | " + entry.started_at;
                    ui.Push_ID(i);
                    w.Label(line);
                    ui.Pop_ID();
                }
            }
            w.styles.pop();
        }

        ui.sizes.push({UI_Size_Fit(), UI_Size_Text(10)});
        w.styles.push(theme::Button_Secondary());
        if (w.Button("Back").flags & UI_SIG_LEFT_RELEASED) {
            was_active = false;
            fetched = false;
            history.clear();
            tab = Menu_Tab::None;
        }
        w.styles.pop();
        ui.sizes.pop();
    }
    return false;
}
