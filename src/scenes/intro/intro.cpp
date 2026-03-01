#include "intro.hpp"

#include "core/defer.hpp"
#include "core/state.hpp"
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>

/*
Fade In to single centered texture
ARGS:
    texture: valid texture
    start_time: start time in milliseconds returned by SDL_GetTicks();
    bg: background color
    fade_time: animation time for fade in (seconds)
    delay_time: time to wait after fade in before (seconds)
RETURNS:
    false: on animating
    true: on done
*/
bool Intro_Render_Fade_In(SDL_Texture *texture, Uint64 start_time, SDL_Color bg, float fade_time,
                          float delay_time) {
    float anim_t = (float)(SDL_GetTicks() - start_time) / 1000.0f / fade_time;
    if (anim_t > fade_time + delay_time)
        return true;
    anim_t = SDL_min(anim_t, 1.0f);

    SDL_SetRenderDrawColor(state.renderer, bg.r * anim_t, bg.g * anim_t, bg.b * anim_t, bg.a);
    SDL_RenderClear(state.renderer);

    SDL_SetTextureAlphaModFloat(texture, anim_t);

    float size =
        (state.window_width < state.window_height) ? state.window_width : state.window_height;
    SDL_FRect dst = {(float)state.window_width / 2.0f - size / 2.0f,
                     (float)state.window_height / 2.0f - size / 2.0f, size, size};
    SDL_RenderTexture(state.renderer, texture, NULL, &dst);
    return false;
}

bool Intro(void) {
    Uint64 start_time = SDL_GetTicks();

    SDL_SetTextureBlendMode(state.texture[paths::team_logo], SDL_BLENDMODE_MOD);
    // probably dont need to do this
    defer(state.texture.Destroy(paths::team_logo));

    bool render_fade_in = true;

    for (;;) {
        state.Update_Delta_Time();

        for (SDL_Event event; SDL_PollEvent(&event);)
            switch (event.type) {
                case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                case SDL_EVENT_QUIT:
                    state.scene = Scene::Exit;
                    return true;
                case SDL_EVENT_WINDOW_RESIZED:
                    SDL_GetWindowSize(state.window, &state.window_width, &state.window_height);
                    break;
            }

        SDL_SetRenderDrawColor(state.renderer, 0x00, 0x00, 0x00, 0x00);
        SDL_RenderClear(state.renderer);

        if (render_fade_in)
            render_fade_in = !Intro_Render_Fade_In(state.texture[paths::team_logo], start_time,
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
