find_package(jwt-cpp QUIET)
if(jwt-cpp_FOUND)
    message(STATUS "Found system jwt-cpp")
    return()
endif()

include(FetchContent)

set(JWT_CPP_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(JWT_CPP_BUILD_TESTS OFF CACHE BOOL "" FORCE)

FetchContent_Declare(
    jwt-cpp
    GIT_REPOSITORY https://github.com/Thalhammer/jwt-cpp.git
    GIT_TAG        08bcf77a687fb06e34138e9e9fa12a4ecbe12332 # v0.7.0
    GIT_SHALLOW    TRUE
    EXCLUDE_FROM_ALL
)

FetchContent_MakeAvailable(jwt-cpp)
