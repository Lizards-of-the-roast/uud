#pragma once

#include <expected>
#include <filesystem>
#include <memory>
#include <string>
#include <unordered_map>

#include <cle/core/card.hpp>
#include <cle/lua/engine.hpp>

namespace mtg::engine {

class CardRegistry {
public:
    CardRegistry();

    [[nodiscard]] auto load_cards_from_directory(const std::filesystem::path& dir) -> int;
    [[nodiscard]] auto create_card_instance(const std::string& name,
                                            cle::lua::CardEngine& engine) const
        -> std::expected<std::shared_ptr<cle::core::Card>, std::string>;
    [[nodiscard]] auto has_card(const std::string& name) const -> bool {
        return card_paths_.contains(name);
    }
    [[nodiscard]] auto get_lua_path(const std::string& name) const
        -> std::optional<std::filesystem::path>;
    [[nodiscard]] auto card_count() const -> size_t { return card_paths_.size(); }
    [[nodiscard]] auto available_cards() const -> std::vector<std::string>;

private:
    cle::lua::CardEngine loading_engine_;
    std::unordered_map<std::string, std::filesystem::path> card_paths_;
};

}  // namespace mtg::engine
