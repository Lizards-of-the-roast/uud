#include "resources.hpp"

#include <unordered_map>

#include "core/defer.hpp"
#include "state.hpp"
#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>

Texture_Resource::~Texture_Resource() {
    for (const auto &i : map)
        SDL_DestroyTexture(i.second);
    map.clear();
}

bool Texture_Resource::Destroy(const std::string &key) {
    auto it = map.find(key);
    if (it == map.end())
        return false;
    SDL_DestroyTexture(it->second);
    map.erase(it);
    return true;
}

SDL_Texture *&Texture_Resource::operator[](const std::string &key) {
    auto [entry, inserted] = map.try_emplace(key, nullptr);
    if (!inserted)
        return entry->second;

    SDL_Renderer *renderer = state.renderer;
    if (!renderer)
        return entry->second;

    SDL_Surface *img = IMG_Load(key.c_str());
    if (!img)
        return entry->second;
    defer(SDL_DestroySurface(img));

    SDL_Texture *texture = SDL_CreateTextureFromSurface(renderer, img);
    if (!texture)
        return NULL_texture;

    entry->second = texture;
    return entry->second;
}

Font_Resource::~Font_Resource() {
    this->Clear();
}
void Font_Resource::Clear(void) {
    if (this->map.size() == 0)
        return;

    for (auto &i : this->map) if (i.second)
    {
        TTF_CloseFont(i.second);
        i.second = NULL;
    }

    this->map.clear();
}

bool Font_Resource::Destroy(const std::string &key) {
    auto it = map.find(key);
    if (it == map.end())
        return false;
    TTF_CloseFont(it->second);
    map.erase(it);
    return true;
}

TTF_Font *&Font_Resource::operator[](const std::string &key) {
    auto [it, inserted] = map.try_emplace(key, nullptr);
    if (inserted) {
        // font size can be changed later with TTF_SetFontSize
        it->second = TTF_OpenFont(key.c_str(), 20.0f);
    }
    return it->second;
}
