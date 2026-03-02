#pragma once

union SDL_Event;
class UI_Context;
class Widget_Context;

bool Handle_Window_Event(const SDL_Event &event);
void Present_Frame(UI_Context &ui, Widget_Context &w);
