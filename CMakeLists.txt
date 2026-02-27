cmake_minimum_required(VERSION 3.30.0)
project(UUD_Client LANGUAGES CXX)

include(FetchContent)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# SDL fixes

find_package(SDL3 QUIET)
if (NOT SDL3_FOUND)
    set(SDL_X11_XTEST OFF CACHE BOOL "" FORCE)
    FetchContent_Declare(
      SDL3
      GIT_REPOSITORY https://github.com/libsdl-org/SDL.git
      GIT_TAG release-3.4.2
      GIT_SHALLOW TRUE
      GIT_SUBMODULES ""
      GIT_SUBMODULES_RECURSE FALSE
    )
    FetchContent_MakeAvailable(SDL3)
endif()

find_package(SDL3_ttf QUIET)
if (NOT SDL3_ttf_FOUND)
    FetchContent_Declare(
      SDL3_ttf
      GIT_REPOSITORY https://github.com/libsdl-org/SDL_ttf.git
      GIT_TAG release-3.2.2
      GIT_SHALLOW TRUE
      GIT_SUBMODULES ""
      GIT_SUBMODULES_RECURSE FALSE
    )
    FetchContent_MakeAvailable(SDL3_ttf)
endif()

find_package(SDL3_image QUIET)
if (NOT SDL3_image_FOUND)
    set(SDLIMAGE_PNG_SHARED OFF CACHE BOOL "" FORCE)
    set(SDLIMAGE_DEPS_SHARED OFF CACHE BOOL "" FORCE)
    FetchContent_Declare(
      SDL3_image
      GIT_REPOSITORY https://github.com/libsdl-org/SDL_image.git
      GIT_TAG release-3.4.0
      GIT_SHALLOW TRUE
      GIT_SUBMODULES ""
      GIT_SUBMODULES_RECURSE FALSE
    )
    FetchContent_MakeAvailable(SDL3_image)
endif()

add_executable(uud
  src/main.cpp
  src/menu.cpp
  src/resources.cpp
  src/intro.cpp
  src/state.cpp
  src/simp_ui.cpp
  src/widgets.cpp
  src/match.cpp
)

target_link_libraries(uud
  PRIVATE
  SDL3
  SDL3_image
  SDL3_ttf
)

set_property(TARGET uud PROPERTY CXX_STANDARD 23)
target_compile_options(uud PRIVATE -Wall -Wpedantic)

file(COPY res DESTINATION ${CMAKE_CURRENT_BINARY_DIR})
