include(FetchContent)

FetchContent_Declare(
    sol2
    GIT_REPOSITORY https://github.com/ThePhD/sol2.git
    GIT_TAG        c1f95a773c6f8f4fde8ca3efe872e7286afe4444
    GIT_SHALLOW    TRUE
)

FetchContent_MakeAvailable(sol2)

if(NOT TARGET sol2::sol2)
    add_library(sol2::sol2 ALIAS sol2)
endif()
