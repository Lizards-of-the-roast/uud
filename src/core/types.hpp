/*
Random types
*/

#pragma once

#include <stdexcept>

#include <SDL3/SDL.h>
#include <SDL3_ttf/SDL_ttf.h>

/////////////////
// RING BUFFER //

template <typename T, const int N>
struct Ring_Buffer {
    T buffer[N];
    int ref[N];

    Ring_Buffer() {
        for (int i = 0; i < N; i += 1)
            ref[i] = i;
    }

    T &Push_Front(T value) {
        this->buffer[this->ref[0]] = value;
        this->ref[N - 1] = this->ref[0];
        for (int i = 0; i < N - 1; i++)
            this->ref[i] = this->ref[i + 1];

        return this->buffer[this->ref[N - 1]];
    }
    T &Push_Back(T value) {
        this->buffer[this->ref[N - 1]] = value;
        this->ref[0] = this->ref[N - 1];
        for (int i = N - 1; i > 0; i--)
            this->ref[i] = this->ref[i - 1];

        return this->buffer[this->ref[0]];
    }
    T &Get(const int i) {
        int idx = (i >= 0) ? i : (N + i);
        if (idx < N && idx >= 0)
            return this->buffer[this->ref[idx]];
        else
            throw std::out_of_range("OUT OF RANGE");
    }
    T &End(void) { return this->Get(N - 1); }
    const int Size(void) { return N; }
    T &operator[](const int i) { return this->Get(i); }
};

///////////////////
// vecs and recs //

template <typename T>
struct G2 {
    T x;
    T y;

    T Dot(G2<T> v) const { return this->x * v.x + this->y * v.y; }
    T Length(void) const { return SDL_sqrt((float)this->Dot(*this)); }

    void operator=(T s) {
        *this = {s, s};
        return;
    }
    T &operator[](int i) {
        if (i >= 0 && i < 2)
            return ((T *)this)[i];
        else
            throw std::out_of_range((i < 0) ? "index too small" : "index too big");
    }
    G2<T> operator+(G2<T> v) { return {this->x + v.x, this->y + v.y}; }
    G2<T> operator-(G2<T> v) { return {this->x - v.x, this->y - v.y}; }
    G2<T> operator*(G2<T> v) { return {this->x * v.x, this->y * v.y}; }
    G2<T> operator/(G2<T> v) { return {this->x / v.x, this->y / v.y}; }

    G2<T> operator+(T v) { return {this->x + v, this->y + v}; }
    G2<T> operator-(T v) { return {this->x - v, this->y - v}; }
    G2<T> operator*(T v) { return {this->x * v, this->y * v}; }
    G2<T> operator/(T v) { return {this->x / v, this->y / v}; }
};

struct V2 : G2<float> {
    void operator=(G2<float> v) {
        *this = V2{v.x, v.y};
        return;
    }
    void operator=(SDL_FPoint v) {
        *this = V2{v.x, v.y};
        return;
    }
    operator SDL_FPoint() { return *((SDL_FPoint *)this); }
    operator const SDL_FPoint() const { return *((SDL_FPoint *)this); }
};
struct Rect  //: SDL_FRect
{
    float x;
    float y;
    float w;
    float h;

    bool Collision(G2<float> point) const {
        return point.x >= this->x && point.y >= this->y && point.x <= this->x + this->w &&
               point.y <= this->y + this->h;
    }
    bool Collision(Rect other) const {
        return this->Collision(other.pos()) &&
               this->Collision(other.pos() + other.size() * V2{0, 1}) &&
               this->Collision(other.pos() + other.size() * V2{1, 0}) &&
               this->Collision(other.pos() + other.size() * V2{1, 1});
    }
    Rect Intersection(Rect other) const {
        Rect ret;
        ret.x = SDL_max(this->x, other.x);
        ret.y = SDL_max(this->y, other.y);
        ret.w = SDL_min(this->x + this->w, other.x + other.w) - ret.x;
        ret.h = SDL_min(this->y + this->h, other.y + other.h) - ret.y;
        return ret;
    }

    V2 pos() const { return {this->x, this->y}; }
    V2 size() const { return {this->w, this->h}; }

    SDL_Rect IRect_Round() const {
        return SDL_Rect{
            (int)SDL_roundf(this->x),
            (int)SDL_roundf(this->y),
            (int)SDL_roundf(this->w),
            (int)SDL_roundf(this->h),
        };
    }
    SDL_Rect IRect() const {
        return SDL_Rect{
            (int)this->x,
            (int)this->y,
            (int)this->w,
            (int)this->h,
        };
    }
    SDL_FRect sdl() const { return *((SDL_FRect *)this); }

    float &operator[](int i) const {
        if (i >= 0 && i < 4)
            return ((float *)this)[i];
        else
            throw std::out_of_range((i < 0) ? "index too small" : "index too big");
    }
    void operator=(SDL_FRect v) {
        this->x = v.x;
        this->y = v.y;
        this->w = v.w;
        this->h = v.h;
        return;
    }
    operator SDL_FRect() const { return this->sdl(); }
    operator SDL_Rect() const { return this->IRect(); }
};

//data driven way of interacting with TTF_Text
struct TTF_Text_Properties {
    std::optional<SDL_Color>        color; //TODO: varent FColor
    std::optional<TTF_Direction>    direction;
    std::optional<TTF_TextEngine *> engine;
    std::optional<TTF_Font *>       font;
    std::optional<Uint32>           script;
    std::optional<std::string>      string;
    std::optional<bool>             wrap_whitespace_visible;
    std::optional<int>              wrap_width;

    int Set(TTF_Text* text)
    {
        int n_opts = 0;
        if (this->color.has_value() && ++n_opts)
            TTF_SetTextColor(text,
                            this->color.value().r,
                            this->color.value().g,
                            this->color.value().b,
                            this->color.value().a
                            );
        if (this->direction.has_value() && ++n_opts)
            TTF_SetTextDirection(text, this->direction.value());
        if (this->engine.has_value() && ++n_opts)
            TTF_SetTextEngine(text, this->engine.value());
        if (this->font.has_value() && ++n_opts)
            TTF_SetTextFont(text, this->font.value());
        if (this->script.has_value() && ++n_opts)
            TTF_SetTextScript(text, this->script.value());
        if (this->string.has_value() && ++n_opts)
            TTF_SetTextString(text,
                            this->string.value().c_str(),
                            this->string.value().length()
                            );
        if (this->wrap_whitespace_visible.has_value() && ++n_opts)
            TTF_SetTextWrapWhitespaceVisible(text, this->wrap_whitespace_visible.value());
        if (this->wrap_width.has_value() && ++n_opts)
            TTF_SetTextWrapWidth(text, this->wrap_width.value());

        return n_opts;
    }

    TTF_Text_Properties &Get(TTF_Text* text)
    {
        if (!text) return *this;

        SDL_Color c;
        TTF_GetTextColor(text,
                            &c.r,
                            &c.g,
                            &c.b,
                            &c.a
                        );
        this->color = c;

        this->direction = TTF_GetTextDirection(text);
        this->engine = TTF_GetTextEngine(text);
        this->font = TTF_GetTextFont(text);
        this->script = TTF_GetTextScript(text);
        this->string = std::string(text->text);
        //getter doesnt exist lmao
        //this->wrap_whitespace_visible = TTF_GetTextWrapWhitespaceVisible(text);
        int w = 0;
        if (TTF_GetTextWrapWidth(text, &w))
            this->wrap_width = w;

        return *this;
    }
};

struct TTF_Font_Properties {
    //doesnt exist? (my version may be out of date)
    //std::optional<int>                     char_spacing;
    std::optional<TTF_Direction>           direction;
    std::optional<TTF_HintingFlags>        hinting;
    std::optional<bool>                    kerning;
    std::optional<std::string>             language;
    std::optional<int>                     line_skip;
    std::optional<int>                     outline;
    std::optional<Uint32>                  script;
    std::optional<bool>                    sdf;
    std::optional<float>                   size;  //TODO: varent SizeDPI
    std::optional<TTF_FontStyleFlags>      style;
    std::optional<TTF_HorizontalAlignment> wrap_alignment;

    int Set(TTF_Font* font)
    {
        int n_opts = 0;
        /* doesnt exist? (my version may be out of date)
        if (this->char_spacing.has_value() && ++n_opts)
            TTF_SetFontCharSpacing(font, this->char_spacing.value());
        */
        if (this->direction.has_value() && ++n_opts)
            TTF_SetFontDirection(font, this->direction.value());
        if (this->hinting.has_value() && ++n_opts)
            TTF_SetFontHinting(font, this->hinting.value());
        if (this->kerning.has_value() && ++n_opts)
            TTF_SetFontKerning(font, this->kerning.value());
        if (this->language.has_value() && ++n_opts)
            TTF_SetFontLanguage(font, this->language.value().c_str());
        if (this->line_skip.has_value() && ++n_opts)
            TTF_SetFontLineSkip(font, this->line_skip.value());
        if (this->outline.has_value() && ++n_opts)
            TTF_SetFontOutline(font, this->outline.value());
        if (this->script.has_value() && ++n_opts)
            TTF_SetFontScript(font, this->script.value());
        if (this->sdf.has_value() && ++n_opts)
            TTF_SetFontSDF(font, this->sdf.value());
        if (this->size.has_value() && ++n_opts)
            TTF_SetFontSize(font, this->size.value());
        if (this->style.has_value() && ++n_opts)
            TTF_SetFontStyle(font, this->style.value());
        if (this->wrap_alignment.has_value() && ++n_opts)
            TTF_SetFontWrapAlignment(font, this->wrap_alignment.value());

        return n_opts;
    }
    TTF_Font_Properties Get(TTF_Font* font)
    {
        if (!font) return *this;

        this->direction = TTF_GetFontDirection(font);
        this->hinting = TTF_GetFontHinting(font);
        this->kerning = TTF_GetFontKerning(font);
        //getter doesnt exist
        //this->language = TTF_GetFontLanguage(font);
        this->line_skip = TTF_GetFontLineSkip(font);
        this->outline = TTF_GetFontOutline(font);
        this->script = TTF_GetFontScript(font);
        this->sdf = TTF_GetFontSDF(font);
        this->size = TTF_GetFontSize(font);
        this->style = TTF_GetFontStyle(font);
        this->wrap_alignment = TTF_GetFontWrapAlignment(font);

        return *this;
    }
};
