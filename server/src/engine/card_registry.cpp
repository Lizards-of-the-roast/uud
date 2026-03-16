#include "engine/card_registry.hpp"

#include <spdlog/spdlog.h>

namespace mtg::engine {

CardRegistry::CardRegistry() = default;

int CardRegistry::load_cards_from_directory(const std::filesystem::path& dir) {
    int count = 0;
    if (!std::filesystem::exists(dir)) {
        spdlog::warn("Card directory does not exist: {}", dir.string());
        return 0;
    }
    for (const auto& entry : std::filesystem::recursive_directory_iterator(dir)) {
        if (!entry.is_regular_file()) {
            continue;
        }
        if (entry.path().filename() == "card.lua" || entry.path().extension() == ".lua") {
            auto result = loading_engine_.load_card_from_file(entry.path());
            if (result) {
                card_paths_[(*result)->name()] = entry.path();
                ++count;
                spdlog::debug("Registered card: {}", (*result)->name());
            } else {
                spdlog::error("Failed to load card from {}: {}", entry.path().string(),
                              result.error().detail);
            }
        }
    }
    spdlog::info("Loaded {} card definitions from {}", count, dir.string());
    return count;
}

auto CardRegistry::create_card_instance(const std::string& name, cle::lua::CardEngine& engine) const
    -> std::expected<std::shared_ptr<cle::core::Card>, std::string> {
    auto it = card_paths_.find(name);
    if (it == card_paths_.end()) {
        return std::unexpected("Card not found: " + name);
    }
    auto result = engine.load_card_from_file(it->second);
    if (!result) {
        return std::unexpected(result.error().detail);
    }
    return *result;
}

auto CardRegistry::get_lua_path(const std::string& name) const
    -> std::optional<std::filesystem::path> {
    auto it = card_paths_.find(name);
    if (it == card_paths_.end()) {
        return std::nullopt;
    }
    return it->second;
}

auto CardRegistry::available_cards() const -> std::vector<std::string> {
    std::vector<std::string> names;
    names.reserve(card_paths_.size());
    for (const auto& [name, _] : card_paths_) {
        names.push_back(name);
    }
    return names;
}

}  // namespace mtg::engine
