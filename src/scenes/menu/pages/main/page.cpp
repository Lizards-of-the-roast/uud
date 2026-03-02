#include "page.hpp"

#include "core/state.hpp"
#include "scenes/menu/menu_types.hpp"
#include "ui/widgets/widgets.hpp"

static void Offset_If_Hovered(UI_Signal sig, V2 offset) {
    bool hovering = sig.flags & (UI_SIG_HOVERING | UI_SIG_LEFT_DOWN);
    V2 mouse_rel = (V2)(sig.mouse_pos - sig.box->area.pos() - sig.box->offset);
    bool mouse_over_offset = mouse_rel.x > offset.x && mouse_rel.y > offset.y;
    if (hovering && (mouse_over_offset || sig.box->offset.x))
        sig.box->offset = offset;
    else
        sig.box->offset = V2{};
}

bool Menu_Main_Page(Widget_Context &w, UI_Context &ui, Menu_Tab &tab) {
    const float val = 20.0f;

    UI_Signal start = w.Button("Start Game");
    Offset_If_Hovered(start, {val, 0});
    if (start.flags & UI_SIG_LEFT_RELEASED) {
        tab = (tab != Menu_Tab::Match) ? Menu_Tab::Match : Menu_Tab::None;
    }

    UI_Signal settings = w.Button("Settings");
    Offset_If_Hovered(settings, {val, 0});
    if (settings.flags & UI_SIG_LEFT_RELEASED)
        tab = (tab != Menu_Tab::Settings) ? Menu_Tab::Settings : Menu_Tab::None;

    UI_Signal about = w.Button("About");
    Offset_If_Hovered(about, {val, 0});
    if (about.flags & UI_SIG_LEFT_RELEASED)
        tab = (tab != Menu_Tab::About) ? Menu_Tab::About : Menu_Tab::None;

    UI_Signal credits = w.Button("Credits");
    Offset_If_Hovered(credits, {val, 0});
    if (credits.flags & UI_SIG_LEFT_RELEASED)
        tab = (tab != Menu_Tab::Credits) ? Menu_Tab::Credits : Menu_Tab::None;

    UI_Signal quit = w.Button("Quit");
    Offset_If_Hovered(quit, {val, 0});
    if (quit.flags & UI_SIG_LEFT_RELEASED) {
        state.scene = Scene::Exit;
        return true;
    }

    return false;
}
