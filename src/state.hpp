#ifndef _STATE_HPP_
#define _STATE_HPP_

#include "resources.hpp"
#include "widgets.hpp"

#define RES_PATH "./res/"
#define LOGO_PATH RES_PATH"logo/"
#define TEAM_LOGO_PATH LOGO_PATH"lizards_of_the_roast_logo-1-1.png"

#define FONT_PATH RES_PATH"fonts/"
#define FONT_BELEREN_BOLD_PATH              FONT_PATH"Beleren2016-Bold-Asterisk.ttf"
#define FONT_GOUDY_MEDIAEVAL_DEMIBOLD_PATH  FONT_PATH"GoudyMediaeval-DemiBold.ttf"
#define FONT_GOUDY_MEDIAEVAL_REGULAR_PATH   FONT_PATH"GoudyMediaeval-Regular.ttf"
#define FONT_GOUDY_MEDIAEVAL_ALTERNATE_PATH FONT_PATH"GoudyMedieval-Alternate.ttf"
#define FONT_MATRIX_BOLD_PATH               FONT_PATH"Matrix-Bold.ttf"
#define FONT_MPLANTIN_BOLD_ITALIC_PATH      FONT_PATH"MPlantin-BoldItalic.ttf"
#define FONT_MPLANTIN_BOLD_PATH             FONT_PATH"MPlantin-Bold.ttf"
#define FONT_FONT_MPLANTIN_ITALIC_PATH      FONT_PATH"MPlantin-Italic.ttf"
#define FONT_FONT_MPLANTIN_REGULAR_PATH     FONT_PATH"MPlantin-Regular.ttf"
#define FONT_NDPMTG_PATH                    FONT_PATH"NDPMTG.ttf"

#define TEXTURE_PATH RES_PATH"textures/"
#define TEXTURE_BG_PATH TEXTURE_PATH"bg.jpg"
#define TEXTURE_CARD_PATH TEXTURE_PATH"card.png"

enum Game_State {
    GAME_STATE_INTRO = 0,
    GAME_STATE_MAIN_MENU,
    GAME_STATE_MATCH,
    GAME_STATE_EXIT,
};
struct State {
    // Just one window
    SDL_Window *window;
    int window_width, window_height;
    // For now use SDL's simple renderer to get something that works
    // If there is time change to either SDL_GPU or some other rendering api
    SDL_Renderer *renderer;

    Game_State game_state;

    Texture_Resource texture;
    Font_Resource font;

    Uint64 tick;
    double delta_time;
};
extern State state;

void Get_Delta_Time(double *delta_time, Uint64 *tick);
void Get_Delta_Time(void);

#endif //ifndef _STATE_HPP_
