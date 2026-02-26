#include "widgets.hpp"

static void Widget_Draw_Label_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Button_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Toggle_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Slider_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Textbox_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Div_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);

Widget_Context::Widget_Context(SDL_Renderer *renderer, UI_Context *context)
{
    this->renderer = renderer;
    this->ui = context;

    this->styles = {};

    this->default_style[WIDGET_STYLE_DEFAULT] = {
        .background = {0x4C, 0x4C, 0x4C, 0xFF},
        .border = {0x3F, 0x3F, 0x3F, 0xFF},
    };
    this->default_style[WIDGET_STYLE_HOVERING] = {
        .background = {0x7F, 0x7F, 0x7F, 0xFF},
        .border = {0x3F, 0x3F, 0x3F, 0xFF},
    };
    this->default_style[WIDGET_STYLE_PRESSED] = {
        .background = {0xCC, 0xCC, 0xCC, 0xFF},
        .border = {0x3F, 0x3F, 0x3F, 0xFF},
    };
    this->default_style[WIDGET_STYLE_RELEASED] = {
        .background = {0xFF, 0xFF, 0xFF, 0xFF},
        .border = {0x3F, 0x3F, 0x3F, 0xFF},
    };
}

Widget_Data::Widget_Data(Widget_Context *ctx, Widget_Type type, Widget_Union u)
{
    this->style = (ctx->styles.size())
                ? ctx->styles.top()
                : ctx->default_style;
    this->flags = (ctx->default_flags_override.size())
                ? ctx->default_flags_override.top()
                : 0xFF;
    this->texture = NULL;
    this->type = type;
    this->u = u;
    this->draw_fn = Widget_Draw_Div_Impl;
    switch (type)
    {
        case WIDGET_TYPE_LABEL:
            this->draw_fn = Widget_Draw_Label_Impl;
            break;
        case WIDGET_TYPE_BUTTON:
            this->draw_fn = Widget_Draw_Button_Impl;
            break;
        case WIDGET_TYPE_TOGGLE:
            this->draw_fn = Widget_Draw_Toggle_Impl;
            break;
        case WIDGET_TYPE_SLIDER:
            this->draw_fn = Widget_Draw_Slider_Impl;
            break;
        case WIDGET_TYPE_TEXTBOX:
            this->draw_fn = Widget_Draw_Textbox_Impl;
            break;
        case WIDGET_TYPE_DIV:
            this->draw_fn = Widget_Draw_Div_Impl;
            break;
    }
}
Widget_Data::~Widget_Data()
{
}

static void Draw_Rect(SDL_Renderer *renderer, Rect rect, float inner_thick)
{
    SDL_FRect rect_arr[4];
    for (int i = 0; i<4; i++)
        rect_arr[i] = rect.sdl();
    //top
    rect_arr[0].h = inner_thick;

    //bottom
    rect_arr[1].y += rect.h - inner_thick;
    rect_arr[1].h = inner_thick;

    //left
    rect_arr[2].w = inner_thick;

    //right
    rect_arr[3].x += rect.w - inner_thick;
    rect_arr[3].w = inner_thick;

    SDL_RenderFillRects(renderer, rect_arr, 4);
}
/*
static void Draw_Rect_Corners(SDL_Renderer *renderer, Rect rect, float inner_thick, float len)
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

UI_Signal Widget_Context::Spacer(std::optional<UI_Size> size, const std::source_location source_loc)
{
    V2 fixed_pos = {};
    UI_Box_Flags flags = 0;

    if (size.has_value())
    {
        G2<UI_Size> current_size = (this->ui->sizes.size()) ? this->ui->sizes.top() : G2<UI_Size>{};
        int layout_axis = (this->ui->parents.size()) ? this->ui->parents.top()->child_layout_axis : this->ui->root.child_layout_axis;
        current_size[layout_axis] = size.value();
        this->ui->sizes.push(current_size);
    }

    UI_Signal sig = ui->Box_Make( fixed_pos,
        flags,
        {},
        {},
        source_loc
    );

    if (size.has_value())
        this->ui->sizes.pop();

    /*
    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata))
        data->flags = 0x00;
    */

    return sig;
}
UI_Signal Widget_Context::Div_Begin(std::optional<Rect> area, const std::source_location source_loc)
{
    V2 fixed_pos = {};
    UI_Box_Flags flags = 0;
    if (area.has_value())
    {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make( fixed_pos,
        flags,
        {},
        {},
        source_loc
    );
    if (area.has_value()) ui->sizes.pop();

    /*
    sig.box->userdata = Widget_Data{
        .flags = (this->default_flags_override.size())
               ? this->default_flags_override.top()
               : 0xFF,
        .type = WIDGET_TYPE_DIV
    };
    */
    if (sig.box->frame_created == ui->frame)
        sig.box->userdata = Widget_Data(this, WIDGET_TYPE_DIV, Widget_Union{});

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata))
    {
        data->flags = 0x00;
    }

    ui->parents.push(sig.box);

    return sig;
}
void Widget_Context::Div_End()
{
    ui->parents.pop();
}

UI_Signal Widget_Context::Label(std::string label, std::optional<Rect> area, std::optional<std::string> id_override, const std::source_location source_loc)
{
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_CLIP;
    if (area.has_value())
    {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make( fixed_pos,
        flags,
        label,
        id_override,
        source_loc
    );
    if (area.has_value()) ui->sizes.pop();

    if (sig.box->frame_created == ui->frame)
        sig.box->userdata = Widget_Data( this, WIDGET_TYPE_LABEL, {} );
    return sig;
}

UI_Signal Widget_Context::Button(std::string label, std::optional<Rect> area, std::optional<std::string> id_override, const std::source_location source_loc)
{
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP;
    if (area.has_value())
    {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos,
        flags,
        label,
        id_override,
        source_loc
    );
    if (area.has_value()) ui->sizes.pop();

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata);
        data && data->type == WIDGET_TYPE_BUTTON)
    {
        data->style = (this->styles.size()) ? this->styles.top() : this->default_style;
        data->flags = (this->default_flags_override.size()) ? this->default_flags_override.top() : 0xFF;
    }
    else
    {
        sig.box->userdata = Widget_Data(this, WIDGET_TYPE_BUTTON, {});
    }

    return sig;
}

UI_Signal Widget_Context::Toggle(bool *toggle, std::string label, std::optional<Rect> area, std::optional<std::string> id_override, const std::source_location source_loc)
{
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP;
    if (area.has_value())
    {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos,
        flags,
        label,
        id_override,
        source_loc
    );
    if (area.has_value()) ui->sizes.pop();

    if (sig.flags & UI_SIG_RELEASED)
         *toggle = !*toggle;

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata);
        data && data->type == WIDGET_TYPE_TOGGLE)
    {
        data->style = (this->styles.size()) ? this->styles.top() : this->default_style;
        data->flags = (this->default_flags_override.size()) ? this->default_flags_override.top() : 0xFF;
        data->u.toggle.toggle_state = *toggle;
    }
    else
    {
        sig.box->userdata = Widget_Data(this, WIDGET_TYPE_TOGGLE, (Widget_Union)Widget_Toggle_Data{*toggle});
    }

    return sig;
}
UI_Signal Widget_Context::Slider(float *value, float min, float max, Widget_Slider_Dir dir, std::string label, std::optional<Rect> area, std::optional<std::string> id_override, const std::source_location source_loc)
{
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP;
    if (area.has_value())
    {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos,
        flags,
        label,
        id_override,
        source_loc
    );
    if (area.has_value()) ui->sizes.pop();

    if (sig.flags & UI_SIG_DOWN)
    {
        float v = 0.0f;
        switch (dir)
        {
            case WIDGET_SLIDER_LTR:
                v = (sig.mouse_pos.x - sig.box->area.x) / sig.box->area.w;
                break;
            case WIDGET_SLIDER_RTL:
                v = (sig.box->area.x + sig.box->area.w - sig.mouse_pos.x) / sig.box->area.w;
                break;
            case WIDGET_SLIDER_UTD:
                v = (sig.mouse_pos.y - sig.box->area.y) / sig.box->area.h;
                break;
            case WIDGET_SLIDER_DTU:
                v = (sig.box->area.y + sig.box->area.h - sig.mouse_pos.y) / sig.box->area.h;
                break;
        }
        v = SDL_clamp(v, 0.0f, 1.0f);
        *value = min + (max - min) * v;
    }

    //sig.box->userdata = Widget_Slider_Data{*value, dir, min, max};
    /*
    Widget_Data d = { .type = WIDGET_TYPE_SLIDER };
    d.u.slider = Widget_Slider_Data{*value, dir, min, max};
    sig.box->userdata = d;
    */
    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata);
        data && data->type == WIDGET_TYPE_SLIDER)
    {
        data->style = (this->styles.size()) ? this->styles.top() : this->default_style;
        data->flags = (this->default_flags_override.size()) ? this->default_flags_override.top() : 0xFF;
        data->u.slider = Widget_Slider_Data{*value, dir, min, max};
    }
    else
    {
        sig.box->userdata = Widget_Data(this, WIDGET_TYPE_SLIDER, Widget_Union{ .slider = {*value, dir, min, max} });
    }

    return sig;
}

UI_Signal Widget_Context::Textbox(std::string init_label, std::optional<Rect> area, std::optional<std::string> id_override, const std::source_location source_loc)
{
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_TEXTINPUT | UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP;
    if (area.has_value())
    {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos,
        flags,
        init_label,
        id_override,
        source_loc
    );
    if (area.has_value()) ui->sizes.pop();

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata);
        data && data->type == WIDGET_TYPE_TEXTBOX)
    {
        data->style = (this->styles.size()) ? this->styles.top() : this->default_style;
        data->flags = (this->default_flags_override.size()) ? this->default_flags_override.top() : 0xFF;
    }
    else
    {
        sig.box->userdata = Widget_Data(this, WIDGET_TYPE_TEXTBOX, {});
    }
    
    return sig;
}

UI_Signal Widget_Context::Card(SDL_Texture *texture, std::optional<Rect> area, std::optional<std::string> id_override, const std::source_location source_loc)
{
    UI_Signal sig = this->Button({}, area, id_override, source_loc);

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata))
    {
        data->flags = 0x00;
        data->texture = texture;
    }

    return sig;
}

void Widget_Context::Draw()
{
    if (!renderer) return;

    for (UI_Box *box : ui->Render_It( renderer ))
        this->Draw(box);
    return;
}

bool Widget_Draw_Text( TTF_Text *text, Rect area, G2<Alignment> alignment )
{
    int x, y;
    TTF_GetTextSize(text, &x, &y);

    V2 pos = {area.x, area.y};
    V2 size = V2{(float)x, (float)y};

    for (int i = 0; i < 2; i++) switch (alignment[i])
    {
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

static void Widget_Draw_Label_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data)
{
    (void)ctx;
    (void)box;
    (void)data;
}

static void Widget_Draw_Button_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data)
{
    (void)ctx;
    (void)box;
    (void)data;
}

static void Widget_Draw_Toggle_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data)
{
    ctx->Toggle_Draw(box, data);
}

static void Widget_Draw_Slider_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data)
{
    ctx->Slider_Draw(box, data);
}

static void Widget_Draw_Textbox_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data)
{
    ctx->Textbox_Draw(box, data);
}

static void Widget_Draw_Div_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data)
{
    (void)ctx;
    (void)box;
    (void)data;
}

void Widget_Context::Draw(UI_Box *box)
{
    Widget_Data *data = std::any_cast<Widget_Data>(&box->userdata);
    if (!data) return;


    Widget_Style_State style_state;
    if (box->signal_last.flags & UI_SIG_RELEASED)
        style_state = WIDGET_STYLE_RELEASED;
    else if (box->signal_last.flags & UI_SIG_DOWN)
        style_state = WIDGET_STYLE_PRESSED;
    else if (box->signal_last.flags & UI_SIG_HOVERING)
        style_state = WIDGET_STYLE_HOVERING;
    else
        style_state = WIDGET_STYLE_DEFAULT;

    Widget_Style style = data->style[style_state];

    if (data->flags & WIDGET_FLAG_DRAW_BACKGROUND)
    {

        SDL_Color c = style.background;
        SDL_SetRenderDrawColor( renderer, c.r, c.g, c.b, c.a);
        SDL_RenderFillRect( renderer, (SDL_FRect *)&box->area );
    }

    if (data->texture)
    {
        SDL_RenderFillRect( renderer, (SDL_FRect *)&box->area );
        SDL_RenderTexture( renderer, data->texture, NULL, (SDL_FRect *)&box->area );
    }

    if (data->flags & WIDGET_FLAG_DRAW_BORDER)
    {
        //SDL_SetRenderDrawColorFloat( renderer, 0.25f, 0.25f, 0.25f, 1.0f);
        SDL_Color c = style.border;
        SDL_SetRenderDrawColor( renderer, c.r, c.g, c.b, c.a);
        Draw_Rect(renderer, box->area, 5);
    }

    /*
    SDL_SetRenderDrawColor( renderer, 0xFF, 0, 0, 0xFF);
    Draw_Rect(renderer, box->area, 1);

    SDL_SetRenderDrawColor( renderer, 0xFF, 0, 0xFF, 0xFF);
    Draw_Rect(renderer, box->layout_box, 1);
    */

    if (data->draw_fn)
        data->draw_fn(this, box, data);

    if (data->flags & WIDGET_FLAG_DRAW_TEXT)
    {
        if (style.text.has_value())
        {
            SDL_Color c = style.text.value();
            TTF_SetTextColor(box->label, c.r, c.g, c.b, c.a);
        }
        Widget_Draw_Text(box->label, box->area, box->label_alignment);
    }

    return;
}

void Widget_Context::Toggle_Draw(UI_Box *box, Widget_Data *data )
{
    if (!renderer) return;

    const float margin = 0.2;
    SDL_FRect dst = box->area;
    dst.x += dst.w * margin;
    dst.y += dst.h * margin;
    dst.w *= 1-margin*2;
    dst.h *= 1-margin*2;
    if (data->u.toggle.toggle_state)
        SDL_SetRenderDrawColorFloat( renderer, 0.0f, 1.0f, 0.0f, 1.0f);
    else
        SDL_SetRenderDrawColorFloat( renderer, 0.0f, 0.0f, 0.0f, 1.0f);
    SDL_RenderFillRect( renderer, &dst );
}

void Widget_Context::Slider_Draw(UI_Box *box, Widget_Data *data )
{
    SDL_SetRenderDrawColorFloat( renderer, 0.3f, 0.3f, 0.3f, 1.0f);
    SDL_RenderFillRect( renderer, (SDL_FRect*)&box->area );

    SDL_FRect dst = box->area;

    float v = (data->u.slider.value - data->u.slider.min)/(data->u.slider.max - data->u.slider.min);
    switch (data->u.slider.dir)
    {
        case WIDGET_SLIDER_LTR:
            dst.w *= v;
            break;
        case WIDGET_SLIDER_RTL:
            dst.x += dst.w * (1-v);
            dst.w -= dst.w * (1-v);
            break;
        case WIDGET_SLIDER_UTD:
            dst.h *= v;
            break;
        case WIDGET_SLIDER_DTU:
            dst.y += dst.h * (1-v);
            dst.h -= dst.h * (1-v);
            break;
    }
    SDL_SetRenderDrawColor(renderer, 0x00, 0x00, 0xFF, 0xFF);
    SDL_RenderFillRect(renderer, &dst);

    return;
}

void Widget_Context::Textbox_Draw( UI_Box *box, Widget_Data *data )
{
    if (!renderer) return;
    SDL_SetRenderDrawColorFloat( renderer, 0.25f, 0.25f, 0.25f, 1.0f);
    SDL_RenderRect( renderer, (SDL_FRect*)&box->area );

    if (box->signal_last.flags & UI_SIG_FOCUSED)
    {

        int x, y;
        TTF_GetTextSize(box->label, &x, &y);
        float h = (float)TTF_GetFontAscent(TTF_GetTextFont(box->label));

        V2 size = V2{(float)x, (y) ? (float)y : h};
        V2 pos = box->area.pos();

        for (int i = 0; i < 2; i++) switch (box->label_alignment[i])
        {
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
        if (TTF_GetTextSubString(box->label, box->cursor, &cursor_substr))
        {
            SDL_SetRenderDrawColor(renderer, 0xFF, 0xFF, 0xFF, 0xFF);
            SDL_FRect dst = {
                pos.x + (float)cursor_substr.rect.x,
                pos.y + (float)cursor_substr.rect.y,
                //(cursor_substr.rect.w) ? (float)cursor_substr.rect.w : 5.0f,
                2.5f,
                h
            };
            SDL_RenderFillRect(renderer, &dst);
        }
    }
    else
        TTF_SetTextWrapWhitespaceVisible(box->label, false);

    //Widget_Draw_Text(box->label, box->area, box->label_alignment);

    return;
}
