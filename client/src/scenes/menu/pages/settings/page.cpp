#include "page.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "scenes/common/ui_theme.hpp"
#include "ui/widgets/widgets.hpp"
#include <SDL3/SDL.h>

void Menu_Settings_Page(Widget_Context &w, UI_Context &ui) {
    DIV(&w) {
        UI_Box *div = ui.leafs.back();
        div->child_layout_axis = 1;
        div->elem_align = UI_ALIGN_CENTER;
        theme::Apply_Panel(div, theme::Panel());

        ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
        defer(ui.label_alignments.pop());

        ui.sizes.push({UI_Size_Parent(0.9), UI_Size_Text(10)});
        defer(ui.sizes.pop());

        static std::string status_msg;

        w.styles.push(theme::Label_Title());
        w.Label("Settings");
        w.styles.pop();

        w.styles.push(theme::Label_Body());
        w.Label("Server: " + state.server_address);
        w.styles.pop();

        w.styles.push(theme::Label_Body());
        w.Label(state.offline ? "Status: Offline" : "Status: Connected as " + state.username);
        w.styles.pop();

        ui.sizes.push({UI_Size_Fit(), UI_Size_Text(8)});
        defer(ui.sizes.pop());

        w.Spacer(UI_Size_Pixels(8));
    }
}
