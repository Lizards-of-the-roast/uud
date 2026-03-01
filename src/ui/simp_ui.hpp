#ifndef _SIMP_UI_
#define _SIMP_UI_

#include <SDL3/SDL.h>
#include <SDL3_ttf/SDL_ttf.h>
#include <string>
#include <optional>
#include <vector>
#include <stack>
#include <unordered_map>
#include <source_location>
#include <tuple>
#include <any>

#include "types.hpp"

typedef size_t UI_ID;

struct UI_Signal;
struct UI_Box;
struct UI_Context;
struct UI_Size;

enum UI_Size_Type
{
    UI_SIZE_PIXELS            = 0x01 << 0,
    UI_SIZE_PERCENT_OF_PARENT = 0x01 << 1,
    UI_SIZE_FIT               = 0x01 << 2,
    UI_SIZE_CHILD_SUM         = 0x01 << 3,
    UI_SIZE_TEXT_CONTENT      = 0x01 << 4,

    UI_SIZE_STANDALONE         = UI_SIZE_PIXELS | UI_SIZE_TEXT_CONTENT,
    UI_SIZE_UPWARD_DEPENDENT   = UI_SIZE_PERCENT_OF_PARENT | UI_SIZE_FIT,
    UI_SIZE_DOWNWARD_DEPENDENT = UI_SIZE_CHILD_SUM,
};
struct UI_Size
{
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

enum Alignment
{
    UI_ALIGN_LEFT,
    UI_ALIGN_CENTER,
    UI_ALIGN_RIGHT,

    UI_ALIGN_TOP = UI_ALIGN_LEFT,
    UI_ALIGN_BOTTOM = UI_ALIGN_RIGHT,
};

typedef Uint64 UI_Signal_Flags;
enum
{
    UI_SIG_LEFT_PRESSED           = (UI_Signal_Flags)0x01 << 0 ,
    UI_SIG_MIDDLE_PRESSED         = (UI_Signal_Flags)0x01 << 1 ,
    UI_SIG_RIGHT_PRESSED          = (UI_Signal_Flags)0x01 << 2 ,
    UI_SIG_X1_PRESSED             = (UI_Signal_Flags)0x01 << 3 ,
    UI_SIG_X2_PRESSED             = (UI_Signal_Flags)0x01 << 4 ,

    UI_SIG_LEFT_DOWN              = (UI_Signal_Flags)0x01 << 5 ,
    UI_SIG_MIDDLE_DOWN            = (UI_Signal_Flags)0x01 << 6 ,
    UI_SIG_RIGHT_DOWN             = (UI_Signal_Flags)0x01 << 7 ,
    UI_SIG_X1_DOWN                = (UI_Signal_Flags)0x01 << 8 ,
    UI_SIG_X2_DOWN                = (UI_Signal_Flags)0x01 << 9 ,

    UI_SIG_LEFT_RELEASED          = (UI_Signal_Flags)0x01 << 10,
    UI_SIG_MIDDLE_RELEASED        = (UI_Signal_Flags)0x01 << 11,
    UI_SIG_RIGHT_RELEASED         = (UI_Signal_Flags)0x01 << 12,
    UI_SIG_X1_RELEASED            = (UI_Signal_Flags)0x01 << 13,
    UI_SIG_X2_RELEASED            = (UI_Signal_Flags)0x01 << 14,

    UI_SIG_LEFT_CLICKED           = (UI_Signal_Flags)0x01 << 15,
    UI_SIG_MIDDLE_CLICKED         = (UI_Signal_Flags)0x01 << 16,
    UI_SIG_RIGHT_CLICKED          = (UI_Signal_Flags)0x01 << 17,
    UI_SIG_X1_CLICKED             = (UI_Signal_Flags)0x01 << 18,
    UI_SIG_X2_CLICKED             = (UI_Signal_Flags)0x01 << 19,

    UI_SIG_LEFT_DOUBLE_CLICKED    = (UI_Signal_Flags)0x01 << 20,
    UI_SIG_MIDDLE_DOUBLE_CLICKED  = (UI_Signal_Flags)0x01 << 21,
    UI_SIG_RIGHT_DOUBLE_CLICKED   = (UI_Signal_Flags)0x01 << 22,
    UI_SIG_X1_DOUBLE_CLICKED      = (UI_Signal_Flags)0x01 << 23,
    UI_SIG_X2_DOUBLE_CLICKED      = (UI_Signal_Flags)0x01 << 24,

    UI_SIG_LEFT_TRIPLE_CLICKED   = (UI_Signal_Flags)0x01 << 25,
    UI_SIG_MIDDLE_TRIPLE_CLICKED = (UI_Signal_Flags)0x01 << 26,
    UI_SIG_RIGHT_TRIPLE_CLICKED  = (UI_Signal_Flags)0x01 << 27,
    UI_SIG_X1_TRIPLE_CLICKED     = (UI_Signal_Flags)0x01 << 28,
    UI_SIG_X2_TRIPLE_CLICKED     = (UI_Signal_Flags)0x01 << 29,

    UI_SIG_HOVERING               = (UI_Signal_Flags)0x01 << 30,
    UI_SIG_MOUSE_OVER             = (UI_Signal_Flags)0x01 << 31,

    UI_SIG_FOCUSED                = (UI_Signal_Flags)0x01 << 32,

    UI_SIG_PRESSED         = UI_SIG_LEFT_PRESSED         | UI_SIG_MIDDLE_PRESSED         | UI_SIG_RIGHT_PRESSED         | UI_SIG_X1_PRESSED         | UI_SIG_X2_PRESSED,
    UI_SIG_DOWN            = UI_SIG_LEFT_DOWN            | UI_SIG_MIDDLE_DOWN            | UI_SIG_RIGHT_DOWN            | UI_SIG_X1_DOWN            | UI_SIG_X2_DOWN,
    UI_SIG_RELEASED        = UI_SIG_LEFT_RELEASED        | UI_SIG_MIDDLE_RELEASED        | UI_SIG_RIGHT_RELEASED        | UI_SIG_X1_RELEASED        | UI_SIG_X2_RELEASED,
    UI_SIG_CLICKED         = UI_SIG_LEFT_CLICKED         | UI_SIG_MIDDLE_CLICKED         | UI_SIG_RIGHT_CLICKED         | UI_SIG_X1_CLICKED         | UI_SIG_X2_CLICKED,
    UI_SIG_DOUBLE_CLICKED  = UI_SIG_LEFT_DOUBLE_CLICKED  | UI_SIG_MIDDLE_DOUBLE_CLICKED  | UI_SIG_RIGHT_DOUBLE_CLICKED  | UI_SIG_X1_DOUBLE_CLICKED  | UI_SIG_X2_DOUBLE_CLICKED,
    UI_SIG_TRIPLE_CLICKED = UI_SIG_LEFT_TRIPLE_CLICKED | UI_SIG_MIDDLE_TRIPLE_CLICKED | UI_SIG_RIGHT_TRIPLE_CLICKED | UI_SIG_X1_TRIPLE_CLICKED | UI_SIG_X2_TRIPLE_CLICKED,
};

struct UI_Signal
{
    UI_Box *box;
    V2 mouse_pos;
    V2 mouse_delta;
    UI_Signal_Flags flags;
};

typedef Uint16 UI_Box_Flags;
enum
{
    // Interaction
    UI_BOX_FLAG_CLICKABLE          = 0x01 << 0,
    UI_BOX_FLAG_TEXTINPUT          = 0x01 << 1,
    UI_BOX_FLAG_FOCUS_HOT          = 0x01 << 2,
    UI_BOX_FLAG_FOCUS_ACTIVE       = 0x01 << 3,
    UI_BOX_FLAG_VIEW_SCROLL_X      = 0x01 << 4,
    UI_BOX_FLAG_VIEW_SCROLL_Y      = 0x01 << 5,

    // Layout
    UI_BOX_FLAG_FLOATING_X         = 0x01 << 6, // Fixed X, layout at most positions relative to parent
    UI_BOX_FLAG_FLOATING_Y         = 0x01 << 7,
    UI_BOX_FLAG_SKIP_VIEW_OFFSET_X = 0x01 << 8,
    UI_BOX_FLAG_SKIP_VIEW_OFFSET_Y = 0x01 << 9,

    // Rendering
    UI_BOX_FLAG_CLIP               = 0x01 << 10,
    UI_BOX_FLAG_HOT_ANIMATION      = 0x01 << 11,
    UI_BOX_FLAG_ACTIVE_ANIMATION   = 0x01 << 12,

    UI_BOX_FLAG_VIEW_SCROLL      = UI_BOX_FLAG_VIEW_SCROLL_X      | UI_BOX_FLAG_VIEW_SCROLL_Y,
    UI_BOX_FLAG_FLOATING         = UI_BOX_FLAG_FLOATING_X         | UI_BOX_FLAG_FLOATING_Y,
    UI_BOX_FLAG_SKIP_VIEW_OFFSET = UI_BOX_FLAG_SKIP_VIEW_OFFSET_X | UI_BOX_FLAG_SKIP_VIEW_OFFSET_Y,

};

struct UI_Margin {
    float left, right,
          top, bottom;
};
struct UI_Box
{
    // n-ary tree ui hierarchy
    struct UI_Box *parent, *next_sibling, *prev_sibling,*first_child, *last_child;
    size_t child_count;

    UI_Box *next_iteration, *prev_iteration;
    size_t iteration;
    size_t source_key;

    UI_ID id;

    UI_Box_Flags flags;

    /*
    basically fixed_pos but stable in algorithm
    */
    Rect layout_box;
    //the actual rect of the box
    Rect area;

    G2<UI_Size> size;
    UI_Margin margin;
    V2 offset;
    V2 min_size;
    int child_layout_axis; 

    bool has_clip_ancestor;
    Rect clip_ancestor_rect;

    V2 fixed_position; //for internal use
    V2 fixed_size; //for internal use
    V2 view_offset;
    V2 view_bounds;
    V2 position_delta; //change in position between frames
    G2<Alignment> elem_align;


    //std::string label;
    TTF_Text *label;
    size_t cursor;
    G2<Alignment> label_alignment;

    Uint64 frame_created;
    Uint64 frame_last_touched;

    bool do_disable; //set this one
    bool is_disabled; //look at this one

    UI_Signal signal_last;

    ~UI_Box();

    /*
    size_t userdata_type_hash;
    void *userdata;
    */
    std::any userdata;

    /*
        Search for next neighbor in the tree
        neighbor searches first for child, then sibling, then parent's sibling.
        Siblings are searched next, children are searched first.

        RETURNS:
            UI_Box * - neighbor or NULL
            int      - n, vertical displacement in the tree
                        +n means it moved up n times -1 means it moved down
    */
    std::tuple< UI_Box *, int > Neighbor_Next();
    /*
        Search for next neighbor in the tree
        neighbor searches first for child, then sibling, then parent's sibling.
        Siblings are searched prev, children are searched last.

        RETURNS:
            UI_Box * - neighbor or NULL
            int      - n, vertical displacement in the tree
                        +n means it moved up n times -1 means it moved down
    */
    std::tuple< UI_Box *, int > Neighbor_Prev();

    /*
        Get a UI_Signal from box, containing all interaction with UI element

        ARGS:
            ctx - UI_Context that owns this box
        RETURNS:
            UI_Signal - signal
    */
    UI_Signal Signal(UI_Context *ctx);

    void Text_Insert(UI_Context *ctx, std::string text);
    void Text_Delete(void);
    void Text_Cursor_Left(void);
    void Text_Cursor_Right(void);
};

struct UI_Mouse_State
{
    UI_ID box;
    Uint64 frame;
    V2 pos;
    SDL_MouseButtonFlags buttons;
    Uint64 timestamp;
};

struct UI_Context
{
    std::unordered_map<UI_ID, UI_Box *> boxes;

    std::unordered_map<size_t, size_t> source_iteration_counter;
    std::unordered_map<size_t, UI_Box *> last_iteration_box;

    UI_ID hot;
    UI_ID active;
    UI_ID focused; //for text

    UI_Box root;
    // UI_Box pointers are stable (wont move),
    // downside: memory isnt 'packed' (cache misses)
    // upside: simple, and since this is a tree no memory gaps
    std::vector<UI_Box *> leafs;

    std::vector<UI_Box *> box_pool;
    std::vector<UI_Box *> free_boxes;

    std::stack<UI_Box *> parents;
    std::stack<G2<Alignment>> label_alignments;
    std::stack<G2<UI_Size>> sizes;
    std::stack<UI_Margin> margins;
    std::stack<TTF_Font *> fonts;

    Uint64 frame;

    //UI_Mouse_State mouse_history[3];
    Ring_Buffer<UI_Mouse_State, 3> mouse_history;

    V2 mouse_pos;
    // state just for this frame
    V2 mouse_delta;
    SDL_MouseButtonFlags mouse_up_buttons;
    SDL_MouseButtonFlags mouse_down_buttons;
    V2 mouse_wheel;

    // render data
    TTF_TextEngine *text_engine;
    std::stack<SDL_Rect> clip_stack;

    SDL_Window *window;
    float window_width, window_height;

    UI_Context(SDL_Window *window, TTF_TextEngine *text_engine);
    ~UI_Context();

    struct Render_It_Range
    {
        UI_Context *ctx;
        SDL_Renderer *renderer;
        struct Iterator
        {
            UI_Box *box;

            UI_Context *ctx;
            SDL_Renderer *renderer;

            std::stack<int> pop_stack;
            int level;
            int n;

            UI_Box *operator*();
            Iterator& operator++();
            bool operator!=(const Iterator& other);
        };

        ~Render_It_Range();

        Iterator begin();
        Iterator end();
    };
    /*
        Iterator for rendering, returns boxes in created order.
        i.e reverse order to painters algo. (unfortunately).
        for (UI_Box *box : ui_context.Render_It_Range())

        ARGS:
            renderer - SDL_Renderer to use for setting clips
        RETURNS:
            Render_It_Range - container class for iterator
    */
    Render_It_Range Render_It( SDL_Renderer *renderer );

    struct Layout_It_Range
    {
        UI_Context *ctx;
        struct Iterator
        {
            UI_Box *box;

            UI_Context *ctx;

            std::stack<int> pop_stack;
            int level;
            int n;

            UI_Box *operator*();
            Iterator& operator++();
            bool operator!=(const Iterator& other);
        };

        ~Layout_It_Range();

        Iterator begin();
        Iterator end();
    };
    Layout_It_Range Layout_It(void);

    UI_Box *Get_Box(UI_ID id);
    UI_Box *Alloc_Box();
    void Free_Box(UI_Box *box);

    /*
        Input events from SDL to allow the ui_context to get state

        ARGS:
            event - sdl_event from SDL_PollEvents
    */
    void Pass_Event(SDL_Event event);

    /*
        Create a UI_Box in this UI_Context

        ARGS:
            (optional) fixed_pos - fixed position of box, (use floating)
            (optional) flags - box flags
            (optional) label - text label
            (optional) id_override - id override rather then source loc and iteration
            (optional) source_loc - location of top level call used for id
        RETURNS:
            signal - signal as returned by box.Signal()
    */
    UI_Signal Box_Make(
        V2 fixed_pos = {},
        UI_Box_Flags flags = 0,
        std::string label = "",
        std::optional<std::string> id_override = {},
        const std::source_location source_loc = std::source_location::current()
    );
    UI_Signal Box_Make(
        Rect area,
        UI_Box_Flags flags = 0,
        std::string label = "",
        std::optional<std::string> id_override = {},
        const std::source_location source_loc = std::source_location::current()
    );
    //void Box_Delete(UI_ID box);


    // Box userdata alloc functions

    // Iteration functions
    bool Box_Iterate_Next( UI_Box **box );
    bool Box_Iterate_Prev( UI_Box **box );

    /*
        Begin UI environment
    */
    void Begin();
    /*
        End UI environment
    */
    void End();

    void Layout_Calc_Standalone(void);
    void Layout_Calc_Upwards_Dependent(void);
    void Layout_Calc_Downwards_Dependant(void);
    void Layout_Solve_Violation(void);
    void Layout_Comp_Relative(void);
    void Layout_Compute(void);
};

#endif //ifndef _SIMP_UI_
