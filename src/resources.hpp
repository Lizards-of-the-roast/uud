#ifndef _RESOURCES_HPP_
#define _RESOURCES_HPP_

#include <unordered_map>
#include <string>

#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>

#include "defer.hpp"

struct Texture_Resource
{
    std::unordered_map<std::string, SDL_Texture *> map;
    SDL_Texture *NULL_texture;

    // Cant use a constructor without State state = {0} style zii
    ~Texture_Resource();

    bool Destroy(const std::string &key);

    SDL_Texture * & operator[](const std::string &key);
};

struct Font_Resource
{
    std::unordered_map<std::string, TTF_Font *> map;
    SDL_Texture *NULL_font;

    // Cant use a constructor without State state = {0} style zii
    ~Font_Resource();

    bool Destroy(const std::string &key);

    TTF_Font * & operator[](const std::string &key);
};

#endif //ifndef _RESOURCES_HPP_
