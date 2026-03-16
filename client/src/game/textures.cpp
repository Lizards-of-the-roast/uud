#include "textures.hpp"

#include <algorithm>
#include <cctype>
#include <filesystem>

#include <SDL3_image/SDL_image.h>

namespace Game {

Card_Textures card_textures;

static std::string Name_To_Slug(const std::string &name) {
    std::string slug;
    slug.reserve(name.size());
    for (char ch : name) {
        if (ch == ' ' || ch == '_')
            slug += '-';
        else if (std::isalnum(static_cast<unsigned char>(ch)) || ch == '-')
            slug += static_cast<char>(std::tolower(static_cast<unsigned char>(ch)));
    }
    return slug;
}

void Card_Textures::Scan_Card_Directory(const std::string &base_path) {
    namespace fs = std::filesystem;
    if (!fs::is_directory(base_path))
        return;

    for (const auto &color_dir : fs::directory_iterator(base_path)) {
        if (!color_dir.is_directory())
            continue;
        for (const auto &card_dir : fs::directory_iterator(color_dir.path())) {
            if (!card_dir.is_directory())
                continue;
            auto img_path = card_dir.path() / "card.jpg";
            if (!fs::exists(img_path)) {
                img_path = card_dir.path() / "card.png";
                if (!fs::exists(img_path))
                    continue;
            }
            std::string slug = card_dir.path().filename().string();
            slug_to_path_[slug] = img_path.string();
        }
    }
}

SDL_Texture *Card_Textures::Try_Load(const std::string &name) {
    if (!renderer_ || name.empty())
        return nullptr;
    if (load_failed_.contains(name))
        return nullptr;

    std::string slug = Name_To_Slug(name);
    auto it = slug_to_path_.find(slug);
    if (it == slug_to_path_.end()) {
        load_failed_.insert(name);
        return nullptr;
    }

    SDL_Texture *tex = IMG_LoadTexture(renderer_, it->second.c_str());
    if (!tex) {
        SDL_Log("Failed to load card texture '%s': %s", it->second.c_str(), SDL_GetError());
        load_failed_.insert(name);
        return nullptr;
    }

    textures_[name] = tex;
    return tex;
}

SDL_Texture *Card_Textures::Get(std::string name) {
    if (auto it = this->textures_.find(name); it != this->textures_.end())
        return it->second;

    SDL_Texture *loaded = Try_Load(name);
    if (loaded)
        return loaded;

    return default_texture;
}

void Card_Textures::Set(std::string name, SDL_Texture *texture) {
    this->textures_[name] = texture;
}

}  // namespace Game
