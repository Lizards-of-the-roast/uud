#include "scene_helpers.hpp"

#include "core/state.hpp"
#include "ui/widgets/widgets.hpp"
#include <SDL3/SDL.h>

bool Handle_Window_Event(const SDL_Event &event) {
    switch (event.type) {
        case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
        case SDL_EVENT_QUIT:
            state.scene = Scene::Exit;
            return true;
        case SDL_EVENT_WINDOW_RESIZED:
            SDL_GetWindowSize(state.window, &state.window_width, &state.window_height);
            break;
    }
    return false;
}

void Present_Frame(UI_Context &ui, Widget_Context &w) {
    ui.End();
    w.Draw();
    SDL_RenderPresent(state.renderer);
}
