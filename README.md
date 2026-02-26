# Untap Upkeep Draw (UUD) Client
A Hackathon project by
Brendan Egan, Matthew Conroy, Ian Fogarty and Thibault Wysocinski

Client lead
Matthew Conroy

# Build
depends on SDL3, SDL_TTF and SDL_image, each of these can be fetched if the CMakeLists.txt file has some lines uncommented
```bash
# debug
cmake --preset debug
cmake --build --preset debug

# release
cmake --preset release
cmake --build --preset release
```

# file summary (thus far)
## defer.hpp
defer statement made by:
https://www.gingerbill.org/article/2015/08/19/defer-in-cpp/

## intro.cpp, intro.hpp
loop for intro (fade in)

## main.cpp
entry and state setup

## match.cpp, match.hpp
loop for a game match

## menu.cpp, menu.hpp
loop for the main menu

## resources.cpp, resources.hpp
handles loading and unloading resources (textures, fonts, etc) from file path.

## simp\_ui.cpp, simp\_ui.hpp
core UI implementation.
This is for autolayouts and user-interaction.

## state.cpp, state.hpp
state structure definition

## types.hpp
miscellaneous types

## widgets.cpp, widgets.hpp
builder UI implementation.
this is for buttons, toggles, sliders etc.
