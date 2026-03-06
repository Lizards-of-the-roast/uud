#pragma once

#include <any>
#include <string>
#include <tuple>

#include "ui_types.hpp"

struct UI_Context;

struct UI_Box {
    // n-ary tree ui hierarchy
    struct UI_Box *parent, *next_sibling, *prev_sibling, *first_child, *last_child;
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
    // the actual rect of the box
    Rect area;

    G2<UI_Size> size;
    UI_Margin margin;
    V2 offset;
    V2 min_size;
    int child_layout_axis;

    bool has_clip_ancestor;
    Rect clip_ancestor_rect;

    V2 fixed_position;  // for internal use
    V2 fixed_size;      // for internal use
    V2 view_offset;
    V2 view_bounds;
    V2 position_delta;  // change in position between frames
    G2<Alignment> elem_align;

    // std::string label;
    TTF_Text *label;
    TTF_Font *font; //optional if copied
    size_t cursor;
    G2<Alignment> label_alignment;

    Uint64 frame_created;
    Uint64 frame_last_touched;

    bool do_disable;   // set this one
    bool is_disabled;  // look at this one

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
    std::tuple<UI_Box *, int> Neighbor_Next();
    /*
        Search for next neighbor in the tree
        neighbor searches first for child, then sibling, then parent's sibling.
        Siblings are searched prev, children are searched last.

        RETURNS:
            UI_Box * - neighbor or NULL
            int      - n, vertical displacement in the tree
                        +n means it moved up n times -1 means it moved down
    */
    std::tuple<UI_Box *, int> Neighbor_Prev();

    /*
        Get a UI_Signal from box, containing all interaction with UI element

        ARGS:
            ctx - UI_Context that owns this box
        RETURNS:
            UI_Signal - signal
    */
    UI_Signal Signal(UI_Context *ctx);

    void Text_Create(UI_Context *ctx, std::string str, TTF_Text_Properties props);
    void Text_Copy_Font(TTF_Font *font, TTF_Font_Properties props = {});
    void Text_Insert(UI_Context *ctx, std::string text);
    void Text_Delete(void);
    void Text_Cursor_Left(void);
    void Text_Cursor_Right(void);
};
