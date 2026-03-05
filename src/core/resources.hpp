#pragma once

#include <string>
#include <unordered_map>

#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>

struct Texture_Resource {
    std::unordered_map<std::string, SDL_Texture *> map;
    SDL_Texture *NULL_texture;

    // Cant use a constructor without State state = {0} style zii
    ~Texture_Resource();

    bool Destroy(const std::string &key);

    SDL_Texture *&operator[](const std::string &key);
};

struct Font_Resource {
    std::unordered_map<std::string, TTF_Font *> map;
    SDL_Texture *NULL_font;

    ~Font_Resource();

    void Clear(void);

    bool Destroy(const std::string &key);

    TTF_Font *&operator[](const std::string &key);
};
