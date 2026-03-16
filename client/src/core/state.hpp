#pragma once

#include <cstdint>
#include <string>

#include "resources.hpp"

namespace paths {
constexpr auto team_logo = "./res/logo/lizards_of_the_roast_logo-1-1.png";

constexpr auto beleren_bold = "./res/fonts/Beleren2016-Bold-Asterisk.ttf";
constexpr auto goudy_demibold = "./res/fonts/GoudyMediaeval-DemiBold.ttf";
constexpr auto goudy_regular = "./res/fonts/GoudyMediaeval-Regular.ttf";
constexpr auto goudy_alternate = "./res/fonts/GoudyMedieval-Alternate.ttf";
constexpr auto matrix_bold = "./res/fonts/Matrix-Bold.ttf";
constexpr auto mplantin_bold_italic = "./res/fonts/MPlantin-BoldItalic.ttf";
constexpr auto mplantin_bold = "./res/fonts/MPlantin-Bold.ttf";
constexpr auto mplantin_italic = "./res/fonts/MPlantin-Italic.ttf";
constexpr auto mplantin_regular = "./res/fonts/MPlantin-Regular.ttf";
constexpr auto ndpmtg = "./res/fonts/NDPMTG.ttf";

constexpr auto bg_texture = "./res/textures/bg.jpg";
constexpr auto card_texture = "./res/textures/card.png";
constexpr auto crack_texture = "./res/textures/gimp_crack.png";
}  // namespace paths

enum class Scene {
    Intro = 0,
    Login,
    Main_Menu,
    Match,
    Exit,
};

struct State {
    SDL_Window *window;
    int window_width, window_height;
    SDL_Renderer *renderer;

    Scene scene = Scene::Intro;

    Texture_Resource texture;
    Font_Resource font;

    Uint64 tick;
    double delta_time;

#ifdef OFFLINE_MODE
    bool offline = true;
#else
    bool offline = false;
#endif

    uint64_t user_id = 0;
    std::string username;
    std::string server_address = "localhost:50051";  // this can be made dynamic if needed
    std::string current_game_id;
    bool joined_via_matchmaking = false;
    std::string selected_deck_name;

    void Update_Delta_Time();
};
extern State state;
