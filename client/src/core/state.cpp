#include "state.hpp"

#include <SDL3/SDL.h>
#include <SDL3/SDL_stdinc.h>
#include <SDL3/SDL_timer.h>

State state{};

void State::Update_Delta_Time() {
    const Uint64 now = SDL_GetPerformanceCounter();

    if (tick == 0) {
        tick = now;
        delta_time = 0.0;
        return;
    }

    const Uint64 freq = SDL_GetPerformanceFrequency();
    if (freq == 0) {
        tick = now;
        delta_time = 0.0;
        return;
    }

    delta_time = (static_cast<double>(now - tick) * 1000.0) / static_cast<double>(freq);
    tick = now;
}
