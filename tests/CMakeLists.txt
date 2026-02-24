include(IncludeCatch2)
include(Catch)

add_executable(cle_tests
    unit/test_card.cpp
    unit/test_mana_cost.cpp
)

target_link_libraries(cle_tests
    PRIVATE
    Catch2::Catch2WithMain
    CardLuaEngine::cle
)

catch_discover_tests(cle_tests)

if(CLE_BUILD_PROTO)
    add_executable(cle_proto_tests
        unit/test_card_serialization.cpp
    )

    target_link_libraries(cle_proto_tests
        PRIVATE
        Catch2::Catch2WithMain
        CardLuaEngine::cle_proto
    )

    catch_discover_tests(cle_proto_tests)
endif()
