#include "simp_ui.hpp"

#include "defer.hpp"

#include <SDL3/SDL_timer.h>

UI_Box::~UI_Box()
{
    if (this->label) TTF_DestroyText(this->label);
    if (this->id == 0) return;
    //SDL_Log("Box %lu killed", this->id);
}

UI_Size UI_Size_Pixels(float pixels, float strictness)
{
    return UI_Size{
        .type = UI_SIZE_PIXELS,
        .value = pixels,
        .strictness = strictness,
    };
}
UI_Size UI_Size_Parent(float percent, float strictness)
{
    return UI_Size{
        .type = UI_SIZE_PERCENT_OF_PARENT,
        .value = percent,
        .strictness = strictness,
    };
}
UI_Size UI_Size_Fit(float strictness)
{
    return UI_Size{
        .type = UI_SIZE_FIT,
        .value = 0,
        .strictness = strictness,
    };
}
UI_Size UI_Size_Child(float strictness)
{
    return UI_Size{
        .type = UI_SIZE_CHILD_SUM,
        .value = 0,
        .strictness = strictness,
    };
}
UI_Size UI_Size_Text(float margins, float strictness)
{
    return UI_Size{
        .type = UI_SIZE_TEXT_CONTENT,
        .value = margins,
        .strictness = strictness,
    };
}

UI_Context::UI_Context(SDL_Window *window, TTF_TextEngine *text_engine)
{
    // literally no way to 0 init a struct with a construct
    // on the stack, with new you can aparently have a sub class and init that
    // but nothing for stack allocated structs, Which is silly.
    // If weird stuff is happening with context its because I didnt
    // list out every single member and `= 0` them
    boxes = {};
    source_iteration_counter = {};
    last_iteration_box = {};
    box_pool = {};
    free_boxes = {};
    hot = {};
    active = {};
    focused = {};
    root = {};
    leafs = {};
    parents = {};
    label_alignments = {};
    sizes = {};
    margins = {};
    fonts = {};
    frame = {};
    mouse_history = {};
    mouse_pos = V2{};
    mouse_delta = V2{};
    mouse_up_buttons = {};
    mouse_down_buttons = {};
    mouse_wheel = V2{};
    //text_engine = {};
    clip_stack = {};
    //window = {};
    //window_height = {};
    //window_width = {};

    this->text_engine = text_engine;
    this->window = window;

    int x, y;
    SDL_GetWindowSize(this->window, &x, &y);
    this->window_width = (float)x;
    this->window_height = (float)y;

    /*
    this->active = 0;
    this->hot    = 0;
    this->frame  = 0;
    */

}

UI_Context::~UI_Context()
{
    for (UI_Box *box : this->box_pool)
        delete box;
}

UI_Box *UI_Context::Alloc_Box()
{
    UI_Box *box = NULL;

    if (!this->free_boxes.empty())
    {
        box = this->free_boxes.back();
        this->free_boxes.pop_back();
        box->~UI_Box();
        new (box) UI_Box{};
        return box;
    }

    box = new UI_Box{};
    this->box_pool.push_back(box);
    return box;
}

void UI_Context::Free_Box(UI_Box *box)
{
    if (!box) return;
    box->~UI_Box();
    new (box) UI_Box{};
    this->free_boxes.push_back(box);
}

UI_Box *UI_Context::Get_Box(UI_ID id)
{
    if (auto it = this->boxes.find(id); it != this->boxes.end())
        return it->second;

    return NULL;
}

void UI_Context::Pass_Event(SDL_Event event)
{
    switch (event.type)
    {
        case SDL_EVENT_WINDOW_RESIZED: if (event.window.windowID == SDL_GetWindowID(this->window))
        {
            int x, y;
            SDL_GetWindowSize(SDL_GetWindowFromID(event.window.windowID), &x, &y);
            this->window_width = (float)x;
            this->window_height = (float)y;
            break;
        }
        case SDL_EVENT_MOUSE_MOTION: if (event.motion.windowID == SDL_GetWindowID(this->window))
        {
            this->mouse_pos = V2{event.motion.x, event.motion.y};
            this->mouse_delta = V2{event.motion.xrel, event.motion.yrel};
            break;
        }
        case SDL_EVENT_MOUSE_BUTTON_UP: if (event.button.windowID == SDL_GetWindowID(this->window))
        {
            this->mouse_up_buttons |= SDL_BUTTON_MASK(event.button.button);
            break;
        }
        case SDL_EVENT_MOUSE_BUTTON_DOWN: if (event.button.windowID == SDL_GetWindowID(this->window))
        {
            this->mouse_down_buttons |= SDL_BUTTON_MASK(event.button.button);
            break;
        }
        case SDL_EVENT_MOUSE_WHEEL: if (event.wheel.windowID == SDL_GetWindowID(this->window))
        {
            this->mouse_wheel = V2{event.wheel.x, event.wheel.y};
            break;
        }
        case SDL_EVENT_TEXT_INPUT: if (event.text.windowID == SDL_GetWindowID(this->window))
        {
            if (!this->focused)
                break;

            UI_Box *box = this->Get_Box(this->focused);

            if (!box || !(box->flags & UI_BOX_FLAG_TEXTINPUT))
                break;

            box->Text_Insert(this, event.text.text);

            //SDL_Log("Input text: %s", event.text.text);
            break;
        }
        /* yeah no I'm not supporting localization sorry
        case SDL_EVENT_TEXT_EDITING:
        {
        }
        */
        case SDL_EVENT_KEY_DOWN: if (event.key.windowID == SDL_GetWindowID(this->window))
        {
            if (!this->focused)
                break;

            UI_Box *box = this->Get_Box(this->focused);

            if (!box || !(box->flags & UI_BOX_FLAG_TEXTINPUT))
                break;

            switch (event.key.key)
            {
                case SDLK_BACKSPACE:
                    box->Text_Delete();
                    break;
                case SDLK_RETURN:
                    box->Text_Insert(this, "\n");
                    break;
                case SDLK_LEFT:
                    box->Text_Cursor_Left();
                    break;
                case SDLK_RIGHT:
                    box->Text_Cursor_Right();
                    break;
                case SDLK_UP:
                    break;
                case SDLK_DOWN:
                    break;
            }

            break;
        }
    }
}

UI_Signal UI_Context::Box_Make(
    V2 fixed_pos,
    UI_Box_Flags flags,
    std::string label,
    std::optional<std::string> id_override,
    const std::source_location source_loc
)
{
    size_t id = 0;
    size_t source_key = 0;
    size_t iteration = 0;
    UI_Box *box = NULL;
    bool box_exists = false;

    if (id_override.has_value())
    {
        id = std::hash<std::string>{}(id_override.value());
        source_key = id;
        iteration = 0;
        if (auto it = this->boxes.find(id); it != this->boxes.end())
        {
            box = it->second;
            box_exists = true;
        }
    }
    else
    {
        source_key = std::hash<std::string>{}(
            std::string(source_loc.file_name()) + "::"
            + std::to_string(source_loc.line()) + ":"
            + std::to_string(source_loc.column())
        );

        iteration = this->source_iteration_counter[source_key]++;
        id = std::hash<std::string>{}(
            std::to_string(source_key) + "#" + std::to_string(iteration)
        );

        if (auto it = this->boxes.find(id); it != this->boxes.end())
        {
            box = it->second;
            box_exists = true;
        }
    }

    if (!box_exists)
    {
        box = this->Alloc_Box();
        this->boxes[id] = box;
        box->id = id;
        box->frame_created = this->frame;
    }

    box->frame_last_touched = this->frame;

    box->next_iteration = NULL;
    box->prev_iteration = NULL;
    box->iteration = iteration;
    box->source_key = source_key;

    if (auto it = this->last_iteration_box.find(source_key); it != this->last_iteration_box.end() && it->second != box)
    {
        box->prev_iteration = it->second;
        it->second->next_iteration = box;
    }
    
    this->last_iteration_box[source_key] = box;

    // set flags
    box->flags = flags;

    // set label
    if (!label.empty() || (flags & UI_BOX_FLAG_TEXTINPUT && !box->label))
    {
        if (!box->label)
            box->label = TTF_CreateText(this->text_engine, NULL, label.c_str(), label.length());
        else 
            TTF_SetTextString(box->label, label.c_str(), label.length());
        TTF_Font *font = (this->fonts.size()) ? this->fonts.top() : NULL;
        TTF_SetTextFont(box->label, font);
    }

    // get from style stack or dont (hard code)

    // get from text align stack
    if (this->label_alignments.size() > 0)
    {
        box->label_alignment = this->label_alignments.top();
    }

    // semantic size or dont (just fixed size)
    //box->area = area;
    box->size = (this->sizes.size()) ? this->sizes.top() : G2<UI_Size>{};
    box->fixed_size = V2{};
    box->fixed_position = fixed_pos;
    box->margin = (this->margins.size()) ? this->margins.top() : UI_Margin{};


    // Add node to tree
    // NOTE: maybe the root can just be appended to the stack at begin()
    box->parent = (this->parents.size()) ? this->parents.top() : &this->root;

    //NOTE: since if the parent stack is empty it is set to this->root
    //      we can basically assume it is not NULL
    if (box->parent->first_child == NULL)
    {
        box->parent->first_child = box;
        box->parent->last_child = box;
        if (box->parent != &this->root)
            this->leafs.pop_back(); //pop the parent off the leaf list
    }
    else if (box->parent->last_child != NULL)
    {
        box->parent->last_child->next_sibling = box;
        box->prev_sibling = box->parent->last_child;
        box->parent->last_child = box;
        box->next_sibling = NULL;
    }
    this->leafs.push_back(box);
    box->parent->child_count += 1;

    box->is_disabled = box->do_disable || (box->parent && box->parent->is_disabled);

    UI_Signal sig = box->Signal(this);
    // Set Cursor (based on signal)
    return sig;
}

void UI_Context::Begin()
{

    //Events?

    this->source_iteration_counter.clear();
    this->last_iteration_box.clear();

    std::vector<UI_ID> deletion_list;

    // Prune old boxes and clear tree
    for (const auto& [id, box] : this->boxes)
    {
        if (box == &this->root) continue;
        if (this->frame - box->frame_last_touched > 1)
        {
            this->Free_Box(box);
            deletion_list.push_back(id);
            continue;
        }

        box->first_child = box->last_child= box->prev_sibling = box->next_sibling = NULL;
        box->child_count = 0;
    }
    for (UI_ID id : deletion_list)
    {
        this->boxes.erase(id);
    }
    this->root = {};
    this->root.fixed_size = V2{this->window_width, this->window_height};
    this->root.size.x.value = this->window_width;
    this->root.size.y.value = this->window_height;
    this->root.area = Rect{
    0,0,
    this->window_width, this->window_height
    };
    this->root.layout_box = this->root.area;

    this->leafs.clear();

    return;
}
void UI_Context::End()
{
    if (this->mouse_up_buttons && this->focused)
    {
        UI_Box *focused = this->Get_Box(this->focused);
        if (focused && !(focused->signal_last.flags & UI_SIG_MOUSE_OVER))
        {
            SDL_StopTextInput(this->window);
            this->focused = 0;
            SDL_Log("Ended Text Input");
        }
    }

    this->mouse_up_buttons = 0;
    this->mouse_down_buttons = 0;

    // layout
    this->Layout_Compute();

    this->frame += 1;

    return;
}

std::tuple< UI_Box *, int > UI_Box::Neighbor_Next() 
{
    int n = 0;

    if (this->first_child != NULL)
        return {this->first_child, -1};

    for (UI_Box *p = this; p != NULL; p = p->parent)
    {
        if (p->next_sibling != NULL)
            return {p->next_sibling, n};
        n += 1;
    }

    return {NULL, 0};
}
std::tuple< UI_Box *, int > UI_Box::Neighbor_Prev() 
{
    int n = 0;

    if (this->last_child != NULL)
        return {this->last_child, -1};

    for (UI_Box *p = this; p != NULL; p = p->parent)
    {
        if (p->prev_sibling != NULL)
            return {p->prev_sibling, n};
        n += 1;
    }

    return {NULL, 0};
}

void UI_Box::Text_Insert(UI_Context *ctx, std::string text)
{
    TTF_Font *font = NULL;
    if (this->label && this->label->text == NULL)
    {
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
void UI_Box::Text_Delete(void)
{
    if (this->cursor < 0) return;

    if (this->cursor) this->cursor--;
    TTF_DeleteTextString(this->label, this->cursor, 1);
}
void UI_Box::Text_Cursor_Left(void)
{
    this->cursor = SDL_clamp((ssize_t)this->cursor - 1, 0, SDL_strlen(this->label->text));
}
void UI_Box::Text_Cursor_Right(void)
{
    this->cursor = SDL_clamp(this->cursor + 1, 0, SDL_strlen(this->label->text));
}

bool UI_Context::Box_Iterate_Next( UI_Box **box )
{
    if (*box == &this->root) return false;
    *box = std::get<0>((*box)->Neighbor_Next());
    return box != NULL;
}
bool UI_Context::Box_Iterate_Prev( UI_Box **box )
{
    if (*box == &this->root) return false;
    *box = std::get<0>((*box)->Neighbor_Prev());
    return box != NULL;
}

static const int SDL_Button_Flag_To_Sig_Flag( SDL_MouseButtonFlags buttons, UI_Signal_Flags first_sig )
{
    UI_Signal_Flags result = 0;
    for (int i = 0; i < 4; i++)
    {
        if (buttons & (0x01 << i))
            result |= first_sig << i;
    }
    return result;
}

static bool UI_Is_Multi_Click(
    const UI_Mouse_State &newer,
    const UI_Mouse_State &older,
    Uint64 max_gap_ms,
    float max_dist_px
)
{
    if (!newer.timestamp || !older.timestamp) return false;
    if (newer.timestamp < older.timestamp) return false;
    if (newer.timestamp - older.timestamp > max_gap_ms) return false;

    if (newer.box != older.box) return false;
    if (newer.buttons != older.buttons) return false;

    V2 d = V2 {
        newer.pos.x - older.pos.x,
        newer.pos.y - older.pos.y
    };

    if (SDL_fabsf(d.x) > max_dist_px) return false;
    if (SDL_fabsf(d.y) > max_dist_px) return false;

    return true;
}

UI_Signal UI_Box::Signal(UI_Context *ctx)
{
    if (this == &ctx->root) return {};

    UI_Signal sig = {0};
    defer (this->signal_last = sig);

    sig.box = this;
    sig.mouse_pos = ctx->mouse_pos;

    if (this->is_disabled)
    {
        if (ctx->active == this->id)
            ctx->active = 0;
        if (ctx->hot == this->id)
            ctx->hot = 0;
        if (ctx->focused == this->id)
            ctx->focused = 0;
        return sig;
    }

    bool is_mouse_over = this->area.Collision(ctx->mouse_pos);

    if (this->flags & UI_BOX_FLAG_CLICKABLE)
    {
        if (ctx->mouse_up_buttons && ctx->active == this->id)
        {

            ctx->active = 0;
            sig.flags |= SDL_Button_Flag_To_Sig_Flag(ctx->mouse_up_buttons, UI_SIG_LEFT_RELEASED);

            if (is_mouse_over)
            {
                sig.flags |= SDL_Button_Flag_To_Sig_Flag(ctx->mouse_up_buttons, UI_SIG_LEFT_CLICKED);
                if (this->flags & UI_BOX_FLAG_TEXTINPUT && ctx->focused != this->id)
                {
                    ctx->focused = this->id;
                    SDL_StartTextInput(ctx->window);
                    SDL_Log("Began Text Input");
                    
                }
            }
            else if (ctx->hot)
                ctx->hot = 0;
        }
        if (ctx->mouse_down_buttons && is_mouse_over && ctx->mouse_history[-1].frame != ctx->frame)
        {
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

            bool is_double_click = UI_Is_Multi_Click(mouse_state, prev, multi_click_window_ms, multi_click_slop_px);
            bool is_triple_click = is_double_click && UI_Is_Multi_Click(prev, prev2, multi_click_window_ms, multi_click_slop_px);

            if (is_double_click)
                sig.flags |= SDL_Button_Flag_To_Sig_Flag(ctx->mouse_down_buttons, UI_SIG_LEFT_DOUBLE_CLICKED);
            if (is_triple_click)
                sig.flags |= SDL_Button_Flag_To_Sig_Flag(ctx->mouse_down_buttons, UI_SIG_LEFT_TRIPLE_CLICKED);

            ctx->mouse_history.Push_Front(mouse_state);
        }

        if (ctx->mouse_history[-1].box    == this->id
        &&  ctx->mouse_history[-2].frame != ctx->frame
        &&  ctx->active                  == this->id)
        {
            sig.mouse_delta = ctx->mouse_history[-1].pos - ctx->mouse_pos;
            sig.flags |= SDL_Button_Flag_To_Sig_Flag(ctx->mouse_history[-1].buttons, UI_SIG_LEFT_DOWN);
        }
    }

    /*
    if (box->flags & UI_BOX_FLAG_VIEW_SCROLL && is_mouse_over && (ctx->mouse_wheel.x != 0 || ctx->mouse_wheel.y != 0) )
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
        this->view_offset.x += SDL_clamp(ctx->mouse_wheel.x * mask.x * step, 0, max_view_off.x);
        this->view_offset.y += SDL_clamp(ctx->mouse_wheel.y * mask.y * step, 0, max_view_off.y);
    }
    */

    if (is_mouse_over)
    {
        sig.flags |= UI_SIG_MOUSE_OVER;

        UI_Box *hot = ctx->Get_Box(ctx->hot);
        bool has_hot = (hot != NULL);

        if (this->flags & UI_BOX_FLAG_CLICKABLE
        && (!has_hot || hot->frame_last_touched != ctx->frame || ctx->hot == this->id)
        && (ctx->active == 0 || ctx->active == this->id))
        {
            sig.flags |= UI_SIG_HOVERING;
            ctx->hot = this->id;
        }

    }
    else if (ctx->hot == this->id)
        ctx->hot = 0;

    if (ctx->focused == this->id)
        sig.flags |= UI_SIG_FOCUSED;
    return sig;
}


// I love c++ \s //

UI_Context::Render_It_Range UI_Context::Render_It( SDL_Renderer *renderer )
{
    Render_It_Range r = {};
    r.ctx = this;
    r.renderer = renderer;
    return r;
}

UI_Context::Render_It_Range::~Render_It_Range()
{
    for (;ctx->clip_stack.size();) ctx->clip_stack.pop();
    SDL_SetRenderClipRect( this->renderer, NULL );
}

UI_Context::Render_It_Range::Iterator UI_Context::Render_It_Range::begin()
{
    Iterator it = {};
    it.ctx = ctx;
    it.renderer = this->renderer;
    it.box = &ctx->root;
    ++it;
    return it;
}
UI_Context::Render_It_Range::Iterator UI_Context::Render_It_Range::end()
{
    Iterator it = {};
    it.box = NULL;
    return it;
}

static Rect Get_Clip_Rect( UI_Box *box)
{
    if (box->has_clip_ancestor)
        return box->area.Intersection(box->clip_ancestor_rect);

    return box->area;
}
UI_Box *UI_Context::Render_It_Range::Iterator::operator*()
{
    // deep tree unwinds
    while (this->pop_stack.size() > 0 && this->pop_stack.top() <= this->level)
    {
        ctx->clip_stack.pop();
        if (ctx->clip_stack.size())
            SDL_SetRenderClipRect(this->renderer, &ctx->clip_stack.top());
        else
            SDL_SetRenderClipRect(this->renderer, NULL);
        this->pop_stack.pop();
    }

    if (this->box->flags & UI_BOX_FLAG_CLIP)
    {
        Rect clip = Get_Clip_Rect( this->box );
        ctx->clip_stack.push(clip.IRect_Round());
        SDL_SetRenderClipRect( this->renderer, &ctx->clip_stack.top() );
        this->pop_stack.push(level);
    }

    return this->box;
}

UI_Context::Render_It_Range::Iterator &UI_Context::Render_It_Range::Iterator::operator++()
{
    std::tie(this->box, this->n) = box->Neighbor_Prev();
    this->level += n;


    return *this;
}
bool UI_Context::Render_It_Range::Iterator::operator!=(const Iterator& other)
{
    return this->box != other.box;
}

// layout it

UI_Context::Layout_It_Range UI_Context::Layout_It()
{
    Layout_It_Range r = {};
    r.ctx = this;
    return r;
}

UI_Context::Layout_It_Range::~Layout_It_Range()
{
}

UI_Context::Layout_It_Range::Iterator UI_Context::Layout_It_Range::begin()
{
    Iterator it = {};
    it.ctx = ctx;
    it.box = &ctx->root;
    //++it;
    return it;
}
UI_Context::Layout_It_Range::Iterator UI_Context::Layout_It_Range::end()
{
    Iterator it = {};
    it.box = NULL;
    return it;
}
UI_Box *UI_Context::Layout_It_Range::Iterator::operator*()
{
    return this->box;
}

UI_Context::Layout_It_Range::Iterator &UI_Context::Layout_It_Range::Iterator::operator++()
{
    std::tie(this->box, this->n) = box->Neighbor_Next();
    this->level += n;

    return *this;
}
bool UI_Context::Layout_It_Range::Iterator::operator!=(const Iterator& other)
{
    return this->box != other.box;
}


// LAYOUT //

void UI_Context::Layout_Calc_Standalone(void)
{
    for (UI_Box *box : this->Layout_It()) for (int i = 0; i<2; i++) switch(box->size[i].type)
    {
        case UI_SIZE_PIXELS:
            box->fixed_size[i] = box->size[i].value;
            break;
        case UI_SIZE_TEXT_CONTENT:
            int text_size[2];
            TTF_GetTextSize(box->label, &text_size[0], &text_size[1]);
            box->fixed_size[i] = box->size[i].value + (float)text_size[i] + box->size[i].value;
            break;
        default:
            break;
    }
}
void UI_Context::Layout_Calc_Upwards_Dependent(void)
{
    // % of parent
    for (UI_Box *box : this->Layout_It()) for (int i = 0; i < 2; i++)
    {
        if (box->size[i].type != UI_SIZE_PERCENT_OF_PARENT)
            continue;

        for (UI_Box *p = box->parent; p; p = p->parent)
        {
            if (p->fixed_size[i] > 0.0f)
            {
                box->fixed_size[i] = p->fixed_size[i] * box->size[i].value;
                break;
            }
        }
    }

    // fit fb
    for (UI_Box *box : this->Layout_It()) for (int i = 0; i < 2; i++)
    {
        if (box->size[i].type != UI_SIZE_FIT)
            continue;

        for (UI_Box *p = box->parent; p; p = p->parent)
        {
            if (p->fixed_size[i] <= 0.0f)
                continue;

            if (p->child_layout_axis != i)
                box->fixed_size[i] = p->fixed_size[i];

            break;
        }
    }

    // siblings get space first
    for (UI_Box *parent : this->Layout_It()) for (int i = 0; i < 2; i++)
    {
        if (parent->child_layout_axis != i) continue;
        if (parent->fixed_size[i] <= 0.0f) continue;

        float used_by_non_fit = 0.0f;
        int fit_count = 0;

        for (UI_Box *child = parent->first_child; child; child = child->next_sibling)
        {
            if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                continue;

            if (child->size[i].type == UI_SIZE_FIT)
                fit_count += 1;
            else
                used_by_non_fit += child->fixed_size[i];
        }

        if (fit_count <= 0) continue;

        const float remaining = SDL_max(0.0f, parent->fixed_size[i] - used_by_non_fit);
        const float each_fit = remaining / (float)fit_count;

        for (UI_Box *child = parent->first_child; child; child = child->next_sibling)
        {
            if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                continue;
            if (child->size[i].type == UI_SIZE_FIT)
                child->fixed_size[i] = each_fit;
        }
    }
}

void UI_Context::Layout_Calc_Downwards_Dependant(void)
{
    int skip_n = 0;
    for (int i = 0; i<2; i++)
    for (UI_Box *leaf : this->leafs)
    {
        if (skip_n > 0)
        {
            skip_n--;
            continue;
        }
        UI_Box *last_box = NULL;
        for (UI_Box *box = leaf; box; box = box->parent)
        {
            defer (last_box = box);
            switch (box->size[i].type)
            {
                case UI_SIZE_CHILD_SUM:
                {
                    // Find the last downward dependant child, if it isnt the last node whose branch we traveled up
                    // then skip traversing that from this leaf and get the next so that we first solve all downward dependancies
                    UI_Box *last_downward_dependant_child = NULL;
                    for (UI_Box *child = box->last_child; child; child = child->prev_sibling)
                    {
                        if (child->size[i].type & UI_SIZE_DOWNWARD_DEPENDENT)
                            last_downward_dependant_child = child;
                        if (child == last_box) break;
                    }
                    if (last_downward_dependant_child && last_downward_dependant_child != last_box)
                        goto continue_leaf_loop;

                    float sum = 0;
                    for (UI_Box *child = box->first_child; child; child = child->next_sibling)
                    {
                        if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                            continue;

                        if (i == child->child_layout_axis)
                            sum += child->fixed_size[i];
                        else
                            sum = SDL_max( sum, child->fixed_size[i] );
                    }
                    box->fixed_size[i] = sum;

                    break;
                }
                default:
                    if (box->parent)
                        skip_n = box->child_count - 1;
                    break;
            }
        }
        continue_leaf_loop:
    }
}

//NOTE: To be completely honest, I dont remember how this works
void UI_Context::Layout_Solve_Violation(void)
{
    for (UI_Box *box : this->Layout_It()) for (int i = 0; i<2; i++)
    {
        // - fixup children sizes on non child layout axis
        if (i != box->child_layout_axis)
        {
            for (UI_Box *child = box->first_child; child; child = child->next_sibling)
            {
                if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                    continue;
                float violation = child->fixed_size[i] - box->fixed_size[i];
                float fixup = SDL_clamp( violation, 0, child->fixed_size[i] );
                if (fixup > 0)
                    child->fixed_size[i] -= fixup;
            }
        }
        else // - fixup children sizes on child layout axis
        {
            //float total_allowed_size = box->fixed_size[i];
            float total_size = 0;
            float total_weighted_size = 0;
            for (UI_Box *child = box->first_child; child; child = child->next_sibling)
            {
                if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                    continue;

                total_size += child->fixed_size[i];
                total_weighted_size += child->fixed_size[i] * (1-child->size[i].strictness);
            }

            float violation = total_size - box->fixed_size[i];

            if (violation > 0 && total_weighted_size > 0)
            {
                static thread_local std::vector<float> child_fixup;
                child_fixup.assign(box->child_count, 0.0f);
                int idx = 0;

                for (UI_Box *child = box->first_child; child; child = child->next_sibling, idx++)
                {
                    if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                        continue;

                    child_fixup[idx] = SDL_max(0.0f, child->fixed_size[i]);
                }

                idx = 0;
                const float fixup_pct = violation / total_weighted_size;
                for (UI_Box *child = box->first_child; child; child = child->next_sibling, idx++)
                {
                    if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                        continue;

                    child->fixed_size[i] -= child_fixup[idx] * fixup_pct;
                }
            }
        }

        //XXX: if allowOverFlow flag is added then fixup upwards relitve sizes

        for (UI_Box *child = box->first_child; child; child = child->next_sibling)
        {
            child->fixed_size[i] = SDL_max( child->fixed_size[i], child->min_size[i] );
        }
    }
}

static void Apply_Margin( Rect *r, UI_Margin margin )
{
    r->x += margin.left;
    r->w -= margin.left;

    r->y += margin.top;
    r->h -= margin.top;

    r->w -= margin.right;
    r->h -= margin.bottom;
}

void UI_Context::Layout_Comp_Relative(void)
{
    for (UI_Box *box : this->Layout_It()) for (int i = 0; i<2; i++)
    {
        float layout_position = 0;
        float bounds = 0;
        for (UI_Box *child = box->first_child; child; child = child->next_sibling)
        {
            float original_position = child->layout_box[i];

            if ( !(child->flags & (UI_BOX_FLAG_FLOATING_X << i)) )
            {
                child->fixed_position[i] += layout_position;
                if (i == box->child_layout_axis)
                {
                    layout_position += child->fixed_size[i];
                    bounds += child->fixed_size[i];
                }
                else
                    bounds = SDL_max(bounds, child->fixed_size[i]);
            }

            //XXX: handle animation stuff in an alternate case
            //XXX: add view offset to below
            child->layout_box[i] = box->layout_box[i] + box->offset[i] + child->fixed_position[i];
            if ( !(child->flags & (UI_BOX_FLAG_FLOATING_X << i)) )
                child->layout_box[i] -= box->view_offset[i];

            child->layout_box[2+i] = child->fixed_size[i];

            child->position_delta[i] = original_position - child->layout_box[i];
        }

        // Element alignment
        //NOTE: Second pass so that bounds is up to date but i could skip the second pass and use last frames bounds
        for (UI_Box *child = box->first_child; child; child = child->next_sibling)
        {
            if (child->flags & (UI_BOX_FLAG_FLOATING_X << i))
                break;

            float original_position = child->layout_box[i] + child->position_delta[i];
            float size = (i == box->child_layout_axis) ? bounds : child->layout_box[2+i];
            switch (box->elem_align[i])
            {
                case UI_ALIGN_LEFT:
                    // Do nothing
                    break;
                case UI_ALIGN_RIGHT:
                    child->fixed_position[i] += box->layout_box[2+i] - size;
                    break;
                case UI_ALIGN_CENTER:
                    child->fixed_position[i] += (box->layout_box[2+i] - size) / 2;
                    break;
            }

            child->layout_box[i] = box->layout_box[i] + box->offset[i] + child->fixed_position[i];
            child->position_delta[i] = original_position - child->layout_box[i];

        }

        box->view_bounds[i] = bounds;

        // apply margin / offset
        box->area = box->layout_box;

        Apply_Margin(&box->area, box->margin);

        box->area.x += box->offset.x;
        box->area.y += box->offset.y;

        // cache nearest clipping ancestor
        if (box == &this->root || !box->parent)
        {
            box->has_clip_ancestor = false;
        }
        else if (box->parent->flags & UI_BOX_FLAG_CLIP)
        {
            if (box->parent->has_clip_ancestor)
                box->clip_ancestor_rect = box->parent->area.Intersection(box->parent->clip_ancestor_rect);
            else
                box->clip_ancestor_rect = box->parent->area;

            box->has_clip_ancestor = true;
        }
        else
        {
            box->has_clip_ancestor = box->parent->has_clip_ancestor;
            if (box->has_clip_ancestor)
                box->clip_ancestor_rect = box->parent->clip_ancestor_rect;
        }
    }

}

void UI_Context::Layout_Compute(void)
{
    // Calculate "standalone" sizes:
    //     sizes that dont depend on any other widget
    this->Layout_Calc_Standalone();

    // Calculate "Upwards-dependant" sizes
    //     sizes that depend only on widgets above
    this->Layout_Calc_Upwards_Dependent();

    // Calculate “downwards-dependent” sizes
    //     sizes that depend only on widgets below
    this->Layout_Calc_Downwards_Dependant();

    // Solve violations
    //     ensure that content is not extening past parents boundries, use strictness
    this->Layout_Solve_Violation();

    // compute the relative positions
    this->Layout_Comp_Relative();
}
