#include "systems.hpp"

#include <algorithm>
#include <cmath>
#include <variant>

#include "core/defer.hpp"
#include "core/state.hpp"
#include "game/instances.hpp"
#include "game/permanent.hpp"
#include "game/textures.hpp"
#include "scenes/common/ui_theme.hpp"

const float card_width = 143.0f;
const float card_height = 200.0f;
const float card_aspect = card_width / card_height;
const float card_offset = 40.0f;
const float card_grow_amount = 100.0f;

UI_ID battlefield_ui = 0;
UI_ID graveyard_ui = 0;
UI_ID exile_ui = 0;
UI_ID hand_ui = 0;
UI_ID library_ui = 0;

static std::unordered_map<uint64_t, SDL_FRect> card_positions;
SDL_FRect player_library_rect = {};
SDL_FRect opp_library_rect = {};

static void Sort_Battlefield(std::vector<Game::Permanent_ID> &bf) {
    auto sort_key = [](Game::Permanent_ID id) -> std::pair<int, int> {
        const Game::Permanent_State *p = Game::instances.Find(id);
        if (!p)
            return {1, 0};
        const Game::Card *c = Game::instances.Find(p->card);
        int group = 1;
        if (c) {
            if (c->type == Game::Card_Type::Creature)
                group = 0;
            else if (c->type == Game::Card_Type::Land)
                group = 2;
        }
        return {group, p->tapped ? 1 : 0};
    };
    std::stable_sort(bf.begin(), bf.end(), [&](Game::Permanent_ID a, Game::Permanent_ID b) {
        return sort_key(a) < sort_key(b);
    });
}

static Drag_Play drag_play;
Drag_Play &Pending_Drag_Play() {
    return drag_play;
}

static Drag_Attack drag_attack;
Drag_Attack &Pending_Drag_Attack() {
    return drag_attack;
}

static Drag_Block drag_block;
Drag_Block &Pending_Drag_Block() {
    return drag_block;
}

static Drag_Activate drag_activate;
Drag_Activate &Pending_Drag_Activate() {
    return drag_activate;
}

void Track_Card_Position(uint64_t id, const SDL_FRect &rect) {
    card_positions[id] = rect;
}

const SDL_FRect *Last_Card_Position(uint64_t id) {
    auto it = card_positions.find(id);
    return it != card_positions.end() ? &it->second : nullptr;
}

void Clear_Card_Positions() {
    card_positions.clear();
}

UI_ID player_battlefield_scroll_id = 0;
UI_ID opp_battlefield_scroll_id = 0;

/*
I assume this is the servers job but i dont have the server yet
*/
static Game::Permanent_ID Card_To_Permanent(Game::Card_ID card, Game::Player_State *player) {
    Game::Permanent_State p = {0};
    p.card = card;
    p.permanent_id = (Game::Permanent_ID)SDL_GetTicksNS(); /*tmp*/
    p.controller_id = p.owner_id = player->player_id;
    Game::instances.Add(p);
    return p.permanent_id;
}

static bool Move_Card(Game::Player_State *player, Game::Card_ID card, UI_Signal sig, UI_ID zone) {
    if ((~sig.flags) & UI_SIG_DROPPED_OUT || sig.drop_site == zone)
        return false;

    if (sig.drop_site == battlefield_ui) {
        player->battlefield.push_back(Card_To_Permanent(card, player));
        return true;
    } else if (sig.drop_site == graveyard_ui) {
        player->graveyard.push_back(card);
        return true;
    } else if (sig.drop_site == exile_ui) {
        player->exile.push_back(card);
        return true;
    } else if (sig.drop_site == hand_ui) {
        player->hand_count++;
        player->hand.push_back(card);
        return true;
    } else if (sig.drop_site == library_ui) {
        return true;
    }
    return false;
}

static void Player_Battlefield_UI(Widget_Context &w, UI_Context &ui, UI_Box *div,
                                  Game::Player_State *player, Combat_UI_State *combat,
                                  bool is_local) {
    SCROLL_O(&w, 0, true, Rect{0, div->layout_box.h - card_height, div->layout_box.w, card_height}) {
        UI_Box *scroll = ui.leafs.back();
        player_battlefield_scroll_id = scroll->id;
        scroll->elem_align.y = UI_ALIGN_BOTTOM;
        ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
        defer(ui.sizes.pop());
        for (size_t i = 0; i < player->battlefield.size(); i++) {
            Game::Permanent_State *p =
                (Game::Permanent_State *)Game::instances.Find(player->battlefield[i]);
            if (!p)
                continue;
            const Game::Card *c = Game::instances.Find(p->card);
            if (!c)
                continue;
            ui.Push_ID(player->battlefield[i]);
            UI_Signal sig = w.Card_Overlayed(p->card, p->permanent_id, {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_DRAGGABLE);
            ui.Pop_ID();
            Track_Card_Position(player->battlefield[i], sig.box->layout_box);
            //pending_overlays.push_back({sig.box, *c, *p, true});
            Widget_Data *widget = std::any_cast<Widget_Data>(&sig.box->userdata);
            sig.box->margin = {0};
            if (is_local && sig.flags & UI_SIG_RIGHT_RELEASED)
                p->tapped = !p->tapped;
            if (p->tapped) {
                auto tmp = sig.box->size.x;
                sig.box->size.x = sig.box->size.y;
                sig.box->size.y = tmp;
                widget->texture_rotaton = Widget_Rotation::Rot_90;
            } else
                widget->texture_rotaton = Widget_Rotation::Rot_0;
            uint64_t perm_id = player->battlefield[i];
            if (!is_local && (sig.flags & UI_SIG_RIGHT_RELEASED))
                drag_activate = {perm_id, true};
            bool combat_handled = false;
            if (combat) {
                combat->permanent_rects[perm_id] = sig.box->layout_box;

                if (combat->attacker_prompt_active) {
                    bool is_eligible = combat->eligible_attackers.contains(perm_id);
                    bool is_selected = std::find(combat->selected_attackers.begin(),
                                                 combat->selected_attackers.end(),
                                                 perm_id) != combat->selected_attackers.end();
                    if (is_eligible || is_selected) {
                        widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                        SDL_Color color = is_selected ? SDL_Color{0xFF, 0x40, 0x40, 0xFF}
                                                      : SDL_Color{0x40, 0xFF, 0x40, 0xFF};
                        for (auto &s : widget->style)
                            s.border = color;
                        if (is_selected)
                            sig.box->margin.top = -30.0f;
                        if (sig.flags & UI_SIG_LEFT_RELEASED) {
                            if (is_selected)
                                combat->selected_attackers.erase(
                                    std::remove(combat->selected_attackers.begin(),
                                                combat->selected_attackers.end(), perm_id),
                                    combat->selected_attackers.end());
                            else
                                combat->selected_attackers.push_back(perm_id);
                        }
                        if (is_eligible && !is_selected && (sig.flags & UI_SIG_DROPPED_OUT))
                            combat->selected_attackers.push_back(perm_id);
                        combat_handled = true;
                    } else if (!is_eligible) {
                        if (widget->texture)
                            SDL_SetTextureAlphaMod(widget->texture, 0x80);
                    }
                } else if (combat->blocker_prompt_active) {
                    bool is_eligible = combat->eligible_blockers.contains(perm_id);
                    bool is_assigned = false;
                    for (const auto &[b, a] : combat->selected_blockers)
                        if (b == perm_id) {
                            is_assigned = true;
                            break;
                        }
                    bool is_pending = (combat->pending_blocker == perm_id);

                    if (is_eligible || is_assigned) {
                        widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                        SDL_Color color = is_assigned  ? SDL_Color{0x40, 0x80, 0xFF, 0xFF}
                                          : is_pending ? SDL_Color{0xFF, 0xFF, 0x40, 0xFF}
                                                       : SDL_Color{0x40, 0xFF, 0x40, 0xFF};
                        for (auto &s : widget->style)
                            s.border = color;
                        if (is_assigned)
                            sig.box->margin.top = -30.0f;
                        if (sig.flags & UI_SIG_LEFT_RELEASED) {
                            if (is_assigned) {
                                combat->selected_blockers.erase(
                                    std::remove_if(combat->selected_blockers.begin(),
                                                   combat->selected_blockers.end(),
                                                   [perm_id](const auto &pair) {
                                                       return pair.first == perm_id;
                                                   }),
                                    combat->selected_blockers.end());
                            } else {
                                combat->pending_blocker = perm_id;
                            }
                        }
                        if (is_eligible && !is_assigned && (sig.flags & UI_SIG_DROPPED_OUT))
                            combat->pending_blocker = perm_id;
                        combat_handled = true;
                    }
                }
            }

            if (combat && !combat_handled && !combat->legal_targets.empty()) {
                bool is_target = combat->legal_targets.contains(perm_id);
                if (is_target) {
                    widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                    SDL_Color magenta = {0xFF, 0x40, 0xFF, 0xFF};
                    for (auto &s : widget->style)
                        s.border = magenta;
                    if (sig.flags & UI_SIG_LEFT_RELEASED)
                        combat->clicked_target = perm_id;
                }
            }

            if (!combat_handled) {
                if (p->attacking) {
                    widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                    SDL_Color red = {0xFF, 0x40, 0x40, 0xFF};
                    for (auto &s : widget->style)
                        s.border = red;
                    sig.box->margin.top = -30.0f;
                } else if (p->blocking) {
                    widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                    SDL_Color blue = {0x40, 0x80, 0xFF, 0xFF};
                    for (auto &s : widget->style)
                        s.border = blue;
                    sig.box->margin.top = -30.0f;
                }
            }
            if (sig.flags & UI_SIG_HOVERING || sig.flags & UI_SIG_LEFT_DOWN) {
                sig.box->size.x.value += card_grow_amount * ((!p->tapped) ? card_aspect : 1.0f);
                sig.box->margin.top = -card_grow_amount * ((p->tapped) ? card_aspect : 1.0f);
                if (combat && (sig.flags & UI_SIG_HOVERING))
                    combat->hovered_card_id = c->instance_id;
            }
            if (is_local && Move_Card(player, c->instance_id, sig, battlefield_ui)) {
                player->battlefield.erase(player->battlefield.begin() + i);
                i--;
            }
        }
        UI_Margin m = {};
        m.left = m.right = m.bottom = 1;
        ui.margins.push(m);
    }
    ui.margins.pop();
}
static void Opp_Battlefield_UI(Widget_Context &w, UI_Context &ui, UI_Box *div,
                               Game::Player_State *player, Combat_UI_State *combat) {
    SCROLL_O(&w, 0, true, Rect{0, 0, div->layout_box.w, card_height}) {
        UI_Box *scroll = ui.leafs.back();
        opp_battlefield_scroll_id = scroll->id;

        ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
        defer(ui.sizes.pop());
        for (size_t i = 0; i < player->battlefield.size(); i++) {
            Game::Permanent_State *p =
                (Game::Permanent_State *)Game::instances.Find(player->battlefield[i]);
            if (!p)
                continue;
            const Game::Card *c = Game::instances.Find(p->card);
            if (!c)
                continue;
            ui.Push_ID(player->battlefield[i]);
            UI_Signal sig = w.Card_Overlayed(p->card, p->permanent_id, {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_DRAGGABLE);
            ui.Pop_ID();
            Track_Card_Position(player->battlefield[i], sig.box->layout_box);
            //pending_overlays.push_back({sig.box, *c, *p, true});
            Widget_Data *widget = std::any_cast<Widget_Data>(&sig.box->userdata);
            widget->texture_flip = SDL_FLIP_VERTICAL;
            sig.box->margin = {0};
            if (p->tapped) {
                auto tmp = sig.box->size.x;
                sig.box->size.x = sig.box->size.y;
                sig.box->size.y = tmp;
                widget->texture_rotaton = Widget_Rotation::Rot_90;
            } else
                widget->texture_rotaton = Widget_Rotation::Rot_0;
            uint64_t perm_id = player->battlefield[i];
            if (combat) {
                combat->permanent_rects[perm_id] = sig.box->layout_box;

                if (combat->blocker_prompt_active && p->attacking) {
                    widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                    SDL_Color red = {0xFF, 0x40, 0x40, 0xFF};
                    for (auto &s : widget->style)
                        s.border = red;
                    sig.box->margin.bottom = -30.0f;
                    if (sig.flags & UI_SIG_LEFT_RELEASED && combat->pending_blocker != 0) {
                        combat->selected_blockers.push_back({combat->pending_blocker, perm_id});
                        combat->pending_blocker = 0;
                    }
                } else if (p->attacking) {
                    widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                    SDL_Color red = {0xFF, 0x40, 0x40, 0xFF};
                    for (auto &s : widget->style)
                        s.border = red;
                    sig.box->margin.bottom = -30.0f;
                } else if (p->blocking) {
                    widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                    SDL_Color blue = {0x40, 0x80, 0xFF, 0xFF};
                    for (auto &s : widget->style)
                        s.border = blue;
                    sig.box->margin.bottom = -30.0f;
                }
                if (!combat->legal_targets.empty()) {
                    bool is_target = combat->legal_targets.contains(perm_id);
                    if (is_target) {
                        widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                        SDL_Color magenta = {0xFF, 0x40, 0xFF, 0xFF};
                        for (auto &s : widget->style)
                            s.border = magenta;
                        if (sig.flags & UI_SIG_LEFT_RELEASED)
                            combat->clicked_target = perm_id;
                    }
                }
            } else {
                if (p->attacking) {
                    widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                    SDL_Color red = {0xFF, 0x40, 0x40, 0xFF};
                    for (auto &s : widget->style)
                        s.border = red;
                    sig.box->margin.bottom = -30.0f;
                } else if (p->blocking) {
                    widget->flags |= WIDGET_FLAG_DRAW_BORDER;
                    SDL_Color blue = {0x40, 0x80, 0xFF, 0xFF};
                    for (auto &s : widget->style)
                        s.border = blue;
                    sig.box->margin.bottom = -30.0f;
                }
            }
            if (sig.flags & UI_SIG_HOVERING || sig.flags & UI_SIG_LEFT_DOWN) {
                widget->texture_flip = SDL_FLIP_NONE;
                sig.box->size.x.value += card_grow_amount * ((!p->tapped) ? card_aspect : 1.0f);
                sig.box->margin.bottom = -card_grow_amount * ((p->tapped) ? card_aspect : 1.0f);
                if (combat && (sig.flags & UI_SIG_HOVERING))
                    combat->hovered_card_id = c->instance_id;
            }
        }
    }
}

void Battlefield_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                    Game::Player_State *opp, Combat_UI_State *combat, bool is_local) {
    UI_Box *div = ui.leafs.back();
    battlefield_ui = div->id;

    Sort_Battlefield(player->battlefield);
    Sort_Battlefield(opp->battlefield);

    div->child_layout_axis = 1;
    div->elem_align = {UI_ALIGN_LEFT, UI_ALIGN_BOTTOM};

    if (Widget_Data *widget = std::any_cast<Widget_Data>(&div->userdata)) {
        widget->flags |= WIDGET_FLAG_DRAW_BORDER;
    }

    UI_Box *hot = ui.Get_Box(ui.hot);
    if (hot && hot->parent && hot->parent->id == player_battlefield_scroll_id) {
        Player_Battlefield_UI(w, ui, div, player, combat, is_local);
        Opp_Battlefield_UI(w, ui, div, opp, combat);
    } else {
        Opp_Battlefield_UI(w, ui, div, opp, combat);
        Player_Battlefield_UI(w, ui, div, player, combat, is_local);
    }
}

void Hand_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player, bool is_local) {
    UI_Box *div = ui.leafs.back();
    hand_ui = div->id;
    div->offset.y = card_height / 2;

    ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
    defer(ui.sizes.pop());
    size_t hovered = INT32_MAX;
    for (size_t i = 0; i < player->hand.size(); i++) {
        const Game::Card *c = Game::instances.Find(player->hand[i]);
        if (!c)
            continue;
        UI_Signal button =
            w.Card_Overlayed(player->hand[i], 0, {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP | UI_BOX_FLAG_DRAGGABLE,
                   "Card[" + std::to_string(c->instance_id));
        Track_Card_Position(player->hand[i], button.box->layout_box);
        //pending_overlays.push_back({button.box, *c, {}, false});
        if (!is_local) {
            if ((button.flags & UI_SIG_DROPPED_OUT) && button.drop_site != hand_ui) {
                drag_play = {c->instance_id, true};
            }
        } else if (Move_Card(player, c->instance_id, button, hand_ui)) {
            player->hand_count--;
            player->hand.erase(player->hand.begin() + i);
            i--;
            continue;
        }
        if (button.flags & (UI_SIG_HOVERING | UI_SIG_LEFT_DOWN)) {
            hovered = i;
            div->offset.y = 0;
            button.box->margin.top = -card_grow_amount;
            button.box->margin.right = -card_grow_amount * card_aspect;
        } else {
            button.box->margin.top = 0;
            button.box->margin.right = 0;
        }
        button.box->offset.x = -card_offset * ((!hovered || i < hovered) ? i : i - 1) +
                               ((i > hovered) ? card_grow_amount : 0);
    }
}
void Opp_Hand_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player) {
    UI_Box *div = ui.leafs.back();
    div->offset.y = -card_height / 2;

    ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
    defer(ui.sizes.pop());
    int hovered = INT32_MAX;
    for (int i = 0; i < player->hand_count; i++) {
        UI_Signal button = w.Card({}, {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP,
                                  "Opp_Card[" + std::to_string(i));
        if (Widget_Data *widget = std::any_cast<Widget_Data>(&button.box->userdata)) {
            widget->texture_flip = SDL_FLIP_VERTICAL;
        }
        if (button.flags & (UI_SIG_HOVERING | UI_SIG_LEFT_DOWN)) {
            hovered = i;
            div->offset.y = 0;
            button.box->margin.bottom = -card_grow_amount;
            button.box->margin.right = -card_grow_amount * card_aspect;
        } else {
            button.box->margin.bottom = 0;
            button.box->margin.right = 0;
        }
        button.box->offset.x = -card_offset * ((!hovered || i < hovered) ? i : i - 1) +
                               ((i > hovered) ? card_grow_amount : 0);
    }
}

static UI_Signal Library_UI(
    Widget_Context &w, UI_Context &ui, Game::Player_State *player, SDL_Texture *card_texture,
    bool is_local = true, const std::source_location source_loc = std::source_location::current()) {
    UI_Signal library = w.Label(
        {}, {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP | UI_BOX_FLAG_DRAG_DROP, {}, source_loc);
    Widget_Data *widget = std::any_cast<Widget_Data>(&library.box->userdata);
    if (!widget)
        return library;

    if (library.flags & UI_SIG_HOVERING || library.flags & UI_SIG_LEFT_DOWN)
        library.box->margin.right = library.box->margin.bottom = -30.0f;
    else
        library.box->margin.right = library.box->margin.bottom = 0.0f;
    widget->flags = 0x00;
    widget->texture = card_texture;

    if (is_local && library.flags & UI_SIG_DROPPED_OUT) {
        Game::Card c = {};
        c.instance_id = SDL_GetTicksNS();
        if (Move_Card(player, c.instance_id, library, library_ui))
            Game::instances.Add(c);
    }
    return library;
}
static UI_Signal Yard_UI(Widget_Context &w, UI_Context &ui, std::vector<Game::Card_ID> *yard,
                         std::string label) {
    UI_Signal sig;

    w.styles.push(theme::Button_Primary());
    defer(w.styles.pop());

    if (!yard->empty()) {
        Game::Card_ID c = yard->back();
        const Game::Card *card = Game::instances.Find(c);

        if (card)
            sig = w.Card(*card, {},
                         UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP | UI_BOX_FLAG_DRAG_DROP, label);
        else
            sig = w.Label(label, {}, UI_BOX_FLAG_CLIP | UI_BOX_FLAG_DROPPABLE, label);
    } else
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

static void Graveyard_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                         bool is_local) {
    UI_Signal sig = Yard_UI(w, ui, &player->graveyard, "Graveyard");
    if (!sig.box)
        return;
    graveyard_ui = sig.box->id;
    if (is_local && player->graveyard.size() &&
        Move_Card(player, player->graveyard.back(), sig, graveyard_ui)) {
        player->graveyard.pop_back();
    }
}
static void Exile_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player, bool is_local) {
    UI_Signal sig = Yard_UI(w, ui, &player->exile, "Exile");
    if (!sig.box)
        return;
    exile_ui = sig.box->id;
    if (is_local && player->exile.size() &&
        Move_Card(player, player->exile.back(), sig, exile_ui)) {
        player->exile.pop_back();
    }
}

static void Player_Info_Panel(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                              TTF_Font *font, const Game::Game_Snapshot *snapshot,
                              bool *leave_pressed) {
    ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Child()});
    UI_Signal div = w.Div_Begin();
    defer(w.Div_End());

    div.box->child_layout_axis = 1;
    div.box->elem_align = UI_ALIGN_CENTER;
    theme::Apply_Panel(div.box, theme::Panel());

    ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Text(2)});
    defer(ui.sizes.pop());
    const float m = 4.0f;
    ui.margins.push({m, m, m / 2, m / 2});
    defer(ui.margins.pop());

    w.styles.push(theme::Label_Title(font));
    w.Label(std::to_string(player->life_total) + " HP");
    w.styles.pop();

    const auto &mp = player->mana_pool;
    if (mp.white || mp.blue || mp.black || mp.red || mp.green || mp.colorless) {
        std::string mana;
        if (mp.white)
            mana += "W" + std::to_string(mp.white) + " ";
        if (mp.blue)
            mana += "U" + std::to_string(mp.blue) + " ";
        if (mp.black)
            mana += "B" + std::to_string(mp.black) + " ";
        if (mp.red)
            mana += "R" + std::to_string(mp.red) + " ";
        if (mp.green)
            mana += "G" + std::to_string(mp.green) + " ";
        if (mp.colorless)
            mana += "C" + std::to_string(mp.colorless);
        w.styles.push(theme::Label_Body(font));
        w.Label(mana);
        w.styles.pop();
    }

    if (player->clock_remaining_ms > 0) {
        int secs = player->clock_remaining_ms / 1000;
        std::string clock = std::to_string(secs / 60) + ":" + (secs % 60 < 10 ? "0" : "") +
                            std::to_string(secs % 60);
        w.styles.push(theme::Label_Body(font));
        w.Label(clock);
        w.styles.pop();
    }

    if (snapshot) {
        auto phase_name = [](Game::Phase p) -> const char * {
            switch (p) {
                case Game::Phase::Untap:
                    return "Untap";
                case Game::Phase::Upkeep:
                    return "Upkeep";
                case Game::Phase::Draw:
                    return "Draw";
                case Game::Phase::Main_1:
                    return "Main 1";
                case Game::Phase::Beginning_Of_Combat:
                    return "Combat";
                case Game::Phase::Declare_Attackers:
                    return "Attackers";
                case Game::Phase::Declare_Blockers:
                    return "Blockers";
                case Game::Phase::First_Strike_Damage:
                    return "First Strike";
                case Game::Phase::Combat_Damage:
                    return "Damage";
                case Game::Phase::End_Of_Combat:
                    return "End Combat";
                case Game::Phase::Main_2:
                    return "Main 2";
                case Game::Phase::End_Step:
                    return "End";
                case Game::Phase::Cleanup:
                    return "Cleanup";
                default:
                    return "???";
            }
        };
        auto body = theme::Label_Body(font);
        for (auto &s : body)
            s.text.color = theme::TEXT_GOLD;
        w.styles.push(body);
        w.Label(std::string(phase_name(snapshot->current_phase)) + " | T" +
                std::to_string(snapshot->turn_number));
        w.styles.pop();
    }

    if (leave_pressed) {
        w.styles.push(theme::Button_Danger(font));
        ui.sizes.push({UI_Size_Parent(0.9f), UI_Size_Text(2)});
        *leave_pressed = w.Button("Leave").flags & UI_SIG_LEFT_RELEASED;
        ui.sizes.pop();
        w.styles.pop();
    }
}

void Side_Zones_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                   SDL_Texture *library_texture, TTF_Font *font,
                   const Game::Game_Snapshot *snapshot, bool *leave_pressed, bool is_local) {
    const float margin = 10.0f;
    ui.sizes.push({UI_Size_Pixels(card_width + margin * 2), UI_Size_Parent(1.0f)});
    UI_Signal div = w.Div_Begin();
    defer(w.Div_End());

    div.box->child_layout_axis = 1;
    div.box->elem_align = UI_ALIGN_CENTER;

    Player_Info_Panel(w, ui, player, font, snapshot, leave_pressed);

    w.Spacer(UI_Size_Pixels(margin));

    ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
    defer(ui.sizes.pop());

    {
        auto lib_sig = Library_UI(w, ui, player, library_texture, is_local);
        library_ui = lib_sig.box->id;
        player_library_rect = lib_sig.box->layout_box;
    }

    w.Spacer(UI_Size_Pixels(margin));

    Graveyard_UI(w, ui, player, is_local);

    w.Spacer(UI_Size_Pixels(margin));

    Exile_UI(w, ui, player, is_local);
}

static void Opp_Graveyard_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                             bool is_local) {
    UI_Signal sig = Yard_UI(w, ui, &player->graveyard, "Opp Graveyard");
    if (!sig.box)
        return;

    if (Widget_Data *widget = std::any_cast<Widget_Data>(&sig.box->userdata)) {
        widget->texture_flip = SDL_FLIP_VERTICAL;
    }

    if (is_local && player->graveyard.size() &&
        Move_Card(player, player->graveyard.back(), sig, graveyard_ui)) {
        player->graveyard.pop_back();
    }
}
static void Opp_Exile_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                         bool is_local) {
    UI_Signal sig = Yard_UI(w, ui, &player->exile, "Opp Exile");
    if (!sig.box)
        return;

    if (Widget_Data *widget = std::any_cast<Widget_Data>(&sig.box->userdata)) {
        widget->texture_flip = SDL_FLIP_VERTICAL;
    }

    if (is_local && player->exile.size() &&
        Move_Card(player, player->exile.back(), sig, exile_ui)) {
        player->exile.pop_back();
    }
}

static void Opp_Info_Panel(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                           TTF_Font *font) {
    ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Child()});
    UI_Signal div = w.Div_Begin();
    defer(w.Div_End());

    div.box->child_layout_axis = 1;
    div.box->elem_align = UI_ALIGN_CENTER;
    theme::Apply_Panel(div.box, theme::Panel());

    ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Text(2)});
    defer(ui.sizes.pop());
    const float m = 4.0f;
    ui.margins.push({m, m, m / 2, m / 2});
    defer(ui.margins.pop());

    auto name_style = theme::Label_Body(font);
    for (auto &s : name_style)
        s.text.color = theme::TEXT_INFO;
    w.styles.push(name_style);
    w.Label(player->username.empty() ? "Opponent" : player->username);
    w.styles.pop();

    w.styles.push(theme::Label_Title(font));
    w.Label(std::to_string(player->life_total) + " HP");
    w.styles.pop();

    w.styles.push(theme::Label_Body(font));
    w.Label(std::to_string(player->hand_count) + " cards");
    w.styles.pop();

    if (player->clock_remaining_ms > 0) {
        int secs = player->clock_remaining_ms / 1000;
        std::string clock = std::to_string(secs / 60) + ":" + (secs % 60 < 10 ? "0" : "") +
                            std::to_string(secs % 60);
        w.styles.push(theme::Label_Body(font));
        w.Label(clock);
        w.styles.pop();
    }
}

void Opp_Side_Zones_UI(Widget_Context &w, UI_Context &ui, Game::Player_State *player,
                       SDL_Texture *library_texture, TTF_Font *font, bool is_local) {
    const float margin = 10.0f;
    ui.sizes.push({UI_Size_Pixels(card_width + margin * 2), UI_Size_Parent(1.0f)});
    UI_Signal div = w.Div_Begin();
    defer(w.Div_End());

    div.box->child_layout_axis = 1;
    div.box->elem_align = UI_ALIGN_CENTER;

    Opp_Info_Panel(w, ui, player, font);

    w.Spacer(UI_Size_Pixels(margin * 2));

    ui.sizes.push({UI_Size_Pixels(card_width), UI_Size_Pixels(card_height)});
    defer(ui.sizes.pop());

    Opp_Exile_UI(w, ui, player, is_local);

    w.Spacer(UI_Size_Pixels(margin));

    Opp_Graveyard_UI(w, ui, player, is_local);

    w.Spacer(UI_Size_Pixels(margin));

    UI_Signal lib = Library_UI(w, ui, player, library_texture, is_local);
    opp_library_rect = lib.box->layout_box;
    if (Widget_Data *widget = std::any_cast<Widget_Data>(&lib.box->userdata)) {
        widget->texture_flip = (SDL_FlipMode)(SDL_FLIP_HORIZONTAL | SDL_FLIP_VERTICAL);
    }
}

void Drag_Overlay_UI(UI_Context &ui) {
    UI_Box *active = ui.Get_Box(ui.active);
    if (!active || ((~active->signal_last.flags) & UI_SIG_LEFT_DOWN) ||
        ((~active->flags) & UI_BOX_FLAG_DRAGGABLE))
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

    if (drop_rect.w <= 0 || drop_rect.h <= 0 || !std::isfinite(drop_rect.w) ||
        !std::isfinite(drop_rect.h) || drop_rect.w > 16384 || drop_rect.h > 16384) {
        return;
    }
    SDL_SetTextureAlphaMod(widget->texture, 0xFF * 0.7);
    float rot = (float)widget->texture_rotaton * 90;
    if (widget->texture_rotaton == Widget_Rotation::Rot_90 ||
        widget->texture_rotaton == Widget_Rotation::Rot_270) {
        SDL_FPoint c = {drop_rect.x + drop_rect.w / 2, drop_rect.y + drop_rect.h / 2};
        float tmp = drop_rect.w;
        drop_rect.w = drop_rect.h;
        drop_rect.h = tmp;

        drop_rect.x = c.x - drop_rect.w / 2;
        drop_rect.y = c.y - drop_rect.h / 2;
    }
    SDL_RenderTextureRotated(state.renderer, widget->texture, NULL, &drop_rect, rot, NULL,
                             SDL_FLIP_NONE);
    SDL_SetTextureAlphaMod(widget->texture, 0xFF);
}

static void Draw_Intention_Line(SDL_Renderer *renderer, float x1, float y1, float x2, float y2,
                                SDL_Color color, float time_s) {
    float dx = x2 - x1, dy = y2 - y1;
    float len = std::sqrt(dx * dx + dy * dy);
    if (len < 1.0f)
        return;
    float nx = dx / len, ny = dy / len;

    SDL_SetRenderDrawColor(renderer, color.r, color.g, color.b, color.a / 4);
    for (int offset = -2; offset <= 2; offset++) {
        SDL_RenderLine(renderer, x1 - ny * offset, y1 + nx * offset, x2 - ny * offset,
                       y2 + nx * offset);
    }

    const float dash_len = 10.0f, gap_len = 6.0f;
    float phase = std::fmod(time_s * 40.0f, dash_len + gap_len);
    float d = -phase;
    while (d < len) {
        float start = std::max(d, 0.0f);
        float end = std::min(d + dash_len, len);
        if (start < end) {
            float sx = x1 + nx * start, sy = y1 + ny * start;
            float ex = x1 + nx * end, ey = y1 + ny * end;
            SDL_SetRenderDrawColor(renderer, color.r, color.g, color.b, color.a);
            SDL_RenderLine(renderer, sx, sy, ex, ey);
            SDL_RenderLine(renderer, sx - ny, sy + nx, ex - ny, ey + nx);
        }
        d += dash_len + gap_len;
    }

    auto draw_dot = [&](float cx, float cy) {
        SDL_SetRenderDrawColor(renderer, color.r, color.g, color.b, color.a);
        SDL_FRect dot = {cx - 3, cy - 3, 6, 6};
        SDL_RenderFillRect(renderer, &dot);
    };
    draw_dot(x1, y1);
    draw_dot(x2, y2);
}

void Combat_Lines_UI(SDL_Renderer *renderer, Combat_UI_State *combat) {
    if (!combat)
        return;

    float time_s = (float)SDL_GetTicks() / 1000.0f;

    if (combat->attacker_prompt_active) {
        for (uint64_t atk_id : combat->selected_attackers) {
            auto it = combat->permanent_rects.find(atk_id);
            if (it == combat->permanent_rects.end())
                continue;
            const auto &r = it->second;
            float cx = r.x + r.w / 2, cy = r.y;
            Draw_Intention_Line(renderer, cx, cy, cx, cy - 60.0f, {0xFF, 0x40, 0x40, 0xCC}, time_s);
        }
    }

    for (const auto &[blocker_id, attacker_id] : combat->selected_blockers) {
        auto blocker_it = combat->permanent_rects.find(blocker_id);
        auto attacker_it = combat->permanent_rects.find(attacker_id);
        if (blocker_it == combat->permanent_rects.end() ||
            attacker_it == combat->permanent_rects.end())
            continue;

        const auto &br = blocker_it->second;
        const auto &ar = attacker_it->second;
        Draw_Intention_Line(renderer, br.x + br.w / 2, br.y + br.h / 2, ar.x + ar.w / 2,
                            ar.y + ar.h / 2, {0x40, 0x80, 0xFF, 0xCC}, time_s);
    }

    if (combat->blocker_prompt_active && combat->pending_blocker != 0) {
        auto it = combat->permanent_rects.find(combat->pending_blocker);
        if (it != combat->permanent_rects.end()) {
            const auto &r = it->second;
            float bx = r.x + r.w / 2, by = r.y;
            Draw_Intention_Line(renderer, bx, by, bx, by - 40.0f, {0xFF, 0xFF, 0x40, 0xCC}, time_s);
        }
    }
}

static void Stack_UI(Widget_Context &w, UI_Context &ui, const Game::Game_Snapshot &snapshot,
                     Combat_UI_State *combat = nullptr) {
    if (snapshot.stack.empty())
        return;

    const float stack_width = card_width + 20.0f;
    ui.sizes.push({UI_Size_Pixels(stack_width), UI_Size_Parent(1.0f)});
    UI_Signal div = w.Div_Begin({}, {}, "StackZone");
    defer(w.Div_End());

    div.box->child_layout_axis = 1;
    div.box->elem_align = {UI_ALIGN_CENTER, UI_ALIGN_CENTER};
    if (Widget_Data *wd = std::any_cast<Widget_Data>(&div.box->userdata)) {
        wd->flags = WIDGET_FLAG_DRAW_BORDER;
    }

    //const float entry_height = 40.0f;
    const float entry_height = card_height;
    ui.sizes.push({UI_Size_Pixels(stack_width - 4.0f), UI_Size_Pixels(entry_height)});
    defer(ui.sizes.pop());

    for (size_t i = 0; i < snapshot.stack.size(); i++) {
        const auto &entry = snapshot.stack[i];
        std::string label;
        if (entry.spell.has_value())
            label = entry.spell->name;
        else if (entry.ability_description.has_value())
            label = *entry.ability_description;
        else
            label = "???";

        std::string id = "Stack[" + std::to_string(i);
        UI_Signal sig = w.Label(label, {}, UI_BOX_FLAG_CLIP, id);
        if (Widget_Data *wd = std::any_cast<Widget_Data>(&sig.box->userdata)) {
            wd->flags = WIDGET_FLAG_DRAW_TEXT | WIDGET_FLAG_DRAW_BORDER;
            if (entry.spell.has_value())
                wd->texture = Game::card_textures.Get(entry.spell->name);
        }
        if (combat && entry.spell.has_value() && (sig.flags & UI_SIG_HOVERING))
            combat->hovered_card_id = entry.spell->instance_id;
    }
}

void Game_UI(Widget_Context &w, UI_Context &ui, const Game::Game_Snapshot &snapshot,
             SDL_Texture *library_texture, TTF_Font *font, uint64_t my_user_id, bool *leave_pressed,
             Combat_UI_State *combat, bool is_local) {
    if (snapshot.players.empty())
        return;

    int my_idx = 0;
    if (my_user_id != 0 && snapshot.players.size() >= 2) {
        for (int i = 0; i < static_cast<int>(snapshot.players.size()); i++) {
            if (snapshot.players[i].player_id == my_user_id) {
                my_idx = i;
                break;
            }
        }
    }
    int opp_idx = (my_idx == 0) ? 1 : 0;
    if (snapshot.players.size() < 2)
        opp_idx = my_idx;

    Game::Player_State p = snapshot.players[my_idx];
    Game::Player_State o = snapshot.players[opp_idx];

    Side_Zones_UI(w, ui, &p, library_texture, font, &snapshot, leave_pressed, is_local);

    ui.sizes.push({UI_Size_Fit(), UI_Size_Parent(1.0f)});
    DIV(&w) {
        // These have to be floating since the hands
        // go over the battlefeild

        UI_Box *div = ui.leafs.back();

        div->child_layout_axis = 1;
        ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Pixels(card_height)});
        DIV(&w) {
            UI_Box *h_div = ui.leafs.back();
            h_div->flags |= UI_BOX_FLAG_FLOATING;
            h_div->fixed_position = V2{0, 0};
            Opp_Hand_UI(w, ui, &o);
        }
        ui.sizes.pop();

        ui.sizes.push({UI_Size_Parent(1.0f), UI_Size_Pixels(card_height)});
        DIV_O(&w, {}, UI_BOX_FLAG_DROPPABLE) {
            UI_Box *h_div = ui.leafs.back();
            h_div->flags |= UI_BOX_FLAG_FLOATING;
            h_div->fixed_position = V2{0, div->layout_box.h - card_height};
            Hand_UI(w, ui, &p, is_local);
        }
        ui.sizes.pop();

        ui.sizes.push({UI_Size_Pixels(div->layout_box.w),
                       UI_Size_Pixels(div->layout_box.h - card_height * 2)});
        w.styles.push(theme::Button_Primary());
        DIV_O(&w, {}, UI_BOX_FLAG_DROPPABLE | UI_BOX_FLAG_CLIP) {
            UI_Box *b_div = ui.leafs.back();
            b_div->flags |= UI_BOX_FLAG_FLOATING;
            b_div->fixed_position = V2{0, card_height};
            Battlefield_UI(w, ui, &p, &o, combat, is_local);
        }
        w.styles.pop();
        ui.sizes.pop();
    }
    ui.sizes.pop();

    Stack_UI(w, ui, snapshot, combat);

    Opp_Side_Zones_UI(w, ui, &o, library_texture, font, is_local);
}
