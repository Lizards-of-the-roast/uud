find_package(SDL3 CONFIG QUIET)
find_package(SDL3_ttf CONFIG QUIET)
find_package(SDL3_image CONFIG QUIET)

if(SDL3_FOUND AND SDL3_ttf_FOUND AND SDL3_image_FOUND)
  message(STATUS "Using system-installed SDL3, SDL3_ttf, SDL3_image")
else()
  message(STATUS "System SDL3 not fully found, building from source")
  include(FetchContent)

  # SDL fixes
  set(SDL_X11_XTEST OFF CACHE BOOL "" FORCE)
  set(SDLIMAGE_PNG_SHARED OFF CACHE BOOL "" FORCE)
  set(SDLIMAGE_DEPS_SHARED OFF CACHE BOOL "" FORCE)

  FetchContent_Declare(
    SDL3
    GIT_REPOSITORY https://github.com/libsdl-org/SDL.git
    GIT_TAG release-3.4.2
    GIT_SHALLOW TRUE
    GIT_SUBMODULES ""
    GIT_SUBMODULES_RECURSE FALSE
  )

  FetchContent_Declare(
    SDL3_ttf
    GIT_REPOSITORY https://github.com/libsdl-org/SDL_ttf.git
    GIT_TAG release-3.2.2
    GIT_SHALLOW TRUE
    GIT_SUBMODULES ""
    GIT_SUBMODULES_RECURSE FALSE
  )

  FetchContent_Declare(
    SDL3_image
    GIT_REPOSITORY https://github.com/libsdl-org/SDL_image.git
    GIT_TAG release-3.4.0
    GIT_SHALLOW TRUE
    GIT_SUBMODULES ""
    GIT_SUBMODULES_RECURSE FALSE
  )

  FetchContent_MakeAvailable(SDL3 SDL3_ttf SDL3_image)
endif()
