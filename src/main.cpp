#include <iostream>
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>

#include "state.hpp"
#include "intro.hpp"
#include "menu.hpp"
#include "match.hpp"
#include "defer.hpp"

int main(void)
{
    ////////////////////////
    // INIT

    // SDL
    if (!SDL_Init(SDL_INIT_VIDEO))
    {
        std::cerr << "ERROR: " << "Couldnt Init SDL" << SDL_GetError() << '\n';
        return 1;
    }
    defer (SDL_Quit());

    // Window
    state.window = SDL_CreateWindow("UUD", 1280, 720, SDL_WINDOW_RESIZABLE);
    if (!state.window)
    {
        std::cerr << "ERROR: " << "Couldnt Create Window" << SDL_GetError() << '\n';
        return 1;
    }
    defer (SDL_DestroyWindow(state.window));
    SDL_GetWindowSize(state.window, &state.window_width, &state.window_height);

    // Renderer
    /*
    SDL_Log("Available Renderers:");
    for (int i = 0; i < SDL_GetNumRenderDrivers(); i++)
    {
        SDL_Log("\t%s", SDL_GetRenderDriver(i));
    }
    */

    state.renderer = SDL_CreateRenderer(state.window, NULL);
    if (!state.renderer)
    {
        std::cerr << "ERROR: " << "Couldnt Create Renderer" << SDL_GetError() << '\n';
        return 1;
    }
    defer (SDL_DestroyRenderer(state.renderer));
    SDL_Log("Created Renderer: %s", SDL_GetRendererName(state.renderer));

    SDL_SetRenderDrawBlendMode(state.renderer, SDL_BLENDMODE_BLEND);

    // Enable vsync on SDL_RenderPresent
    // 1: sync on every frame
    SDL_SetRenderVSync(state.renderer, 1);


    // TTF 
    if (!TTF_Init())
    {
        std::cerr << "ERROR: couldnt init SDL_TTF: " << SDL_GetError() << '\n';
        return 1;
    }

    defer (TTF_Quit());

    // fail so ui can assume its not null
    if (!state.font[FONT_BELEREN_BOLD_PATH])
    {
        std::cerr << "couldnt load font '" << FONT_BELEREN_BOLD_PATH << "': " << SDL_GetError() << '\n';
        return 1;
    }

    // skip intro for testing
#ifndef NDEBUG
    state.game_state = GAME_STATE_MAIN_MENU;
#endif
    ////////////////////////
    // Loop
    for (;;)
    {
        switch (state.game_state)
        {
            case GAME_STATE_INTRO: //Maybe this should be the same as main_menu
                if (!Intro())
                    return 1;
                break;
            case GAME_STATE_MAIN_MENU:
                if (!Main_Menu())
                    return 1;
                break;
            case GAME_STATE_MATCH:
                if (!Match())
                    return 1;
                return 0;
            case GAME_STATE_EXIT:
                return 0;
        }
    }

    return 0;
}
