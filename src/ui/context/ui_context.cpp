#include "ui_context.hpp"

UI_Context::UI_Context(SDL_Window *window, TTF_TextEngine *text_engine) {
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
    // text_engine = {};
    clip_stack = {};
    // window = {};
    // window_height = {};
    // window_width = {};

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

UI_Context::~UI_Context() {
    for (UI_Box *box : this->box_pool)
        delete box;
}

UI_Box *UI_Context::Alloc_Box() {
    UI_Box *box = NULL;

    if (!this->free_boxes.empty()) {
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

void UI_Context::Free_Box(UI_Box *box) {
    if (!box)
        return;
    box->~UI_Box();
    new (box) UI_Box{};
    this->free_boxes.push_back(box);
}

UI_Box *UI_Context::Get_Box(UI_ID id) {
    if (auto it = this->boxes.find(id); it != this->boxes.end())
        return it->second;

    return NULL;
}

void UI_Context::Pass_Event(SDL_Event event) {
    switch (event.type) {
        case SDL_EVENT_WINDOW_RESIZED:
            if (event.window.windowID == SDL_GetWindowID(this->window)) {
                int x, y;
                SDL_GetWindowSize(SDL_GetWindowFromID(event.window.windowID), &x, &y);
                this->window_width = (float)x;
                this->window_height = (float)y;
                break;
            }
        case SDL_EVENT_MOUSE_MOTION:
            if (event.motion.windowID == SDL_GetWindowID(this->window)) {
                this->mouse_pos = V2{event.motion.x, event.motion.y};
                this->mouse_delta = V2{event.motion.xrel, event.motion.yrel};
                break;
            }
        case SDL_EVENT_MOUSE_BUTTON_UP:
            if (event.button.windowID == SDL_GetWindowID(this->window)) {
                this->mouse_up_buttons |= SDL_BUTTON_MASK(event.button.button);
                break;
            }
        case SDL_EVENT_MOUSE_BUTTON_DOWN:
            if (event.button.windowID == SDL_GetWindowID(this->window)) {
                this->mouse_down_buttons |= SDL_BUTTON_MASK(event.button.button);
                break;
            }
        case SDL_EVENT_MOUSE_WHEEL:
            if (event.wheel.windowID == SDL_GetWindowID(this->window)) {
                this->mouse_wheel = V2{event.wheel.x, event.wheel.y};
                break;
            }
        case SDL_EVENT_TEXT_INPUT:
            if (event.text.windowID == SDL_GetWindowID(this->window)) {
                if (!this->focused)
                    break;

                UI_Box *box = this->Get_Box(this->focused);

                if (!box || !(box->flags & UI_BOX_FLAG_TEXTINPUT))
                    break;

                box->Text_Insert(this, event.text.text);

                // SDL_Log("Input text: %s", event.text.text);
                break;
            }
        /* yeah no I'm not supporting localization sorry
        case SDL_EVENT_TEXT_EDITING:
        {
        }
        */
        case SDL_EVENT_KEY_DOWN:
            if (event.key.windowID == SDL_GetWindowID(this->window)) {
                if (!this->focused)
                    break;

                UI_Box *box = this->Get_Box(this->focused);

                if (!box || !(box->flags & UI_BOX_FLAG_TEXTINPUT))
                    break;

                switch (event.key.key) {
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

UI_Signal UI_Context::Box_Make(V2 fixed_pos, UI_Box_Flags flags, std::string label,
                               std::optional<std::string> id_override,
                               const std::source_location source_loc) {
    size_t id = 0;
    size_t source_key = 0;
    size_t iteration = 0;
    UI_Box *box = NULL;
    bool box_exists = false;

    if (id_override.has_value()) {
        id = std::hash<std::string>{}(id_override.value());
        source_key = id;
        iteration = 0;
        if (auto it = this->boxes.find(id); it != this->boxes.end()) {
            box = it->second;
            box_exists = true;
        }
    } else {
        source_key = std::hash<std::string>{}(std::string(source_loc.file_name()) +
                                              "::" + std::to_string(source_loc.line()) + ":" +
                                              std::to_string(source_loc.column()));

        iteration = this->source_iteration_counter[source_key]++;
        id = std::hash<std::string>{}(std::to_string(source_key) + "#" + std::to_string(iteration));

        if (auto it = this->boxes.find(id); it != this->boxes.end()) {
            box = it->second;
            box_exists = true;
        }
    }

    if (!box_exists) {
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

    if (auto it = this->last_iteration_box.find(source_key);
        it != this->last_iteration_box.end() && it->second != box) {
        box->prev_iteration = it->second;
        it->second->next_iteration = box;
    }

    this->last_iteration_box[source_key] = box;

    // set flags
    box->flags = flags;

    // set label
    if (!label.empty() || (flags & UI_BOX_FLAG_TEXTINPUT && !box->label)) {
        if (!box->label)
            box->label = TTF_CreateText(this->text_engine, NULL, label.c_str(), label.length());
        else
            TTF_SetTextString(box->label, label.c_str(), label.length());
        TTF_Font *font = (this->fonts.size()) ? this->fonts.top() : NULL;
        TTF_SetTextFont(box->label, font);
    }

    // get from style stack or dont (hard code)

    // get from text align stack
    if (this->label_alignments.size() > 0) {
        box->label_alignment = this->label_alignments.top();
    }

    // semantic size or dont (just fixed size)
    // box->area = area;
    box->size = (this->sizes.size()) ? this->sizes.top() : G2<UI_Size>{};
    box->fixed_size = V2{};
    box->fixed_position = fixed_pos;
    box->margin = (this->margins.size()) ? this->margins.top() : UI_Margin{};

    // Add node to tree
    // NOTE: maybe the root can just be appended to the stack at begin()
    box->parent = (this->parents.size()) ? this->parents.top() : &this->root;

    // NOTE: since if the parent stack is empty it is set to this->root
    //       we can basically assume it is not NULL
    if (box->parent->first_child == NULL) {
        box->parent->first_child = box;
        box->parent->last_child = box;
        if (box->parent != &this->root)
            this->leafs.pop_back();  // pop the parent off the leaf list
    } else if (box->parent->last_child != NULL) {
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

void UI_Context::Begin() {
    // Events?

    this->source_iteration_counter.clear();
    this->last_iteration_box.clear();

    std::vector<UI_ID> deletion_list;

    // Prune old boxes and clear tree
    for (const auto &[id, box] : this->boxes) {
        if (box == &this->root)
            continue;
        if (this->frame - box->frame_last_touched > 1) {
            this->Free_Box(box);
            deletion_list.push_back(id);
            continue;
        }

        box->first_child = box->last_child = box->prev_sibling = box->next_sibling = NULL;
        box->child_count = 0;
    }
    for (UI_ID id : deletion_list) {
        this->boxes.erase(id);
    }
    this->root = {};
    this->root.fixed_size = V2{this->window_width, this->window_height};
    this->root.size.x.value = this->window_width;
    this->root.size.y.value = this->window_height;
    this->root.area = Rect{0, 0, this->window_width, this->window_height};
    this->root.layout_box = this->root.area;

    this->leafs.clear();

    return;
}
void UI_Context::End() {
    if (this->mouse_up_buttons && this->focused) {
        UI_Box *focused = this->Get_Box(this->focused);
        if (focused && !(focused->signal_last.flags & UI_SIG_MOUSE_OVER)) {
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

// I love c++ \s //

UI_Context::Render_It_Range UI_Context::Render_It(SDL_Renderer *renderer) {
    Render_It_Range r = {};
    r.ctx = this;
    r.renderer = renderer;
    return r;
}

UI_Context::Render_It_Range::~Render_It_Range() {
    for (; ctx->clip_stack.size();)
        ctx->clip_stack.pop();
    SDL_SetRenderClipRect(this->renderer, NULL);
}

UI_Context::Render_It_Range::Iterator UI_Context::Render_It_Range::begin() {
    Iterator it = {};
    it.ctx = ctx;
    it.renderer = this->renderer;
    it.box = &ctx->root;
    ++it;
    return it;
}
UI_Context::Render_It_Range::Iterator UI_Context::Render_It_Range::end() {
    Iterator it = {};
    it.box = NULL;
    return it;
}

static Rect Get_Clip_Rect(UI_Box *box) {
    if (box->has_clip_ancestor)
        return box->area.Intersection(box->clip_ancestor_rect);

    return box->area;
}
UI_Box *UI_Context::Render_It_Range::Iterator::operator*() {
    // deep tree unwinds
    while (this->pop_stack.size() > 0 && this->pop_stack.top() <= this->level) {
        ctx->clip_stack.pop();
        if (ctx->clip_stack.size())
            SDL_SetRenderClipRect(this->renderer, &ctx->clip_stack.top());
        else
            SDL_SetRenderClipRect(this->renderer, NULL);
        this->pop_stack.pop();
    }

    if (this->box->flags & UI_BOX_FLAG_CLIP) {
        Rect clip = Get_Clip_Rect(this->box);
        ctx->clip_stack.push(clip.IRect_Round());
        SDL_SetRenderClipRect(this->renderer, &ctx->clip_stack.top());
        this->pop_stack.push(level);
    }

    return this->box;
}

UI_Context::Render_It_Range::Iterator &UI_Context::Render_It_Range::Iterator::operator++() {
    std::tie(this->box, this->n) = box->Neighbor_Prev();
    this->level += n;

    return *this;
}
bool UI_Context::Render_It_Range::Iterator::operator!=(const Iterator &other) {
    return this->box != other.box;
}

// layout it

UI_Context::Layout_It_Range UI_Context::Layout_It() {
    Layout_It_Range r = {};
    r.ctx = this;
    return r;
}

UI_Context::Layout_It_Range::~Layout_It_Range() {}

UI_Context::Layout_It_Range::Iterator UI_Context::Layout_It_Range::begin() {
    Iterator it = {};
    it.ctx = ctx;
    it.box = &ctx->root;
    //++it;
    return it;
}
UI_Context::Layout_It_Range::Iterator UI_Context::Layout_It_Range::end() {
    Iterator it = {};
    it.box = NULL;
    return it;
}
UI_Box *UI_Context::Layout_It_Range::Iterator::operator*() {
    return this->box;
}

UI_Context::Layout_It_Range::Iterator &UI_Context::Layout_It_Range::Iterator::operator++() {
    std::tie(this->box, this->n) = box->Neighbor_Next();
    this->level += n;

    return *this;
}
bool UI_Context::Layout_It_Range::Iterator::operator!=(const Iterator &other) {
    return this->box != other.box;
}
