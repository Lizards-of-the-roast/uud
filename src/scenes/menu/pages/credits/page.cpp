#include "page.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "ui/widgets/widgets.hpp"

void Menu_Credits_Page(Widget_Context &w, UI_Context &ui) {
    DIV(&w) {
        UI_Box *div = ui.leafs.back();
        div->child_layout_axis = 1;
        div->elem_align = UI_ALIGN_CENTER;

        ui.sizes.push({UI_Size_Text(50), UI_Size_Text(50)});
        defer(ui.sizes.pop());

        ui.fonts.push(state.font[paths::beleren_bold]);
        defer(ui.fonts.pop());

        UI_Signal label = w.Label(
            "Lizards Of the Roast™\n"
            "Engine:\n"
            "    Ian Fogarty\n"
            "Client:\n"
            "    Matthew Conroy\n"
            "Card Scripting and Game Design:\n"
            "    Brendan Egan\n"
            "    Thibault Wysocinski\n");
        label.box->label_alignment = UI_ALIGN_CENTER;
    }
}
