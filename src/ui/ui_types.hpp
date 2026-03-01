#pragma once

#include "core/types.hpp"
#include <SDL3/SDL.h>
#include <SDL3_ttf/SDL_ttf.h>

typedef size_t UI_ID;

enum UI_Size_Type {
    UI_SIZE_PIXELS = 0x01 << 0,
    UI_SIZE_PERCENT_OF_PARENT = 0x01 << 1,
    UI_SIZE_FIT = 0x01 << 2,
    UI_SIZE_CHILD_SUM = 0x01 << 3,
    UI_SIZE_TEXT_CONTENT = 0x01 << 4,

    UI_SIZE_STANDALONE = UI_SIZE_PIXELS | UI_SIZE_TEXT_CONTENT,
    UI_SIZE_UPWARD_DEPENDENT = UI_SIZE_PERCENT_OF_PARENT | UI_SIZE_FIT,
    UI_SIZE_DOWNWARD_DEPENDENT = UI_SIZE_CHILD_SUM,
};
struct UI_Size {
    UI_Size_Type type;
    float value;
    float strictness;
};
/*
    Helper for passing semantic size

    ARGS:
        pixels - value of size
        strictness - how strict to treat this size in layout
                     defaults to 1.0f
    RETURNS:
        UI_Size - semantic size
*/
UI_Size UI_Size_Pixels(float pixels, float strictness = 1.0f);
/*
    Helper for passing semantic size

    ARGS:
        strictness - how strict to treat this size in layout
                     defaults to 1.0f
    RETURNS:
        UI_Size - semantic size
*/
UI_Size UI_Size_Fit(float strictness = 1.0f);
/*
    Helper for passing semantic size

    ARGS:
        percent - value of size
        strictness - how strict to treat this size in layout
                     defaults to 1.0f
    RETURNS:
        UI_Size - semantic size
*/
UI_Size UI_Size_Parent(float percent, float strictness = 1.0f);
/*
    Helper for passing semantic size

    ARGS:
        strictness - how strict to treat this size in layout
                     defaults to 1.0f
    RETURNS:
        UI_Size - semantic size
*/
UI_Size UI_Size_Child(float strictness = 1.0f);
/*
    Helper for passing semantic size

    ARGS:
        margin - value of size
        strictness - how strict to treat this size in layout
                     defaults to 1.0f
    RETURNS:
        UI_Size - semantic size
*/
UI_Size UI_Size_Text(float margins, float strictness = 1.0f);

enum Alignment {
    UI_ALIGN_LEFT,
    UI_ALIGN_CENTER,
    UI_ALIGN_RIGHT,

    UI_ALIGN_TOP = UI_ALIGN_LEFT,
    UI_ALIGN_BOTTOM = UI_ALIGN_RIGHT,
};

typedef Uint64 UI_Signal_Flags;
enum {
    UI_SIG_LEFT_PRESSED = (UI_Signal_Flags)0x01 << 0,
    UI_SIG_MIDDLE_PRESSED = (UI_Signal_Flags)0x01 << 1,
    UI_SIG_RIGHT_PRESSED = (UI_Signal_Flags)0x01 << 2,
    UI_SIG_X1_PRESSED = (UI_Signal_Flags)0x01 << 3,
    UI_SIG_X2_PRESSED = (UI_Signal_Flags)0x01 << 4,

    UI_SIG_LEFT_DOWN = (UI_Signal_Flags)0x01 << 5,
    UI_SIG_MIDDLE_DOWN = (UI_Signal_Flags)0x01 << 6,
    UI_SIG_RIGHT_DOWN = (UI_Signal_Flags)0x01 << 7,
    UI_SIG_X1_DOWN = (UI_Signal_Flags)0x01 << 8,
    UI_SIG_X2_DOWN = (UI_Signal_Flags)0x01 << 9,

    UI_SIG_LEFT_RELEASED = (UI_Signal_Flags)0x01 << 10,
    UI_SIG_MIDDLE_RELEASED = (UI_Signal_Flags)0x01 << 11,
    UI_SIG_RIGHT_RELEASED = (UI_Signal_Flags)0x01 << 12,
    UI_SIG_X1_RELEASED = (UI_Signal_Flags)0x01 << 13,
    UI_SIG_X2_RELEASED = (UI_Signal_Flags)0x01 << 14,

    UI_SIG_LEFT_CLICKED = (UI_Signal_Flags)0x01 << 15,
    UI_SIG_MIDDLE_CLICKED = (UI_Signal_Flags)0x01 << 16,
    UI_SIG_RIGHT_CLICKED = (UI_Signal_Flags)0x01 << 17,
    UI_SIG_X1_CLICKED = (UI_Signal_Flags)0x01 << 18,
    UI_SIG_X2_CLICKED = (UI_Signal_Flags)0x01 << 19,

    UI_SIG_LEFT_DOUBLE_CLICKED = (UI_Signal_Flags)0x01 << 20,
    UI_SIG_MIDDLE_DOUBLE_CLICKED = (UI_Signal_Flags)0x01 << 21,
    UI_SIG_RIGHT_DOUBLE_CLICKED = (UI_Signal_Flags)0x01 << 22,
    UI_SIG_X1_DOUBLE_CLICKED = (UI_Signal_Flags)0x01 << 23,
    UI_SIG_X2_DOUBLE_CLICKED = (UI_Signal_Flags)0x01 << 24,

    UI_SIG_LEFT_TRIPLE_CLICKED = (UI_Signal_Flags)0x01 << 25,
    UI_SIG_MIDDLE_TRIPLE_CLICKED = (UI_Signal_Flags)0x01 << 26,
    UI_SIG_RIGHT_TRIPLE_CLICKED = (UI_Signal_Flags)0x01 << 27,
    UI_SIG_X1_TRIPLE_CLICKED = (UI_Signal_Flags)0x01 << 28,
    UI_SIG_X2_TRIPLE_CLICKED = (UI_Signal_Flags)0x01 << 29,

    UI_SIG_HOVERING = (UI_Signal_Flags)0x01 << 30,
    UI_SIG_MOUSE_OVER = (UI_Signal_Flags)0x01 << 31,

    UI_SIG_FOCUSED = (UI_Signal_Flags)0x01 << 32,

    UI_SIG_PRESSED = UI_SIG_LEFT_PRESSED | UI_SIG_MIDDLE_PRESSED | UI_SIG_RIGHT_PRESSED |
                     UI_SIG_X1_PRESSED | UI_SIG_X2_PRESSED,
    UI_SIG_DOWN =
        UI_SIG_LEFT_DOWN | UI_SIG_MIDDLE_DOWN | UI_SIG_RIGHT_DOWN | UI_SIG_X1_DOWN | UI_SIG_X2_DOWN,
    UI_SIG_RELEASED = UI_SIG_LEFT_RELEASED | UI_SIG_MIDDLE_RELEASED | UI_SIG_RIGHT_RELEASED |
                      UI_SIG_X1_RELEASED | UI_SIG_X2_RELEASED,
    UI_SIG_CLICKED = UI_SIG_LEFT_CLICKED | UI_SIG_MIDDLE_CLICKED | UI_SIG_RIGHT_CLICKED |
                     UI_SIG_X1_CLICKED | UI_SIG_X2_CLICKED,
    UI_SIG_DOUBLE_CLICKED = UI_SIG_LEFT_DOUBLE_CLICKED | UI_SIG_MIDDLE_DOUBLE_CLICKED |
                            UI_SIG_RIGHT_DOUBLE_CLICKED | UI_SIG_X1_DOUBLE_CLICKED |
                            UI_SIG_X2_DOUBLE_CLICKED,
    UI_SIG_TRIPLE_CLICKED = UI_SIG_LEFT_TRIPLE_CLICKED | UI_SIG_MIDDLE_TRIPLE_CLICKED |
                            UI_SIG_RIGHT_TRIPLE_CLICKED | UI_SIG_X1_TRIPLE_CLICKED |
                            UI_SIG_X2_TRIPLE_CLICKED,
};

struct UI_Box;

struct UI_Signal {
    UI_Box *box;
    V2 mouse_pos;
    V2 mouse_delta;
    UI_Signal_Flags flags;
};

typedef Uint16 UI_Box_Flags;
enum {
    // Interaction
    UI_BOX_FLAG_CLICKABLE = 0x01 << 0,
    UI_BOX_FLAG_TEXTINPUT = 0x01 << 1,
    UI_BOX_FLAG_FOCUS_HOT = 0x01 << 2,
    UI_BOX_FLAG_FOCUS_ACTIVE = 0x01 << 3,
    UI_BOX_FLAG_VIEW_SCROLL_X = 0x01 << 4,
    UI_BOX_FLAG_VIEW_SCROLL_Y = 0x01 << 5,

    // Layout
    UI_BOX_FLAG_FLOATING_X = 0x01 << 6,  // Fixed X, layout at most positions relative to parent
    UI_BOX_FLAG_FLOATING_Y = 0x01 << 7,
    UI_BOX_FLAG_SKIP_VIEW_OFFSET_X = 0x01 << 8,
    UI_BOX_FLAG_SKIP_VIEW_OFFSET_Y = 0x01 << 9,

    // Rendering
    UI_BOX_FLAG_CLIP = 0x01 << 10,
    UI_BOX_FLAG_HOT_ANIMATION = 0x01 << 11,
    UI_BOX_FLAG_ACTIVE_ANIMATION = 0x01 << 12,

    UI_BOX_FLAG_VIEW_SCROLL = UI_BOX_FLAG_VIEW_SCROLL_X | UI_BOX_FLAG_VIEW_SCROLL_Y,
    UI_BOX_FLAG_FLOATING = UI_BOX_FLAG_FLOATING_X | UI_BOX_FLAG_FLOATING_Y,
    UI_BOX_FLAG_SKIP_VIEW_OFFSET = UI_BOX_FLAG_SKIP_VIEW_OFFSET_X | UI_BOX_FLAG_SKIP_VIEW_OFFSET_Y,

};

struct UI_Margin {
    float left, right, top, bottom;
};

struct UI_Mouse_State {
    UI_ID box;
    Uint64 frame;
    V2 pos;
    SDL_MouseButtonFlags buttons;
    Uint64 timestamp;
};
