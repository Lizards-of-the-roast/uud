#include "game_state_local.hpp"

#include <algorithm>
#include <iostream>
#include <string>
#include <variant>

#include "game/instances.hpp"
#include <SDL3/SDL.h>

using namespace Game;

void Local_Game_State::Apply_Snapshot(const Game_Snapshot &snapshot) {
    snapshot_ = snapshot;
    has_snapshot_ = true;
}

static bool Stack_Has_Entry(const std::vector<Stack_Entry> &stack, uint64_t id) {
    return std::any_of(stack.begin(), stack.end(),
                       [id](const Stack_Entry &s) { return s.entry_id == id; });
}

static Player_State *Find_Player(Game_Snapshot &snap, uint64_t player_id) {
    for (auto &p : snap.players)
        if (p.player_id == player_id)
            return &p;
    return nullptr;
}

void Local_Game_State::Apply_Event(const Game_Event &event) {
    std::visit(
        [this](const auto &e) {
            using T = std::decay_t<decltype(e)>;

            if constexpr (std::is_same_v<T, Game_Snapshot_Event>) {
                Apply_Snapshot(e.snapshot);
                log_.Add("Game state loaded");

            } else if constexpr (std::is_same_v<T, Game_Over_Event>) {
                std::cerr << "local_state: GAME OVER winner=" << e.winner_id
                          << " reason=" << e.reason << " is_draw=" << e.is_draw << '\n';
                game_over_ = true;
                game_over_winner_ = e.winner_id;
                game_over_reason_ = e.reason;
                game_over_is_draw_ = e.is_draw;
                if (e.is_draw) {
                    game_over_msg_ = "Game Over - Draw";
                } else {
                    game_over_msg_ = "Game Over - Winner ID: " + std::to_string(e.winner_id);
                }
                if (!e.reason.empty())
                    game_over_msg_ += " (" + e.reason + ")";
                log_.Add("GAME OVER: " + game_over_msg_, 0xFFD700FF);

            } else if constexpr (std::is_same_v<T, Life_Changed_Event>) {
                if (has_snapshot_) {
                    if (auto *p = Find_Player(snapshot_, e.player_id)) {
                        int old_life = p->life_total;
                        p->life_total = e.new_total;
                        std::string name = p->username.empty()
                                               ? "Player " + std::to_string(e.player_id)
                                               : p->username;
                        log_.Add(name + " life: " + std::to_string(old_life) + " -> " +
                                     std::to_string(e.new_total),
                                 e.delta < 0 ? 0xFF4040FF : 0x40FF40FF);
                        pending_life_pulses.push_back({e.player_id, e.delta, SDL_GetTicks()});
                    }
                }

            } else if constexpr (std::is_same_v<T, Phase_Changed_Event>) {
                if (has_snapshot_) {
                    uint64_t prev_active = snapshot_.active_player_id;
                    snapshot_.current_phase = e.new_phase;
                    snapshot_.turn_number = e.turn_number;
                    snapshot_.active_player_id = e.active_player_id;
                    phase_just_changed = true;

                    if (e.new_phase == Phase::Untap && e.active_player_id != prev_active)
                        turn_just_changed = true;

                    int key = e.turn_number * 100 + static_cast<int>(e.new_phase);
                    if (key != last_phase_turn_key_) {
                        last_phase_turn_key_ = key;
                        activated_this_phase_.clear();
                    }

                    if (e.new_phase == Phase::Main_2 || e.new_phase == Phase::End_Step ||
                        e.new_phase == Phase::Untap) {
                        for (auto &player : snapshot_.players) {
                            for (auto perm_id : player.battlefield) {
                                auto *perm = const_cast<Permanent_State *>(instances.Find(perm_id));
                                if (perm) {
                                    perm->attacking = false;
                                    perm->attacking_player_id = 0;
                                    perm->blocking = false;
                                    perm->blocking_target_id = 0;
                                }
                            }
                        }
                    }
                }

            } else if constexpr (std::is_same_v<T, Priority_Changed_Event>) {
                if (has_snapshot_) {
                    snapshot_.priority_player_id = e.player_id;
                    for (auto &player : snapshot_.players)
                        player.has_priority = (player.player_id == e.player_id);
                }

            } else if constexpr (std::is_same_v<T, Card_Drawn_Event>) {
                if (has_snapshot_) {
                    instances.Upsert(e.card);
                    if (auto *p = Find_Player(snapshot_, e.player_id)) {
                        if (std::find(p->hand.begin(), p->hand.end(), e.card.instance_id) ==
                            p->hand.end())
                            p->hand.push_back(e.card.instance_id);
                        if (static_cast<int>(p->hand.size()) >= p->hand_count)
                            p->hand_count = static_cast<int>(p->hand.size());
                        else
                            p->hand_count++;
                        if (p->library_count > 0)
                            p->library_count--;
                    }
                    log_.Add("Drew " + (e.card.name.empty() ? "a card" : e.card.name));
                    pending_card_anims.push_back({e.card.instance_id, e.card.name, 2, 0});
                }

            } else if constexpr (std::is_same_v<T, Card_Played_Event>) {
                if (has_snapshot_) {
                    instances.Upsert(e.card);
                    if (auto *p = Find_Player(snapshot_, e.player_id)) {
                        auto it = std::find(p->hand.begin(), p->hand.end(), e.card.instance_id);
                        if (it != p->hand.end()) {
                            p->hand.erase(it);
                            p->hand_count = static_cast<int>(p->hand.size());
                        } else {
                            if (p->hand_count > 0)
                                p->hand_count--;
                        }
                    }
                    log_.Add("Cast " + (e.card.name.empty() ? "a spell" : e.card.name), 0xFFD700FF);
                    pending_card_anims.push_back({e.card.instance_id, e.card.name, 0, 4});
                    if (e.card.type != Card_Type::Land && e.stack_entry_id != 0 &&
                        !Stack_Has_Entry(snapshot_.stack, e.stack_entry_id)) {
                        Stack_Entry entry;
                        entry.entry_id = e.stack_entry_id;
                        entry.spell = e.card;
                        entry.controller_id = e.player_id;
                        snapshot_.stack.push_back(std::move(entry));
                    }
                }

            } else if constexpr (std::is_same_v<T, Permanent_Destroyed_Event>) {
                log_.Add(e.card_name.empty() ? "A permanent was destroyed"
                                             : e.card_name + " was destroyed",
                         0xFF4040FF);
                pending_card_anims.push_back({e.permanent_id, e.card_name, 1, 3});
                if (has_snapshot_) {
                    for (auto &player : snapshot_.players) {
                        auto &bf = player.battlefield;
                        auto it = std::find(bf.begin(), bf.end(), e.permanent_id);
                        if (it != bf.end()) {
                            bf.erase(it);
                            const auto *perm = instances.Find(e.permanent_id);
                            if (perm && !perm->is_token) {
                                player.graveyard.push_back(perm->card);
                            }
                            break;
                        }
                    }
                }

            } else if constexpr (std::is_same_v<T, Permanent_Entered_Battlefield_Event>) {
                if (has_snapshot_) {
                    Permanent_State perm;
                    perm.permanent_id = e.permanent_id;
                    perm.card = e.card.instance_id;
                    perm.controller_id = e.controller_id;
                    perm.owner_id = e.controller_id;
                    perm.tapped = e.tapped;
                    perm.is_token = e.is_token;
                    instances.Upsert(e.card);
                    instances.Upsert(std::move(perm));
                    if (auto *p = Find_Player(snapshot_, e.controller_id)) {
                        if (std::find(p->battlefield.begin(), p->battlefield.end(),
                                      e.permanent_id) == p->battlefield.end())
                            p->battlefield.push_back(e.permanent_id);
                    }
                    pending_card_anims.push_back({e.permanent_id, e.card.name, 4, 1});
                }

            } else if constexpr (std::is_same_v<T, Token_Created_Event>) {
                if (has_snapshot_) {
                    instances.Upsert(e.token);
                    if (auto *p = Find_Player(snapshot_, e.token.controller_id))
                        p->battlefield.push_back(e.token.permanent_id);
                }

            } else if constexpr (std::is_same_v<T, Zone_Transfer_Event>) {
                if (has_snapshot_) {
                    if (auto *p = Find_Player(snapshot_, e.player_id)) {
                        bool is_draw_transfer =
                            (e.from_zone == Zone_Type::Library && e.to_zone == Zone_Type::Hand);

                        if (is_draw_transfer) {
                            Card_Anim_Hint h;
                            h.card_id = e.card_id;
                            h.from_zone = 2;
                            h.to_zone = 0;
                            h.is_opponent = true;
                            pending_card_anims.push_back(std::move(h));
                        }

                        auto remove_from = [&](std::vector<Card_ID> &zone) {
                            auto it = std::find(zone.begin(), zone.end(),
                                                static_cast<Card_ID>(e.card_id));
                            if (it != zone.end())
                                zone.erase(it);
                        };
                        auto remove_perm = [&](std::vector<Permanent_ID> &zone) {
                            auto it = std::find(zone.begin(), zone.end(), e.card_id);
                            if (it != zone.end())
                                zone.erase(it);
                        };

                        switch (e.from_zone) {
                            case Zone_Type::Hand: {
                                size_t old_sz = p->hand.size();
                                remove_from(p->hand);
                                if (p->hand.size() < old_sz)
                                    p->hand_count = static_cast<int>(p->hand.size());
                                else if (p->hand_count > 0)
                                    p->hand_count--;
                                break;
                            }
                            case Zone_Type::Battlefield:
                                remove_perm(p->battlefield);
                                break;
                            case Zone_Type::Graveyard:
                                remove_from(p->graveyard);
                                break;
                            case Zone_Type::Exile:
                                remove_from(p->exile);
                                break;
                            case Zone_Type::Library:
                                if (p->library_count > 0)
                                    p->library_count--;
                                break;
                            default:
                                break;
                        }

                        auto card_id_typed = static_cast<Card_ID>(e.card_id);
                        switch (e.to_zone) {
                            case Zone_Type::Hand:
                                if (std::find(p->hand.begin(), p->hand.end(), card_id_typed) ==
                                    p->hand.end())
                                    p->hand.push_back(card_id_typed);
                                p->hand_count = static_cast<int>(p->hand.size());
                                break;
                            case Zone_Type::Battlefield:
                                if (std::find(p->battlefield.begin(), p->battlefield.end(),
                                              e.card_id) == p->battlefield.end())
                                    p->battlefield.push_back(e.card_id);
                                break;
                            case Zone_Type::Graveyard:
                                if (std::find(p->graveyard.begin(), p->graveyard.end(),
                                              card_id_typed) == p->graveyard.end())
                                    p->graveyard.push_back(card_id_typed);
                                break;
                            case Zone_Type::Exile:
                                if (std::find(p->exile.begin(), p->exile.end(), card_id_typed) ==
                                    p->exile.end())
                                    p->exile.push_back(card_id_typed);
                                break;
                            case Zone_Type::Library:
                                p->library_count++;
                                break;
                            default:
                                break;
                        }
                    }
                }

            } else if constexpr (std::is_same_v<T, Counter_Changed_Event>) {
                if (has_snapshot_) {
                    auto *perm = const_cast<Permanent_State *>(instances.Find(e.permanent_id));
                    if (perm) {
                        bool found = false;
                        for (auto &c : perm->counters) {
                            if (c.type == e.counter_type) {
                                c.count = e.new_count;
                                found = true;
                                break;
                            }
                        }
                        if (!found && e.new_count > 0)
                            perm->counters.push_back({e.counter_type, e.new_count});
                    }
                }

            } else if constexpr (std::is_same_v<T, Mana_Added_Event>) {
                if (has_snapshot_) {
                    if (auto *p = Find_Player(snapshot_, e.player_id)) {
                        switch (e.color) {
                            case Mana_Color::White:
                                p->mana_pool.white += e.amount;
                                break;
                            case Mana_Color::Blue:
                                p->mana_pool.blue += e.amount;
                                break;
                            case Mana_Color::Black:
                                p->mana_pool.black += e.amount;
                                break;
                            case Mana_Color::Red:
                                p->mana_pool.red += e.amount;
                                break;
                            case Mana_Color::Green:
                                p->mana_pool.green += e.amount;
                                break;
                            case Mana_Color::Colorless:
                                p->mana_pool.colorless += e.amount;
                                break;
                        }
                    }
                }

            } else if constexpr (std::is_same_v<T, Damage_Dealt_Event>) {
                if (has_snapshot_) {
                    auto *perm = const_cast<Permanent_State *>(instances.Find(e.target_id));
                    if (perm) {
                        perm->damage_marked += e.amount;
                    } else {
                        if (auto *p = Find_Player(snapshot_, e.target_id)) {
                            int old_life = p->life_total;
                            p->life_total -= e.amount;
                            pending_life_pulses.push_back({e.target_id, -e.amount, SDL_GetTicks()});
                            std::string name = p->username.empty()
                                                   ? "Player " + std::to_string(e.target_id)
                                                   : p->username;
                            log_.Add(name + " takes " + std::to_string(e.amount) + " damage (" +
                                         std::to_string(old_life) + " -> " +
                                         std::to_string(p->life_total) + ")",
                                     0xFF4040FF);
                        }
                    }
                }
                log_.Add(std::to_string(e.amount) + " damage dealt", 0xFF4040FF);
                pending_damage_floats.push_back(
                    {e.target_id, {0, 0}, false, e.amount, SDL_GetTicks()});

            } else if constexpr (std::is_same_v<T, Action_Prompt>) {
                pending_prompt_ = e;

            } else if constexpr (std::is_same_v<T, Draw_Offer_Event>) {
                if (local_user_id_ != 0 && e.from_player_id == local_user_id_) {
                    draw_offered_by_us_ = true;
                    log_.Add("Draw offer sent, waiting for response...");
                } else {
                    draw_offer_from_ = e.from_player_id;
                    log_.Add("Opponent offered a draw!", 0xFFD700FF);
                }

            } else if constexpr (std::is_same_v<T, Draw_Declined_Event>) {
                draw_offer_from_.reset();
                draw_offered_by_us_ = false;

            } else if constexpr (std::is_same_v<T, Attack_Declared_Event>) {
                if (has_snapshot_) {
                    auto *perm = const_cast<Permanent_State *>(instances.Find(e.attacker_id));
                    if (perm) {
                        perm->attacking = true;
                        perm->attacking_player_id = e.defending_player_id;
                        perm->tapped = true;
                        const auto *card = instances.Find(perm->card);
                        log_.Add(
                            (card && !card->name.empty() ? card->name : "Creature") + " attacks",
                            0xFF8040FF);
                    }
                }
            } else if constexpr (std::is_same_v<T, Block_Declared_Event>) {
                if (has_snapshot_) {
                    auto *perm = const_cast<Permanent_State *>(instances.Find(e.blocker_id));
                    if (perm) {
                        perm->blocking = true;
                        perm->blocking_target_id = e.attacker_id;
                    }
                }
            } else if constexpr (std::is_same_v<T, Spell_Resolved_Event>) {
                if (has_snapshot_) {
                    auto &stk = snapshot_.stack;
                    auto it = std::find_if(stk.begin(), stk.end(), [&](const Stack_Entry &s) {
                        return s.entry_id == e.stack_entry_id;
                    });
                    if (it != stk.end())
                        stk.erase(it);
                    log_.Add((e.card_name.empty() ? "Spell" : e.card_name) + " resolved");
                }
            } else if constexpr (std::is_same_v<T, Ability_Activated_Event>) {
                if (has_snapshot_ && e.stack_entry_id != 0 &&
                    !Stack_Has_Entry(snapshot_.stack, e.stack_entry_id)) {
                    Stack_Entry entry;
                    entry.entry_id = e.stack_entry_id;
                    entry.ability_description = e.ability_text;
                    snapshot_.stack.push_back(std::move(entry));
                }
                log_.Add(e.ability_text.empty() ? "Ability activated"
                                                : e.ability_text,
                         0xAA80FFFF);
            } else if constexpr (std::is_same_v<T, Trigger_Fired_Event>) {
                if (has_snapshot_ && e.stack_entry_id != 0 &&
                    !Stack_Has_Entry(snapshot_.stack, e.stack_entry_id)) {
                    Stack_Entry entry;
                    entry.entry_id = e.stack_entry_id;
                    entry.ability_description =
                        e.description.empty() ? e.trigger_type : e.description;
                    entry.controller_id = e.controller_id;
                    snapshot_.stack.push_back(std::move(entry));
                }
                log_.Add(e.description.empty()
                             ? (e.trigger_type.empty() ? "Triggered ability" : e.trigger_type)
                             : e.description,
                         0x80AAFFFF);
            } else if constexpr (std::is_same_v<T, Player_Eliminated_Event>) {
                std::cerr << "local_state: player eliminated id=" << e.player_id
                          << " reason=" << e.reason << '\n';
                log_.Add("Player eliminated: " + e.reason, 0xFF4040FF);
            } else if constexpr (std::is_same_v<T, Rope_Warning_Event>) {
                std::cerr << "local_state: rope warning player=" << e.player_id
                          << " seconds=" << e.seconds_remaining << '\n';
            } else if constexpr (std::is_same_v<T, Game_Log_Entry_Event>) {
                uint32_t color = 0xFFFFFFFF;
                if (e.category == "combat")
                    color = 0xFF6060FF;
                else if (e.category == "spell")
                    color = 0x80CCFFFF;
                else if (e.category == "life")
                    color = 0x60FF60FF;
                log_.Add(e.text, color);
            } else if constexpr (std::is_same_v<T, Unknown_Event>) {
                std::cerr << "game: ignoring unknown event: " << e.description << '\n';
            }
        },
        event.event);
}

bool Local_Game_State::Has_Snapshot() const {
    return has_snapshot_;
}

const Game_Snapshot &Local_Game_State::Snapshot() const {
    return snapshot_;
}

const Player_State *Local_Game_State::My_State(uint64_t my_user_id) const {
    if (!has_snapshot_)
        return nullptr;
    for (const auto &player : snapshot_.players)
        if (player.player_id == my_user_id)
            return &player;
    return nullptr;
}

const Player_State *Local_Game_State::Opponent_State(uint64_t my_user_id) const {
    if (!has_snapshot_)
        return nullptr;
    for (const auto &player : snapshot_.players)
        if (player.player_id != my_user_id)
            return &player;
    return nullptr;
}

Player_State *Local_Game_State::My_State_Mut(uint64_t my_user_id) {
    if (!has_snapshot_)
        return nullptr;
    for (auto &player : snapshot_.players)
        if (player.player_id == my_user_id)
            return &player;
    return nullptr;
}

std::string Local_Game_State::Phase_Name() const {
    if (!has_snapshot_)
        return "Unknown";
    switch (snapshot_.current_phase) {
        case Phase::Untap:
            return "Untap";
        case Phase::Upkeep:
            return "Upkeep";
        case Phase::Draw:
            return "Draw";
        case Phase::Main_1:
            return "Main 1";
        case Phase::Beginning_Of_Combat:
            return "Begin Combat";
        case Phase::Declare_Attackers:
            return "Declare Attackers";
        case Phase::Declare_Blockers:
            return "Declare Blockers";
        case Phase::First_Strike_Damage:
            return "First Strike Damage";
        case Phase::Combat_Damage:
            return "Combat Damage";
        case Phase::End_Of_Combat:
            return "End Combat";
        case Phase::Main_2:
            return "Main 2";
        case Phase::End_Step:
            return "End Step";
        case Phase::Cleanup:
            return "Cleanup";
        default:
            return "Unknown";
    }
}

bool Local_Game_State::Is_Game_Over() const {
    return game_over_;
}

std::string Local_Game_State::Game_Over_Message() const {
    return game_over_msg_;
}

bool Local_Game_State::Has_Prompt() const {
    return pending_prompt_.has_value();
}

const Action_Prompt &Local_Game_State::Pending_Prompt() const {
    return *pending_prompt_;
}

void Local_Game_State::Clear_Prompt() {
    pending_prompt_.reset();
}

bool Local_Game_State::Has_Draw_Offer() const {
    return draw_offer_from_.has_value();
}

uint64_t Local_Game_State::Draw_Offer_From() const {
    return draw_offer_from_.value_or(0);
}

void Local_Game_State::Clear_Draw_Offer() {
    draw_offer_from_.reset();
    draw_offered_by_us_ = false;
}

bool Local_Game_State::Already_Activated(uint64_t permanent_id) const {
    return activated_this_phase_.contains(permanent_id);
}

void Local_Game_State::Mark_Activated(uint64_t permanent_id) {
    activated_this_phase_.insert(permanent_id);
}
