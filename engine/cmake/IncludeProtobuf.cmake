find_package(Protobuf QUIET)
if(Protobuf_FOUND OR protobuf_FOUND)
    message(STATUS "Found system protobuf")
    return()
endif()

include(FetchContent)

set(protobuf_BUILD_TESTS OFF CACHE BOOL "" FORCE)
set(protobuf_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(protobuf_INSTALL OFF CACHE BOOL "" FORCE)
set(utf8_range_ENABLE_TESTS OFF CACHE BOOL "" FORCE)
set(utf8_range_ENABLE_INSTALL OFF CACHE BOOL "" FORCE)
set(ABSL_PROPAGATE_CXX_STD ON CACHE BOOL "" FORCE)
set(ABSL_ENABLE_INSTALL OFF CACHE BOOL "" FORCE)

FetchContent_Declare(
    protobuf
    GIT_REPOSITORY https://github.com/protocolbuffers/protobuf.git
    GIT_TAG        v29.3
    GIT_SHALLOW    TRUE
    EXCLUDE_FROM_ALL
)

if(COMMAND uud_fetch_content_quiet)
    uud_fetch_content_quiet(protobuf)
    uud_silence_warnings_in_dir("${protobuf_BINARY_DIR}")
else()
    FetchContent_MakeAvailable(protobuf)
endif()
