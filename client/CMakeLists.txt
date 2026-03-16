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

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

include(IncludeSDL3)

set(CLIENT_PROTO_DIR "${UUD_PROTO_DIR}")
set(CLIENT_PROTO_GEN_DIR "${CMAKE_CURRENT_BINARY_DIR}/proto_gen")

set(_PROTOC $<TARGET_FILE:protobuf::protoc>)
set(_GRPC_CPP_PLUGIN ${UUD_GRPC_CPP_PLUGIN})

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
        DEPENDS "${proto_file}" protobuf::protoc
        COMMENT "Generating protobuf and grpc for ${proto_rel}"
    )

    list(APPEND MTG_PROTO_SRCS "${pb_src}" "${grpc_src}")
endforeach()

add_library(mtg_proto_client STATIC ${MTG_PROTO_SRCS})
target_include_directories(mtg_proto_client PUBLIC "${CLIENT_PROTO_GEN_DIR}")
target_link_libraries(mtg_proto_client PUBLIC protobuf::libprotobuf ${UUD_GRPC_LIB} CardLuaEngine::cle_proto)
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
    src/net/convert/proto_convert.cpp
    src/scenes/common/scene_helpers.cpp
    src/scenes/intro/intro.cpp
    src/scenes/intro/fade_in.cpp
    src/scenes/login/login.cpp
    src/scenes/menu/menu.cpp
    src/scenes/menu/pages/main/page.cpp
    src/scenes/menu/pages/match/page.cpp
    src/scenes/menu/pages/matchmaking/page.cpp
    src/scenes/menu/pages/settings/page.cpp
    src/scenes/menu/pages/about/page.cpp
    src/scenes/menu/pages/credits/page.cpp
    src/scenes/menu/pages/match_history/page.cpp
    src/scenes/match/match.cpp
    src/scenes/match/game_state_local.cpp
    src/scenes/match/systems.cpp
    src/scenes/match/card_render.cpp
    src/ui/box/ui_box.cpp
    src/ui/context/ui_context.cpp
    src/ui/context/ui_layout.cpp
    src/ui/widgets/widgets.cpp
    src/ui/widgets/widget_draw.cpp
    src/game/instances.cpp
    src/game/textures.cpp
)

target_include_directories(uud PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/src)

target_link_libraries(uud
    PRIVATE
    SDL3
    SDL3_image
    SDL3_ttf
    mtg_proto_client
    ${UUD_GRPC_OPENSSL}
)

set_property(TARGET uud PROPERTY CXX_STANDARD 23)
target_compile_options(uud PRIVATE -Wall -Werror -Wpedantic)

if(OFFLINE_MODE)
    target_compile_definitions(uud PRIVATE OFFLINE_MODE)
endif()

file(COPY res DESTINATION ${CMAKE_CURRENT_BINARY_DIR})
file(COPY "${CMAKE_SOURCE_DIR}/cards" DESTINATION "${CMAKE_CURRENT_BINARY_DIR}/res")
