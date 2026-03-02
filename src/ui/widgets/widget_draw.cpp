#include "widgets.hpp"

static void Widget_Draw_Label_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Button_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Toggle_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Slider_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Textbox_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Div_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);

Widget_Data::Widget_Data(Widget_Context *ctx, Widget_Type type, Widget_Union u) {
    this->style = (ctx->styles.size()) ? ctx->styles.top() : ctx->default_style;
    this->flags = (ctx->default_flags_override.size()) ? ctx->default_flags_override.top() : 0xFF;
    this->texture = NULL;
    this->type = type;
    this->u = u;
    this->draw_fn = Widget_Draw_Div_Impl;
    switch (type) {
        case Widget_Type::Label:
            this->draw_fn = Widget_Draw_Label_Impl;
            break;
        case Widget_Type::Button:
            this->draw_fn = Widget_Draw_Button_Impl;
            break;
        case Widget_Type::Toggle:
            this->draw_fn = Widget_Draw_Toggle_Impl;
            break;
        case Widget_Type::Slider:
            this->draw_fn = Widget_Draw_Slider_Impl;
            break;
        case Widget_Type::Textbox:
            this->draw_fn = Widget_Draw_Textbox_Impl;
            break;
        case Widget_Type::Div:
            this->draw_fn = Widget_Draw_Div_Impl;
            break;
    }
}

static void Draw_Rect(SDL_Renderer *renderer, Rect rect, float inner_thick) {
    SDL_FRect rect_arr[4];
    for (int i = 0; i < 4; i++)
        rect_arr[i] = rect.sdl();
    // top
    rect_arr[0].h = inner_thick;

    // bottom
    rect_arr[1].y += rect.h - inner_thick;
    rect_arr[1].h = inner_thick;

    // left
    rect_arr[2].w = inner_thick;

    // right
    rect_arr[3].x += rect.w - inner_thick;
    rect_arr[3].w = inner_thick;

    SDL_RenderFillRects(renderer, rect_arr, 4);
}
/*
static void Draw_Rect_Corners(SDL_Renderer *renderer, Rect rect, float
inner_thick, float len)
{
    float run = len/2.0f;
    SDL_FRect rect_arr[4];
    for (int i = 0; i<4; i++)
        rect_arr[i] = rect.sdl();
    //top
    rect_arr[0].h = inner_thick;
    rect_arr[0].x += rect.w - run;
    rect_arr[0].w = run;

    //bottom
    rect_arr[1].y += rect.h - inner_thick;
    rect_arr[1].h = inner_thick;
    rect_arr[1].w = run;

    //left
    rect_arr[2].w = inner_thick;
    rect_arr[2].y += rect.h - run;
    rect_arr[2].h = run;

    //right
    rect_arr[3].x += rect.w - inner_thick;
    rect_arr[3].w = inner_thick;
    rect_arr[3].h = run;

    SDL_RenderFillRects(renderer, rect_arr, 4);
}
*/

bool Widget_Draw_Text(TTF_Text *text, Rect area, G2<Alignment> alignment) {
    int x, y;
    TTF_GetTextSize(text, &x, &y);

    V2 pos = {area.x, area.y};
    V2 size = V2{(float)x, (float)y};

    for (int i = 0; i < 2; i++)
        switch (alignment[i]) {
            case UI_ALIGN_LEFT:
                break;
            case UI_ALIGN_RIGHT:
                pos[i] += area.size()[i] - size[i];
                break;
            case UI_ALIGN_CENTER:
                pos[i] += area.size()[i] / 2 - size[i] / 2;
                break;
        }

    TTF_DrawRendererText(text, pos.x, pos.y);
    return true;
}

static void Widget_Draw_Label_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data) {
    (void)ctx;
    (void)box;
    (void)data;
}

static void Widget_Draw_Button_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data) {
    (void)ctx;
    (void)box;
    (void)data;
}

static void Widget_Draw_Toggle_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data) {
    ctx->Toggle_Draw(box, data);
}

static void Widget_Draw_Slider_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data) {
    ctx->Slider_Draw(box, data);
}

static void Widget_Draw_Textbox_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data) {
    ctx->Textbox_Draw(box, data);
}

static void Widget_Draw_Div_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data) {
    (void)ctx;
    (void)box;
    (void)data;
}

void Widget_Context::Draw() {
    if (!renderer)
        return;

    for (UI_Box *box : ui->Render_It(renderer))
        this->Draw(box);
    return;
}

void Widget_Context::Draw(UI_Box *box) {
    Widget_Data *data = std::any_cast<Widget_Data>(&box->userdata);
    if (!data)
        return;

    Widget_Style_State style_state;
    if (box->signal_last.flags & UI_SIG_RELEASED)
        style_state = Widget_Style_State::Released;
    else if (box->signal_last.flags & UI_SIG_DOWN)
        style_state = Widget_Style_State::Pressed;
    else if (box->signal_last.flags & UI_SIG_HOVERING)
        style_state = Widget_Style_State::Hovering;
    else
        style_state = Widget_Style_State::Default;

    Widget_Style style = data->style[static_cast<size_t>(style_state)];

    if (data->flags & WIDGET_FLAG_DRAW_BACKGROUND) {
        SDL_Color c = style.background;
        SDL_SetRenderDrawColor(renderer, c.r, c.g, c.b, c.a);
        SDL_RenderFillRect(renderer, (SDL_FRect *)&box->area);
    }

    if (data->texture) {
        SDL_RenderFillRect(renderer, (SDL_FRect *)&box->area);
        SDL_RenderTexture(renderer, data->texture, NULL, (SDL_FRect *)&box->area);
    }

    if (data->flags & WIDGET_FLAG_DRAW_BORDER) {
        // SDL_SetRenderDrawColorFloat( renderer, 0.25f, 0.25f, 0.25f, 1.0f);
        SDL_Color c = style.border;
        SDL_SetRenderDrawColor(renderer, c.r, c.g, c.b, c.a);
        Draw_Rect(renderer, box->area, 2);
    }

    /*
    SDL_SetRenderDrawColor( renderer, 0xFF, 0, 0, 0xFF);
    Draw_Rect(renderer, box->area, 1);

    SDL_SetRenderDrawColor( renderer, 0xFF, 0, 0xFF, 0xFF);
    Draw_Rect(renderer, box->layout_box, 1);
    */

    if (data->draw_fn)
        data->draw_fn(this, box, data);

    if (data->flags & WIDGET_FLAG_DRAW_TEXT) {
        if (style.text.has_value()) {
            SDL_Color c = style.text.value();
            TTF_SetTextColor(box->label, c.r, c.g, c.b, c.a);
        }
        Widget_Draw_Text(box->label, box->area, box->label_alignment);
    }

    return;
}

void Widget_Context::Toggle_Draw(UI_Box *box, Widget_Data *data) {
    if (!renderer)
        return;

    const float margin = 0.2;
    SDL_FRect dst = box->area;
    dst.x += dst.w * margin;
    dst.y += dst.h * margin;
    dst.w *= 1 - margin * 2;
    dst.h *= 1 - margin * 2;
    if (data->u.toggle.toggle_state)
        SDL_SetRenderDrawColorFloat(renderer, 0.0f, 1.0f, 0.0f, 1.0f);
    else
        SDL_SetRenderDrawColorFloat(renderer, 0.0f, 0.0f, 0.0f, 1.0f);
    SDL_RenderFillRect(renderer, &dst);
}

void Widget_Context::Slider_Draw(UI_Box *box, Widget_Data *data) {
    SDL_SetRenderDrawColorFloat(renderer, 0.3f, 0.3f, 0.3f, 1.0f);
    SDL_RenderFillRect(renderer, (SDL_FRect *)&box->area);

    SDL_FRect dst = box->area;

    float v =
        (data->u.slider.value - data->u.slider.min) / (data->u.slider.max - data->u.slider.min);
    switch (data->u.slider.dir) {
        case Widget_Slider_Dir::LTR:
            dst.w *= v;
            break;
        case Widget_Slider_Dir::RTL:
            dst.x += dst.w * (1 - v);
            dst.w -= dst.w * (1 - v);
            break;
        case Widget_Slider_Dir::UTD:
            dst.h *= v;
            break;
        case Widget_Slider_Dir::DTU:
            dst.y += dst.h * (1 - v);
            dst.h -= dst.h * (1 - v);
            break;
    }
    SDL_SetRenderDrawColor(renderer, 0x00, 0x00, 0xFF, 0xFF);
    SDL_RenderFillRect(renderer, &dst);

    return;
}

void Widget_Context::Textbox_Draw(UI_Box *box, Widget_Data *data) {
    if (!renderer)
        return;
    SDL_SetRenderDrawColorFloat(renderer, 0.25f, 0.25f, 0.25f, 1.0f);
    SDL_RenderRect(renderer, (SDL_FRect *)&box->area);

    if (box->signal_last.flags & UI_SIG_FOCUSED) {
        int x, y;
        TTF_GetTextSize(box->label, &x, &y);
        float h = (float)TTF_GetFontAscent(TTF_GetTextFont(box->label));

        V2 size = V2{(float)x, (y) ? (float)y : h};
        V2 pos = box->area.pos();

        for (int i = 0; i < 2; i++)
            switch (box->label_alignment[i]) {
                case UI_ALIGN_LEFT:
                    break;
                case UI_ALIGN_RIGHT:
                    pos[i] += box->area.size()[i] - size[i];
                    break;
                case UI_ALIGN_CENTER:
                    pos[i] += box->area.size()[i] / 2 - size[i] / 2;
                    break;
            }

        TTF_SetTextWrapWhitespaceVisible(box->label, true);
        TTF_SubString cursor_substr = {};
        if (TTF_GetTextSubString(box->label, box->cursor, &cursor_substr)) {
            SDL_SetRenderDrawColor(renderer, 0xFF, 0xFF, 0xFF, 0xFF);
            SDL_FRect dst = {pos.x + (float)cursor_substr.rect.x,
                             pos.y + (float)cursor_substr.rect.y,
                             //(cursor_substr.rect.w) ? (float)cursor_substr.rect.w : 5.0f,
                             2.5f, h};
            SDL_RenderFillRect(renderer, &dst);
        }
    } else
        TTF_SetTextWrapWhitespaceVisible(box->label, false);

    // Widget_Draw_Text(box->label, box->area, box->label_alignment);

    return;
}
