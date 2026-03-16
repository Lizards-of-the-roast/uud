find_package(yaml-cpp QUIET)
if(yaml-cpp_FOUND)
    message(STATUS "Found system yaml-cpp")
    return()
endif()

include(FetchContent)

set(YAML_CPP_BUILD_TESTS OFF CACHE BOOL "" FORCE)
set(YAML_CPP_BUILD_TOOLS OFF CACHE BOOL "" FORCE)
set(YAML_CPP_INSTALL OFF CACHE BOOL "" FORCE)

FetchContent_Declare(
    yaml-cpp
    GIT_REPOSITORY https://github.com/jbeder/yaml-cpp.git
    GIT_TAG        f7320141120f720aecc4c32be25586e7da9eb978 # 0.8.0
    GIT_SHALLOW    TRUE
    EXCLUDE_FROM_ALL
)

FetchContent_MakeAvailable(yaml-cpp)
