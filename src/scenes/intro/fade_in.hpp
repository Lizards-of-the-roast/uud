#pragma once

#include <SDL3/SDL_render.h>

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
bool Render_Fade_In(SDL_Texture *texture, Uint64 start_time, SDL_Color bg, float fade_time,
                    float delay_time);
