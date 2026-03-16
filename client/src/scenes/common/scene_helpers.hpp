#pragma once

union SDL_Event;
struct UI_Context;
struct Widget_Context;

bool Handle_Window_Event(const SDL_Event &event);
void Present_Frame(UI_Context &ui, Widget_Context &w);
