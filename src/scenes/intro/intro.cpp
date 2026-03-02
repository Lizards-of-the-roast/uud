#include "intro.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include "fade_in.hpp"
#include "scenes/common/scene_helpers.hpp"
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>

bool Scene_Intro(void) {
    Uint64 start_time = SDL_GetTicks();

    SDL_SetTextureBlendMode(state.texture[paths::team_logo], SDL_BLENDMODE_MOD);
    // probably dont need to do this
    defer(state.texture.Destroy(paths::team_logo));

    bool render_fade_in = true;

    for (;;) {
        state.Update_Delta_Time();

        for (SDL_Event event; SDL_PollEvent(&event);)
            if (Handle_Window_Event(event))
                return true;

        SDL_SetRenderDrawColor(state.renderer, 0x00, 0x00, 0x00, 0x00);
        SDL_RenderClear(state.renderer);

        if (render_fade_in)
            render_fade_in = !Render_Fade_In(state.texture[paths::team_logo], start_time,
                                                   {0xFF, 0xFF, 0xFF, 0xFF}, 1.0f, 2.5f);
        else {
            state.scene = Scene::Main_Menu;
            return true;
        }

        SDL_RenderPresent(state.renderer);
    }
    state.scene = Scene::Exit;
    return false;
}
