#include <set>
#include <string>
#include <unordered_map>

#include <SDL3/SDL.h>

namespace Game {
struct Card_Textures {
    SDL_Texture *Get(std::string name);
    void Set(std::string name, SDL_Texture *texture);
    void Set_Renderer(SDL_Renderer *r) { renderer_ = r; }
    void Scan_Card_Directory(const std::string &base_path);
    SDL_Texture *default_texture = nullptr;

private:
    SDL_Texture *Try_Load(const std::string &name);
    SDL_Renderer *renderer_ = nullptr;
    std::unordered_map<std::string, SDL_Texture *> textures_;
    std::unordered_map<std::string, std::string> slug_to_path_;
    std::set<std::string> load_failed_;
};
extern Card_Textures card_textures;
}  // namespace Game
