#include "textures.hpp"

namespace Game {

Card_Textures card_textures;

SDL_Texture *Card_Textures::Get(std::string name)
{
    if (auto it = this->textures_.find(name); it != this->textures_.end())
        return it->second;
    return default_texture;
}
void Card_Textures::Set(std::string name, SDL_Texture *texture)
{
    this->textures_[name] = texture;
}

} // namespace Game
