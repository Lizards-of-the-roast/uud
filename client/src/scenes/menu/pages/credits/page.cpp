#include "page.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "scenes/common/ui_theme.hpp"
#include "ui/widgets/widgets.hpp"

void Menu_Credits_Page(Widget_Context &w, UI_Context &ui) {
    DIV(&w) {
        UI_Box *div = ui.leafs.back();
        div->child_layout_axis = 1;
        div->elem_align = UI_ALIGN_CENTER;
        theme::Apply_Panel(div, theme::Panel());

        // TTF_Font *font_title = state.font[paths::beleren_bold];
        // TTF_SetFontSize(font_title, 22);
        // ui.fonts.push(font_title);

        ui.sizes.push({UI_Size_Text(50), UI_Size_Text(10)});
        w.styles.push(theme::Label_Title());
        UI_Signal title = w.Label("Credits");
        title.box->label_alignment = UI_ALIGN_CENTER;
        w.styles.pop();
        ui.sizes.pop();

        // ui.fonts.pop();

        TTF_Font *font_body = state.font[paths::mplantin_regular];
        // TTF_SetFontSize(font_body, 16);
        /*
        ui.fonts.push(font_body);
        defer(ui.fonts.pop());
        */

        ui.sizes.push({UI_Size_Text(50), UI_Size_Text(50)});
        defer(ui.sizes.pop());

        w.styles.push(theme::Label_Body(font_body));
        UI_Signal label = w.Label(
            "Lizards Of the Roast\n"
            "Engine:\n"
            "    Ian Fogarty\n"
            "Client:\n"
            "    Matthew Conroy\n"
            "Card Scripting and Game Design:\n"
            "    Brendan Egan\n"
            "    Thibault Wysocinski\n");
        label.box->label_alignment = UI_ALIGN_CENTER;
        w.styles.pop();
    }
}
