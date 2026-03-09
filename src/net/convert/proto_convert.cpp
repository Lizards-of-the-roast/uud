#include "proto_convert.hpp"

namespace convert {

Card_Type From_Proto(cle::proto::CardType proto) {
    switch (proto) {
        case cle::proto::CARD_TYPE_CREATURE:
            return Card_Type::Creature;
        case cle::proto::CARD_TYPE_INSTANT:
            return Card_Type::Instant;
        case cle::proto::CARD_TYPE_SORCERY:
            return Card_Type::Sorcery;
        case cle::proto::CARD_TYPE_ENCHANTMENT:
            return Card_Type::Enchantment;
        case cle::proto::CARD_TYPE_ARTIFACT:
            return Card_Type::Artifact;
        case cle::proto::CARD_TYPE_PLANESWALKER:
            return Card_Type::Planeswalker;
        case cle::proto::CARD_TYPE_LAND:
            return Card_Type::Land;
        default:
            return Card_Type::Creature;
    }
}

Mana_Color From_Proto(cle::proto::ManaColor proto) {
    switch (proto) {
        case cle::proto::MANA_COLOR_COLORLESS:
            return Mana_Color::Colorless;
        case cle::proto::MANA_COLOR_WHITE:
            return Mana_Color::White;
        case cle::proto::MANA_COLOR_BLUE:
            return Mana_Color::Blue;
        case cle::proto::MANA_COLOR_BLACK:
            return Mana_Color::Black;
        case cle::proto::MANA_COLOR_RED:
            return Mana_Color::Red;
        case cle::proto::MANA_COLOR_GREEN:
            return Mana_Color::Green;
        default:
            return Mana_Color::Colorless;
    }
}

Phase From_Proto(mtg::proto::Phase proto) {
    switch (proto) {
        case mtg::proto::PHASE_UNTAP:
            return Phase::Untap;
        case mtg::proto::PHASE_UPKEEP:
            return Phase::Upkeep;
        case mtg::proto::PHASE_DRAW:
            return Phase::Draw;
        case mtg::proto::PHASE_MAIN_1:
            return Phase::Main_1;
        case mtg::proto::PHASE_BEGINNING_OF_COMBAT:
            return Phase::Beginning_Of_Combat;
        case mtg::proto::PHASE_DECLARE_ATTACKERS:
            return Phase::Declare_Attackers;
        case mtg::proto::PHASE_DECLARE_BLOCKERS:
            return Phase::Declare_Blockers;
        case mtg::proto::PHASE_FIRST_STRIKE_DAMAGE:
            return Phase::First_Strike_Damage;
        case mtg::proto::PHASE_COMBAT_DAMAGE:
            return Phase::Combat_Damage;
        case mtg::proto::PHASE_END_OF_COMBAT:
            return Phase::End_Of_Combat;
        case mtg::proto::PHASE_MAIN_2:
            return Phase::Main_2;
        case mtg::proto::PHASE_END_STEP:
            return Phase::End_Step;
        case mtg::proto::PHASE_CLEANUP:
            return Phase::Cleanup;
        default:
            return Phase::Untap;
    }
}

Zone_Type From_Proto(mtg::proto::ZoneType proto) {
    switch (proto) {
        case mtg::proto::ZONE_LIBRARY:
            return Zone_Type::Library;
        case mtg::proto::ZONE_HAND:
            return Zone_Type::Hand;
        case mtg::proto::ZONE_BATTLEFIELD:
            return Zone_Type::Battlefield;
        case mtg::proto::ZONE_GRAVEYARD:
            return Zone_Type::Graveyard;
        case mtg::proto::ZONE_EXILE:
            return Zone_Type::Exile;
        case mtg::proto::ZONE_STACK:
            return Zone_Type::Stack;
        case mtg::proto::ZONE_COMMAND:
            return Zone_Type::Command;
        default:
            return Zone_Type::Library;
    }
}

Mana_Cost From_Proto(const cle::proto::ManaCost &proto) {
    Mana_Cost cost;
    cost.colorless = proto.colorless();
    cost.white = proto.white();
    cost.blue = proto.blue();
    cost.black = proto.black();
    cost.red = proto.red();
    cost.green = proto.green();
    cost.x_count = proto.x_count();
    for (const auto &h : proto.hybrid_costs())
        cost.hybrid_costs.push_back({From_Proto(h.primary()), From_Proto(h.secondary())});
    return cost;
}

Mana_Pool From_Proto(const mtg::proto::ManaPoolProto &proto) {
    return {
        .white = proto.white(),
        .blue = proto.blue(),
        .black = proto.black(),
        .red = proto.red(),
        .green = proto.green(),
        .colorless = proto.colorless(),
    };
}

Counter From_Proto(const mtg::proto::Counter &proto) {
    return {.type = proto.type(), .count = proto.count()};
}

Card From_Proto(const cle::proto::CardData &proto) {
    Card card;
    card.name = proto.name();
    card.type = From_Proto(proto.type());
    card.mana_cost = From_Proto(proto.mana_cost());
    card.colors = proto.colors();
    card.oracle_text = proto.oracle_text();
    card.flavor_text = proto.flavor_text();
    for (const auto &s : proto.subtypes())
        card.subtypes.push_back(s);
    for (const auto &k : proto.keywords())
        card.keywords.push_back(k);
    if (proto.has_creature_stats())
        card.creature_stats =
            Creature_Stats{proto.creature_stats().power(), proto.creature_stats().toughness()};
    card.instance_id = proto.instance_id();
    for (const auto &t : proto.trigger_types())
        card.trigger_types.push_back(t);
    for (const auto &a : proto.activated_abilities())
        card.activated_abilities.push_back(
            {a.cost_text(), a.effect_text(), a.sorcery_speed_only()});
    for (const auto &s : proto.static_abilities())
        card.static_abilities.push_back({s.description()});
    if (proto.has_modal()) {
        Modal_Ability modal;
        modal.min_choices = proto.modal().min_choices();
        modal.max_choices = proto.modal().max_choices();
        for (const auto &m : proto.modal().modes())
            modal.modes.push_back({m.text()});
        card.modal = modal;
    }
    return card;
}

Permanent_State From_Proto(const mtg::proto::PermanentState &proto) {
    Permanent_State perm;
    perm.permanent_id = proto.permanent_id();
    perm.card = From_Proto(proto.card());
    perm.controller_id = proto.controller_id();
    perm.owner_id = proto.owner_id();
    perm.tapped = proto.tapped();
    for (const auto &c : proto.counters())
        perm.counters.push_back(From_Proto(c));
    perm.damage_marked = proto.damage_marked();
    perm.summoning_sick = proto.summoning_sick();
    for (const auto &k : proto.granted_keywords())
        perm.granted_keywords.push_back(k);
    perm.power_modifier = proto.power_modifier();
    perm.toughness_modifier = proto.toughness_modifier();
    if (proto.has_attached_to())
        perm.attached_to = proto.attached_to();
    for (const auto &a : proto.attachments())
        perm.attachments.push_back(a);
    perm.is_token = proto.is_token();
    return perm;
}

Stack_Entry From_Proto(const mtg::proto::StackEntry &proto) {
    Stack_Entry entry;
    entry.entry_id = proto.entry_id();
    if (proto.has_spell())
        entry.spell = From_Proto(proto.spell());
    if (proto.source_case() == mtg::proto::StackEntry::kAbilityDescription)
        entry.ability_description = proto.ability_description();
    entry.controller_id = proto.controller_id();
    for (const auto &t : proto.targets())
        entry.targets.push_back(t);
    return entry;
}

Player_State From_Proto(const mtg::proto::PlayerState &proto) {
    Player_State player;
    player.player_id = proto.player_id();
    player.username = proto.username();
    player.life_total = proto.life_total();
    player.poison_counters = proto.poison_counters();
    player.mana_pool = From_Proto(proto.mana_pool());
    player.hand_count = proto.hand_count();
    player.library_count = proto.library_count();
    for (const auto &c : proto.hand())
        player.hand.push_back(From_Proto(c).instance_id);
    for (const auto &p : proto.battlefield())
        player.battlefield.push_back(From_Proto(p).permanent_id);
    for (const auto &c : proto.graveyard())
        player.graveyard.push_back(From_Proto(c).instance_id);
    for (const auto &c : proto.exile())
        player.exile.push_back(From_Proto(c).instance_id);
    player.has_priority = proto.has_priority();
    player.lands_played_this_turn = proto.lands_played_this_turn();
    return player;
}

Game_Snapshot From_Proto(const mtg::proto::GameSnapshot &proto) {
    Game_Snapshot snap;
    snap.game_id = proto.game_id();
    snap.current_phase = From_Proto(proto.current_phase());
    snap.active_player_id = proto.active_player_id();
    snap.priority_player_id = proto.priority_player_id();
    snap.turn_number = proto.turn_number();
    for (const auto &p : proto.players())
        snap.players.push_back(From_Proto(p));
    for (const auto &s : proto.stack())
        snap.stack.push_back(From_Proto(s));
    return snap;
}

Action_Prompt From_Proto(const mtg::proto::ActionPrompt &proto) {
    Action_Prompt prompt;
    prompt.player_id = proto.player_id();
    prompt.prompt_id = proto.prompt_id();

    switch (proto.prompt_type_case()) {
        case mtg::proto::ActionPrompt::kPriority: {
            Priority_Prompt p;
            for (const auto &a : proto.priority().legal_actions())
                p.legal_actions.push_back(a);
            for (const auto &id : proto.priority().castable_card_ids())
                p.castable_card_ids.push_back(id);
            for (const auto &id : proto.priority().activatable_permanent_ids())
                p.activatable_permanent_ids.push_back(id);
            p.can_play_land = proto.priority().can_play_land();
            prompt.prompt = p;
            break;
        }
        case mtg::proto::ActionPrompt::kTarget: {
            Target_Prompt t;
            t.filter = proto.target().filter();
            for (const auto &id : proto.target().legal_targets())
                t.legal_targets.push_back(id);
            prompt.prompt = t;
            break;
        }
        case mtg::proto::ActionPrompt::kModeChoice: {
            Mode_Choice_Prompt m;
            m.min_choices = proto.mode_choice().min_choices();
            m.max_choices = proto.mode_choice().max_choices();
            for (const auto &mode : proto.mode_choice().modes())
                m.modes.push_back(mode);
            prompt.prompt = m;
            break;
        }
        case mtg::proto::ActionPrompt::kYesNo:
            prompt.prompt = Yes_No_Prompt{proto.yes_no().question()};
            break;
        case mtg::proto::ActionPrompt::kAttackers: {
            Attacker_Prompt a;
            for (const auto &id : proto.attackers().eligible_attackers())
                a.eligible_attackers.push_back(id);
            for (const auto &id : proto.attackers().defending_players())
                a.defending_players.push_back(id);
            prompt.prompt = a;
            break;
        }
        case mtg::proto::ActionPrompt::kBlockers: {
            Blocker_Prompt b;
            for (const auto &id : proto.blockers().eligible_blockers())
                b.eligible_blockers.push_back(id);
            for (const auto &id : proto.blockers().attacking_creatures())
                b.attacking_creatures.push_back(id);
            prompt.prompt = b;
            break;
        }
        case mtg::proto::ActionPrompt::kDiscard: {
            Discard_Prompt d;
            d.count = proto.discard().count();
            for (const auto &c : proto.discard().hand())
                d.hand.push_back(From_Proto(c));
            prompt.prompt = d;
            break;
        }
        case mtg::proto::ActionPrompt::kColorChoice:
            prompt.prompt = Color_Choice_Prompt{};
            break;
        case mtg::proto::ActionPrompt::kCreatureType:
            prompt.prompt = Creature_Type_Prompt{};
            break;
        case mtg::proto::ActionPrompt::kManaPayment:
            prompt.prompt = Mana_Payment_Prompt{
                proto.mana_payment().cost_description(),
                From_Proto(proto.mana_payment().available()),
            };
            break;
        case mtg::proto::ActionPrompt::kOrderBlockers: {
            Order_Blockers_Prompt o;
            o.attacker_id = proto.order_blockers().attacker_id();
            for (const auto &id : proto.order_blockers().unordered_blockers())
                o.unordered_blockers.push_back(id);
            prompt.prompt = o;
            break;
        }
        case mtg::proto::ActionPrompt::kDamageAssignment: {
            Damage_Assignment_Prompt da;
            da.attacker_id = proto.damage_assignment().attacker_id();
            for (const auto &id : proto.damage_assignment().ordered_blockers())
                da.ordered_blockers.push_back(id);
            da.total_damage = proto.damage_assignment().total_damage();
            da.defending_player_id = proto.damage_assignment().defending_player_id();
            prompt.prompt = da;
            break;
        }
        default:
            prompt.prompt = Priority_Prompt{};
            break;
    }
    return prompt;
}

Game_Event From_Proto(const mtg::proto::GameEvent &proto) {
    Game_Event ge;
    ge.game_id = proto.game_id();
    ge.sequence_number = proto.sequence_number();

    switch (proto.event_case()) {
        case mtg::proto::GameEvent::kCardDrawn:
            ge.event = Card_Drawn_Event{proto.card_drawn().player_id(),
                                        From_Proto(proto.card_drawn().card())};
            break;
        case mtg::proto::GameEvent::kCardPlayed:
            ge.event = Card_Played_Event{proto.card_played().player_id(),
                                         From_Proto(proto.card_played().card())};
            break;
        case mtg::proto::GameEvent::kPermanentDestroyed:
            ge.event = Permanent_Destroyed_Event{proto.permanent_destroyed().permanent_id(),
                                                 proto.permanent_destroyed().card_name()};
            break;
        case mtg::proto::GameEvent::kDamageDealt:
            ge.event =
                Damage_Dealt_Event{proto.damage_dealt().source_id(),
                                   proto.damage_dealt().target_id(), proto.damage_dealt().amount()};
            break;
        case mtg::proto::GameEvent::kLifeChanged:
            ge.event =
                Life_Changed_Event{proto.life_changed().player_id(),
                                   proto.life_changed().new_total(), proto.life_changed().delta()};
            break;
        case mtg::proto::GameEvent::kPhaseChanged:
            ge.event = Phase_Changed_Event{From_Proto(proto.phase_changed().new_phase()),
                                           proto.phase_changed().active_player_id(),
                                           proto.phase_changed().turn_number()};
            break;
        case mtg::proto::GameEvent::kPriorityChanged:
            ge.event = Priority_Changed_Event{proto.priority_changed().player_id()};
            break;
        case mtg::proto::GameEvent::kAttackDeclared:
            ge.event = Attack_Declared_Event{proto.attack_declared().attacker_id(),
                                             proto.attack_declared().defending_player_id()};
            break;
        case mtg::proto::GameEvent::kBlockDeclared:
            ge.event = Block_Declared_Event{proto.block_declared().blocker_id(),
                                            proto.block_declared().attacker_id()};
            break;
        case mtg::proto::GameEvent::kSpellResolved:
            ge.event = Spell_Resolved_Event{proto.spell_resolved().stack_entry_id(),
                                            proto.spell_resolved().card_name()};
            break;
        case mtg::proto::GameEvent::kAbilityActivated:
            ge.event = Ability_Activated_Event{proto.ability_activated().permanent_id(),
                                               proto.ability_activated().ability_text()};
            break;
        case mtg::proto::GameEvent::kTriggerFired:
            ge.event = Trigger_Fired_Event{proto.trigger_fired().source_id(),
                                           proto.trigger_fired().trigger_type()};
            break;
        case mtg::proto::GameEvent::kTokenCreated:
            ge.event = Token_Created_Event{From_Proto(proto.token_created().token())};
            break;
        case mtg::proto::GameEvent::kZoneTransfer:
            ge.event = Zone_Transfer_Event{
                proto.zone_transfer().card_id(), From_Proto(proto.zone_transfer().from_zone()),
                From_Proto(proto.zone_transfer().to_zone()), proto.zone_transfer().player_id()};
            break;
        case mtg::proto::GameEvent::kCounterChanged:
            ge.event = Counter_Changed_Event{
                proto.counter_changed().permanent_id(), proto.counter_changed().counter_type(),
                proto.counter_changed().new_count(), proto.counter_changed().delta()};
            break;
        case mtg::proto::GameEvent::kManaAdded: {
            Mana_Color color = Mana_Color::Colorless;
            const auto &c = proto.mana_added().color();
            if (c == "W" || c == "white")
                color = Mana_Color::White;
            else if (c == "U" || c == "blue")
                color = Mana_Color::Blue;
            else if (c == "B" || c == "black")
                color = Mana_Color::Black;
            else if (c == "R" || c == "red")
                color = Mana_Color::Red;
            else if (c == "G" || c == "green")
                color = Mana_Color::Green;
            ge.event = Mana_Added_Event{proto.mana_added().player_id(), color,
                                        proto.mana_added().amount()};
        } break;
        case mtg::proto::GameEvent::kGameOver:
            ge.event = Game_Over_Event{proto.game_over().winner_id(), proto.game_over().reason()};
            break;
        case mtg::proto::GameEvent::kSnapshot:
            ge.event = Game_Snapshot_Event{From_Proto(proto.snapshot().snapshot())};
            break;
        case mtg::proto::GameEvent::kActionPrompt:
            ge.event = From_Proto(proto.action_prompt());
            break;
        default:
            ge.event = Game_Over_Event{0, "Unknown event"};
            break;
    }
    return ge;
}

Queue_Status From_Proto(const mtg::proto::QueueStatusResponse &proto) {
    return {
        .matched = proto.matched(),
        .game_id = proto.game_id(),
        .queue_position = proto.queue_position(),
        .estimated_wait_seconds = proto.estimated_wait_seconds(),
    };
}

mtg::proto::ManaPoolProto To_Proto(const Mana_Pool &pool) {
    mtg::proto::ManaPoolProto proto;
    proto.set_white(pool.white);
    proto.set_blue(pool.blue);
    proto.set_black(pool.black);
    proto.set_red(pool.red);
    proto.set_green(pool.green);
    proto.set_colorless(pool.colorless);
    return proto;
}

mtg::proto::PlayerAction To_Proto(const Player_Action &action) {
    mtg::proto::PlayerAction proto;
    proto.set_game_id(action.game_id);
    proto.set_prompt_id(action.prompt_id);

    std::visit(
        [&proto](const auto &a) {
            using T = std::decay_t<decltype(a)>;

            if constexpr (std::is_same_v<T, Pass_Priority_Action>) {
                proto.mutable_pass();
            } else if constexpr (std::is_same_v<T, Play_Card_Action>) {
                proto.mutable_play_card()->set_card_instance_id(a.card_instance_id);
            } else if constexpr (std::is_same_v<T, Play_Land_Action>) {
                proto.mutable_play_land()->set_card_instance_id(a.card_instance_id);
            } else if constexpr (std::is_same_v<T, Activate_Ability_Action>) {
                auto *act = proto.mutable_activate_ability();
                act->set_permanent_id(a.permanent_id);
                act->set_ability_index(a.ability_index);
            } else if constexpr (std::is_same_v<T, Declare_Attackers_Action>) {
                auto *act = proto.mutable_declare_attackers();
                for (const auto &atk : a.attackers) {
                    auto *decl = act->add_attackers();
                    decl->set_creature_id(atk.creature_id);
                    decl->set_defending_player_id(atk.defending_player_id);
                }
            } else if constexpr (std::is_same_v<T, Declare_Blockers_Action>) {
                auto *act = proto.mutable_declare_blockers();
                for (const auto &blk : a.blockers) {
                    auto *decl = act->add_blockers();
                    decl->set_blocker_id(blk.blocker_id);
                    decl->set_attacker_id(blk.attacker_id);
                }
            } else if constexpr (std::is_same_v<T, Select_Target_Action>) {
                proto.mutable_select_target()->set_target_id(a.target_id);
            } else if constexpr (std::is_same_v<T, Select_Mode_Action>) {
                auto *act = proto.mutable_select_mode();
                for (int m : a.chosen_modes)
                    act->add_chosen_modes(m);
            } else if constexpr (std::is_same_v<T, Yes_No_Action>) {
                proto.mutable_yes_no()->set_choice(a.choice);
            } else if constexpr (std::is_same_v<T, Discard_Action>) {
                auto *act = proto.mutable_discard();
                for (uint64_t id : a.card_instance_ids)
                    act->add_card_instance_ids(id);
            } else if constexpr (std::is_same_v<T, Select_Color_Action>) {
                proto.mutable_select_color()->set_color(
                    std::string(1, "CWUBRG"[static_cast<int>(a.color)]));
            } else if constexpr (std::is_same_v<T, Select_Creature_Type_Action>) {
                proto.mutable_select_creature_type()->set_creature_type(a.creature_type);
            } else if constexpr (std::is_same_v<T, Pay_Mana_Action>) {
                *proto.mutable_pay_mana()->mutable_payment() = convert::To_Proto(a.payment);
            } else if constexpr (std::is_same_v<T, Order_Blockers_Action>) {
                auto *act = proto.mutable_order_blockers();
                for (uint64_t id : a.ordered_blocker_ids)
                    act->add_ordered_blocker_ids(id);
            } else if constexpr (std::is_same_v<T, Damage_Assignment_Action>) {
                auto *act = proto.mutable_damage_assignment();
                for (int dmg : a.damage_to_each_blocker)
                    act->add_damage_to_each_blocker(dmg);
                act->set_damage_to_player(a.damage_to_player);
            } else if constexpr (std::is_same_v<T, Concede_Action>) {
                proto.mutable_concede();
            }
        },
        action.action);

    return proto;
}

}  // namespace convert
