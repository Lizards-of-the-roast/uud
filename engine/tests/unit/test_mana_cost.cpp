#include <catch2/catch_test_macros.hpp>
#include <cle/mana/mana_cost.hpp>

using namespace cle::mana;

TEST_CASE("ManaCost parsing -  colorless", "[mana]") {
    auto result = ManaCost::from_string("{3}");
    REQUIRE(result);

    auto cost = result.value();
    REQUIRE(cost.colorless == 3);
    REQUIRE(cost.mana_value() == 3);
}

TEST_CASE("ManaCost parsing - single color", "[mana]") {
    auto result = ManaCost::from_string("{R}");
    REQUIRE(result);

    auto cost = result.value();
    REQUIRE(cost.red == 1);
    REQUIRE(cost.mana_value() == 1);
}

TEST_CASE("ManaCost parsing - multiple colors", "[mana]") {
    auto result = ManaCost::from_string("{W}{U}{B}{R}{G}");
    REQUIRE(result);

    auto cost = result.value();
    REQUIRE(cost.white == 1);
    REQUIRE(cost.blue == 1);
    REQUIRE(cost.black == 1);
    REQUIRE(cost.red == 1);
    REQUIRE(cost.green == 1);
    REQUIRE(cost.mana_value() == 5);
}

TEST_CASE("ManaCost parsing - strings", "[mana]") {
    auto result = ManaCost::from_string("{2}{W}{W}");
    REQUIRE(result);

    auto cost = result.value();
    REQUIRE(cost.colorless == 2);
    REQUIRE(cost.white == 2);
    REQUIRE(cost.mana_value() == 4);
}

TEST_CASE("ManaCost parsing - x cost", "[mana]") {
    auto result = ManaCost::from_string("{X}{R}");
    REQUIRE(result);

    auto cost = result.value();
    REQUIRE(cost.x_count == 1);
    REQUIRE(cost.red == 1);
}

TEST_CASE("ManaCost parsing - hybrid mana", "[mana]") {
    auto result = ManaCost::from_string("{W/U}");
    REQUIRE(result);

    auto cost = result.value();
    REQUIRE(cost.hybrid_costs.size() == 1);
    REQUIRE(cost.hybrid_costs[0].primary == ManaColor::White);
    REQUIRE(cost.hybrid_costs[0].secondary == ManaColor::Blue);
    REQUIRE(cost.mana_value() == 1);
}

TEST_CASE("ManaCost parsing - colorless hybrid", "[mana]") {
    auto result = ManaCost::from_string("{2/W}");
    REQUIRE(result);

    auto cost = result.value();
    REQUIRE(cost.hybrid_costs.size() == 1);
    REQUIRE(cost.hybrid_costs[0].primary == ManaColor::Colorless);
    REQUIRE(cost.hybrid_costs[0].secondary == ManaColor::White);
    REQUIRE(cost.mana_value() == 2);  // Colorless hybrid contributes 2
}

TEST_CASE("ManaCost parsing - invalid format", "[mana]") {
    REQUIRE_FALSE(ManaCost::from_string(""));
    REQUIRE_FALSE(ManaCost::from_string("3"));
    REQUIRE_FALSE(ManaCost::from_string("{"));
    REQUIRE_FALSE(ManaCost::from_string("{}"));
}