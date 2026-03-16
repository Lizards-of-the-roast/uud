#pragma once

#include <optional>
#include <source_location>
#include <stack>
#include <string>
#include <unordered_map>
#include <vector>

#include "ui/box/ui_box.hpp"

struct UI_Context {
    std::unordered_map<UI_ID, UI_Box *> boxes;

    std::unordered_map<size_t, size_t> source_iteration_counter;
    std::unordered_map<size_t, UI_Box *> last_iteration_box;

    UI_ID hot;
    UI_ID active;
    UI_ID focused;  // for text
    UI_ID drop_site;

    UI_Box root;
    // UI_Box pointers are stable (wont move),
    // downside: memory isnt 'packed' (cache misses)
    // upside: simple, and since this is a tree no memory gaps
    std::vector<UI_Box *> leafs;

    std::vector<UI_Box *> box_pool;
    std::vector<UI_Box *> free_boxes;

    std::stack<UI_Box *> parents;
    std::stack<size_t> id_stack;
    std::stack<G2<Alignment>> label_alignments;
    std::stack<G2<UI_Size>> sizes;
    std::stack<UI_Margin> margins;
    // std::stack<TTF_Font *> fonts;

    Uint64 frame;

    // UI_Mouse_State mouse_history[3];
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

    bool debug_overlay_enabled;
    bool debug_dump_enabled;
    bool debug_dump_once_requested;
    bool debug_show_root;
    std::string debug_dump_dir;

    UI_Context(SDL_Window *window, TTF_TextEngine *text_engine);
    ~UI_Context();

    struct Render_It_Range {
        UI_Context *ctx;
        SDL_Renderer *renderer;
        struct Iterator {
            UI_Box *box;

            UI_Context *ctx;
            SDL_Renderer *renderer;

            std::stack<int> pop_stack;
            int level;
            int n;

            UI_Box *operator*();
            Iterator &operator++();
            bool operator!=(const Iterator &other);
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
    Render_It_Range Render_It(SDL_Renderer *renderer);

    struct Layout_It_Range {
        UI_Context *ctx;
        struct Iterator {
            UI_Box *box;

            UI_Context *ctx;

            std::stack<int> pop_stack;
            int level;
            int n;

            UI_Box *operator*();
            Iterator &operator++();
            bool operator!=(const Iterator &other);
        };

        ~Layout_It_Range();

        Iterator begin();
        Iterator end();
    };
    Layout_It_Range Layout_It(void);

    UI_Box *Get_Box(UI_ID id);
    UI_Box *Alloc_Box();
    void Free_Box(UI_Box *box);

    void Push_ID(size_t seed);
    void Pop_ID();

    /*
        Input events from SDL to allow the ui_context to get state

        ARGS:
            event - sdl_event from SDL_PollEvents
    */
    void Pass_Event(SDL_Event event);

    std::string Source_Loc_Str(
        const std::source_location source_loc = std::source_location::current());
    /*
        Create a UI_Box in this UI_Context

        ARGS:
            (optional) fixed_pos - fixed position of box, (use floating)
            (optional) flags - box flags
            (optional) label - text label
            (optional) id_override - id override rather then source loc and
       iteration (optional) source_loc - location of top level call used for id
        RETURNS:
            signal - signal as returned by box.Signal()
    */
    UI_Signal Box_Make(V2 fixed_pos = {}, UI_Box_Flags flags = 0,
                       std::optional<std::string> id_override = {},
                       const std::source_location source_loc = std::source_location::current());
    UI_Signal Box_Make(Rect area, UI_Box_Flags flags = 0,
                       std::optional<std::string> id_override = {},
                       const std::source_location source_loc = std::source_location::current());
    // void Box_Delete(UI_ID box);

    // Box userdata alloc functions

    // Iteration functions
    bool Box_Iterate_Next(UI_Box **box);
    bool Box_Iterate_Prev(UI_Box **box);

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

    void Debug_Dump_Layout_JSON();
    void Debug_Render_Overlay(SDL_Renderer *renderer);
};
