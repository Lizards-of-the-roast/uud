#include "page.hpp"

#include "core/defer.hpp"
#include "scenes/common/ui_theme.hpp"
#include "ui/widgets/widgets.hpp"

void Menu_About_Page(Widget_Context &w, UI_Context &ui) {
    DIV(&w) {
        UI_Box *div = ui.leafs.back();
        div->child_layout_axis = 1;
        div->elem_align = UI_ALIGN_CENTER;
        theme::Apply_Panel(div, theme::Panel());

        ui.label_alignments.push({UI_ALIGN_CENTER, UI_ALIGN_CENTER});
        defer(ui.label_alignments.pop());

        ui.sizes.push({UI_Size_Parent(0.9), UI_Size_Text(10)});
        defer(ui.sizes.pop());

        w.styles.push(theme::Label_Title());
        w.Label("Untap Upkeep Draw");
        w.styles.pop();

        w.styles.push(theme::Label_Body());
        w.Label("A Magic: The Gathering game");
        w.Label("Built with SDL3, gRPC, and Protobuf");
        w.Spacer(UI_Size_Pixels(8));
        w.Label("Lizards of the Roast");
        w.styles.pop();
    }
}
