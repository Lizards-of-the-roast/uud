#include "page.hpp"

#include "core/state.hpp"
#include "net/net_client.hpp"
#include "net/token_store.hpp"
#include "scenes/common/ui_theme.hpp"
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

    {
        auto user_style = theme::Label_Body();
        for (auto &s : user_style)
            s.text.color = theme::TEXT_INFO;
        w.styles.push(user_style);
        UI_Box *label = NULL;
        if (state.offline)
            label = w.Label("Playing as Guest").box;
        else if (!state.username.empty())
            label = w.Label("Logged in as: " + state.username).box;
        w.styles.pop();
        if (label)
            label->Text_Copy_Font(user_style[0].text.font.value(), {.size = 40});
    }

    w.Spacer(UI_Size_Pixels(20));

    UI_Signal play = w.Button("Play");
    Offset_If_Hovered(play, {val, 0});
    if (play.flags & UI_SIG_LEFT_RELEASED)
        tab = (tab != Menu_Tab::Matchmaking) ? Menu_Tab::Matchmaking : Menu_Tab::None;

    UI_Signal deck = w.Button("Deck Builder");
    Offset_If_Hovered(deck, {val, 0});
    if (deck.flags & UI_SIG_LEFT_RELEASED)
        tab = (tab != Menu_Tab::Deck_Builder) ? Menu_Tab::Deck_Builder : Menu_Tab::None;

    w.Spacer(UI_Size_Pixels(8));

    w.styles.push(theme::Button_Secondary());

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

    w.styles.pop();

    w.Spacer(UI_Size_Pixels(8));

    w.styles.push(theme::Button_Danger());

    if (state.offline) {
        w.styles.push(theme::Button_Secondary());
        UI_Signal login = w.Button("Login");
        w.styles.pop();
        Offset_If_Hovered(login, {val, 0});
        if (login.flags & UI_SIG_LEFT_RELEASED) {
            state.scene = Scene::Login;
            w.styles.pop();
            return true;
        }
    } else {
        UI_Signal logout = w.Button("Logout");
        Offset_If_Hovered(logout, {val, 0});
        if (logout.flags & UI_SIG_LEFT_RELEASED) {
            Token_Store::Clear();
            net.Disconnect();
            state.user_id = 0;
            state.username.clear();
            state.offline = true;
            state.scene = Scene::Login;
            w.styles.pop();
            return true;
        }
    }

    UI_Signal quit = w.Button("Quit");
    Offset_If_Hovered(quit, {val, 0});
    if (quit.flags & UI_SIG_LEFT_RELEASED) {
        state.scene = Scene::Exit;
        w.styles.pop();
        return true;
    }

    w.styles.pop();

    return false;
}
