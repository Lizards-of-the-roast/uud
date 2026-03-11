#include <iostream>

#include "core/defer.hpp"
#include "core/state.hpp"
#include "scenes/intro/intro.hpp"
#include "scenes/login/login.hpp"
#include "scenes/match/match.hpp"
#include "scenes/menu/menu.hpp"
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>

int main(void) {
    ////////////////////////
    // INIT

    // SDL
    if (!SDL_Init(SDL_INIT_VIDEO)) {
        std::cerr << "ERROR: " << "Couldnt Init SDL" << SDL_GetError() << '\n';
        return 1;
    }
    defer(SDL_Quit());

    // Window
    state.window = SDL_CreateWindow("UUD", 1280, 720, SDL_WINDOW_RESIZABLE);
    if (!state.window) {
        std::cerr << "ERROR: " << "Couldnt Create Window" << SDL_GetError() << '\n';
        return 1;
    }
    defer(SDL_DestroyWindow(state.window));
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
    if (!state.renderer) {
        std::cerr << "ERROR: " << "Couldnt Create Renderer" << SDL_GetError() << '\n';
        return 1;
    }
    defer(SDL_DestroyRenderer(state.renderer));
    SDL_Log("Created Renderer: %s", SDL_GetRendererName(state.renderer));

    SDL_SetRenderDrawBlendMode(state.renderer, SDL_BLENDMODE_BLEND);

    // Enable vsync on SDL_RenderPresent
    // 1: sync on every frame
    SDL_SetRenderVSync(state.renderer, 1);

    // TTF
    if (!TTF_Init()) {
        std::cerr << "ERROR: couldnt init SDL_TTF: " << SDL_GetError() << '\n';
        return 1;
    }

    defer(state.font.Clear();  // free fonts before closing TTF
          TTF_Quit(););

    // fail so ui can assume its not null
    if (!state.font[paths::beleren_bold]) {
        std::cerr << "couldnt load font '" << paths::beleren_bold << "': " << SDL_GetError()
                  << '\n';
        return 1;
    }
    if (!state.font[paths::matrix_bold]) {
        std::cerr << "couldnt load font '" << paths::matrix_bold << "': " << SDL_GetError() << '\n';
        return 1;
    }
    if (!state.font[paths::mplantin_regular]) {
        std::cerr << "couldnt load font '" << paths::mplantin_regular << "': " << SDL_GetError()
                  << '\n';
        return 1;
    }

    // skip intro for testing
#ifndef NDEBUG
    //state.scene = state.offline ? Scene::Main_Menu : Scene::Intro;
    state.current_game_id = "local";
    state.scene = Scene::Match;
#endif
    ////////////////////////
    // Loop
    for (;;) {
        switch (state.scene) {
            case Scene::Intro:
                if (!Scene_Intro())
                    return 1;
                break;
            case Scene::Login:
                if (!Scene_Login())
                    return 1;
                break;
            case Scene::Main_Menu:
                if (!Scene_Menu())
                    return 1;
                break;
            case Scene::Match:
                if (!Scene_Match())
                    return 1;
                break;
            case Scene::Exit:
                return 0;
        }
    }
    return 0;
}
