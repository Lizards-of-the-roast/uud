cmake_minimum_required(VERSION 3.30)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(CardLuaEngine
    VERSION 0.1.0
    DESCRIPTION "MTG card creation lib using lua for cards"
    LANGUAGES CXX C
)

# opts
option(CLE_BUILD_TESTS "build tests" ${PROJECT_IS_TOP_LEVEL})

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

# deps
include(IncludeLua)
include(IncludeSol2)

# lib
add_library(cle INTERFACE)
add_library(CardLuaEngine::cle ALIAS cle)

target_include_directories(cle
    INTERFACE
    $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
    $<INSTALL_INTERFACE:include>
)

target_link_libraries(cle
    INTERFACE
    sol2::sol2
    Lua::Lua
)

option(CLE_BUILD_PROTO "build protobuf ser" ON)

if(CLE_BUILD_PROTO)
    include(IncludeProtobuf)

    set(CLE_PROTO_GEN_DIR "${CMAKE_CURRENT_BINARY_DIR}/proto_gen")
    set(CLE_PROTO_SRC "${CLE_PROTO_GEN_DIR}/cle/card.pb.cc")
    set(CLE_PROTO_HDR "${CLE_PROTO_GEN_DIR}/cle/card.pb.h")

    add_custom_command(
        OUTPUT "${CLE_PROTO_SRC}" "${CLE_PROTO_HDR}"
        COMMAND ${CMAKE_COMMAND} -E make_directory "${CLE_PROTO_GEN_DIR}/cle"
        COMMAND protobuf::protoc
            --cpp_out="${CLE_PROTO_GEN_DIR}"
            -I "${CMAKE_CURRENT_SOURCE_DIR}/proto"
            "${CMAKE_CURRENT_SOURCE_DIR}/proto/cle/card.proto"
        DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/proto/cle/card.proto" protobuf::protoc
    )

    add_library(cle_proto STATIC "${CLE_PROTO_SRC}")
    add_library(CardLuaEngine::cle_proto ALIAS cle_proto)

    target_link_libraries(cle_proto
        PUBLIC
        CardLuaEngine::cle
        protobuf::libprotobuf
    )

    target_include_directories(cle_proto
        PUBLIC
        $<BUILD_INTERFACE:${CLE_PROTO_GEN_DIR}>
    )
endif()

# tests
if(CLE_BUILD_TESTS)
    enable_testing()
    add_subdirectory(tests)
endif()
