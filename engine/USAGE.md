# Usage

## Building

you need a C++23 compiler (clang recommended)

```bash
cmake --preset release
cmake --build --preset release
ctest --test-dir build/release
```

## Linking

Add it via FetchContent or as a subdir

```cmake
add_subdirectory(card-lua-engine)
target_link_libraries(your_target PRIVATE CardLuaEngine::cle)

# if u need serialization (protobuf)
target_link_libraries(your_target PRIVATE CardLuaEngine::cle_proto)
```

## Loading cards

```cpp
#include <cle/lua/engine.hpp>

cle::lua::CardEngine engine;

// from file
auto result = engine.load_card_from_file("cards/some_card.lua");
if (!result) {
    // result.error().code  (CardLoadError enum)
    // result.error().detail (details abt err)
}
auto card = *result;

// from string
auto result2 = engine.load_card_from_string(R"(
    return Instant("Lightning Bolt")
        :colors({"Red"})
        :mana_cost("{R}")
        :oracle_text("Deal 3 damage to any target.")
        :build()
)");
```

## Game Context

Implement the `GameContext` interface and pass it to the engines:

```cpp
#include <cle/game/game_context.hpp>

class MyGame : public cle::game::GameContext {
    void draw_cards(uint64_t player_id, int count) override {}
    void gain_life(uint64_t player_id, int amount) override {}
    // u should impl all virt methods
};

auto game = std::make_shared<MyGame>();
engine.set_game_context(game);
```

## Serialization

link `CardLuaEngine::cle_proto` for protobuf serialization

only serializes static card data (name, type, mana cost, etc). lua functions (triggers/abilities) cant be serialized so u pass the lua path to reload them

```cpp
#include <cle/serialization/card_serializer.hpp>

// serialize
auto card = *engine.load_card_from_file("cards/cat.lua");
auto data = cle::serialization::serialize_card(*card, "cards/cat.lua");

// to binary
std::string binary;
data.SerializeToString(&binary);

// from binary
cle::proto::CardData parsed;
parsed.ParseFromString(binary);

// deserialize (keep in mind this reloads the lua file)
auto rebuilt = cle::serialization::deserialize_card(parsed, engine);
```
