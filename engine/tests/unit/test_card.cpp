#include <catch2/catch_test_macros.hpp>
#include <cle/core/card.hpp>

using namespace cle::core;

TEST_CASE("Card construction", "[card]") {
    Card card("Cat's card", CardType::Instant);

    REQUIRE(card.name() == "Cat's card");
    REQUIRE(card.type() == CardType::Instant);
    REQUIRE(card.instance_id() > 0);
}

TEST_CASE("Card unique instance ID", "[card]") {
    Card card1("kitty", CardType::Land);
    Card card2("kitty", CardType::Land);

    REQUIRE(card1.instance_id() != card2.instance_id());
}

TEST_CASE("Card copy creates new instance ID", "[card]") {
    Card original("cat", CardType::Artifact);
    Card copy = original;

    REQUIRE(copy.name() == original.name());
    REQUIRE(copy.type() == original.type());
    REQUIRE(copy.instance_id() != original.instance_id());
}

TEST_CASE("Card setters and getters", "[card]") {
    Card card("Llanowar Elves", CardType::Creature);

    card.set_oracle_text("Meow");
    card.set_flavor_text("Rawr");
    card.set_colors(Color::Green);
    card.add_subtype("Elf");
    card.add_subtype("Druid");
    card.add_keyword("Haste");

    REQUIRE(card.oracle_text() == "Meow");
    REQUIRE(card.flavor_text() == "Rawr");
    REQUIRE(card.colors() == Color::Green);
    REQUIRE(card.subtypes().size() == 2);
    REQUIRE(card.subtypes()[0] == "Elf");
    REQUIRE(card.subtypes()[1] == "Druid");
    REQUIRE(card.keywords().size() == 1);
    REQUIRE(card.keywords()[0] == "Haste");
}

TEST_CASE("CreatureStats", "[card]") {
    Card card("Grizzly Bears", CardType::Creature);
    card.set_creature_stats({2, 2});

    REQUIRE(card.creature_stats().has_value());
    REQUIRE(card.creature_stats()->power == 2);
    REQUIRE(card.creature_stats()->toughness == 2);
    REQUIRE(card.creature_stats()->to_string() == "2/2");
}

TEST_CASE("Non-creature has no creature stats", "[card]") {
    Card card("Cat's card", CardType::Instant);

    REQUIRE_FALSE(card.creature_stats().has_value());
}
