#include "widgets.hpp"

#include "game/textures.hpp"
#include "game/instances.hpp"

Widget_Context::Widget_Context(SDL_Renderer *renderer, UI_Context *context) {
    this->renderer = renderer;
    this->ui = context;

    this->styles = {};

    this->default_style[static_cast<size_t>(Widget_Style_State::Default)] = {
        .background = {0x4C, 0x4C, 0x4C, 0xFF},
        .border = {0x3F, 0x3F, 0x3F, 0xFF},
    };
    this->default_style[static_cast<size_t>(Widget_Style_State::Hovering)] = {
        .background = {0x7F, 0x7F, 0x7F, 0xFF},
        .border = {0x3F, 0x3F, 0x3F, 0xFF},
    };
    this->default_style[static_cast<size_t>(Widget_Style_State::Pressed)] = {
        .background = {0xCC, 0xCC, 0xCC, 0xFF},
        .border = {0x3F, 0x3F, 0x3F, 0xFF},
    };
    this->default_style[static_cast<size_t>(Widget_Style_State::Released)] = {
        .background = {0xFF, 0xFF, 0xFF, 0xFF},
        .border = {0x3F, 0x3F, 0x3F, 0xFF},
    };
}

Widget_Data::~Widget_Data() {}

UI_Signal Widget_Context::Spacer(std::optional<UI_Size> size,
                                 const std::source_location source_loc) {
    V2 fixed_pos = {};
    UI_Box_Flags flags = 0;

    if (size.has_value()) {
        G2<UI_Size> current_size = (this->ui->sizes.size()) ? this->ui->sizes.top() : G2<UI_Size>{};
        int layout_axis = (this->ui->parents.size()) ? this->ui->parents.top()->child_layout_axis
                                                     : this->ui->root.child_layout_axis;
        current_size[layout_axis] = size.value();
        this->ui->sizes.push(current_size);
    }

    UI_Signal sig = ui->Box_Make(fixed_pos, flags, {}, source_loc);
    // this->Set_Text(&sig.box->label, label);

    if (size.has_value())
        this->ui->sizes.pop();

    /*
    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata))
        data->flags = 0x00;
    */

    return sig;
}
//TODO: optionally pass v2 instead of rect for just fixed_pos + floating
UI_Signal Widget_Context::Div_Begin(std::optional<Rect> area, UI_Box_Flags flags,
                                    std::optional<std::string> id_override,
                                    const std::source_location source_loc) {
    V2 fixed_pos = {};
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, id_override, source_loc);
    if (area.has_value())
        ui->sizes.pop();

    /*
    sig.box->userdata = Widget_Data{
        .flags = (this->default_flags_override.size())
               ? this->default_flags_override.top()
               : 0xFF,
        .type = Widget_Type::Div
    };
    */
    if (sig.box->frame_created == ui->frame)
        sig.box->userdata = Widget_Data(this, Widget_Type::Div, Widget_Union{});

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata)) {
        data->flags = 0x00;
    }

    ui->parents.push(sig.box);

    return sig;
}
void Widget_Context::Div_End() {
    ui->parents.pop();
}
UI_Signal Widget_Context::Scroll_Begin(int axis, bool hide, std::optional<Rect> area, UI_Box_Flags flags,
                                       std::optional<std::string> id_override,
                                       const std::source_location source_loc) {
    std::string id_base = id_override.value_or(ui->Source_Loc_Str(source_loc));
    Div_Begin(area, {}, "Scroll_div[" + id_base).box->child_layout_axis = !axis;
    // TODO: better sizing for scroll bars
    ui->sizes.push({UI_Size_Parent(1.0f), UI_Size_Parent(1.0f)});
    UI_Signal sig = Div_Begin({}, flags | (UI_BOX_FLAG_VIEW_SCROLL_X << axis), id_base);
    sig.box->margin = {0};
    sig.box->child_layout_axis = axis;
    float max_offset = SDL_max(0, sig.box->view_bounds[axis] - sig.box->area.size()[axis]);
    if (!hide || max_offset > 0)
        sig.box->size[!axis] = UI_Size_Parent(0.95f);
    return sig;
}
// TODO: not have axis need to be passed twice
void Widget_Context::Scroll_End(int axis, bool hide, std::optional<std::string> id_override,
                                const std::source_location source_loc) {
    std::string id_base = id_override.value_or(ui->Source_Loc_Str(source_loc));

    UI_Box *scroll_div = ui->parents.top();
    Div_End();
    ui->sizes.pop();

    float tmp = -scroll_div->view_offset[axis];
    float max_offset = SDL_max(0, scroll_div->view_bounds[axis] - scroll_div->area.size()[axis]);
    if (!hide || max_offset > 0)
    {
        G2<UI_Size> size;
        size[axis] = UI_Size_Parent(1.0f);
        size[!axis] = UI_Size_Parent(0.05f);
        ui->sizes.push(size);
        // TODO: slider styles
        UI_Signal slider =
            Slider(&tmp, 0, max_offset, (axis) ? Widget_Slider_Dir::UTD : Widget_Slider_Dir::LTR, Widget_Slider_Style::SCROLL, {},
                {}, UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP, "Scroll_Slider[" + id_base);
        ui->sizes.pop();
        if (Widget_Data *widget = std::any_cast<Widget_Data>(&slider.box->userdata))
            widget->u.slider.scroll_size = slider.box->area.size()[axis] * (scroll_div->area.size()[axis] / scroll_div->view_bounds[axis]);
        //slider.box->margin = {0};
        float delta = (-max_offset) - tmp;
        if (delta > scroll_div->scroll_step || delta < -scroll_div->scroll_step)
            scroll_div->view_offset[axis] = -tmp;
    }

    Div_End();
}
UI_Signal Widget_Context::Window_Begin(Rect area, bool *should_close, std::string title,
                                       UI_Box_Flags flags, std::optional<std::string> id_override,
                                       const std::source_location source_loc) {
    const float size_v = 20.0f;
    std::string id_base = id_override.value_or(ui->Source_Loc_Str(source_loc));
    const std::string id_win = "Window_div[" + id_base;
    UI_Box *win = ui->Get_Box(std::hash<std::string>{}(id_win));
    bool win_existed = win != NULL;
    if (win_existed)
        for (int i = 0; i < 2; i++) {
            area[i] = win->fixed_position[i];
            area[2 + i] = win->layout_box.size()[i];
        }
    win = Div_Begin(area, UI_BOX_FLAG_FLOATING | UI_BOX_FLAG_CLIP, id_win).box;
    win->margin = {0};
    win->child_layout_axis = 1;

    ui->sizes.push({UI_Size_Parent(1.0), UI_Size_Pixels(size_v)});
    Div_Begin({}, {}, "Window_Title_Div[" + id_base).box->margin = {0};
    ui->sizes.push({UI_Size_Pixels(win->area.w - size_v), UI_Size_Parent(1.0)});
    UI_Signal title_sig =
        Label(title, {}, UI_BOX_FLAG_CLIP | UI_BOX_FLAG_CLICKABLE, "Window_Title_Label[" + id_base);
    title_sig.box->margin = {0};
    ui->sizes.pop();
    ui->sizes.push({UI_Size_Pixels(size_v), UI_Size_Parent(1.0)});
    UI_Signal close_button = Button("X", {}, UI_BOX_FLAG_CLIP | UI_BOX_FLAG_CLICKABLE, "Window_Title_Button[" + id_base);
    close_button.box->margin = {0};
    ui->sizes.pop();
    Div_End();
    ui->sizes.pop();

    if (close_button.flags & UI_SIG_LEFT_PRESSED)
        *should_close = true;
    else
        *should_close = false;
    // TODO: dont use static
    //       technically it should be fine; cant drag 2 windows
    //       at once, but ew
    static V2 mouse_rel;
    if (title_sig.flags & UI_SIG_LEFT_PRESSED)
        mouse_rel = title_sig.mouse_pos - win->layout_box.pos();

    if (title_sig.flags & UI_SIG_LEFT_DOWN)
        win->fixed_position = title_sig.mouse_pos - win->parent->layout_box.pos() - mouse_rel;

    ui->sizes.push({UI_Size_Parent(1.0), UI_Size_Pixels(win->area.h - size_v)});
    UI_Signal sig = Div_Begin({}, flags, id_base);
    ui->sizes.pop();
    return sig;
}
void Widget_Context::Window_End() {
    Div_End();
    Div_End();
}

UI_Signal Widget_Context::Label(std::string label, std::optional<Rect> area, UI_Box_Flags flags,
                                std::optional<std::string> id_override,
                                const std::source_location source_loc) {
    V2 fixed_pos = {};
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, id_override, source_loc);
    if (area.has_value())
        ui->sizes.pop();

    if (sig.box->frame_created == ui->frame)
        sig.box->userdata = Widget_Data(this, Widget_Type::Label, {});

    sig.box->Text_Create(
        ui, label, this->Get_Style(sig.box, std::any_cast<Widget_Data>(&sig.box->userdata)).text);

    return sig;
}

UI_Signal Widget_Context::Button(std::string label, std::optional<Rect> area,
                                 UI_Box_Flags flags,
                                 std::optional<std::string> id_override,
                                 const std::source_location source_loc) {
    V2 fixed_pos = {};
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, id_override, source_loc);
    if (area.has_value())
        ui->sizes.pop();

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata);
        data && data->type == Widget_Type::Button) {
        data->style = (this->styles.size()) ? this->styles.top() : this->default_style;
        data->flags =
            (this->default_flags_override.size()) ? this->default_flags_override.top() : 0xFF;
    } else {
        sig.box->userdata = Widget_Data(this, Widget_Type::Button, {});
    }

    sig.box->Text_Create(
        ui, label, this->Get_Style(sig.box, std::any_cast<Widget_Data>(&sig.box->userdata)).text);

    return sig;
}

UI_Signal Widget_Context::Toggle(bool *toggle, std::string label, std::optional<Rect> area,
                                 UI_Box_Flags flags,
                                 std::optional<std::string> id_override,
                                 const std::source_location source_loc) {
    V2 fixed_pos = {};
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, id_override, source_loc);
    if (area.has_value())
        ui->sizes.pop();

    if (sig.flags & UI_SIG_RELEASED)
        *toggle = !*toggle;

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata);
        data && data->type == Widget_Type::Toggle) {
        data->style = (this->styles.size()) ? this->styles.top() : this->default_style;
        data->flags =
            (this->default_flags_override.size()) ? this->default_flags_override.top() : 0xFF;
        data->u.toggle.toggle_state = *toggle;
    } else {
        sig.box->userdata =
            Widget_Data(this, Widget_Type::Toggle, (Widget_Union)Widget_Toggle_Data{*toggle});
    }

    sig.box->Text_Create(
        ui, label, this->Get_Style(sig.box, std::any_cast<Widget_Data>(&sig.box->userdata)).text);
    return sig;
}
UI_Signal Widget_Context::Slider(float *value, float min, float max, Widget_Slider_Dir dir, Widget_Slider_Style style,
                                 std::string label, std::optional<Rect> area,
                                 UI_Box_Flags flags,
                                 std::optional<std::string> id_override,
                                 const std::source_location source_loc) {
    V2 fixed_pos = {};
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, id_override, source_loc);
    if (area.has_value())
        ui->sizes.pop();

    if (sig.flags & UI_SIG_DOWN) {
        float v = 0.0f;
        switch (dir) {
            case Widget_Slider_Dir::LTR:
                v = (sig.mouse_pos.x - sig.box->area.x) / sig.box->area.w;
                break;
            case Widget_Slider_Dir::RTL:
                v = (sig.box->area.x + sig.box->area.w - sig.mouse_pos.x) / sig.box->area.w;
                break;
            case Widget_Slider_Dir::UTD:
                v = (sig.mouse_pos.y - sig.box->area.y) / sig.box->area.h;
                break;
            case Widget_Slider_Dir::DTU:
                v = (sig.box->area.y + sig.box->area.h - sig.mouse_pos.y) / sig.box->area.h;
                break;
        }
        v = SDL_clamp(v, 0.0f, 1.0f);
        *value = min + (max - min) * v;
    }

    // sig.box->userdata = Widget_Slider_Data{*value, dir, min, max};
    /*
    Widget_Data d = { .type = Widget_Type::Slider };
    d.u.slider = Widget_Slider_Data{*value, dir, min, max};
    sig.box->userdata = d;
    */
    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata);
        data && data->type == Widget_Type::Slider) {
        data->style = (this->styles.size()) ? this->styles.top() : this->default_style;
        data->flags =
            (this->default_flags_override.size()) ? this->default_flags_override.top() : 0xFF;
        data->u.slider = Widget_Slider_Data{*value, dir, style, min, max};
    } else {
        sig.box->userdata =
            Widget_Data(this, Widget_Type::Slider, Widget_Union{.slider = {*value, dir, style, min, max}});
    }

    sig.box->Text_Create(
        ui, label, this->Get_Style(sig.box, std::any_cast<Widget_Data>(&sig.box->userdata)).text);

    return sig;
}

UI_Signal Widget_Context::Textbox(std::string init_label, std::optional<Rect> area,
                                  UI_Box_Flags flags,
                                  std::optional<std::string> id_override,
                                  const std::source_location source_loc) {
    V2 fixed_pos = {};
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, id_override, source_loc);
    if (area.has_value())
        ui->sizes.pop();

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata);
        data && data->type == Widget_Type::Textbox) {
        data->style = (this->styles.size()) ? this->styles.top() : this->default_style;
        data->flags =
            (this->default_flags_override.size()) ? this->default_flags_override.top() : 0xFF;
    } else {
        sig.box->userdata = Widget_Data(this, Widget_Type::Textbox, {});
    }

    if (!sig.box->label) {
        sig.box->Text_Create(
            ui, init_label,
            this->Get_Style(sig.box, std::any_cast<Widget_Data>(&sig.box->userdata)).text);
    }

    // NOTE: size is still slightly too small, but I'm not sure how to fix that
    //       without a sketchy TTF_GetStringSize() call.
    //       adding TTF_GetFontAscent(font) to the hight seeming makes it slightly too big
    //       too
    if (sig.box->size.y.type == UI_SIZE_TEXT_CONTENT && !sig.box->min_size.y) {
        TTF_Font *font = TTF_GetTextFont(sig.box->label);
        if (font)
            sig.box->min_size.y = (float)(TTF_GetFontHeight(font)) + sig.box->size.y.value * 2;
    }

    return sig;
}

UI_Signal Widget_Context::Card(Game::Card card, std::optional<Rect> area,
                               UI_Box_Flags flags,
                               std::optional<std::string> id_override,
                               const std::source_location source_loc) {
    UI_Signal sig = this->Button({}, area, flags, id_override, source_loc);

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata)) {
        data->flags = 0x00;
        data->texture = Game::card_textures.Get(card.name);
    }

    return sig;
}

UI_Signal Widget_Context::Card_Overlayed(Game::Card_ID card, Game::Permanent_ID perm, std::optional<Rect> area,
                                UI_Box_Flags flags,
                                std::optional<std::string> id_override,
                                const std::source_location source_loc){
    V2 fixed_pos = {};
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, id_override, source_loc);
    if (area.has_value())
        ui->sizes.pop();

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata);
        data && data->type == Widget_Type::Card) {
        data->style = (this->styles.size()) ? this->styles.top() : this->default_style;
        data->flags =
            (this->default_flags_override.size()) ? this->default_flags_override.top() : 0xFF;
        data->u.card.card = card;
        data->u.card.perm = perm;
        data->flags = 0x00;
        if (const Game::Card *c = Game::instances.Find(card))
            data->texture = Game::card_textures.Get(c->name);
    } else {
        sig.box->userdata =
            Widget_Data(this, Widget_Type::Card, Widget_Union{ .card = {card, perm} });
    }
    return sig;
}
