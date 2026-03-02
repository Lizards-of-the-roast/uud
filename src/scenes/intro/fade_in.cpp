#include "fade_in.hpp"

#include "core/state.hpp"
#include <SDL3/SDL.h>

bool Render_Fade_In(SDL_Texture *texture, Uint64 start_time, SDL_Color bg, float fade_time,
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
