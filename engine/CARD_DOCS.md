# lua card reference

every card is a lua file that returns a builder chain ending in `:build()`. if u forget `:build()` it WILL NOT WORK!!!!

```lua
return Creature("Ankle Biter")
    :mana_cost("{G}")
    :colors({"Green"})
    :subtypes({"Goblin"})
    :power_toughness(1, 1)
    :deathtouch()
    :oracle_text("Deathtouch")
    :build()
```

## card types

`Creature(name)`, `Instant(name)`, `Sorcery(name)`, `Enchantment(name)`, `Artifact(name)`, `Planeswalker(name)`, `Land(name)`

## properties

```lua
:mana_cost("{2}{B}{B}")     -- {W} white, {U} blue, {B} black, {R} red, {G} green, {0}-{9} generic
:colors({"Black", "Red"})   -- White, Blue, Black, Red, Green
:subtypes({"Elf", "Assassin"})
:power_toughness(2, 3)      -- creatures only
:oracle_text("does stuff")
:flavor_text("The meowl meowd meowingly")
```

## keywords

chain keyword methods or pass strings, both work:

```lua
:flying()
:deathtouch()
:lifelink()
-- or
:keywords({"Flying", "Deathtouch", "Lifelink"})
```

available: `flying`, `trample`, `haste`, `first_strike`, `double_strike`, `deathtouch`, `lifelink`, `vigilance`, `reach`, `hexproof`, `indestructible`, `menace`, `defender`, `flash`, `wither`, `changeling`, `convoke`

## triggers

all trigger callbacks take `(ctx, event)`. event has `player_id`, `source_id`, `target_id`

```lua
:on_enter_battlefield(function(ctx, event)
    ctx:gain_life(event.player_id, 2)
end)

:on_cast(function(ctx, event)
    local target = ctx:choose_target(event.player_id, "creature")
    if target ~= 0 then
        ctx:deal_damage(event.source_id, target, 2)
    end
end)

:on_death(function(ctx, event)
    ctx:draw_cards(event.player_id, 1)
    ctx:mill(event.player_id, 1)
end)
```

all triggers: `:on_enter_battlefield`, `:on_leave_battlefield`, `:on_death`, `:on_cast`, `:on_draw`, `:on_tap`, `:on_untap`, `:on_damage_dealt`, `:on_damage_taken`, `:on_beginning_of_upkeep`, `:on_end_of_turn`, `:on_attack`, `:on_block`, `:on_discard`, `:on_sacrifice`, `:on_exile`, `:on_return_to_hand`, `:on_milled`, `:on_targeted`, `:on_countered`, `:on_beginning_of_combat`, `:on_end_of_combat`, `:on_blocked_by`, `:on_blocks_creature`, `:on_deals_combat_damage`, `:on_deals_player_damage`, `:on_life_gain`, `:on_life_loss`, `:on_mana_added`, `:on_another_creature_enters`, `:on_another_creature_dies`, `:on_landfall`, `:on_artifact_enters`, `:on_enchantment_enters`, `:on_spell_cast`, `:on_instant_or_sorcery_cast`, `:on_beginning_of_end_step`, `:on_beginning_of_draw_step`, `:on_beginning_of_main_phase`

## ctx methods

### player stuff

```lua
ctx:draw_cards(player_id, count)
ctx:discard_cards(player_id, count)
ctx:gain_life(player_id, amount)
ctx:lose_life(player_id, amount)
ctx:add_mana(player_id, color, amount)
ctx:blight(player_id, count)                -- -1/-1 counters
```

### permanents

```lua
ctx:deal_damage(source_id, target_id, amount)
ctx:destroy_permanent(permanent_id)
ctx:exile_card(card_id)
ctx:return_to_hand(card_id)
ctx:tap_permanent(permanent_id)
ctx:untap_permanent(permanent_id)
ctx:sacrifice(player_id, permanent_id)
ctx:animate(permanent_id, power, toughness) -- noncreature becomes creature til eot
ctx:attach(equipment_id, target_id)
```

### library/graveyard

```lua
ctx:mill(player_id, count)
ctx:surveil(player_id, count)
ctx:search_library(player_id, filter)
ctx:search_library_to_battlefield(player_id, filter, tapped)
ctx:return_from_graveyard(player_id, card_id)
```

### tokens

```lua
ctx:create_token(name, type, power, toughness)
ctx:create_token_tapped(name, type, power, toughness)
```

### counters

```lua
ctx:add_counter(permanent_id, counter_type, count)    -- "+1/+1", "-1/-1", etc
ctx:remove_counter(permanent_id, counter_type, count)
ctx:get_counters(permanent_id, counter_type)
```

### combat

```lua
ctx:fight(creature_a, creature_b)
```

### targeting/choice

`choose_target` returns 0 if nothing valid

```lua
ctx:choose_target(player_id, target_type)
ctx:choose_mode(player_id, min_choices, max_choices, total_modes)
ctx:choose_creature_type(player_id)
ctx:choose_color(player_id)
ctx:player_may(player_id, prompt)           -- yes/no
```

target types: `"creature"`, `"artifact"`, `"enchantment"`, `"land"`, `"instant_or_sorcery"`, `"player"`, `"player_opponent"`, `"creature_you_control"`, `"creature_opponent"`, `"creature_with_flying"`, `"creature_in_graveyard"`, `"creature_mv2_or_less"`, `"elf_you_control"` (works with any subtype), `"card_in_graveyard"`

### p/t and keywords

```lua
ctx:modify_power_toughness(permanent_id, power_mod, toughness_mod)
ctx:get_power(permanent_id)
ctx:get_toughness(permanent_id)
ctx:grant_keyword(permanent_id, keyword)
ctx:remove_keyword(permanent_id, keyword)
```

### queries

```lua
ctx:get_subtypes(permanent_id)
ctx:get_mana_value(permanent_id)
ctx:get_all_creatures()
ctx:get_permanents_with_type(player_id, type)
ctx:get_permanents_with_subtype(player_id, subtype)
ctx:get_cards_in_graveyard(player_id)
ctx:get_graveyard_cards_with_subtype(player_id, subtype)
ctx:get_opponents(player_id)
ctx:get_life_total(player_id)
ctx:get_controller(permanent_id)
ctx:get_owner(card_id)
ctx:get_permanent_id(card_instance_id)
ctx:get_card_zone(card_id)
ctx:get_current_phase()
```

### delayed triggers

```lua
ctx:register_delayed_trigger(permanent_id, event_type, effect_description)
```

## abilities

### activated

```lua
:activated_ability(cost, description, function(ctx, event)
    -- ...
end, sorcery_speed_only)
```

cost is a string like `"{2}{B}"` or `"Pay 1 life"`. sorcery_speed_only is optional bool (defaults false).

```lua
:activated_ability("{2}", "Equip {2}", function(ctx, event)
    local target = ctx:choose_target(event.player_id, "creature_you_control")
    if target ~= 0 then
        ctx:attach(event.source_id, target)
    end
end, true)
```

### static

```lua
:static_ability("Equipped creature gets +1/+2.", function(ctx, event)
    ctx:modify_power_toughness(event.target_id, 1, 2)
end)
```

### modal

```lua
:modal(min_choices, max_choices)
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
