find_package(Catch2 3 QUIET)
if(Catch2_FOUND)
    message(STATUS "Found system Catch2")
    list(APPEND CMAKE_MODULE_PATH "${Catch2_DIR}")
    return()
endif()

include(FetchContent)

FetchContent_Declare(
    Catch2
    GIT_REPOSITORY https://github.com/catchorg/Catch2.git
    GIT_TAG        fc94246664433dbdec2f766b939bb075cfcb8c23 # v3.5.2
    GIT_SHALLOW    TRUE
    EXCLUDE_FROM_ALL
)

FetchContent_MakeAvailable(Catch2)

if(COMMAND uud_silence_warnings_in_dir)
    uud_silence_warnings_in_dir("${catch2_BINARY_DIR}")
endif()

list(APPEND CMAKE_MODULE_PATH ${catch2_SOURCE_DIR}/extras)
