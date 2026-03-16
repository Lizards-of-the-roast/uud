return Sorcery("Kapow!")
    :mana_cost("{2}{G}")
    :colors({"Green"})
    :oracle_text("Put a +1/+1 counter on target creature you control. It fights target creature your opponent controls.")
    :flavor_text("People are in danger, I don't have time for your games!")
    :on_cast(function(ctx, event)
        local my_creature = ctx:choose_target(event.player_id, "creature_you_control")
        if my_creature == 0 then return end
        ctx:add_counter(my_creature, "+1/+1", 1)
        local their_creature = ctx:choose_target(event.player_id, "creature_opponent")
        if their_creature == 0 then return end
        ctx:fight(my_creature, their_creature)
    end)
    :build()
