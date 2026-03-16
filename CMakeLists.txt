cmake_minimum_required(VERSION 3.30)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(uud
    VERSION 0.1.0
    DESCRIPTION "UUD"
    LANGUAGES CXX C
)

# symlink compile_commands.json for clangd
if(CMAKE_EXPORT_COMPILE_COMMANDS)
    execute_process(
        COMMAND ${CMAKE_COMMAND} -E create_symlink
        ${CMAKE_BINARY_DIR}/compile_commands.json
        ${CMAKE_SOURCE_DIR}/compile_commands.json
    )
endif()

set(UUD_PROTO_DIR "${CMAKE_SOURCE_DIR}/proto" CACHE PATH "Shared proto directory")

option(UUD_BUILD_SERVER "Build the server" ON)
option(UUD_BUILD_CLIENT "Build the client" ON)
option(UUD_BUILD_ENGINE_TESTS "Build cle tests" OFF)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")
include(Helpers)

include(IncludeGrpc)

if(UUD_GRPC_FROM_SOURCE)
    FetchContent_GetProperties(grpc)
    FetchContent_SetPopulated(protobuf
        SOURCE_DIR "${grpc_SOURCE_DIR}/third_party/protobuf"
        BINARY_DIR "${grpc_BINARY_DIR}/third_party/protobuf"
    )
endif()

# cle
set(CLE_BUILD_TESTS ${UUD_BUILD_ENGINE_TESTS} CACHE BOOL "" FORCE)
set(CLE_BUILD_PROTO ON CACHE BOOL "" FORCE)
add_subdirectory(engine)

# server
if(UUD_BUILD_SERVER)
    add_subdirectory(server)
endif()

# client
if(UUD_BUILD_CLIENT)
    add_subdirectory(client)
endif()
