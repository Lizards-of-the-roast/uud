#include "ui_context.hpp"

#include "core/state.hpp"

#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <sstream>

static bool Parse_Bool_Env(const char *name) {
    const char *raw = std::getenv(name);
    return raw && (raw[0] == '1' || raw[0] == 't' || raw[0] == 'T');
}

static const char *Scene_To_String(Scene scene) {
    switch (scene) {
        case Scene::Intro:
            return "Intro";
        case Scene::Login:
            return "Login";
        case Scene::Main_Menu:
            return "Main_Menu";
        case Scene::Match:
            return "Match";
        case Scene::Exit:
            return "Exit";
    }
    return "Unknown";
}

static std::string Json_Escape(const char *input) {
    if (!input)
        return "";

    std::ostringstream escaped;
    for (const char c : std::string(input)) {
        switch (c) {
            case '\"': escaped << "\\\""; break;
            case '\\': escaped << "\\\\"; break;
            case '\n': escaped << "\\n"; break;
            case '\t': escaped << "\\t"; break;
            default: escaped << c; break;
        }
    }
    return escaped.str();
}

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
    id_stack = {};
    label_alignments = {};
    sizes = {};
    margins = {};
    // fonts = {};
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
    debug_overlay_enabled = false;
    debug_dump_enabled = false;
    debug_dump_once_requested = false;
    debug_show_root = false;
    debug_dump_dir = "./qa/layout";

    this->text_engine = text_engine;
    this->window = window;

    int x, y;
    SDL_GetWindowSize(this->window, &x, &y);
    this->window_width = (float)x;
    this->window_height = (float)y;

    if (const char *dump_dir = std::getenv("UUD_UI_DUMP_DIR"); dump_dir && dump_dir[0] != '\0')
        this->debug_dump_dir = dump_dir;
    this->debug_overlay_enabled = Parse_Bool_Env("UUD_UI_DEBUG_OVERLAY");
    this->debug_dump_enabled = Parse_Bool_Env("UUD_UI_DUMP");
    this->debug_show_root = Parse_Bool_Env("UUD_UI_DEBUG_SHOW_ROOT");

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

    if (box->id == this->active)
        this->active = 0;
    if (box->id == this->hot)
        this->hot = 0;
    if (box->id == this->focused)
        this->focused = 0;
    if (box->id == this->drop_site)
        this->focused = 0;

    box->~UI_Box();
    new (box) UI_Box{};
    this->free_boxes.push_back(box);
}

void UI_Context::Push_ID(size_t seed) {
    size_t top = this->id_stack.empty() ? 0 : this->id_stack.top();
    this->id_stack.push(std::hash<size_t>{}(top ^ (seed + 0x9e3779b9 + (top << 6) + (top >> 2))));
}

void UI_Context::Pop_ID() {
    this->id_stack.pop();
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
                const bool is_toggle_key = event.key.key == SDLK_F8 || event.key.key == SDLK_F9 ||
                                           event.key.key == SDLK_F10;
                if (is_toggle_key && event.key.repeat)
                    break;

                if (event.key.key == SDLK_F8) {
                    this->debug_overlay_enabled = !this->debug_overlay_enabled;
                    SDL_Log("ui debug overlay: %s",
                            this->debug_overlay_enabled ? "enabled" : "disabled");
                    break;
                }
                if (event.key.key == SDLK_F9) {
                    this->debug_dump_enabled = !this->debug_dump_enabled;
                    SDL_Log("ui layout dump: %s", this->debug_dump_enabled ? "enabled" : "disabled");
                    break;
                }
                if (event.key.key == SDLK_F10) {
                    this->debug_dump_once_requested = true;
                    SDL_Log("ui layout dump: requested single frame");
                    break;
                }

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
                        if (box->allow_multiline)
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

std::string UI_Context::Source_Loc_Str(const std::source_location source_loc) {
    return std::string(source_loc.file_name()) + ":" + std::string(source_loc.function_name()) +
           ":" + std::to_string(source_loc.line()) + ":" + std::to_string(source_loc.column());
}

UI_Signal UI_Context::Box_Make(V2 fixed_pos, UI_Box_Flags flags,
                               std::optional<std::string> id_override,
                               const std::source_location source_loc) {
    size_t id = 0;
    size_t source_key = 0;
    size_t iteration = 0;
    UI_Box *box = NULL;
    bool box_exists = false;

    size_t id_seed = this->id_stack.empty() ? 0 : this->id_stack.top();

    if (id_override.has_value()) {
        id = std::hash<std::string>{}(id_override.value());
        if (id_seed)
            id ^= id_seed + 0x9e3779b9 + (id << 6) + (id >> 2);
        source_key = id;
        iteration = 0;
        if (auto it = this->boxes.find(id); it != this->boxes.end()) {
            box = it->second;
            box_exists = true;
        }
    } else {
        source_key = std::hash<std::string>{}(Source_Loc_Str(source_loc));
        if (id_seed)
            source_key ^= id_seed + 0x9e3779b9 + (source_key << 6) + (source_key >> 2);

        iteration = this->source_iteration_counter[source_key]++;
        id = std::hash<std::string>{}(Source_Loc_Str(source_loc) + "#" + std::to_string(iteration));
        if (id_seed)
            id ^= id_seed + 0x9e3779b9 + (id << 6) + (id >> 2);

        if (auto it = this->boxes.find(id); it != this->boxes.end()) {
            box = it->second;
            box_exists = true;
        }
    }

    if (box_exists && box->frame_last_touched == this->frame) {
        SDL_Log("duplicate box ID in same frame (id=%zu, override=%s) use Push_ID()",
                id, id_override.has_value() ? id_override.value().c_str() : "(none)");
        size_t dedup = id;
        do {
            dedup = std::hash<size_t>{}(dedup + 1);
        } while (this->boxes.count(dedup));
        box = this->Alloc_Box();
        this->boxes[dedup] = box;
        box->id = dedup;
        box->frame_created = this->frame;
        id = dedup;
    } else if (!box_exists) {
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

    // get from style stack or dont (hard code)

    // get from text align stack
    if (this->label_alignments.size() > 0) {
        box->label_alignment = this->label_alignments.top();
    }

    // semantic size or dont (just fixed size)
    // box->area = area;
    box->size = (this->sizes.size()) ? this->sizes.top() : G2<UI_Size>{};
    box->fixed_size = V2{-1, -1};
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
    while (!this->id_stack.empty())
        this->id_stack.pop();

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
    this->mouse_wheel = V2{0, 0};

    // layout
    this->Layout_Compute();
    this->Debug_Dump_Layout_JSON();

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
        return box->clip_ancestor_rect;

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

void UI_Context::Debug_Dump_Layout_JSON() {
    if (!this->debug_dump_enabled && !this->debug_dump_once_requested)
        return;

    std::error_code ec;
    std::filesystem::create_directories(this->debug_dump_dir, ec);
    if (ec) {
        SDL_Log("ui layout dump: could not create dir '%s' (%s)", this->debug_dump_dir.c_str(),
                ec.message().c_str());
        this->debug_dump_once_requested = false;
        return;
    }

    std::ostringstream frame_name;
    frame_name << this->debug_dump_dir << "/frame_" << std::setfill('0') << std::setw(8)
               << this->frame << ".json";

    std::ofstream file(frame_name.str(), std::ios::trunc);
    if (!file.is_open()) {
        SDL_Log("ui layout dump: could not open '%s' for writing", frame_name.str().c_str());
        this->debug_dump_once_requested = false;
        return;
    }

    file << "{\n";
    file << "  \"frame\": " << this->frame << ",\n";
    file << "  \"scene\": \"" << Scene_To_String(state.scene) << "\",\n";
    file << "  \"scene_id\": " << (int)state.scene << ",\n";
    file << "  \"window\": {\"width\": " << this->window_width << ", \"height\": "
         << this->window_height << "},\n";
    file << "  \"boxes\": [\n";

    bool first = true;
    for (UI_Box *box : this->Layout_It()) {
        if (box == &this->root && !this->debug_show_root)
            continue;

        if (!first)
            file << ",\n";
        first = false;

        const char *label = (box->label && box->label->text) ? box->label->text : "";
        const UI_ID parent_id = box->parent ? box->parent->id : 0;
        const bool is_clickable = (box->flags & UI_BOX_FLAG_CLICKABLE) != 0;
        const bool is_textinput = (box->flags & UI_BOX_FLAG_TEXTINPUT) != 0;
        const bool is_clip = (box->flags & UI_BOX_FLAG_CLIP) != 0;

        file << "    {\n";
        file << "      \"id\": " << box->id << ",\n";
        file << "      \"parent_id\": " << parent_id << ",\n";
        file << "      \"label\": \"" << Json_Escape(label) << "\",\n";
        file << "      \"flags\": " << box->flags << ",\n";
        file << "      \"is_clickable\": " << (is_clickable ? "true" : "false") << ",\n";
        file << "      \"is_textinput\": " << (is_textinput ? "true" : "false") << ",\n";
        file << "      \"is_clip\": " << (is_clip ? "true" : "false") << ",\n";
        file << "      \"has_clip_ancestor\": " << (box->has_clip_ancestor ? "true" : "false")
             << ",\n";
        file << "      \"area\": {\"x\": " << box->area.x << ", \"y\": " << box->area.y
             << ", \"w\": " << box->area.w << ", \"h\": " << box->area.h << "},\n";
        file << "      \"layout\": {\"x\": " << box->layout_box.x << ", \"y\": "
             << box->layout_box.y << ", \"w\": " << box->layout_box.w << ", \"h\": "
             << box->layout_box.h << "},\n";
        file << "      \"clip_ancestor\": ";
        if (box->has_clip_ancestor) {
            file << "{\"x\": " << box->clip_ancestor_rect.x << ", \"y\": "
                 << box->clip_ancestor_rect.y << ", \"w\": " << box->clip_ancestor_rect.w
                 << ", \"h\": " << box->clip_ancestor_rect.h << "}\n";
        } else {
            file << "null\n";
        }
        file << "    }";
    }
    file << "\n  ]\n";
    file << "}\n";
    file.close();

    if (this->debug_dump_once_requested) {
        SDL_Log("ui layout dump: wrote %s", frame_name.str().c_str());
    }
    this->debug_dump_once_requested = false;
}

void UI_Context::Debug_Render_Overlay(SDL_Renderer *renderer) {
    if (!renderer || !this->debug_overlay_enabled)
        return;

    SDL_SetRenderDrawBlendMode(renderer, SDL_BLENDMODE_BLEND);

    for (UI_Box *box : this->Layout_It()) {
        if (box == &this->root && !this->debug_show_root)
            continue;

        SDL_Color color = {0xCC, 0x66, 0xFF, 0xD0};
        if (box->flags & UI_BOX_FLAG_TEXTINPUT)
            color = {0x00, 0xCC, 0xFF, 0xD0};
        else if (box->flags & UI_BOX_FLAG_CLICKABLE)
            color = {0x66, 0xFF, 0x66, 0xD0};
        else if (box->flags & UI_BOX_FLAG_CLIP)
            color = {0xFF, 0xAA, 0x33, 0xD0};
        if (box == &this->root)
            color = {0xAA, 0xAA, 0xAA, 0xD0};

        SDL_SetRenderDrawColor(renderer, color.r, color.g, color.b, color.a);
        SDL_RenderRect(renderer, (SDL_FRect *)&box->area);

        SDL_FRect corner = {box->area.x, box->area.y, 3.0f, 3.0f};
        SDL_RenderFillRect(renderer, &corner);

        const bool clipped_outside =
            box->has_clip_ancestor &&
            (box->area.x < box->clip_ancestor_rect.x ||
             box->area.y < box->clip_ancestor_rect.y ||
             box->area.x + box->area.w > box->clip_ancestor_rect.x + box->clip_ancestor_rect.w ||
             box->area.y + box->area.h > box->clip_ancestor_rect.y + box->clip_ancestor_rect.h);
        if (clipped_outside) {
            SDL_SetRenderDrawColor(renderer, 0xFF, 0x33, 0x33, 0xDD);
            SDL_RenderRect(renderer, (SDL_FRect *)&box->clip_ancestor_rect);
        }

        char debug_text[192];
        const char *label = (box->label && box->label->text) ? box->label->text : "";
        if (label[0])
            SDL_snprintf(debug_text, sizeof(debug_text), "#%zu %.64s", box->id, label);
        else
            SDL_snprintf(debug_text, sizeof(debug_text), "#%zu", box->id);
        SDL_SetRenderDrawColor(renderer, 0xFF, 0xFF, 0x00, 0xFF);
        SDL_RenderDebugText(renderer, box->area.x + 2.0f, box->area.y + 2.0f, debug_text);
    }
}
