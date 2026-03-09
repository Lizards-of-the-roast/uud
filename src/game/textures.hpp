#include <unordered_map>
#include <string>
#include <SDL3/SDL.h>

namespace Game {
struct Card_Textures {
    SDL_Texture *Get(std::string name);
    void Set(std::string name, SDL_Texture *texture);
    SDL_Texture *default_texture;
private:
    std::unordered_map<std::string, SDL_Texture *> textures_;
};
extern Card_Textures card_textures;
}//namespace Game
