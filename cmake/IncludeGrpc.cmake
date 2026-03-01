find_package(gRPC CONFIG QUIET)

# id recommend installing these locally or you'll
# unironically be waiting 30m to compile
if(gRPC_FOUND)
    message(STATUS "Using system-installed gRPC")
else()
    message(STATUS "System gRPC not found, building from source")
    include(FetchContent)

    set(gRPC_BUILD_TESTS OFF CACHE BOOL "" FORCE)
    set(gRPC_BUILD_CSHARP_EXT OFF CACHE BOOL "" FORCE)
    set(gRPC_BUILD_GRPC_CSHARP_PLUGIN OFF CACHE BOOL "" FORCE)
    set(gRPC_BUILD_GRPC_NODE_PLUGIN OFF CACHE BOOL "" FORCE)
    set(gRPC_BUILD_GRPC_OBJECTIVE_C_PLUGIN OFF CACHE BOOL "" FORCE)
    set(gRPC_BUILD_GRPC_PHP_PLUGIN OFF CACHE BOOL "" FORCE)
    set(gRPC_BUILD_GRPC_PYTHON_PLUGIN OFF CACHE BOOL "" FORCE)
    set(gRPC_BUILD_GRPC_RUBY_PLUGIN OFF CACHE BOOL "" FORCE)
    set(gRPC_INSTALL OFF CACHE BOOL "" FORCE)
    set(protobuf_BUILD_TESTS OFF CACHE BOOL "" FORCE)
    set(protobuf_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
    set(protobuf_INSTALL OFF CACHE BOOL "" FORCE)
    set(utf8_range_ENABLE_TESTS OFF CACHE BOOL "" FORCE)
    set(utf8_range_ENABLE_INSTALL OFF CACHE BOOL "" FORCE)
    set(ABSL_PROPAGATE_CXX_STD ON CACHE BOOL "" FORCE)
    set(ABSL_ENABLE_INSTALL OFF CACHE BOOL "" FORCE)
    set(ABSL_BUILD_TESTING OFF CACHE BOOL "" FORCE)

    FetchContent_Declare(
        grpc
        GIT_REPOSITORY https://github.com/grpc/grpc.git
        GIT_TAG 93571f6142f823167d54bc1169fed567b2407d94
        GIT_SHALLOW TRUE
        GIT_PROGRESS TRUE
    )

    set(_prev_cmake_warn_deprecated "${CMAKE_WARN_DEPRECATED}")
    set(_prev_cmake_suppress_dev "${CMAKE_SUPPRESS_DEVELOPER_WARNINGS}")
    set(CMAKE_WARN_DEPRECATED OFF)
    set(CMAKE_SUPPRESS_DEVELOPER_WARNINGS 1)

    FetchContent_MakeAvailable(grpc)

    set(CMAKE_WARN_DEPRECATED "${_prev_cmake_warn_deprecated}")
    set(CMAKE_SUPPRESS_DEVELOPER_WARNINGS "${_prev_cmake_suppress_dev}")
endif()
