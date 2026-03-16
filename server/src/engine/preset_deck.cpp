#include "engine/preset_deck.hpp"

#include <fstream>
#include <sstream>

#include <spdlog/spdlog.h>

namespace mtg::engine {

auto PresetDeckLoader::load_from_directory(const std::filesystem::path& dir) -> int {
    int count = 0;
    if (!std::filesystem::exists(dir)) {
        spdlog::warn("Preset deck directory does not exist: {}", dir.string());
        return 0;
    }
    for (const auto& entry : std::filesystem::directory_iterator(dir)) {
        if (entry.path().extension() != ".deck") {
            continue;
        }

        std::ifstream file(entry.path());
        if (!file.is_open()) {
            spdlog::error("Failed to open preset deck: {}", entry.path().string());
            continue;
        }

        auto deck_name = entry.path().stem().string();
        std::vector<std::string> card_list;
        std::string line;

        while (std::getline(file, line)) {
            if (line.empty()) {
                continue;
            }

            auto space_pos = line.find(' ');
            if (space_pos == std::string::npos) {
                spdlog::error("Invalid deck line in {}: {}", entry.path().string(), line);
                continue;
            }

            int copies = 0;
            try {
                copies = std::stoi(line.substr(0, space_pos));
            } catch (const std::exception& e) {
                spdlog::error("Invalid count in {}: {}", entry.path().string(), line);
                continue;
            }

            auto card_name = line.substr(space_pos + 1);
            for (int i = 0; i < copies; ++i) {
                card_list.push_back(card_name);
            }
        }

        if (!card_list.empty()) {
            spdlog::info("Loaded preset deck '{}' with {} cards", deck_name, card_list.size());
            decks_[deck_name] = std::move(card_list);
            ++count;
        }
    }
    return count;
}

auto PresetDeckLoader::get_deck(const std::string& name) const
    -> std::expected<std::vector<std::string>, std::string> {
    auto it = decks_.find(name);
    if (it == decks_.end()) {
        return std::unexpected("Preset deck not found: " + name);
    }
    return it->second;
}

auto PresetDeckLoader::available_decks() const -> std::vector<std::string> {
    std::vector<std::string> names;
    names.reserve(decks_.size());
    for (const auto& [name, _] : decks_) {
        names.push_back(name);
    }
    return names;
}

}  // namespace mtg::engine
