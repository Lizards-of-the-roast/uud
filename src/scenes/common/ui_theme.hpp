#pragma once

#include "ui/widgets/widgets.hpp"
#include "core/state.hpp"

namespace theme {

inline std::array<Widget_Style, WIDGET_STYLE_COUNT> Button_Primary(TTF_Font *font = NULL) {
    return {{
        {.background = {0x8B, 0x6F, 0x2E, 0xFF},
         .border = {0xBF, 0x9B, 0x30, 0xFF},
         .text = {
             .color = SDL_Color{0xFE, 0xF4, 0xD0, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
         }
        },
        {.background = {0xA8, 0x88, 0x3A, 0xFF},
         .border = {0xD4, 0xAF, 0x37, 0xFF},
         .text = {
             .color = SDL_Color{0xFF, 0xFF, 0xF0, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
         }
        },
        {.background = {0x6B, 0x54, 0x20, 0xFF},
         .border = {0x9B, 0x7B, 0x28, 0xFF},
         .text = {
             .color = SDL_Color{0xCC, 0xBB, 0x88, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
         }
        },
        {.background = {0xD4, 0xAF, 0x37, 0xFF},
         .border = {0xE8, 0xC8, 0x50, 0xFF},
         .text = {
             .color = SDL_Color{0xFF, 0xFF, 0xFF, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
         }
        },
    }};
}

inline std::array<Widget_Style, WIDGET_STYLE_COUNT> Button_Secondary(TTF_Font *font = NULL) {
    return {{
        {.background = {0x3A, 0x3A, 0x42, 0xFF},
         .border = {0x55, 0x55, 0x60, 0xFF},
         .text = {
             .color = SDL_Color{0xCC, 0xCC, 0xD0, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
        {.background = {0x50, 0x50, 0x58, 0xFF},
         .border = {0x70, 0x70, 0x78, 0xFF},
         .text = {
             .color = SDL_Color{0xEE, 0xEE, 0xF0, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
        {.background = {0x28, 0x28, 0x30, 0xFF},
         .border = {0x44, 0x44, 0x4C, 0xFF},
         .text = {
             .color = SDL_Color{0x99, 0x99, 0xA0, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
        {.background = {0x66, 0x66, 0x70, 0xFF},
         .border = {0x88, 0x88, 0x90, 0xFF},
         .text = {
             .color = SDL_Color{0xFF, 0xFF, 0xFF, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
    }};
}

inline std::array<Widget_Style, WIDGET_STYLE_COUNT> Button_Danger(TTF_Font *font = NULL) {
    return {{
        {.background = {0x6B, 0x1C, 0x1C, 0xFF},
         .border = {0x8B, 0x2C, 0x2C, 0xFF},
         .text = {
             .color = SDL_Color{0xFF, 0xCC, 0xCC, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
        {.background = {0x8B, 0x2C, 0x2C, 0xFF},
         .border = {0xAA, 0x33, 0x33, 0xFF},
         .text = {
             .color = SDL_Color{0xFF, 0xDD, 0xDD, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
        {.background = {0x4B, 0x10, 0x10, 0xFF},
         .border = {0x6B, 0x1C, 0x1C, 0xFF},
         .text = {
             .color = SDL_Color{0xCC, 0x88, 0x88, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
        {.background = {0xAA, 0x33, 0x33, 0xFF},
         .border = {0xCC, 0x44, 0x44, 0xFF},
         .text = {
             .color = SDL_Color{0xFF, 0xFF, 0xFF, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
    }};
}

inline std::array<Widget_Style, WIDGET_STYLE_COUNT> Textbox(TTF_Font *font = NULL) {
    return {{
        {.background = {0x12, 0x12, 0x1A, 0xFF},
         .border = {0x55, 0x55, 0x60, 0xFF},
         .text = {
             .color = SDL_Color{0xDD, 0xDD, 0xDD, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
        {.background = {0x18, 0x18, 0x22, 0xFF},
         .border = {0x8B, 0x6F, 0x2E, 0xFF},
         .text = {
             .color = SDL_Color{0xEE, 0xEE, 0xEE, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
        {.background = {0x18, 0x18, 0x22, 0xFF},
         .border = {0xBF, 0x9B, 0x30, 0xFF},
         .text = {
             .color = SDL_Color{0xFF, 0xFF, 0xFF, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
        {.background = {0x18, 0x18, 0x22, 0xFF},
         .border = {0xD4, 0xAF, 0x37, 0xFF},
         .text = {
             .color = SDL_Color{0xFF, 0xFF, 0xFF, 0xFF},
             .font = (font) ? font : state.font[paths::beleren_bold]
          }
        },
    }};
}

inline std::array<Widget_Style, WIDGET_STYLE_COUNT> Label_Title(TTF_Font *font = NULL) {
    std::array<Widget_Style, WIDGET_STYLE_COUNT> s;
    for (auto &st : s) {
        st.background = {0x00, 0x00, 0x00, 0x00};
        st.border = {0x00, 0x00, 0x00, 0x00};
        st.text = {
            .color = SDL_Color{0xD4, 0xAF, 0x37, 0xFF},
            .font = (font) ? font : state.font[paths::beleren_bold]
        };
    }
    return s;
}

inline std::array<Widget_Style, WIDGET_STYLE_COUNT> Label_Body(TTF_Font *font = NULL) {
    std::array<Widget_Style, WIDGET_STYLE_COUNT> s;
    for (auto &st : s) {
        st.background = {0x00, 0x00, 0x00, 0x00};
        st.border = {0x00, 0x00, 0x00, 0x00};
        st.text = {
            .color = SDL_Color{0xCC, 0xCC, 0xD0, 0xFF},
            .font = (font) ? font : state.font[paths::mplantin_regular]
        };
    }
    return s;
}

inline std::array<Widget_Style, WIDGET_STYLE_COUNT> Panel(TTF_Font *font = NULL) {
    std::array<Widget_Style, WIDGET_STYLE_COUNT> s;
    for (auto &st : s) {
        st.background = {0x1A, 0x1A, 0x24, 0xBB};
        st.border = {0x8B, 0x6F, 0x2E, 0x66};
    }
    return s;
}

inline std::array<Widget_Style, WIDGET_STYLE_COUNT> Panel_Inner() {
    std::array<Widget_Style, WIDGET_STYLE_COUNT> s;
    for (auto &st : s) {
        st.background = {0x22, 0x20, 0x2A, 0xAA};
        st.border = {0x55, 0x55, 0x60, 0x44};
    }
    return s;
}

constexpr SDL_Color TEXT_ERROR = {0xFF, 0x55, 0x55, 0xFF};
constexpr SDL_Color TEXT_SUCCESS = {0x55, 0xDD, 0x55, 0xFF};
constexpr SDL_Color TEXT_INFO = {0x88, 0xBB, 0xEE, 0xFF};
constexpr SDL_Color TEXT_GOLD = {0xD4, 0xAF, 0x37, 0xFF};

constexpr SDL_Color SCENE_BG = {0x0A, 0x0B, 0x1A, 0xFF};

inline void Apply_Panel(UI_Box *box, const std::array<Widget_Style, WIDGET_STYLE_COUNT> &style) {
    if (Widget_Data *data = std::any_cast<Widget_Data>(&box->userdata)) {
        data->style = style;
        data->flags = WIDGET_FLAG_DRAW_BACKGROUND | WIDGET_FLAG_DRAW_BORDER;
    }
}

}  // namespace theme
