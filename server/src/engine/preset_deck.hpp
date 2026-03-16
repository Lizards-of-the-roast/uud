#pragma once

#include <expected>
#include <filesystem>
#include <string>
#include <unordered_map>
#include <vector>

namespace mtg::engine {

class PresetDeckLoader {
public:
    [[nodiscard]] auto load_from_directory(const std::filesystem::path& dir) -> int;

    [[nodiscard]] auto get_deck(const std::string& name) const
        -> std::expected<std::vector<std::string>, std::string>;

    [[nodiscard]] auto available_decks() const -> std::vector<std::string>;

private:
    std::unordered_map<std::string, std::vector<std::string>> decks_;
};

}  // namespace mtg::engine
