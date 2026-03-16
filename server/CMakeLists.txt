cmake_minimum_required(VERSION 3.30)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(mtg-server
    VERSION 0.1.0
    DESCRIPTION "MTG server"
    LANGUAGES CXX C
)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

# warnings
add_library(mtg_compile_options INTERFACE)
target_compile_options(mtg_compile_options INTERFACE
    -Wall -Wextra -Wpedantic
    -Wconversion -Wsign-conversion
    -Wshadow -Wnon-virtual-dtor
    -Wold-style-cast -Wcast-align
    -Woverloaded-virtual
    -Wnull-dereference
    -Wformat=2
    $<$<CONFIG:Release>:-fstack-protector-strong>
    $<$<CONFIG:Release>:-D_FORTIFY_SOURCE=2>
)

include(IncludeSpdlog)
include(IncludeYamlCpp)
include(IncludeJwtCpp)
include(IncludeLibpqxx)
include(IncludeHiredis)
include(IncludePrometheusCpp)

# proto gen
set(MTG_PROTO_DIR "${UUD_PROTO_DIR}")
set(MTG_PROTO_GEN_DIR "${CMAKE_CURRENT_BINARY_DIR}/proto_gen")

set(_PROTOC $<TARGET_FILE:protobuf::protoc>)
set(_GRPC_CPP_PLUGIN ${UUD_GRPC_CPP_PLUGIN})

set(MTG_PROTO_FILES
    "${MTG_PROTO_DIR}/mtg/common.proto"
    "${MTG_PROTO_DIR}/mtg/game_state.proto"
    "${MTG_PROTO_DIR}/mtg/game_service.proto"
    "${MTG_PROTO_DIR}/mtg/auth_service.proto"
    "${MTG_PROTO_DIR}/mtg/matchmaking_service.proto"
)

set(MTG_PROTO_SRCS "")
set(MTG_PROTO_HDRS "")
set(MTG_GRPC_SRCS "")
set(MTG_GRPC_HDRS "")

foreach(proto_file ${MTG_PROTO_FILES})
    get_filename_component(proto_name "${proto_file}" NAME_WE)
    get_filename_component(proto_dir "${proto_file}" DIRECTORY)
    file(RELATIVE_PATH proto_rel "${MTG_PROTO_DIR}" "${proto_file}")
    get_filename_component(proto_rel_dir "${proto_rel}" DIRECTORY)

    set(pb_src "${MTG_PROTO_GEN_DIR}/${proto_rel_dir}/${proto_name}.pb.cc")
    set(pb_hdr "${MTG_PROTO_GEN_DIR}/${proto_rel_dir}/${proto_name}.pb.h")
    set(grpc_src "${MTG_PROTO_GEN_DIR}/${proto_rel_dir}/${proto_name}.grpc.pb.cc")
    set(grpc_hdr "${MTG_PROTO_GEN_DIR}/${proto_rel_dir}/${proto_name}.grpc.pb.h")

    add_custom_command(
        OUTPUT "${pb_src}" "${pb_hdr}" "${grpc_src}" "${grpc_hdr}"
        COMMAND ${CMAKE_COMMAND} -E make_directory "${MTG_PROTO_GEN_DIR}/${proto_rel_dir}"
        COMMAND ${_PROTOC}
        --cpp_out="${MTG_PROTO_GEN_DIR}"
        --grpc_out="${MTG_PROTO_GEN_DIR}"
        --plugin=protoc-gen-grpc=${_GRPC_CPP_PLUGIN}
        -I "${MTG_PROTO_DIR}"
        "${proto_file}"
        DEPENDS "${proto_file}" protobuf::protoc
        COMMENT "Generating protobuf/gRPC for ${proto_rel}"
    )

    list(APPEND MTG_PROTO_SRCS "${pb_src}")
    list(APPEND MTG_PROTO_HDRS "${pb_hdr}")
    list(APPEND MTG_GRPC_SRCS "${grpc_src}")
    list(APPEND MTG_GRPC_HDRS "${grpc_hdr}")
endforeach()

add_library(mtg_proto STATIC
    ${MTG_PROTO_SRCS}
    ${MTG_GRPC_SRCS}
)
target_include_directories(mtg_proto PUBLIC "${MTG_PROTO_GEN_DIR}")
target_link_libraries(mtg_proto
    PUBLIC
    protobuf::libprotobuf
    ${UUD_GRPC_LIB}
    CardLuaEngine::cle_proto
)
target_compile_options(mtg_proto PRIVATE -w)

# game engine
add_library(mtg_engine STATIC
    src/engine/mana_pool.cpp
    src/engine/player.cpp
    src/engine/permanent.cpp
    src/engine/zone_manager.cpp
    src/engine/card_registry.cpp
    src/engine/preset_deck.cpp
    src/engine/turn_machine.cpp
    src/engine/priority_system.cpp
    src/engine/the_stack.cpp
    src/engine/action_queue.cpp
    src/engine/state_based_actions.cpp
    src/engine/win_condition.cpp
    src/engine/combat_manager.cpp
    src/engine/game_context_impl.cpp
    src/engine/game_clock.cpp
    src/engine/event_broadcaster.cpp
    src/engine/continuous_effect.cpp
    src/engine/replacement_effect.cpp
    src/engine/game.cpp
    src/engine/target_filter.cpp
    src/engine/deck_validator.cpp
    src/engine/game_manager.cpp
)
target_include_directories(mtg_engine PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/src")
target_include_directories(mtg_engine SYSTEM PUBLIC
    "${CMAKE_SOURCE_DIR}/engine/include"
)

if(UUD_GRPC_FROM_SOURCE)
    target_include_directories(mtg_engine SYSTEM PUBLIC
        "${grpc_SOURCE_DIR}/include"
        "${grpc_SOURCE_DIR}/third_party/protobuf/src"
        "${grpc_SOURCE_DIR}/third_party/abseil-cpp"
    )
endif()
target_link_libraries(mtg_engine
    PUBLIC
    mtg_proto
    CardLuaEngine::cle
    CardLuaEngine::cle_proto
    spdlog::spdlog
    PRIVATE
    mtg_compile_options
)

# server
add_library(mtg_server_lib STATIC
    src/server/config.cpp
    src/server/server.cpp
    src/server/interceptors/auth_interceptor.cpp
    src/server/interceptors/logging_interceptor.cpp
    src/server/interceptors/rate_limit_interceptor.cpp
    src/service/game_service_impl.cpp
    src/service/auth_service_impl.cpp
    src/service/matchmaking_service_impl.cpp
    src/auth/jwt_manager.cpp
    src/auth/password_hasher.cpp
    src/matchmaking/matchmaker.cpp
    src/matchmaking/queue.cpp
    src/util/logging.cpp
    src/util/metrics.cpp
)
target_include_directories(mtg_server_lib PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/src")
target_link_libraries(mtg_server_lib
    PUBLIC
    mtg_engine
    mtg_proto
    ${UUD_GRPC_LIB}
    ${UUD_GRPC_REFLECTION}
    spdlog::spdlog
    yaml-cpp::yaml-cpp
    jwt-cpp::jwt-cpp
    PRIVATE
    mtg_compile_options
)

target_sources(mtg_server_lib PRIVATE
    src/auth/connection_pool.cpp
    src/auth/user_store.cpp
    src/auth/rating_store.cpp
    src/auth/migration_runner.cpp
    src/auth/game_history_store.cpp
)
target_link_libraries(mtg_server_lib PUBLIC pqxx)

target_sources(mtg_server_lib PRIVATE src/auth/session_store.cpp)
target_link_libraries(mtg_server_lib PUBLIC redis++::redis++_static hiredis::hiredis)

target_link_libraries(mtg_server_lib PUBLIC prometheus-cpp::pull prometheus-cpp::core)

# exec
add_executable(mtg-server src/main.cpp)
target_link_libraries(mtg-server PRIVATE mtg_server_lib mtg_compile_options)

file(COPY "${CMAKE_SOURCE_DIR}/cards" DESTINATION "${CMAKE_CURRENT_BINARY_DIR}")
file(COPY config DESTINATION "${CMAKE_CURRENT_BINARY_DIR}")
