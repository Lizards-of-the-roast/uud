#pragma once

#include <cstdint>
#include <optional>
#include <string>
#include <vector>

#include <cle/core/card_type.hpp>

namespace mtg::engine {

class ZoneManager;
class Player;

struct TargetFilter {
    std::optional<cle::core::CardType> card_type;
    std::optional<std::string> subtype;

    enum class Controller { Any, You, Opponent };
    Controller controller{Controller::Any};

    enum class Zone { Battlefield, Graveyard, Hand, Any };
    Zone zone{Zone::Battlefield};

    std::optional<std::string> required_keyword;
    std::optional<int> max_mana_value;
    bool targets_players{false};
};

[[nodiscard]] auto parse_filter(const std::string& filter) -> TargetFilter;

[[nodiscard]] auto apply_filter(const TargetFilter& filter, uint64_t choosing_player_id,
                                const ZoneManager& zones, const std::vector<Player>& players)
    -> std::vector<uint64_t>;

}  // namespace mtg::engine
