#include "widgets.hpp"

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

    UI_Signal sig = ui->Box_Make(fixed_pos, flags, {}, {}, source_loc);

    if (size.has_value())
        this->ui->sizes.pop();

    /*
    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata))
        data->flags = 0x00;
    */

    return sig;
}
UI_Signal Widget_Context::Div_Begin(std::optional<Rect> area,
                                    const std::source_location source_loc) {
    V2 fixed_pos = {};
    UI_Box_Flags flags = 0;
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, {}, {}, source_loc);
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

UI_Signal Widget_Context::Label(std::string label, std::optional<Rect> area,
                                std::optional<std::string> id_override,
                                const std::source_location source_loc) {
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_CLIP;
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, label, id_override, source_loc);
    if (area.has_value())
        ui->sizes.pop();

    if (sig.box->frame_created == ui->frame)
        sig.box->userdata = Widget_Data(this, Widget_Type::Label, {});
    return sig;
}

UI_Signal Widget_Context::Button(std::string label, std::optional<Rect> area,
                                 std::optional<std::string> id_override,
                                 const std::source_location source_loc) {
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP;
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, label, id_override, source_loc);
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

    return sig;
}

UI_Signal Widget_Context::Toggle(bool *toggle, std::string label, std::optional<Rect> area,
                                 std::optional<std::string> id_override,
                                 const std::source_location source_loc) {
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP;
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, label, id_override, source_loc);
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

    return sig;
}
UI_Signal Widget_Context::Slider(float *value, float min, float max, Widget_Slider_Dir dir,
                                 std::string label, std::optional<Rect> area,
                                 std::optional<std::string> id_override,
                                 const std::source_location source_loc) {
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP;
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, label, id_override, source_loc);
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
        data->u.slider = Widget_Slider_Data{*value, dir, min, max};
    } else {
        sig.box->userdata =
            Widget_Data(this, Widget_Type::Slider, Widget_Union{.slider = {*value, dir, min, max}});
    }

    return sig;
}

UI_Signal Widget_Context::Textbox(std::string init_label, std::optional<Rect> area,
                                  std::optional<std::string> id_override,
                                  const std::source_location source_loc) {
    V2 fixed_pos = {};
    UI_Box_Flags flags = UI_BOX_FLAG_TEXTINPUT | UI_BOX_FLAG_CLICKABLE | UI_BOX_FLAG_CLIP;
    if (area.has_value()) {
        flags |= UI_BOX_FLAG_FLOATING;

        ui->sizes.push(G2<UI_Size>{
            .x = UI_Size_Pixels(area.value().w, 1),
            .y = UI_Size_Pixels(area.value().h, 1),
        });
        fixed_pos = area.value().pos();
    }
    UI_Signal sig = ui->Box_Make(fixed_pos, flags, init_label, id_override, source_loc);
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

    return sig;
}

UI_Signal Widget_Context::Card(SDL_Texture *texture, std::optional<Rect> area,
                               std::optional<std::string> id_override,
                               const std::source_location source_loc) {
    UI_Signal sig = this->Button({}, area, id_override, source_loc);

    if (Widget_Data *data = std::any_cast<Widget_Data>(&sig.box->userdata)) {
        data->flags = 0x00;
        data->texture = texture;
    }

    return sig;
}
