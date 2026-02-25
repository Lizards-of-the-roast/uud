cmake_minimum_required(VERSION 4.1)
project(UUD_Client)

include(FetchContent)

###################################################################
# If you want to have cmake fetch 2GB of SDL stuff uncomment this #
###################################################################
# FetchContent_Declare(
#     SDL3
#     GIT_REPOSITORY https://github.com/libsdl-org/SDL.git
#     GIT_TAG 550394eecdc250c7ce542a99f0c2b55683521656
# )
# FetchContent_Declare(
#     SDL3_ttf
#     GIT_REPOSITORY https://github.com/libsdl-org/SDL_ttf.git
#     GIT_TAG 053bbc89517471427748a082583c9eada55c07b5
# )
# FetchContent_Declare(
#     SDL3_image
#     GIT_REPOSITORY https://github.com/libsdl-org/SDL_image.git
#     GIT_TAG 8bd9f3d7f2d2bb59ce4331f13b77d65254cd8c7b
# )
# FetchContent_MakeAvailable(SDL3 SDL3_ttf SDL3_image)

add_executable(uud)
target_sources(uud
    PRIVATE
      src/main.cpp
      src/menu.cpp
      src/resources.cpp
      src/intro.cpp
      src/state.cpp
      src/simp_ui.cpp
      src/widgets.cpp
      src/match.cpp
)
#set_property(TARGET uud PROPERTY CXX_STANDARD 23)
#target_compile_options(uud PRIVATE -Wall -Wpedantic)
target_link_libraries(uud
  PRIVATE
    SDL3
    SDL3_image
    SDL3_ttf
)
file(COPY res DESTINATION ${CMAKE_CURRENT_BINARY_DIR})
