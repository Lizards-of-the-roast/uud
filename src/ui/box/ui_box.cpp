#include "core/defer.hpp"
#include "ui/context/ui_context.hpp"
#include <SDL3/SDL_timer.h>

UI_Box::~UI_Box() {
    if (this->label)
        TTF_DestroyText(this->label);
    if (this->id == 0)
        return;
    // SDL_Log("Box %lu killed", this->id);
}

UI_Size UI_Size_Pixels(float pixels, float strictness) {
    return UI_Size{
        .type = UI_SIZE_PIXELS,
        .value = pixels,
        .strictness = strictness,
    };
}
UI_Size UI_Size_Parent(float percent, float strictness) {
    return UI_Size{
        .type = UI_SIZE_PERCENT_OF_PARENT,
        .value = percent,
        .strictness = strictness,
    };
}
UI_Size UI_Size_Fit(float strictness) {
    return UI_Size{
        .type = UI_SIZE_FIT,
        .value = 0,
        .strictness = strictness,
    };
}
UI_Size UI_Size_Child(float strictness) {
    return UI_Size{
        .type = UI_SIZE_CHILD_SUM,
        .value = 0,
        .strictness = strictness,
    };
}
UI_Size UI_Size_Text(float margins, float strictness) {
    return UI_Size{
        .type = UI_SIZE_TEXT_CONTENT,
        .value = margins,
        .strictness = strictness,
    };
}

std::tuple<UI_Box *, int> UI_Box::Neighbor_Next() {
    int n = 0;

    if (this->first_child != NULL)
        return {this->first_child, -1};

    for (UI_Box *p = this; p != NULL; p = p->parent) {
        if (p->next_sibling != NULL)
            return {p->next_sibling, n};
        n += 1;
    }

    return {NULL, 0};
}
std::tuple<UI_Box *, int> UI_Box::Neighbor_Prev() {
    int n = 0;

    if (this->last_child != NULL)
        return {this->last_child, -1};

    for (UI_Box *p = this; p != NULL; p = p->parent) {
        if (p->prev_sibling != NULL)
            return {p->prev_sibling, n};
        n += 1;
    }

    return {NULL, 0};
}

void UI_Box::Text_Insert(UI_Context *ctx, std::string text) {
    TTF_Font *font = NULL;
    if (this->label && this->label->text == NULL) {
        font = TTF_GetTextFont(this->label);
        TTF_DestroyText(this->label);
        this->label = NULL;
    }

    if (!this->label)
        this->label = TTF_CreateText(ctx->text_engine, font, text.c_str(), text.length());
    else
        TTF_InsertTextString(this->label, this->cursor, text.c_str(), text.length());
    this->cursor += text.length();
}
void UI_Box::Text_Delete(void) {
    if (this->cursor < 0)
        return;

    if (this->cursor)
        this->cursor--;
    TTF_DeleteTextString(this->label, this->cursor, 1);
}
void UI_Box::Text_Cursor_Left(void) {
    this->cursor = SDL_clamp((ssize_t)this->cursor - 1, 0, SDL_strlen(this->label->text));
}
void UI_Box::Text_Cursor_Right(void) {
    this->cursor = SDL_clamp(this->cursor + 1, 0, SDL_strlen(this->label->text));
}

bool UI_Context::Box_Iterate_Next(UI_Box **box) {
    if (*box == &this->root)
        return false;
    *box = std::get<0>((*box)->Neighbor_Next());
    return box != NULL;
}
bool UI_Context::Box_Iterate_Prev(UI_Box **box) {
    if (*box == &this->root)
        return false;
    *box = std::get<0>((*box)->Neighbor_Prev());
    return box != NULL;
}

static const int SDL_Button_Flag_To_Sig_Flag(SDL_MouseButtonFlags buttons,
                                             UI_Signal_Flags first_sig) {
    UI_Signal_Flags result = 0;
    for (int i = 0; i < 4; i++) {
        if (buttons & (0x01 << i))
            result |= first_sig << i;
    }
    return result;
}

static bool UI_Is_Multi_Click(const UI_Mouse_State &newer, const UI_Mouse_State &older,
                              Uint64 max_gap_ms, float max_dist_px) {
    if (!newer.timestamp || !older.timestamp)
        return false;
    if (newer.timestamp < older.timestamp)
        return false;
    if (newer.timestamp - older.timestamp > max_gap_ms)
        return false;

    if (newer.box != older.box)
        return false;
    if (newer.buttons != older.buttons)
        return false;

    V2 d = V2{newer.pos.x - older.pos.x, newer.pos.y - older.pos.y};

    if (SDL_fabsf(d.x) > max_dist_px)
        return false;
    if (SDL_fabsf(d.y) > max_dist_px)
        return false;

    return true;
}

UI_Signal UI_Box::Signal(UI_Context *ctx) {
    if (this == &ctx->root)
        return {};

    UI_Signal sig = {0};
    defer(this->signal_last = sig);

    sig.box = this;
    sig.mouse_pos = ctx->mouse_pos;

    if (this->is_disabled) {
        if (ctx->active == this->id)
            ctx->active = 0;
        if (ctx->hot == this->id)
            ctx->hot = 0;
        if (ctx->focused == this->id)
            ctx->focused = 0;
        return sig;
    }

    bool is_mouse_over = this->area.Collision(ctx->mouse_pos);

    if (this->flags & UI_BOX_FLAG_CLICKABLE) {
        if (ctx->mouse_up_buttons && ctx->active == this->id) {
            ctx->active = 0;
            sig.flags |= SDL_Button_Flag_To_Sig_Flag(ctx->mouse_up_buttons, UI_SIG_LEFT_RELEASED);

            if (is_mouse_over) {
                sig.flags |=
                    SDL_Button_Flag_To_Sig_Flag(ctx->mouse_up_buttons, UI_SIG_LEFT_CLICKED);
                if (this->flags & UI_BOX_FLAG_TEXTINPUT && ctx->focused != this->id) {
                    ctx->focused = this->id;
                    SDL_StartTextInput(ctx->window);
                    SDL_Log("Began Text Input");
                }
            } else if (ctx->hot)
                ctx->hot = 0;
        }
        if (ctx->mouse_down_buttons && is_mouse_over &&
            ctx->mouse_history[-1].frame != ctx->frame) {
            ctx->hot = this->id;
            ctx->active = this->id;

            const Uint64 multi_click_window_ms = 250;
            const float multi_click_slop_px = 6.0f;

            sig.flags |= SDL_Button_Flag_To_Sig_Flag(ctx->mouse_down_buttons, UI_SIG_LEFT_PRESSED);

            UI_Mouse_State mouse_state = {
                .box = this->id,
                .frame = ctx->frame,
                .pos = ctx->mouse_pos,
                .buttons = ctx->mouse_down_buttons,
                .timestamp = SDL_GetTicks(),
            };

            const UI_Mouse_State prev = ctx->mouse_history[-1];
            const UI_Mouse_State prev2 = ctx->mouse_history[-2];

            bool is_double_click =
                UI_Is_Multi_Click(mouse_state, prev, multi_click_window_ms, multi_click_slop_px);
            bool is_triple_click =
                is_double_click &&
                UI_Is_Multi_Click(prev, prev2, multi_click_window_ms, multi_click_slop_px);

            if (is_double_click)
                sig.flags |= SDL_Button_Flag_To_Sig_Flag(ctx->mouse_down_buttons,
                                                         UI_SIG_LEFT_DOUBLE_CLICKED);
            if (is_triple_click)
                sig.flags |= SDL_Button_Flag_To_Sig_Flag(ctx->mouse_down_buttons,
                                                         UI_SIG_LEFT_TRIPLE_CLICKED);

            ctx->mouse_history.Push_Front(mouse_state);
        }

        if (ctx->mouse_history[-1].box == this->id && ctx->mouse_history[-2].frame != ctx->frame &&
            ctx->active == this->id) {
            sig.mouse_delta = ctx->mouse_history[-1].pos - ctx->mouse_pos;
            sig.flags |=
                SDL_Button_Flag_To_Sig_Flag(ctx->mouse_history[-1].buttons, UI_SIG_LEFT_DOWN);
        }
    }

    /*
    if (box->flags & UI_BOX_FLAG_VIEW_SCROLL && is_mouse_over &&
    (ctx->mouse_wheel.x != 0 || ctx->mouse_wheel.y != 0) )
    {
        SDL_FPoint max_view_of = {
            SDL_max(0, this->view_bounds.x - this->area.w ),
            SDL_max(0, this->view_bounds.y - this->area.h ),
        };
        SDL_FPoint mask = {
            !!(this->flags & UI_BOX_FLAG_VIEW_SCROLL_X),
            !!(this->flags & UI_BOX_FLAG_VIEW_SCROLL_Y),
        };
        float step = (this->scroll_step) ? this->scroll_step : 1.0f;
        this->view_offset.x += SDL_clamp(ctx->mouse_wheel.x * mask.x * step, 0,
    max_view_off.x); this->view_offset.y += SDL_clamp(ctx->mouse_wheel.y * mask.y
    * step, 0, max_view_off.y);
    }
    */

    if (is_mouse_over) {
        sig.flags |= UI_SIG_MOUSE_OVER;

        UI_Box *hot = ctx->Get_Box(ctx->hot);
        bool has_hot = (hot != NULL);

        if (this->flags & UI_BOX_FLAG_CLICKABLE &&
            (!has_hot || hot->frame_last_touched != ctx->frame || ctx->hot == this->id) &&
            (ctx->active == 0 || ctx->active == this->id)) {
            sig.flags |= UI_SIG_HOVERING;
            ctx->hot = this->id;
        }

    } else if (ctx->hot == this->id)
        ctx->hot = 0;

    if (ctx->focused == this->id)
        sig.flags |= UI_SIG_FOCUSED;
    return sig;
}
