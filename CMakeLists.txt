cmake_minimum_required(VERSION 3.30.0)
project(UUD_Client LANGUAGES CXX C)

include(FetchContent)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

option(OFFLINE_MODE "Disables need for backend" OFF)

option(USE_SCCACHE "Use sccache" ON)
if(USE_SCCACHE)
    find_program(SCCACHE_PROGRAM sccache)
    if(SCCACHE_PROGRAM)
        set(CMAKE_C_COMPILER_LAUNCHER "${SCCACHE_PROGRAM}")
        set(CMAKE_CXX_COMPILER_LAUNCHER "${SCCACHE_PROGRAM}")
    else()
        message(WARNING "USE_SCCACHE is ON but sccache was not found")
    endif()
endif()

# symlink compile_commands.json for sdl and other libraries intellisense
if(CMAKE_EXPORT_COMPILE_COMMANDS)
    execute_process(
        COMMAND ${CMAKE_COMMAND} -E create_symlink
        ${CMAKE_BINARY_DIR}/compile_commands.json
        ${CMAKE_SOURCE_DIR}/compile_commands.json
    )
endif()

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

include(IncludeSDL3)
include(IncludeGrpc)

set(CLIENT_PROTO_DIR "${CMAKE_CURRENT_SOURCE_DIR}/proto")
set(CLIENT_PROTO_GEN_DIR "${CMAKE_CURRENT_BINARY_DIR}/proto_gen")

set(_PROTOC $<TARGET_FILE:protobuf::protoc>)
if(TARGET gRPC::grpc_cpp_plugin)
    set(_GRPC_CPP_PLUGIN $<TARGET_FILE:gRPC::grpc_cpp_plugin>)
    set(_GRPC_CPP_PLUGIN_DEP gRPC::grpc_cpp_plugin)
else()
    set(_GRPC_CPP_PLUGIN $<TARGET_FILE:grpc_cpp_plugin>)
    set(_GRPC_CPP_PLUGIN_DEP grpc_cpp_plugin)
endif()

# cle proto
set(CLE_PROTO_SRC "${CLIENT_PROTO_GEN_DIR}/cle/card.pb.cc")
set(CLE_PROTO_HDR "${CLIENT_PROTO_GEN_DIR}/cle/card.pb.h")

add_custom_command(
    OUTPUT "${CLE_PROTO_SRC}" "${CLE_PROTO_HDR}"
    COMMAND ${CMAKE_COMMAND} -E make_directory "${CLIENT_PROTO_GEN_DIR}/cle"
    COMMAND ${_PROTOC}
    --cpp_out="${CLIENT_PROTO_GEN_DIR}"
    -I "${CLIENT_PROTO_DIR}"
    "${CLIENT_PROTO_DIR}/cle/card.proto"
    DEPENDS "${CLIENT_PROTO_DIR}/cle/card.proto" protobuf::protoc
    COMMENT "Generating protobuf for cle/card.proto"
)

add_library(cle_proto_client STATIC "${CLE_PROTO_SRC}")
target_include_directories(cle_proto_client PUBLIC "${CLIENT_PROTO_GEN_DIR}")
target_link_libraries(cle_proto_client PUBLIC protobuf::libprotobuf)
target_compile_options(cle_proto_client PRIVATE -w)

# game server protos
set(MTG_PROTO_FILES
    "${CLIENT_PROTO_DIR}/mtg/common.proto"
    "${CLIENT_PROTO_DIR}/mtg/game_state.proto"
    "${CLIENT_PROTO_DIR}/mtg/game_service.proto"
    "${CLIENT_PROTO_DIR}/mtg/auth_service.proto"
    "${CLIENT_PROTO_DIR}/mtg/matchmaking_service.proto"
)

set(MTG_PROTO_SRCS "")

foreach(proto_file ${MTG_PROTO_FILES})
    get_filename_component(proto_name "${proto_file}" NAME_WE)
    file(RELATIVE_PATH proto_rel "${CLIENT_PROTO_DIR}" "${proto_file}")
    get_filename_component(proto_rel_dir "${proto_rel}" DIRECTORY)

    set(pb_src "${CLIENT_PROTO_GEN_DIR}/${proto_rel_dir}/${proto_name}.pb.cc")
    set(pb_hdr "${CLIENT_PROTO_GEN_DIR}/${proto_rel_dir}/${proto_name}.pb.h")
    set(grpc_src "${CLIENT_PROTO_GEN_DIR}/${proto_rel_dir}/${proto_name}.grpc.pb.cc")
    set(grpc_hdr "${CLIENT_PROTO_GEN_DIR}/${proto_rel_dir}/${proto_name}.grpc.pb.h")

    add_custom_command(
        OUTPUT "${pb_src}" "${pb_hdr}" "${grpc_src}" "${grpc_hdr}"
        COMMAND ${CMAKE_COMMAND} -E make_directory "${CLIENT_PROTO_GEN_DIR}/${proto_rel_dir}"
        COMMAND ${_PROTOC}
        --cpp_out="${CLIENT_PROTO_GEN_DIR}"
        --grpc_out="${CLIENT_PROTO_GEN_DIR}"
        --plugin=protoc-gen-grpc=${_GRPC_CPP_PLUGIN}
        -I "${CLIENT_PROTO_DIR}"
        "${proto_file}"
        DEPENDS "${proto_file}" protobuf::protoc ${_GRPC_CPP_PLUGIN_DEP}
        COMMENT "Generating protobuf and grpc for ${proto_rel}"
    )

    list(APPEND MTG_PROTO_SRCS "${pb_src}" "${grpc_src}")
endforeach()

add_library(mtg_proto_client STATIC ${MTG_PROTO_SRCS})
target_include_directories(mtg_proto_client PUBLIC "${CLIENT_PROTO_GEN_DIR}")
if(TARGET gRPC::grpc++)
    target_link_libraries(mtg_proto_client PUBLIC protobuf::libprotobuf gRPC::grpc++ cle_proto_client)
else()
    target_link_libraries(mtg_proto_client PUBLIC protobuf::libprotobuf grpc++ cle_proto_client)
endif()
target_compile_options(mtg_proto_client PRIVATE -w)

add_executable(uud
    src/main.cpp
    src/core/resources.cpp
    src/core/state.cpp
    src/net/net_client.cpp
    src/net/auth_client.cpp
    src/net/matchmaking_client.cpp
    src/net/game_client.cpp
    src/net/token_store.cpp
    src/net/card_catalog.cpp
    src/scenes/common/scene_helpers.cpp
    src/scenes/intro/intro.cpp
    src/scenes/intro/fade_in.cpp
    src/scenes/login/login.cpp
    src/scenes/menu/menu.cpp
    src/scenes/menu/pages/main/page.cpp
    src/scenes/menu/pages/match/page.cpp
    src/scenes/menu/pages/matchmaking/page.cpp
    src/scenes/menu/pages/deck_builder/page.cpp
    src/scenes/menu/pages/settings/page.cpp
    src/scenes/menu/pages/about/page.cpp
    src/scenes/menu/pages/credits/page.cpp
    src/scenes/match/match.cpp
    src/scenes/match/game_state_local.cpp
    src/scenes/match/systems/hand.cpp
    src/scenes/match/systems/drag_overlay.cpp
    src/ui/box/ui_box.cpp
    src/ui/context/ui_context.cpp
    src/ui/context/ui_layout.cpp
    src/ui/widgets/widgets.cpp
    src/ui/widgets/widget_draw.cpp
)

target_include_directories(uud PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/src)

target_link_libraries(uud
    PRIVATE
    SDL3
    SDL3_image
    SDL3_ttf
    mtg_proto_client
)

set_property(TARGET uud PROPERTY CXX_STANDARD 23)
target_compile_options(uud PRIVATE -Wall -Werror -Wpedantic)

if(OFFLINE_MODE)
    target_compile_definitions(uud PRIVATE OFFLINE_MODE)
endif()

file(COPY res DESTINATION ${CMAKE_CURRENT_BINARY_DIR})
