#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>

#include "state.hpp"

// Namespace global state AND init everthing to 0, maybe there is a more cpp way to do this?
State state = {0};

void Get_Delta_Time(double *delta_time, Uint64 *tick)
{
    Uint64 new_tick = SDL_GetPerformanceCounter();
    *delta_time = (double)(new_tick - *tick)*1000.0f / (double)SDL_GetPerformanceFrequency();
    *tick = new_tick;
}
void Get_Delta_Time(void)
{
    Get_Delta_Time(&state.delta_time, &state.tick);
    return;
}
