#include "page.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "scenes/menu/menu_types.hpp"
#include "ui/widgets/widgets.hpp"

bool Menu_Match_Page(Widget_Context &w, UI_Context &ui, Menu_Tab &tab) {
    DIV(&w) {
        UI_Box *div = ui.leafs.back();
        div->child_layout_axis = 1;
        div->elem_align = UI_ALIGN_CENTER;

        /*
        ui.fonts.push(state.font[paths::beleren_bold]);
        defer(ui.fonts.pop());
        */

        ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
        defer(ui.label_alignments.pop());

        ui.sizes.push({UI_Size_Parent(0.9), UI_Size_Text(40)});
        defer(ui.sizes.pop());

        UI_Signal label = w.Label("Enter Server IP");
        w.Textbox().box->size.y = UI_Size_Pixels(label.box->layout_box.h);
        DIV(&w) {
            ui.sizes.push({UI_Size_Fit(), UI_Size_Text(40)});
            defer(ui.sizes.pop());
            if (w.Button("Cancel").flags & UI_SIG_LEFT_RELEASED)
                tab = Menu_Tab::None;
            if (w.Button("Connect").flags & UI_SIG_LEFT_RELEASED) {
                state.scene = Scene::Match;
                return true;
            }
        }
    }
    return false;
}
