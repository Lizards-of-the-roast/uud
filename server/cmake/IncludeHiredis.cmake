find_package(hiredis QUIET)
if(NOT hiredis_FOUND)
    include(FetchContent)

    set(DISABLE_TESTS ON CACHE BOOL "" FORCE)
    set(ENABLE_SSL OFF CACHE BOOL "" FORCE)

    FetchContent_Declare(
        hiredis
        GIT_REPOSITORY https://github.com/redis/hiredis.git
        GIT_TAG        v1.2.0
        GIT_SHALLOW    TRUE
        EXCLUDE_FROM_ALL
    )

    FetchContent_MakeAvailable(hiredis)

    set(_HIREDIS_WRAPPER "${CMAKE_CURRENT_BINARY_DIR}/_hiredis_include")
    file(MAKE_DIRECTORY "${_HIREDIS_WRAPPER}")
    if(NOT EXISTS "${_HIREDIS_WRAPPER}/hiredis")
        file(CREATE_LINK "${hiredis_SOURCE_DIR}" "${_HIREDIS_WRAPPER}/hiredis" SYMBOLIC)
    endif()
    set(HIREDIS_HEADER "${_HIREDIS_WRAPPER}" CACHE PATH "" FORCE)
else()
    message(STATUS "Found system hiredis")
endif()

find_package(redis++ QUIET)
if(redis++_FOUND)
    message(STATUS "Found system redis-plus-plus")
    return()
endif()

include(FetchContent)

set(REDIS_PLUS_PLUS_BUILD_TEST OFF CACHE BOOL "" FORCE)
set(REDIS_PLUS_PLUS_BUILD_STATIC ON CACHE BOOL "" FORCE)
set(REDIS_PLUS_PLUS_BUILD_SHARED OFF CACHE BOOL "" FORCE)
set(REDIS_PLUS_PLUS_USE_TLS OFF CACHE BOOL "" FORCE)

FetchContent_Declare(
    redis-plus-plus
    GIT_REPOSITORY https://github.com/sewenew/redis-plus-plus.git
    GIT_TAG        1.3.12
    GIT_SHALLOW    TRUE
    EXCLUDE_FROM_ALL
)

FetchContent_MakeAvailable(redis-plus-plus)
