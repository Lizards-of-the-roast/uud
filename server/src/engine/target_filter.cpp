#include "engine/target_filter.hpp"

#include <algorithm>
#include <charconv>
#include <sstream>

#include "engine/player.hpp"
#include "engine/zone_manager.hpp"
#include <spdlog/spdlog.h>

namespace mtg::engine {

namespace {

auto split_tokens(const std::string& filter) -> std::vector<std::string> {
    std::vector<std::string> tokens;
    std::istringstream stream(filter);
    std::string token;
    while (std::getline(stream, token, '_')) {
        if (!token.empty()) {
            tokens.push_back(token);
        }
    }
    return tokens;
}

auto try_parse_card_type(const std::string& token) -> std::optional<cle::core::CardType> {
    if (token == "creature") {
        return cle::core::CardType::Creature;
    }
    if (token == "enchantment") {
        return cle::core::CardType::Enchantment;
    }
    if (token == "artifact") {
        return cle::core::CardType::Artifact;
    }
    if (token == "land") {
        return cle::core::CardType::Land;
    }
    if (token == "planeswalker") {
        return cle::core::CardType::Planeswalker;
    }
    if (token == "instant") {
        return cle::core::CardType::Instant;
    }
    if (token == "sorcery") {
        return cle::core::CardType::Sorcery;
    }
    if (token == "permanent") {
        return std::nullopt;
    }
    return std::nullopt;
}

auto is_known_subtype(const std::string& token) -> bool {
    static const std::vector<std::string> known = {
        "elf",       "goblin", "human",  "zombie",  "vampire", "angel", "dragon",  "beast",
        "elemental", "wizard", "knight", "soldier", "spirit",  "demon", "merfolk", "warrior",
        "rogue",     "cleric", "druid",  "shaman",  "cat",     "bird",  "wolf",    "bear",
    };
    return std::find(known.begin(), known.end(), token) != known.end();
}

}  // namespace

auto parse_filter(const std::string& filter) -> TargetFilter {
    TargetFilter result;
    auto tokens = split_tokens(filter);
    if (tokens.empty()) {
        return result;
    }

    bool is_permanent_filter = false;

    for (size_t i = 0; i < tokens.size(); ++i) {
        const auto& tok = tokens[i];

        if (tok == "you" || tok == "control") {
            result.controller = TargetFilter::Controller::You;
            continue;
        }
        if (tok == "opponent") {
            result.controller = TargetFilter::Controller::Opponent;
            continue;
        }

        if (tok == "in" && i + 1 < tokens.size()) {
            const auto& zone_tok = tokens[i + 1];
            if (zone_tok == "graveyard") {
                result.zone = TargetFilter::Zone::Graveyard;
            } else if (zone_tok == "hand") {
                result.zone = TargetFilter::Zone::Hand;
            }
            ++i;
            continue;
        }

        if (tok == "with" && i + 1 < tokens.size()) {
            result.required_keyword = tokens[i + 1];
            ++i;
            continue;
        }

        if (tok.size() > 2 && tok.substr(0, 2) == "mv") {
            int val = 0;
            auto [ptr, ec] = std::from_chars(tok.data() + 2, tok.data() + tok.size(), val);
            if (ec == std::errc()) {
                result.max_mana_value = val;
            }
            continue;
        }

        if (tok == "or" || tok == "less") {
            continue;
        }

        if (tok == "player") {
            result.targets_players = true;
            continue;
        }

        if (tok == "card") {
            continue;
        }

        if (tok == "permanent") {
            is_permanent_filter = true;
            continue;
        }

        auto card_type = try_parse_card_type(tok);
        if (card_type) {
            result.card_type = *card_type;
            continue;
        }

        if (is_known_subtype(tok)) {
            result.subtype = tok;
            continue;
        }

        spdlog::debug("Target filter: unknown token '{}' in '{}', treating as subtype", tok,
                      filter);
        result.subtype = tok;
    }

    if (is_permanent_filter && !result.card_type) {
        result.zone = TargetFilter::Zone::Battlefield;
    }

    return result;
}

auto apply_filter(const TargetFilter& filter, uint64_t choosing_player_id, const ZoneManager& zones,
                  const std::vector<Player>& players) -> std::vector<uint64_t> {
    std::vector<uint64_t> legal;

    if (filter.targets_players) {
        for (const auto& p : players) {
            if (!p.is_alive()) {
                continue;
            }
            switch (filter.controller) {
                case TargetFilter::Controller::Any:
                    legal.push_back(p.id());
                    break;
                case TargetFilter::Controller::You:
                    if (p.id() == choosing_player_id) {
                        legal.push_back(p.id());
                    }
                    break;
                case TargetFilter::Controller::Opponent:
                    if (p.id() != choosing_player_id) {
                        legal.push_back(p.id());
                    }
                    break;
            }
        }
        return legal;
    }

    switch (filter.zone) {
        case TargetFilter::Zone::Battlefield: {
            for (const auto& [id, perm] : zones.get_all_permanents()) {
                if (perm.has_keyword("Shroud")) {
                    continue;
                }
                if (perm.has_keyword("Hexproof") && perm.controller_id() != choosing_player_id) {
                    continue;
                }

                if (filter.card_type && perm.card()->type() != *filter.card_type) {
                    continue;
                }

                if (filter.controller == TargetFilter::Controller::You &&
                    perm.controller_id() != choosing_player_id) {
                    continue;
                }
                if (filter.controller == TargetFilter::Controller::Opponent &&
                    perm.controller_id() == choosing_player_id) {
                    continue;
                }

                if (filter.subtype) {
                    auto subtypes = perm.card()->subtypes();
                    bool found = false;
                    for (const auto& st : subtypes) {
                        std::string lower_st = st;
                        std::transform(lower_st.begin(), lower_st.end(), lower_st.begin(),
                                       [](unsigned char c) { return std::tolower(c); });
                        if (lower_st == *filter.subtype) {
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        continue;
                    }
                }

                if (filter.required_keyword && !perm.has_keyword(*filter.required_keyword)) {
                    continue;
                }

                if (filter.max_mana_value &&
                    perm.card()->mana_cost().mana_value() > *filter.max_mana_value) {
                    continue;
                }

                legal.push_back(id);
            }
            break;
        }
        case TargetFilter::Zone::Graveyard: {
            for (const auto& p : players) {
                if (filter.controller == TargetFilter::Controller::You &&
                    p.id() != choosing_player_id) {
                    continue;
                }
                if (filter.controller == TargetFilter::Controller::Opponent &&
                    p.id() == choosing_player_id) {
                    continue;
                }

                for (const auto& card : zones.get_graveyard(p.id())) {
                    if (filter.card_type && card->type() != *filter.card_type) {
                        continue;
                    }
                    if (filter.subtype) {
                        auto subtypes = card->subtypes();
                        bool found = false;
                        for (const auto& st : subtypes) {
                            std::string lower_st = st;
                            std::transform(lower_st.begin(), lower_st.end(), lower_st.begin(),
                                           [](unsigned char c) { return std::tolower(c); });
                            if (lower_st == *filter.subtype) {
                                found = true;
                                break;
                            }
                        }
                        if (!found) {
                            continue;
                        }
                    }
                    if (filter.max_mana_value &&
                        card->mana_cost().mana_value() > *filter.max_mana_value) {
                        continue;
                    }
                    legal.push_back(card->instance_id());
                }
            }
            break;
        }
        case TargetFilter::Zone::Hand: {
            const auto& hand = zones.get_hand(choosing_player_id);
            for (const auto& card : hand) {
                if (filter.card_type && card->type() != *filter.card_type) {
                    continue;
                }
                legal.push_back(card->instance_id());
            }
            break;
        }
        case TargetFilter::Zone::Any: {
            auto bf_filter = filter;
            bf_filter.zone = TargetFilter::Zone::Battlefield;
            auto bf = apply_filter(bf_filter, choosing_player_id, zones, players);
            legal.insert(legal.end(), bf.begin(), bf.end());

            auto gy_filter = filter;
            gy_filter.zone = TargetFilter::Zone::Graveyard;
            auto gy = apply_filter(gy_filter, choosing_player_id, zones, players);
            legal.insert(legal.end(), gy.begin(), gy.end());
        } break;
    }

    return legal;
}

}  // namespace mtg::engine
