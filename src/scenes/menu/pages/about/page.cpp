#include "page.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "scenes/common/ui_theme.hpp"
#include "ui/widgets/widgets.hpp"

void Menu_About_Page(Widget_Context &w, UI_Context &ui) {
    DIV(&w) {
        UI_Box *div = ui.leafs.back();
        div->child_layout_axis = 1;
        div->elem_align = UI_ALIGN_CENTER;
        theme::Apply_Panel(div, theme::Panel());

        ui.sizes.push({UI_Size_Text(50), UI_Size_Text(50)});
        defer(ui.sizes.pop());

        TTF_Font *font_title = state.font[paths::beleren_bold];
        //TTF_SetFontSize(font_title, 22);
        /*
        ui.fonts.push(font_title);
        defer(ui.fonts.pop());
        */

        w.styles.push(theme::Label_Title(font_title));
        UI_Signal label = w.Label("About (WIP)");
        label.box->label_alignment = UI_ALIGN_CENTER;
        w.styles.pop();
    }
}
