find_package(libpqxx QUIET)
if(libpqxx_FOUND)
    message(STATUS "Found system libpqxx")
    return()
endif()

include(FetchContent)

set(BUILD_TEST OFF CACHE BOOL "" FORCE)

FetchContent_Declare(
    libpqxx
    GIT_REPOSITORY https://github.com/jtv/libpqxx.git
    GIT_TAG        7.9.2
    GIT_SHALLOW    TRUE
    EXCLUDE_FROM_ALL
)

FetchContent_MakeAvailable(libpqxx)
