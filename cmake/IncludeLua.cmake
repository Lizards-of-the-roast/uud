include(FetchContent)

FetchContent_Declare(
    lua
    URL https://www.lua.org/ftp/lua-5.4.7.tar.gz
    URL_HASH SHA256=9fbf5e28ef86c69858f6d3d34eccc32e911c1a28b4120ff3e84aaa70cfbf1e30
    DOWNLOAD_EXTRACT_TIMESTAMP TRUE
)

FetchContent_MakeAvailable(lua)

file(GLOB LUA_LIB_SOURCES "${lua_SOURCE_DIR}/src/*.c")
list(REMOVE_ITEM LUA_LIB_SOURCES "${lua_SOURCE_DIR}/src/lua.c" "${lua_SOURCE_DIR}/src/luac.c")

add_library(lua_static STATIC ${LUA_LIB_SOURCES})
add_library(Lua::Lua ALIAS lua_static)

set_target_properties(lua_static PROPERTIES C_STANDARD 99 POSITION_INDEPENDENT_CODE TRUE)
target_include_directories(lua_static PUBLIC ${lua_SOURCE_DIR}/src)
target_compile_options(lua_static PRIVATE -w)
target_compile_definitions(lua_static PRIVATE LUA_USE_LINUX)
target_link_libraries(lua_static PRIVATE m ${CMAKE_DL_LIBS})
