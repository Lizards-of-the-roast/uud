#include <catch2/catch_test_macros.hpp>
#include <cle/serialization/card_serializer.hpp>

using namespace cle::core;
using namespace cle::lua;
using namespace cle::mana;
using namespace cle::serialization;

TEST_CASE("Serialize and deserialize basic card", "[serialization]") {
    Card card("Kitty Card", CardType::Instant);
    card.set_colors(Color::Red);
    card.set_oracle_text("Deal 3 damage to any target.");

    auto cost = ManaCost::from_string("{R}");
    REQUIRE(cost);
    card.set_mana_cost(*cost);

    auto data = serialize_card(card);
    REQUIRE(data.name() == "Kitty Card");
    REQUIRE(data.type() == cle::proto::CARD_TYPE_INSTANT);
    REQUIRE(data.colors() == static_cast<uint32_t>(Color::Red));
    REQUIRE(data.oracle_text() == "Deal 3 damage to any target.");
    REQUIRE(data.mana_cost().red() == 1);

    CardEngine engine;
    auto result = deserialize_card(data, engine);
    REQUIRE(result);
    auto& rebuilt = *result;

    REQUIRE(rebuilt->name() == "Kitty Card");
    REQUIRE(rebuilt->type() == CardType::Instant);
    REQUIRE(rebuilt->colors() == Color::Red);
    REQUIRE(rebuilt->oracle_text() == "Deal 3 damage to any target.");
    REQUIRE(rebuilt->mana_cost().red == 1);
}

TEST_CASE("Serialize creature with stats", "[serialization]") {
    Card card("Kitty Card", CardType::Creature);
    card.set_colors(Color::Green);
    card.set_creature_stats({2, 2});
    card.add_subtype("Cat");
    card.add_keyword("Meow");

    auto cost = ManaCost::from_string("{1}{G}");
    REQUIRE(cost);
    card.set_mana_cost(*cost);

    auto data = serialize_card(card);
    REQUIRE(data.has_creature_stats());
    REQUIRE(data.creature_stats().power() == 2);
    REQUIRE(data.creature_stats().toughness() == 2);
    REQUIRE(data.subtypes_size() == 1);
    REQUIRE(data.subtypes(0) == "Cat");
    REQUIRE(data.keywords_size() == 1);
    REQUIRE(data.keywords(0) == "Meow");

    CardEngine engine;
    auto result = deserialize_card(data, engine);
    REQUIRE(result);
    auto& rebuilt = *result;

    REQUIRE(rebuilt->creature_stats().has_value());
    REQUIRE(rebuilt->creature_stats()->power == 2);
    REQUIRE(rebuilt->creature_stats()->toughness == 2);
    REQUIRE(rebuilt->subtypes().size() == 1);
    REQUIRE(rebuilt->subtypes()[0] == "Cat");
}

TEST_CASE("Instance ID preserved across serialization", "[serialization]") {
    Card card("Catland", CardType::Land);
    auto original_id = card.instance_id();

    auto data = serialize_card(card);
    REQUIRE(data.instance_id() == original_id);

    CardEngine engine;
    auto result = deserialize_card(data, engine);
    REQUIRE(result);
    REQUIRE((*result)->instance_id() == original_id);
}

TEST_CASE("Serialize mana cost with hybrids", "[serialization]") {
    Card card("Cat spell", CardType::Instant);

    auto cost = ManaCost::from_string("{2}{W/U}{B}");
    REQUIRE(cost);
    card.set_mana_cost(*cost);

    auto data = serialize_card(card);
    REQUIRE(data.mana_cost().colorless() == 2);
    REQUIRE(data.mana_cost().black() == 1);
    REQUIRE(data.mana_cost().hybrid_costs_size() == 1);
    REQUIRE(data.mana_cost().hybrid_costs(0).primary() == cle::proto::MANA_COLOR_WHITE);
    REQUIRE(data.mana_cost().hybrid_costs(0).secondary() == cle::proto::MANA_COLOR_BLUE);

    CardEngine engine;
    auto result = deserialize_card(data, engine);
    REQUIRE(result);
    auto& rebuilt = *result;
    REQUIRE(rebuilt->mana_cost().colorless == 2);
    REQUIRE(rebuilt->mana_cost().black == 1);
    REQUIRE(rebuilt->mana_cost().hybrid_costs.size() == 1);
    REQUIRE(rebuilt->mana_cost().hybrid_costs[0].primary == ManaColor::White);
    REQUIRE(rebuilt->mana_cost().hybrid_costs[0].secondary == ManaColor::Blue);
}

TEST_CASE("Serialize multicolor card", "[serialization]") {
    Card card("Cat card", CardType::Creature);
    card.set_colors(Color::Blue | Color::Black | Color::Red);

    auto data = serialize_card(card);
    auto rebuilt_colors = static_cast<Color>(data.colors());
    REQUIRE((rebuilt_colors & Color::Blue) == Color::Blue);
    REQUIRE((rebuilt_colors & Color::Black) == Color::Black);
    REQUIRE((rebuilt_colors & Color::Red) == Color::Red);
}

TEST_CASE("Serialize card to/from binary string", "[serialization]") {
    Card card("Countercat", CardType::Instant);
    card.set_colors(Color::Blue);
    card.set_oracle_text("Counter target spell.");

    auto cost = ManaCost::from_string("{U}{U}");
    REQUIRE(cost);
    card.set_mana_cost(*cost);

    auto data = serialize_card(card);

    std::string binary;
    REQUIRE(data.SerializeToString(&binary));
    REQUIRE(!binary.empty());

    cle::proto::CardData parsed;
    REQUIRE(parsed.ParseFromString(binary));
    REQUIRE(parsed.name() == "Countercat");
    REQUIRE(parsed.mana_cost().blue() == 2);
}