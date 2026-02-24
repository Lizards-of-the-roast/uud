# guide on making lua cards

## Table of Contents

3.  [Basic Card Structure](#basic-card-structure)
4.  [Card Types](#card-types)
5.  [Card Properties](#card-properties)
6.  [Keywords](#keywords)
7.  [Triggers](#triggers)
8.  [Game Context Methods](#game-context-methods)
9.  [Abilities](#abilities)
10. [Examples](#examples)

## Basic Card Structure

```lua
return CardType("Card Name")
    :mana_cost("{cost}")
    :colors({"Color1", "Color2"})
    :oracle_text("Card ability text")
    :build()

```

YOU MUST :build() ALL CARDS!!!!

---

## Card Types

- **`Creature(name)`** - Creatures with power and toughness
- **`Instant(name)`** - Instant spells
- **`Sorcery(name)`** - Sorcery spells
- **`Enchantment(name)`** - Global enchantments
- **`Artifact(name)`** - Artifacts
- **`Planeswalker(name)`** - Planeswalker cards
- **`Land(name)`** - Land cards

---

## Card Properties

### Mana Cost

```lua
:mana_cost("{2}{B}{B}")
:mana_cost("{G}")
:mana_cost("{1}{U}")
```

**Mana Cost Syntax:**

- `{X}` - Generic mana (X = 0-9)
- `{W}` - White mana
- `{U}` - Blue mana
- `{B}` - Black mana
- `{R}` - Red mana
- `{G}` - Green mana

### Colors

```lua
:colors({"Black"})
:colors({"Black", "Red"})
:colors({"Blue", "Black", "Green"})
```

Valid color names

- `"White"`
- `"Blue"`
- `"Black"`
- `"Red"`
- `"Green"`

### Oracle Text

```lua
:oracle_text("Meow card\nWhenever another creature you control enters, you meow.")
```

### Flavor Text

```lua
:flavor_text("The meowl meowd meowingly")
```

### Subtypes

Can be anything

```lua
:subtypes({"Elf", "Assassin"})
:subtypes({"Equipment"})
:subtypes({"Goblin", "Berserker"})
```

### Power and Toughness (Creatures only)

```lua
:power_toughness(1, 1)
:power_toughness(6, 6)
:power_toughness(5, 5)
```

### Keywords

Add keywords in two ways:

**Using methods:**

```lua
:flying()
:deathtouch()
:lifelink()
:vigilance()
:haste()
```

**Using strings:**

```lua
:keywords({"Flying", "Deathtouch"})
:keywords({"Vigilance", "Haste", "Lifelink"})
```

---

## Keywords

Available keyword methods

| Keyword        | Method              | String Name        |
| -------------- | ------------------- | ------------------ |
| Flying         | `:flying()`         | `"Flying"`         |
| Trample        | `:trample()`        | `"Trample"`        |
| Haste          | `:haste()`          | `"Haste"`          |
| First Strike   | `:first_strike()`   | `"First Strike"`   |
| Double Strike  | `:double_strike()`  | `"Double Strike"`  |
| Deathtouch     | `:deathtouch()`     | `"Deathtouch"`     |
| Lifelink       | `:lifelink()`       | `"Lifelink"`       |
| Vigilance      | `:vigilance()`      | `"Vigilance"`      |
| Reach          | `:reach()`          | `"Reach"`          |
| Hexproof       | `:hexproof()`       | `"Hexproof"`       |
| Indestructible | `:indestructible()` | `"Indestructible"` |
| Menace         | `:menace()`         | `"Menace"`         |
| Defender       | `:defender()`       | `"Defender"`       |
| Flash          | `:flash()`          | `"Flash"`          |
| Wither         | `:wither()`         | `"Wither"`         |
| Changeling     | `:changeling()`     | `"Changeling"`     |
| Convoke        | `:convoke()`        | `"Convoke"`        |

Cards can have multiple chained keyword methods

```lua
local card = Creature("Flying Lifelinker")
    :colors({"White"})
    :power_toughness(2, 2)
    :flying()
    :lifelink()
    :build()
```

Example using multiptrle string keywords

```lua
local card = Creature("Flying Lifelinker")
    :colors({"White"})
    :power_toughness(2, 2)
    :keywords({"Flying", "Lifelink"})
    :build()
```

---

## Triggers

### All Available Triggers

| Trigger                    | Method                            |
| -------------------------- | --------------------------------- |
| Enters battlefield         | `:on_enter_battlefield(fn)`       |
| Leaves battlefield         | `:on_leave_battlefield(fn)`       |
| Dies/Destroyed             | `:on_death(fn)`                   |
| Spell cast (this card)     | `:on_cast(fn)`                    |
| Card drawn                 | `:on_draw(fn)`                    |
| Taps                       | `:on_tap(fn)`                     |
| Untaps                     | `:on_untap(fn)`                   |
| Deals damage               | `:on_damage_dealt(fn)`            |
| Takes damage               | `:on_damage_taken(fn)`            |
| Beginning of upkeep        | `:on_beginning_of_upkeep(fn)`     |
| End of turn                | `:on_end_of_turn(fn)`             |
| Attacks                    | `:on_attack(fn)`                  |
| Blocks                     | `:on_block(fn)`                   |
| Discarded                  | `:on_discard(fn)`                 |
| Sacrificed                 | `:on_sacrifice(fn)`               |
| Exiled                     | `:on_exile(fn)`                   |
| Returned to hand           | `:on_return_to_hand(fn)`          |
| Milled from library        | `:on_milled(fn)`                  |
| Becomes targeted           | `:on_targeted(fn)`                |
| Spell countered            | `:on_countered(fn)`               |
| Beginning of combat        | `:on_beginning_of_combat(fn)`     |
| End of combat              | `:on_end_of_combat(fn)`           |
| Blocked by creature        | `:on_blocked_by(fn)`              |
| Blocks a creature          | `:on_blocks_creature(fn)`         |
| Deals combat damage        | `:on_deals_combat_damage(fn)`     |
| Deals damage to player     | `:on_deals_player_damage(fn)`     |
| Player gains life          | `:on_life_gain(fn)`               |
| Player loses life          | `:on_life_loss(fn)`               |
| Mana added                 | `:on_mana_added(fn)`              |
| Another creature enters    | `:on_another_creature_enters(fn)` |
| Another creature dies      | `:on_another_creature_dies(fn)`   |
| Land enters (landfall)     | `:on_landfall(fn)`                |
| Artifact enters            | `:on_artifact_enters(fn)`         |
| Enchantment enters         | `:on_enchantment_enters(fn)`      |
| Any spell cast (by anyone) | `:on_spell_cast(fn)`              |
| Instant or sorcery cast    | `:on_instant_or_sorcery_cast(fn)` |
| Beginning of end step      | `:on_beginning_of_end_step(fn)`   |
| Beginning of draw step     | `:on_beginning_of_draw_step(fn)`  |
| Beginning of main phase    | `:on_beginning_of_main_phase(fn)` |

### Trigger Function Parameters

All triggers take 2 params:

```lua
:on_enter_battlefield(function(ctx, event)
    -- ctx is access to game state and methods
    -- event is details about the trigger
end)
```

### Accessing Event Information

```lua
:on_enter_battlefield(function(ctx, event)
    local player_id = event.player_id  -- ID of player controlling card
    local source_id = event.source_id  -- ID of the card itself
    local target_id = event.target_id  -- ID of affected permanent
end)
```

### Trigger Examples

```lua
:on_enter_battlefield(function(ctx, event)
    ctx:gain_life(event.player_id, 2)
end)

-- Trigger requiring a target
:on_cast(function(ctx, event)
    local target = ctx:choose_target(event.player_id, "creature")
    if target ~= 0 then
        ctx:deal_damage(event.source_id, target, 2)
    end
end)

-- multiple things
:on_death(function(ctx, event)
    ctx:draw_cards(event.player_id, 1)
    ctx:mill(event.player_id, 1)
end)
```

---

## Game Context Methods

### Player Actions

```lua
ctx:draw_cards(player_id, count)           -- Draw cards from library
ctx:discard_cards(player_id, count)        -- Discard cards from hand
ctx:gain_life(player_id, amount)           -- Gain life
ctx:lose_life(player_id, amount)           -- Lose life
ctx:add_mana(player_id, color, amount)     -- Add mana (e.g., "White", "Black", "Red", "Green", "Blue")
ctx:blight(player_id, count)               -- Apply -1/-1 counters on player's creatures
```

### Permanent Actions

```lua
ctx:deal_damage(source_id, target_id, amount)  -- Deal damage from source to target
ctx:destroy_permanent(permanent_id)            -- Destroy a permanent
ctx:exile_card(card_id)                        -- Exile a card
ctx:return_to_hand(card_id)                    -- Return card to owner's hand
ctx:tap_permanent(permanent_id)                -- Tap a permanent
ctx:untap_permanent(permanent_id)              -- Untap a permanent
ctx:sacrifice(player_id, permanent_id)        -- Sacrifice a permanent
ctx:animate(permanent_id, power, toughness)   -- Turn noncreature into creature until EoT
ctx:attach(equipment_id, target_id)           -- Attach equipment/aura to target
```

### Library & Graveyard

```lua
ctx:mill(player_id, count)                 -- Mill cards from library
ctx:surveil(player_id, count)              -- Surveil (look at top N, arrange order)
ctx:search_library(player_id, filter)      -- Search library for card matching filter
ctx:search_library_to_battlefield(player_id, filter, tapped)  -- Search and put onto battlefield
ctx:return_from_graveyard(player_id, card_id)  -- Return card from graveyard to hand
```

### Token Creation

```lua
ctx:create_token(name, type, power, toughness)           -- Create creature token
ctx:create_token_tapped(name, type, power, toughness)    -- Create creature token tapped
```

### Counters

```lua
ctx:add_counter(permanent_id, counter_type, count)       -- Add counters (+1/+1, -1/-1, etc.)
ctx:remove_counter(permanent_id, counter_type, count)    -- Remove counters
ctx:get_counters(permanent_id, counter_type)             -- Get counter count
```

### Combat

```lua
ctx:fight(creature_a, creature_b)          -- Make two creatures fight each other
```

### Targeting & Choice

```lua
ctx:choose_target(player_id, target_type)           -- Prompt player for target selection
ctx:choose_mode(player_id, min_choices, max_choices, total_modes)  -- Choose modes
ctx:choose_creature_type(player_id)                 -- Choose creature type from player
ctx:choose_color(player_id)                         -- Choose color from player
ctx:player_may(player_id, prompt)                   -- Ask yes/no question
```

### Permanent Modification

```lua
ctx:modify_power_toughness(permanent_id, power_mod, toughness_mod)  -- Modify P/T
ctx:get_power(permanent_id)                         -- Get power
ctx:get_toughness(permanent_id)                     -- Get toughness
ctx:grant_keyword(permanent_id, keyword)            -- Give keyword to permanent
ctx:remove_keyword(permanent_id, keyword)           -- Remove keyword from permanent
```

### Permanent Information

```lua
ctx:get_subtypes(permanent_id)              -- Get list of subtypes
ctx:get_mana_value(permanent_id)            -- Get mana value
```

### Query - Creature Lists

```lua
ctx:get_all_creatures()                     -- Get all creatures on battlefield
ctx:get_permanents_with_type(player_id, type)   -- Get permanents of type
ctx:get_permanents_with_subtype(player_id, subtype)  -- Get permanents with subtype
```

### Query - Graveyard

```lua
ctx:get_cards_in_graveyard(player_id)       -- Get cards in graveyard
ctx:get_graveyard_cards_with_subtype(player_id, subtype)  -- Get graveyard cards with subtype
```

### Query - Player Information

```lua
ctx:get_opponents(player_id)                -- Get opposing player IDs
ctx:get_life_total(player_id)               -- Get current life total
```

### Query - Card/Permanent Info

```lua
ctx:get_controller(permanent_id)            -- Get player controlling permanent
ctx:get_owner(card_id)                      -- Get owner of card
ctx:get_permanent_id(card_instance_id)      -- Convert card instance to permanent ID
ctx:get_card_zone(card_id)                  -- Get zone
ctx:get_current_phase()                     -- Get current phase as string
```

### Delayed Triggers

```lua
ctx:register_delayed_trigger(permanent_id, event_type, effect_description)  -- Register trigger for later
```

### Target Types for `choose_target()`

```lua
-- Basic permanents
"creature"
"artifact"
"enchantment"
"land"
"instant_or_sorcery"

-- Player targeting
"player"
"player_opponent"

-- Creature filters
"creature_you_control"
"creature_opponent"
"creature_with_flying"
"creature_in_graveyard"
"creature_mv2_or_less"  -- Creatures with mana value 2 or less

-- Subtype-specific
"elf_you_control"
-- Can use any creature type: "goblin_you_control", "knight_you_control", etc.

-- Card filters
"card_in_graveyard"
```

Returns 0 if no valid target selected.

### Example Usage

```lua
:on_cast(function(ctx, event)
    local opponent = ctx:choose_target(event.player_id, "player_opponent")
    if opponent ~= 0 then
        ctx:deal_damage(event.source_id, opponent, 3)
    end
end)

:on_enter_battlefield(function(ctx, event)
    ctx:gain_life(event.player_id, 2)
    local creatures = ctx:get_all_creatures()
    for _, creature in ipairs(creatures) do
        ctx:add_counter(creature, "+1/+1", 1)
    end
end)
```

---

## Abilities

### Triggered Abilities

Use `:on_*()` methods shown in [Triggers](#triggers) section.

### Activated Abilities

Abilities a player can activate:

```lua
:activated_ability(cost, description, function(ctx, event)
    -- Ability implementation
end, sorcery_speed_only)
```

Parameters:

- `cost` (string): Mana cost like `"{2}{B}"` or special costs like `"Pay 1 life"`
- `description` (string):
- `function` (fn): Implementation function
- `sorcery_speed_only` (boolean, optional): `true` if only during main phases

Example:

```lua
:activated_ability("{2}", "Equip {2}", function(ctx, event)
    local target = ctx:choose_target(event.player_id, "creature_you_control")
    if target ~= 0 then
        ctx:attach(event.source_id, target)
    end
end, true)

:activated_ability("Pay 1 life", "Blight 2", function(ctx, event)
    ctx:lose_life(event.player_id, 1)
    local opponent = ctx:choose_target(event.player_id, "player_opponent")
    if opponent ~= 0 then
        ctx:blight(opponent, 2)
    end
end)
```

### Static Abilities

Example:

```lua
:static_ability("Equipped creature gets +1/+2.", function(ctx, event)
    ctx:modify_power_toughness(event.target_id, 1, 2)
end)
```

### Modal Abilities

```lua
:modal(min_choices, max_choices)
:mode("Option 1 text", function(ctx, event)
    -- Handle option 1
end)
:mode("Option 2 text", function(ctx, event)
    -- Handle option 2
end)
```

Parameters:

- `min_choices`: min modes to choose
- `max_choices`: max modes to choose

Example:

```lua
:modal(1, 1)
:mode("Destroy target creature with flying", function(ctx, event)
    local target = ctx:choose_target(event.player_id, "creature_with_flying")
    if target ~= 0 then
        ctx:destroy_permanent(target)
    end
end)
:mode("Destroy target enchantment", function(ctx, event)
    local target = ctx:choose_target(event.player_id, "enchantment")
    if target ~= 0 then
        ctx:destroy_permanent(target)
    end
end)
```

Basically:

```lua
:modal(1, 2)
:mode("Option A", function(ctx, event)
end)
:mode("Option B", function(ctx, event)
end)
```
