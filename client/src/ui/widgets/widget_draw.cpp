#include "widgets.hpp"

#include "game/instances.hpp"

// Widgets are meant to be implementation code
// not lib code, the folder organization is a bit off rn
#include "core/state.hpp"

#include <algorithm>
#include <cmath>
#include <string>

static constexpr uint32_t COLOR_WHITE = 1 << 0;
static constexpr uint32_t COLOR_BLUE = 1 << 1;
static constexpr uint32_t COLOR_BLACK = 1 << 2;
static constexpr uint32_t COLOR_RED = 1 << 3;
static constexpr uint32_t COLOR_GREEN = 1 << 4;

static void Widget_Draw_Label_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Button_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Toggle_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Slider_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Textbox_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Div_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);
static void Widget_Draw_Card_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data);

Widget_Data::Widget_Data(Widget_Context *ctx, Widget_Type type, Widget_Union u) {
    this->style = (ctx->styles.size()) ? ctx->styles.top() : ctx->default_style;
    this->flags = (ctx->default_flags_override.size()) ? ctx->default_flags_override.top() : 0xFF;
    this->texture = NULL;
    this->texture_rotaton = Widget_Rotation::Rot_0;
    this->texture_flip = SDL_FLIP_NONE;
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
        case Widget_Type::Card:
            this->draw_fn = Widget_Draw_Card_Impl;
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
static void Widget_Draw_Card_Impl(Widget_Context *ctx, UI_Box *box, Widget_Data *data) {
    ctx->Card_Draw(box, data);
}

Widget_Style Widget_Context::Get_Style(UI_Box *box, Widget_Data *data) {
    if (!box || !data)
        return {};

    Widget_Style_State style_state;
    if (box->signal_last.flags & UI_SIG_RELEASED)
        style_state = Widget_Style_State::Released;
    else if (box->signal_last.flags & UI_SIG_DOWN)
        style_state = Widget_Style_State::Pressed;
    else if (box->signal_last.flags & UI_SIG_HOVERING)
        style_state = Widget_Style_State::Hovering;
    else
        style_state = Widget_Style_State::Default;

    return data->style[static_cast<size_t>(style_state)];
}

void Widget_Context::Draw() {
    if (!renderer)
        return;

    for (UI_Box *box : ui->Render_It(renderer))
        this->Draw(box);
    ui->Debug_Render_Overlay(renderer);
    return;
}

void Widget_Context::Draw(UI_Box *box) {
    Widget_Data *data = std::any_cast<Widget_Data>(&box->userdata);
    if (!data)
        return;

    Widget_Style style = this->Get_Style(box, data);

    if (data->flags & WIDGET_FLAG_DRAW_BACKGROUND) {
        SDL_Color c = style.background;
        SDL_SetRenderDrawColor(renderer, c.r, c.g, c.b, c.a);
        SDL_RenderFillRect(renderer, (SDL_FRect *)&box->area);
    }

    if (data->texture) {
        SDL_FRect dst = box->area.sdl();
        if (dst.w <= 0 || dst.h <= 0 || !std::isfinite(dst.w) || !std::isfinite(dst.h)
            || dst.w > 16384 || dst.h > 16384) {
            SDL_Log("Draw: skipping bad texture rect w=%.1f h=%.1f", dst.w, dst.h);
        } else {
            float rot = (float)data->texture_rotaton * 90.0f;
            if (data->texture_rotaton == Widget_Rotation::Rot_90 || data->texture_rotaton == Widget_Rotation::Rot_270)
            {
                SDL_FPoint c = {dst.x + dst.w/2, dst.y + dst.h/2};
                float tmp = dst.w;
                dst.w = dst.h;
                dst.h = tmp;

                dst.x = c.x - dst.w/2;
                dst.y = c.y - dst.h/2;
            }
            SDL_RenderTextureRotated(renderer, data->texture, NULL, &dst, rot, NULL, data->texture_flip);
        }
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
        if (box->font)
            style.text.font = {};
        style.text.Set(box->label);

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

    float v =
        (data->u.slider.value - data->u.slider.min) / (data->u.slider.max - data->u.slider.min);

    SDL_FRect dst = box->area;
    switch (data->u.slider.style)
    {
        case Widget_Slider_Style::PROGRESS:
        {
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
            break;
        }
        case Widget_Slider_Style::SCROLL:
        {
            float scroll_size = (data->u.slider.scroll_size) ? data->u.slider.scroll_size : 10;
            switch (data->u.slider.dir) {
                case Widget_Slider_Dir::LTR:
                    dst.w = scroll_size;
                    dst.x += box->area.w * v - dst.w * v;
                    break;
                case Widget_Slider_Dir::RTL:
                    dst.w = scroll_size;
                    dst.x += box->area.w * (1 - v) - dst.w * v;
                    break;
                case Widget_Slider_Dir::UTD:
                    dst.h = scroll_size;
                    dst.y += box->area.h * v - dst.h * v;
                    break;
                case Widget_Slider_Dir::DTU:
                    dst.h = scroll_size;
                    dst.y += box->area.h * (1 - v) - dst.h * v;
                    break;
            }
            SDL_SetRenderDrawColor(renderer, 0x67, 0x67, 0x67, 0xFF);
            SDL_RenderFillRect(renderer, &dst);
            SDL_SetRenderDrawColor(renderer, 0xCE, 0xCE, 0xCE, 0xFF);
            SDL_RenderRect(renderer, &dst);
        }
    }

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

static int Pop_Count(uint32_t c) {
    int n = 0;
    if (c & COLOR_WHITE)
        n++;
    if (c & COLOR_BLUE)
        n++;
    if (c & COLOR_BLACK)
        n++;
    if (c & COLOR_RED)
        n++;
    if (c & COLOR_GREEN)
        n++;
    return n;
}

static SDL_Color Card_Frame_Color(const Game::Card &card) {
    if (card.type == Game::Card_Type::Land)
        return {0x8B, 0x6C, 0x3E, 0xFF};  // brown
    int n = Pop_Count(card.colors);
    if (n == 0)
        return {0x71, 0x71, 0x7A, 0xFF};  // zinc/colorless
    if (n > 1)
        return {0xD9, 0x7F, 0x06, 0xFF};  // gold/multicolor
    if (card.colors & COLOR_WHITE)
        return {0xF5, 0xF0, 0xD1, 0xFF};  // amber-light
    if (card.colors & COLOR_BLUE)
        return {0x25, 0x63, 0xEB, 0xFF};  // blue
    if (card.colors & COLOR_BLACK)
        return {0x27, 0x27, 0x2A, 0xFF};  // zinc-dark
    if (card.colors & COLOR_RED)
        return {0xDC, 0x26, 0x26, 0xFF};  // red
    if (card.colors & COLOR_GREEN)
        return {0x16, 0xA3, 0x4A, 0xFF};  // green
    return {0x71, 0x71, 0x7A, 0xFF};
}

static SDL_Color Keyword_Color(const std::string &kw) {
    if (kw == "flying")
        return {0x25, 0x63, 0xEB, 0xB0};
    if (kw == "trample")
        return {0x16, 0xA3, 0x4A, 0xB0};
    if (kw == "deathtouch")
        return {0x7C, 0x3A, 0xED, 0xB0};
    if (kw == "lifelink")
        return {0xD9, 0x7F, 0x06, 0xB0};
    if (kw == "first strike" || kw == "double strike")
        return {0xDC, 0x26, 0x26, 0xB0};
    if (kw == "vigilance")
        return {0xD9, 0x7F, 0x06, 0xB0};
    if (kw == "reach")
        return {0x16, 0xA3, 0x4A, 0xB0};
    if (kw == "hexproof")
        return {0x25, 0x63, 0xEB, 0xB0};
    if (kw == "indestructible")
        return {0xD9, 0x7F, 0x06, 0xB0};
    if (kw == "menace")
        return {0xDC, 0x26, 0x26, 0xB0};
    if (kw == "flash")
        return {0xCA, 0x8A, 0x04, 0xB0};
    if (kw == "defender")
        return {0x52, 0x52, 0x5B, 0xB0};
    if (kw == "haste")
        return {0xDC, 0x26, 0x26, 0xB0};
    return {0x52, 0x52, 0x5B, 0xB0};
}

static void Draw_Filled_Rect(SDL_Renderer *r, float x, float y, float w, float h, SDL_Color c) {
    SDL_SetRenderDrawColor(r, c.r, c.g, c.b, c.a);
    SDL_FRect rect = {x, y, w, h};
    SDL_RenderFillRect(r, &rect);
}

static void Draw_Text_Simple(SDL_Renderer *r, TTF_Font *font, const std::string &text, float x,
                             float y, SDL_Color color, float max_width = 0) {
    if (!font || text.empty())
        return;
    SDL_Surface *surface = TTF_RenderText_Blended(font, text.c_str(), text.size(), color);
    if (!surface)
        return;
    SDL_Texture *tex = SDL_CreateTextureFromSurface(r, surface);
    if (tex) {
        float tw = (float)surface->w;
        float th = (float)surface->h;
        // Scale down to fit within max_width
        if (max_width > 0 && tw > max_width) {
            float scale = max_width / tw;
            tw = max_width;
            th *= scale;
        }
        SDL_FRect dst = {x, y, tw, th};
        SDL_RenderTexture(r, tex, nullptr, &dst);
        SDL_DestroyTexture(tex);
    }
    SDL_DestroySurface(surface);
}

void Widget_Context::Card_Draw(UI_Box *box, Widget_Data *data) {
    if (!renderer || !box || box->area.w < 20 || box->area.h < 20)
        return;

    Game::Card card;
    if (const Game::Card *c = Game::instances.Find(data->u.card.card))
        card = *c;
    else
        return;
    const Game::Permanent_State *perm = Game::instances.Find(data->u.card.perm);
    //XXX: hardcoded
    TTF_Font *font = state.font[paths::beleren_bold];

    const float border = 3.0f;
    SDL_Color frame = Card_Frame_Color(card);
    SDL_SetRenderDrawColor(renderer, frame.r, frame.g, frame.b, frame.a);
    SDL_FRect top = {box->area.x, box->area.y, box->area.w, border};
    SDL_RenderFillRect(renderer, &top);
    SDL_FRect bot = {box->area.x, box->area.y + box->area.h - border, box->area.w, border};
    SDL_RenderFillRect(renderer, &bot);
    SDL_FRect left = {box->area.x, box->area.y, border, box->area.h};
    SDL_RenderFillRect(renderer, &left);
    SDL_FRect right = {box->area.x + box->area.w - border, box->area.y, border, box->area.h};
    SDL_RenderFillRect(renderer, &right);

    if (font && !card.name.empty()) {
        // Dark background strip for name
        float name_h = std::min(box->area.h * 0.12f, 16.0f);
        Draw_Filled_Rect(renderer, box->area.x + 2, box->area.y + 2, box->area.w - 4, name_h,
                         {0x00, 0x00, 0x00, 0xC0});
        Draw_Text_Simple(renderer, font, card.name, box->area.x + 4, box->area.y + 2,
                         {0xFF, 0xFF, 0xFF, 0xFF}, box->area.w - 8);
    }

    if (perm && perm->is_token) {
        float tag_w = std::min(box->area.w * 0.35f, 36.0f);
        float tag_h = std::min(box->area.h * 0.08f, 10.0f);
        Draw_Filled_Rect(renderer, box->area.x + box->area.w - tag_w - 2, box->area.y + 2, tag_w, tag_h,
                         {0xD9, 0x7F, 0x06, 0xC0});
        if (font)
            Draw_Text_Simple(renderer, font, "TOKEN", box->area.x + box->area.w - tag_w, box->area.y + 2,
                             {0xFF, 0xFF, 0xFF, 0xFF});
    }

    if (perm && perm->summoning_sick) {
        SDL_SetRenderDrawColor(renderer, 0x00, 0x00, 0x00, 0x30);
        for (float d = -box->area.h; d < box->area.w + box->area.h; d += 6.0f) {
            float x1 = box->area.x + d;
            float y1 = box->area.y;
            float x2 = box->area.x + d - box->area.h;
            float y2 = box->area.y + box->area.h;
            x1 = std::clamp(x1, box->area.x, box->area.x + box->area.w);
            x2 = std::clamp(x2, box->area.x, box->area.x + box->area.w);
            SDL_RenderLine(renderer, x1, y1, x2, y2);
        }
    }

    if (card.type == Game::Card_Type::Creature && card.creature_stats.has_value()) {
        int base_pow = card.creature_stats->power;
        int base_tou = card.creature_stats->toughness;
        int eff_pow = base_pow + (perm ? perm->power_modifier : 0);
        int eff_tou = base_tou + (perm ? perm->toughness_modifier : 0);

        // Color: green if boosted, red if reduced, white otherwise.
        SDL_Color pt_color = {0xFF, 0xFF, 0xFF, 0xFF};
        if (perm && (eff_pow > base_pow || eff_tou > base_tou))
            pt_color = {0x40, 0xFF, 0x40, 0xFF};
        else if (perm && (eff_pow < base_pow || eff_tou < base_tou))
            pt_color = {0xFF, 0x40, 0x40, 0xFF};

        std::string pt = std::to_string(eff_pow) + "/" + std::to_string(eff_tou);
        float pt_w = std::min(box->area.w * 0.35f, 36.0f);
        float pt_h = std::min(box->area.h * 0.1f, 14.0f);
        float pt_x = box->area.x + box->area.w - pt_w - 3;
        float pt_y = box->area.y + box->area.h - pt_h - 3;
        Draw_Filled_Rect(renderer, pt_x, pt_y, pt_w, pt_h, {0x00, 0x00, 0x00, 0xB0});
        if (font)
            Draw_Text_Simple(renderer, font, pt, pt_x + 2, pt_y, pt_color, pt_w - 4);
    }

    if (perm && perm->damage_marked > 0) {
        std::string dmg = "-" + std::to_string(perm->damage_marked);
        float badge_w = std::min(box->area.w * 0.25f, 28.0f);
        float badge_h = std::min(box->area.h * 0.1f, 14.0f);
        float badge_x = box->area.x + 3;
        float badge_y = box->area.y + box->area.h - badge_h - 3;
        Draw_Filled_Rect(renderer, badge_x, badge_y, badge_w, badge_h, {0xB9, 0x1C, 0x1C, 0xE0});
        if (font)
            Draw_Text_Simple(renderer, font, dmg, badge_x + 2, badge_y, {0xFF, 0xCC, 0xCC, 0xFF});
    }

    if (perm && !perm->counters.empty()) {
        float cx = box->area.x + box->area.w - 3;
        float cy = box->area.y + box->area.h * 0.65f;
        for (const auto &counter : perm->counters) {
            if (counter.count == 0)
                continue;
            SDL_Color bg = {0x16, 0xA3, 0x4A, 0xD0};  // green default
            if (counter.type.find("-1/-1") != std::string::npos)
                bg = {0xB9, 0x1C, 0x1C, 0xD0};
            else if (counter.type.find("loyalty") != std::string::npos)
                bg = {0x25, 0x63, 0xEB, 0xD0};
            else if (counter.type.find("charge") != std::string::npos)
                bg = {0x7C, 0x3A, 0xED, 0xD0};

            std::string label = std::to_string(counter.count);
            float bw = 20.0f, bh = 12.0f;
            cx -= bw + 2;
            Draw_Filled_Rect(renderer, cx, cy, bw, bh, bg);
            if (font)
                Draw_Text_Simple(renderer, font, label, cx + 2, cy, {0xFF, 0xFF, 0xFF, 0xFF});
        }
    }

    std::vector<std::string> all_kw = card.keywords;
    if (perm) {
        for (const auto &kw : perm->granted_keywords) {
            if (std::find(all_kw.begin(), all_kw.end(), kw) == all_kw.end())
                all_kw.push_back(kw);
        }
    }
    if (!all_kw.empty() && box->area.h > 60) {
        float kw_x = box->area.x + 3;
        float kw_y = box->area.y + box->area.h * 0.5f;
        float kw_h = std::min(box->area.h * 0.08f, 11.0f);
        for (const auto &kw : all_kw) {
            if (kw_x > box->area.x + box->area.w - 10)
                break;  // out of space
            SDL_Color bg = Keyword_Color(kw);
            // Abbreviate
            std::string label = kw;
            if (label.size() > 5)
                label = label.substr(0, 4);
            float kw_w = std::max(label.size() * 6.0f + 4.0f, 16.0f);
            Draw_Filled_Rect(renderer, kw_x, kw_y, kw_w, kw_h, bg);
            if (font)
                Draw_Text_Simple(renderer, font, label, kw_x + 2, kw_y, {0xFF, 0xFF, 0xFF, 0xFF});
            kw_x += kw_w + 2;
        }
    }
}
