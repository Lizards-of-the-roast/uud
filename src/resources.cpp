#include <unordered_map>

#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>

#include "resources.hpp"


Texture_Resource::~Texture_Resource()
{
    for (auto i : map)
        SDL_DestroyTexture(i.second);
    map.clear();
}

bool Texture_Resource::Destroy(std::string key)
{
    if ( !map.contains(key) ) return false;
    SDL_DestroyTexture(map[key]);
    map.erase(key);
    return true;
}

SDL_Texture * & Texture_Resource::operator[](const std::string &key)
{
    if ( map.contains(key) )
        return map[key];
    map[key] = NULL;
    if ( !renderer )
        return map[key];

    SDL_Surface *img = IMG_Load(key.c_str());
    if (!img)
        return map[key];
    defer (SDL_DestroySurface(img));

    SDL_Texture *texture = SDL_CreateTextureFromSurface(renderer, img);
    if (!texture)
        return NULL_texture;

    map[key] = texture;
    return map[key];
}

Font_Resource::~Font_Resource()
{
    for (auto i : map)
        TTF_CloseFont(i.second);
    map.clear();
}

bool Font_Resource::Destroy(std::string key)
{
    if ( !map.contains(key) ) return false;
    TTF_CloseFont(map[key]);
    map.erase(key);
    return true;
}

TTF_Font * & Font_Resource::operator[](const std::string &key)
{
    if ( map.contains(key) )
        return map[key];
    //font size can be changed later with TTF_SetFontSize
    TTF_Font *font = TTF_OpenFont(key.c_str(), 20.0f);
    map[key] = font;
    return map[key];
}
