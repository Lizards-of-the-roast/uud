#ifndef _WIDGETS_HPP_
#define _WIDGETS_HPP_

#include "simp_ui.hpp"
#include <array>

#define DIV_O(CTX, ...) for (int _i_ = ((CTX)->Div_Begin( __VA_ARGS__), 0); !_i_; _i_++, (CTX)->Div_End())
#define DIV(CTX) for (int _i_ = ((CTX)->Div_Begin(), 0); !_i_; _i_++, (CTX)->Div_End())

/*
TODO: maybe use a Widget_Base with virtual void draw(...)
*/


enum Widget_Type
{
    WIDGET_TYPE_DIV,
    WIDGET_TYPE_LABEL,
    WIDGET_TYPE_BUTTON,
    WIDGET_TYPE_TOGGLE,
    WIDGET_TYPE_SLIDER,
    WIDGET_TYPE_TEXTBOX,
};

//struct Widget_Label_Data {};
//struct Widget_Button_Data {};
struct Widget_Toggle_Data { bool toggle_state; };

enum Widget_Slider_Dir
{
    WIDGET_SLIDER_RTL,
    WIDGET_SLIDER_LTR,
    WIDGET_SLIDER_UTD,
    WIDGET_SLIDER_DTU,
};
struct Widget_Slider_Data
{
    float value;
    Widget_Slider_Dir dir;
    float min;
    float max;
};

enum
{
    WIDGET_FLAG_DRAW_BACKGROUND = 0x01 << 0,
    WIDGET_FLAG_DRAW_BORDER     = 0x01 << 1,
    WIDGET_FLAG_DRAW_TEXT       = 0x01 << 2,
};
typedef Uint16 Widget_Flags;

union Widget_Union
{
    Widget_Toggle_Data toggle;
    Widget_Slider_Data slider;
};

struct Widget_Style
{
    SDL_Color background;
    SDL_Color border;
    std::optional<SDL_Color> text;
};
enum Widget_Style_State
{
    WIDGET_STYLE_DEFAULT  = 0,
    WIDGET_STYLE_HOVERING = 1,
    WIDGET_STYLE_PRESSED  = 2,
    WIDGET_STYLE_RELEASED = 3,
    WIDGET_STYLE_COUNT    = 4,
};

struct Widget_Context;
struct Widget_Data
{
    std::array<Widget_Style, WIDGET_STYLE_COUNT> style;
    Widget_Flags flags;

    SDL_Texture *texture;

    Widget_Type type;
    Widget_Union u;

    Widget_Data(Widget_Context *ctx, Widget_Type type, Widget_Union u);
    ~Widget_Data();
};

struct Widget_Context
{
    //state
    SDL_Renderer *renderer;
    UI_Context *ui;

    //stack data
    std::stack<std::array<Widget_Style, WIDGET_STYLE_COUNT>> styles;
    std::array<Widget_Style, WIDGET_STYLE_COUNT> default_style;
    std::stack<Widget_Flags> defualt_flags_override;

    //UI_Elements

    UI_Signal Spacer(std::optional<UI_Size> size = {}, const std::source_location source_loc = std::source_location::current());

    UI_Signal Div_Begin(std::optional<Rect> area = {}, const std::source_location source_loc = std::source_location::current());
    void Div_End();

    UI_Signal Label(std::string label = "", std::optional<Rect> area = {}, std::optional<std::string> id_override = {}, const std::source_location source_loc = std::source_location::current());
    UI_Signal Button(std::string label = "", std::optional<Rect> area = {}, std::optional<std::string> id_override = {}, const std::source_location source_loc = std::source_location::current());
    UI_Signal Toggle(bool *toggle, std::string label = "", std::optional<Rect> area = {}, std::optional<std::string> id_override = {}, const std::source_location source_loc = std::source_location::current());
    UI_Signal Slider(float *value, float min = 0.0f, float max = 100.0f, Widget_Slider_Dir dir = WIDGET_SLIDER_LTR, std::string label = "", std::optional<Rect> area = {},  std::optional<std::string> id_override = {}, const std::source_location source_loc = std::source_location::current());
    UI_Signal Textbox(std::string init_label = "", std::optional<Rect> area = {}, std::optional<std::string> id_override = {}, const std::source_location source_loc = std::source_location::current());

    UI_Signal Card(SDL_Texture *texture, std::optional<Rect> area = {}, std::optional<std::string> id_override = {}, const std::source_location source_loc = std::source_location::current());

    Widget_Context(SDL_Renderer *renderer, UI_Context *context);

    //Renderering
    void Draw();
    void Draw(UI_Box *box);

    //void Label_Draw(UI_Box *box, Widget_Data *data );
    //void Button_Draw(UI_Box *box, Widget_Data *data );
    void Toggle_Draw(UI_Box *box, Widget_Data *data );
    void Slider_Draw(UI_Box *box, Widget_Data *data );
    void Textbox_Draw( UI_Box *box, Widget_Data *data );
};

#endif //ifndef _WIDGETS_HPP_
