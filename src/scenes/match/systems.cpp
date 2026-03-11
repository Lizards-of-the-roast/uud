#include "systems.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"

#include "game/textures.hpp"
#include "game/instances.hpp"
#include "game/permanent.hpp"

#include "scenes/common/ui_theme.hpp"

#include <variant>

const float card_width = 143.0f;
const float card_height = 200.0f;
const float card_aspect = card_width / card_height;
const float card_offset = 40.0f;
const float card_grow_amount = 100.0f;

UI_ID battlefield_ui = 0;
UI_ID graveyard_ui = 0;
UI_ID exile_ui = 0;


/*
I assume this is the servers job but i dont have the server yet
*/
static Game::Permanent_ID Card_To_Permanent(Game::Card_ID card, Game::Player_State *player)
{
    Game::Permanent_State p = {0};
    p.card = card;
    p.permanent_id = (Game::Permanent_ID)SDL_GetTicksNS(); /*tmp*/
    p.controller_id = p.owner_id
                    = player->player_id;
    Game::instances.Add(p);
    return p.permanent_id;
}

static bool Move_Card(Game::Player_State *player, Game::Card_ID card, UI_Signal sig, UI_ID zone)
{
    if ((~sig.flags) & UI_SIG_DROPPED_OUT || sig.drop_site == zone)
        return false;

    if (sig.drop_site == battlefield_ui)
    {
        player->battlefield.push_back(Card_To_Permanent(card, player));
        return true;
    }
    else if (sig.drop_site == graveyard_ui)
    {
        player->graveyard.push_back(card);
        return true;
    }
    else if (sig.drop_site == exile_ui)
    {
        player->exile.push_back(card);
        return true;
    }
    return false;
}

void Battlefield_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player) {
    Rect dst = {};
    dst.x = (float)state.window_width * 0.2f;
    dst.w = (float)state.window_width - dst.x;
    dst.h = (float)state.window_height * 0.5f;
    dst.y = (float)state.window_height * 0.5f - dst.h/2;

    w.styles.push(theme::Button_Primary());
    defer (w.styles.pop());
    UI_Signal div = w.Div_Begin(dst,
                                 UI_BOX_FLAG_DROPPABLE);
    battlefield_ui = div.box->id;
    defer (w.Div_End());

    div.box->child_layout_axis = 1;
    div.box->elem_align = {UI_ALIGN_LEFT, UI_ALIGN_BOTTOM};

    if (Widget_Data *widget = std::any_cast<Widget_Data>(&div.box->userdata))
    {
        widget->flags |= WIDGET_FLAG_DRAW_BORDER;
    }
    ui.sizes.push({UI_Size_Fit(), UI_Size_Pixels(card_height)});
    defer (ui.sizes.pop());

    SCROLL_O(&w, 0, {})
    {
        ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
        defer (ui.sizes.pop());
        for (size_t i = 0; i < player->battlefield.size(); i++)
        {
            const Game::Permanent_State *p = Game::instances.Find(player->battlefield[i]);
            const Game::Card *c = Game::instances.Find(p->card);
            UI_Signal sig = w.Card(*c, {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_DRAGGABLE);
            sig.box->margin = {0};
            if (sig.flags & UI_SIG_HOVERING || sig.flags & UI_SIG_LEFT_DOWN)
            {
                sig.box->size.x.value += card_grow_amount * card_aspect;
                sig.box->margin.top = -card_grow_amount;
            }
            if (Move_Card(player, c->instance_id, sig, battlefield_ui))
                player->battlefield.erase(player->battlefield.begin() + i);
        }
    UI_Margin m = {};
    m.left = m.right = m.bottom = 1;
    ui.margins.push(m);
    }
    ui.margins.pop();
}

void Hand_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player) {
    float n = (float)player->hand.size();
    float div_width = card_width * (float)player->hand.size() - ((card_offset * n * (n + 1))/2);
    DIV_O(&w, Rect{state.window_width/2 - div_width/2,
                   state.window_height - card_height / 3.0f, div_width, card_height}) {
        UI_Box *div = ui.leafs.back();
        div->flags &= ~UI_BOX_FLAG_CLIP;
        div->elem_align = UI_ALIGN_CENTER;
        div->offset.y = 0;

        ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
        defer(ui.sizes.pop());
        size_t hovered = INT32_MAX;
        for (size_t i = 0; i < player->hand.size(); i++) {
            const Game::Card *c = Game::instances.Find(player->hand[i]);
            UI_Signal button = w.Card(*c, {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP | UI_BOX_FLAG_DRAGGABLE, "Card[" + std::to_string(c->instance_id));
            if (Move_Card(player, c->instance_id, button, 0))
            {
                player->hand_count--;
                player->hand.erase(player->hand.begin() + i);
            }
            if (button.flags & (UI_SIG_HOVERING | UI_SIG_LEFT_DOWN)) {
                hovered = i;
                div->offset.y = -card_height / 3 * 2;
                button.box->margin.top// = button.box->margin.right
                                       = -card_grow_amount;
                button.box->margin.right = -card_grow_amount * card_aspect; 
            }
            else
            {
                button.box->margin.top = 0;
                button.box->margin.right = 0;
            }
            button.box->offset.x = -card_offset * ((!hovered || i < hovered) ? i : i - 1)
                                                + ((i > hovered) ? card_grow_amount : 0);
        }
    }
}

static void Library_UI(Widget_Context &w, UI_Context &ui, SDL_Texture *card_texture) {
    UI_Signal library =
    w.Label( {},{},
              UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP | UI_BOX_FLAG_DRAGGABLE
    );
    Widget_Data *widget = std::any_cast<Widget_Data>(&library.box->userdata);
    if (!widget)
        return;

    if (library.flags & UI_SIG_HOVERING || library.flags & UI_SIG_LEFT_DOWN)
        library.box->margin.right = library.box->margin.bottom = -30.0f;
    else
        library.box->margin.right = library.box->margin.bottom = 0.0f;
    widget->flags = 0x00;
    widget->texture = card_texture;
}
static UI_Signal Yard_UI(Widget_Context &w, UI_Context &ui, std::vector<Game::Card_ID> *yard, std::string label)
{
    UI_Signal sig;

    w.styles.push(theme::Button_Primary());
    defer (w.styles.pop());

    if (!yard->empty())
    {
        Game::Card_ID c = yard->back();
        const Game::Card *card = Game::instances.Find(c);

        sig = w.Card(*card, {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP | UI_BOX_FLAG_DRAG_DROP, label);
    }
    else
        sig = w.Label(label, {}, UI_BOX_FLAG_CLIP | UI_BOX_FLAG_DROPPABLE, label);

    Widget_Data *widget = std::any_cast<Widget_Data>(&sig.box->userdata);
    if (!widget)
        return {};
    widget->flags = WIDGET_FLAG_DRAW_TEXT | WIDGET_FLAG_DRAW_BORDER;
    if (yard->empty())
        widget->texture = NULL;

    if (sig.flags & UI_SIG_HOVERING || sig.flags & UI_SIG_LEFT_DOWN)
        sig.box->margin.right = sig.box->margin.bottom = -30.0f;
    else
        sig.box->margin.right = sig.box->margin.bottom = 0.0f;

    return sig;
}

static void Graveyard_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player)
{
    UI_Signal sig = Yard_UI(w, ui, &player->graveyard, "Graveyard");
    if (!sig.box)
        return;
    graveyard_ui = sig.box->id;
    if (player->graveyard.size() && Move_Card(player, player->graveyard.back(), sig, graveyard_ui))
    {
        player->graveyard.pop_back();
    }
}
static void Exile_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player)
{
    UI_Signal sig = Yard_UI(w, ui, &player->exile, "Exile");
    if (!sig.box)
        return;
    exile_ui = sig.box->id;
    if (player->exile.size() && Move_Card(player, player->exile.back(), sig, exile_ui))
    {
        player->exile.pop_back();
    }
}

void Side_Zones_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player, SDL_Texture *library_texture)
{
    const float margin = 10.0f;
    UI_Signal div = w.Div_Begin(Rect{
        10.0f, state.window_height * 0.25f,
        card_width, card_height*3 + margin*2
    });
    defer (w.Div_End());
    div.box->child_layout_axis = 1;
    ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
    defer (ui.sizes.pop());

    Library_UI(w,ui, library_texture);

    w.Spacer(UI_Size_Pixels(margin));

    //graveyard_ui = Yard_UI(w, ui, &player->graveyard, "Graveyard").box->id;
    Graveyard_UI(w, ui, player);

    w.Spacer(UI_Size_Pixels(margin));

    Exile_UI(w, ui, player);

}

void Drag_Overlay_UI(UI_Context &ui) {
    UI_Box *active = ui.Get_Box(ui.active);
    if (!active || ((~active->signal_last.flags) & UI_SIG_LEFT_DOWN)
    ||  ((~active->flags) & UI_BOX_FLAG_DRAGGABLE) )
        return;

    Widget_Data *widget = std::any_cast<Widget_Data>(&active->userdata);
    if (!widget || !widget->texture) {
        return;
    }

    V2 box_pos = (V2)(active->area.pos() + active->area.size() / 2);

    V2 v = (V2)(box_pos - ui.mouse_pos);
    float len = v.Length();

    if (len > 0.001f) {
        V2 v_perp_a = (V2)(V2{-v.y, v.x} / len);
        V2 v_perp_b = (V2)(V2{v.y, -v.x} / len);

        float width = active->area.w / 2;

        SDL_SetRenderDrawColor(state.renderer, 0xFF, 0x00, 0x00, 0xFF);
        SDL_RenderLine(state.renderer, box_pos.x + v_perp_a.x * width,
                       box_pos.y + v_perp_a.y * width, ui.mouse_pos.x, ui.mouse_pos.y);
        SDL_RenderLine(state.renderer, box_pos.x + v_perp_b.x * width,
                       box_pos.y + v_perp_b.y * width, ui.mouse_pos.x, ui.mouse_pos.y);
    }

    V2 drop_size = active->area.size();
    V2 drop_pos = (V2)(ui.mouse_pos - active->area.size() / 2);
    SDL_FRect drop_rect = {drop_pos.x, drop_pos.y, drop_size.x, drop_size.y};

    SDL_SetTextureAlphaMod(widget->texture, 0xFF * 0.7);
    SDL_RenderTexture(state.renderer, widget->texture, NULL, &drop_rect);
    SDL_SetTextureAlphaMod(widget->texture, 0xFF);
}
