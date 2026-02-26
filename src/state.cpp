#include <SDL3/SDL.h>
#include <SDL3/SDL_stdinc.h>
#include <SDL3/SDL_timer.h>

#include "state.hpp"

// Namespace global state AND init everything to 0, maybe there is a more cpp way to do this?
State state{};

void Get_Delta_Time(double *delta_time, Uint64 *tick)
{
    // nothing to do, optimization
    if (!delta_time || !tick)
        return;

    const Uint64 now = SDL_GetPerformanceCounter();

    // first call
    if (*tick == 0) {
        *tick = now;
        *delta_time = 0.0;
        return;
    }

    const Uint64 freq = SDL_GetPerformanceFrequency();
    if (freq == 0) {
        *tick = now;
        *delta_time = 0.0;
        return;
    }

    *delta_time = (static_cast<double>(now - *tick) * 1000.0) / static_cast<double>(freq);
    *tick = now;
}

void Get_Delta_Time(void)
{
    Get_Delta_Time(&state.delta_time, &state.tick);
}
