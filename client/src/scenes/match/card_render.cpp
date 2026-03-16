#include "card_render.hpp"

#include <algorithm>
#include <cmath>
#include <string>

#include "game/instances.hpp"

static constexpr uint32_t COLOR_WHITE = 1 << 0;
static constexpr uint32_t COLOR_BLUE = 1 << 1;
static constexpr uint32_t COLOR_BLACK = 1 << 2;
static constexpr uint32_t COLOR_RED = 1 << 3;
static constexpr uint32_t COLOR_GREEN = 1 << 4;

static int Pop_Count(uint32_t c) {
    int n = 0;
    if (c & COLOR_WHITE)
        n++;
    if (c & COLOR_BLUE)
        n++;
    if (c & COLOR_BLACK)
        n++;
    if (c & COLOR_RED)
        n++;
    if (c & COLOR_GREEN)
        n++;
    return n;
}

SDL_Color Card_Frame_Color(const Game::Card &card) {
    if (card.type == Game::Card_Type::Land)
        return {0x8B, 0x6C, 0x3E, 0xFF};  // brown
    int n = Pop_Count(card.colors);
    if (n == 0)
        return {0x71, 0x71, 0x7A, 0xFF};  // zinc/colorless
    if (n > 1)
        return {0xD9, 0x7F, 0x06, 0xFF};  // gold/multicolor
    if (card.colors & COLOR_WHITE)
        return {0xF5, 0xF0, 0xD1, 0xFF};  // amber-light
    if (card.colors & COLOR_BLUE)
        return {0x25, 0x63, 0xEB, 0xFF};  // blue
    if (card.colors & COLOR_BLACK)
        return {0x27, 0x27, 0x2A, 0xFF};  // zinc-dark
    if (card.colors & COLOR_RED)
        return {0xDC, 0x26, 0x26, 0xFF};  // red
    if (card.colors & COLOR_GREEN)
        return {0x16, 0xA3, 0x4A, 0xFF};  // green
    return {0x71, 0x71, 0x7A, 0xFF};
}

static SDL_Color Keyword_Color(const std::string &kw) {
    if (kw == "flying")
        return {0x25, 0x63, 0xEB, 0xB0};
    if (kw == "trample")
        return {0x16, 0xA3, 0x4A, 0xB0};
    if (kw == "deathtouch")
        return {0x7C, 0x3A, 0xED, 0xB0};
    if (kw == "lifelink")
        return {0xD9, 0x7F, 0x06, 0xB0};
    if (kw == "first strike" || kw == "double strike")
        return {0xDC, 0x26, 0x26, 0xB0};
    if (kw == "vigilance")
        return {0xD9, 0x7F, 0x06, 0xB0};
    if (kw == "reach")
        return {0x16, 0xA3, 0x4A, 0xB0};
    if (kw == "hexproof")
        return {0x25, 0x63, 0xEB, 0xB0};
    if (kw == "indestructible")
        return {0xD9, 0x7F, 0x06, 0xB0};
    if (kw == "menace")
        return {0xDC, 0x26, 0x26, 0xB0};
    if (kw == "flash")
        return {0xCA, 0x8A, 0x04, 0xB0};
    if (kw == "defender")
        return {0x52, 0x52, 0x5B, 0xB0};
    if (kw == "haste")
        return {0xDC, 0x26, 0x26, 0xB0};
    return {0x52, 0x52, 0x5B, 0xB0};
}

static void Draw_Filled_Rect(SDL_Renderer *r, float x, float y, float w, float h, SDL_Color c) {
    SDL_SetRenderDrawColor(r, c.r, c.g, c.b, c.a);
    SDL_FRect rect = {x, y, w, h};
    SDL_RenderFillRect(r, &rect);
}

static void Draw_Text_Simple(SDL_Renderer *r, TTF_Font *font, const std::string &text, float x,
                             float y, SDL_Color color, float max_width = 0) {
    if (!font || text.empty())
        return;
    SDL_Surface *surface = TTF_RenderText_Blended(font, text.c_str(), text.size(), color);
    if (!surface)
        return;
    SDL_Texture *tex = SDL_CreateTextureFromSurface(r, surface);
    if (tex) {
        float tw = (float)surface->w;
        float th = (float)surface->h;
        // Scale down to fit within max_width
        if (max_width > 0 && tw > max_width) {
            float scale = max_width / tw;
            tw = max_width;
            th *= scale;
        }
        SDL_FRect dst = {x, y, tw, th};
        SDL_RenderTexture(r, tex, nullptr, &dst);
        SDL_DestroyTexture(tex);
    }
    SDL_DestroySurface(surface);
}

void Draw_Card_Overlay(SDL_Renderer *renderer, const SDL_FRect &rect, const Game::Card &card,
                       const Game::Permanent_State *perm, TTF_Font *font) {
    if (rect.w < 20 || rect.h < 20)
        return;

    const float border = 3.0f;
    SDL_Color frame = Card_Frame_Color(card);
    SDL_SetRenderDrawColor(renderer, frame.r, frame.g, frame.b, frame.a);
    SDL_FRect top = {rect.x, rect.y, rect.w, border};
    SDL_RenderFillRect(renderer, &top);
    SDL_FRect bot = {rect.x, rect.y + rect.h - border, rect.w, border};
    SDL_RenderFillRect(renderer, &bot);
    SDL_FRect left = {rect.x, rect.y, border, rect.h};
    SDL_RenderFillRect(renderer, &left);
    SDL_FRect right = {rect.x + rect.w - border, rect.y, border, rect.h};
    SDL_RenderFillRect(renderer, &right);

    if (font && !card.name.empty()) {
        // Dark background strip for name
        float name_h = std::min(rect.h * 0.12f, 16.0f);
        Draw_Filled_Rect(renderer, rect.x + 2, rect.y + 2, rect.w - 4, name_h,
                         {0x00, 0x00, 0x00, 0xC0});
        Draw_Text_Simple(renderer, font, card.name, rect.x + 4, rect.y + 2,
                         {0xFF, 0xFF, 0xFF, 0xFF}, rect.w - 8);
    }

    if (perm && perm->is_token) {
        float tag_w = std::min(rect.w * 0.35f, 36.0f);
        float tag_h = std::min(rect.h * 0.08f, 10.0f);
        Draw_Filled_Rect(renderer, rect.x + rect.w - tag_w - 2, rect.y + 2, tag_w, tag_h,
                         {0xD9, 0x7F, 0x06, 0xC0});
        if (font)
            Draw_Text_Simple(renderer, font, "TOKEN", rect.x + rect.w - tag_w, rect.y + 2,
                             {0xFF, 0xFF, 0xFF, 0xFF});
    }

    if (perm && perm->summoning_sick) {
        SDL_SetRenderDrawColor(renderer, 0x00, 0x00, 0x00, 0x30);
        for (float d = -rect.h; d < rect.w + rect.h; d += 6.0f) {
            float x1 = rect.x + d;
            float y1 = rect.y;
            float x2 = rect.x + d - rect.h;
            float y2 = rect.y + rect.h;
            x1 = std::clamp(x1, rect.x, rect.x + rect.w);
            x2 = std::clamp(x2, rect.x, rect.x + rect.w);
            SDL_RenderLine(renderer, x1, y1, x2, y2);
        }
    }

    if (card.type == Game::Card_Type::Creature && card.creature_stats.has_value()) {
        int base_pow = card.creature_stats->power;
        int base_tou = card.creature_stats->toughness;
        int eff_pow = base_pow + (perm ? perm->power_modifier : 0);
        int eff_tou = base_tou + (perm ? perm->toughness_modifier : 0);

        // Color: green if boosted, red if reduced, white otherwise.
        SDL_Color pt_color = {0xFF, 0xFF, 0xFF, 0xFF};
        if (perm && (eff_pow > base_pow || eff_tou > base_tou))
            pt_color = {0x40, 0xFF, 0x40, 0xFF};
        else if (perm && (eff_pow < base_pow || eff_tou < base_tou))
            pt_color = {0xFF, 0x40, 0x40, 0xFF};

        std::string pt = std::to_string(eff_pow) + "/" + std::to_string(eff_tou);
        float pt_w = std::min(rect.w * 0.35f, 36.0f);
        float pt_h = std::min(rect.h * 0.1f, 14.0f);
        float pt_x = rect.x + rect.w - pt_w - 3;
        float pt_y = rect.y + rect.h - pt_h - 3;
        Draw_Filled_Rect(renderer, pt_x, pt_y, pt_w, pt_h, {0x00, 0x00, 0x00, 0xB0});
        if (font)
            Draw_Text_Simple(renderer, font, pt, pt_x + 2, pt_y, pt_color, pt_w - 4);
    }

    if (perm && perm->damage_marked > 0) {
        std::string dmg = "-" + std::to_string(perm->damage_marked);
        float badge_w = std::min(rect.w * 0.25f, 28.0f);
        float badge_h = std::min(rect.h * 0.1f, 14.0f);
        float badge_x = rect.x + 3;
        float badge_y = rect.y + rect.h - badge_h - 3;
        Draw_Filled_Rect(renderer, badge_x, badge_y, badge_w, badge_h, {0xB9, 0x1C, 0x1C, 0xE0});
        if (font)
            Draw_Text_Simple(renderer, font, dmg, badge_x + 2, badge_y, {0xFF, 0xCC, 0xCC, 0xFF});
    }

    if (perm && !perm->counters.empty()) {
        float cx = rect.x + rect.w - 3;
        float cy = rect.y + rect.h * 0.65f;
        for (const auto &counter : perm->counters) {
            if (counter.count == 0)
                continue;
            SDL_Color bg = {0x16, 0xA3, 0x4A, 0xD0};  // green default
            if (counter.type.find("-1/-1") != std::string::npos)
                bg = {0xB9, 0x1C, 0x1C, 0xD0};
            else if (counter.type.find("loyalty") != std::string::npos)
                bg = {0x25, 0x63, 0xEB, 0xD0};
            else if (counter.type.find("charge") != std::string::npos)
                bg = {0x7C, 0x3A, 0xED, 0xD0};

            std::string label = std::to_string(counter.count);
            float bw = 20.0f, bh = 12.0f;
            cx -= bw + 2;
            Draw_Filled_Rect(renderer, cx, cy, bw, bh, bg);
            if (font)
                Draw_Text_Simple(renderer, font, label, cx + 2, cy, {0xFF, 0xFF, 0xFF, 0xFF});
        }
    }

    std::vector<std::string> all_kw = card.keywords;
    if (perm) {
        for (const auto &kw : perm->granted_keywords) {
            if (std::find(all_kw.begin(), all_kw.end(), kw) == all_kw.end())
                all_kw.push_back(kw);
        }
    }
    if (!all_kw.empty() && rect.h > 60) {
        float kw_x = rect.x + 3;
        float kw_y = rect.y + rect.h * 0.5f;
        float kw_h = std::min(rect.h * 0.08f, 11.0f);
        for (const auto &kw : all_kw) {
            if (kw_x > rect.x + rect.w - 10)
                break;  // out of space
            SDL_Color bg = Keyword_Color(kw);
            // Abbreviate
            std::string label = kw;
            if (label.size() > 5)
                label = label.substr(0, 4);
            float kw_w = std::max(label.size() * 6.0f + 4.0f, 16.0f);
            Draw_Filled_Rect(renderer, kw_x, kw_y, kw_w, kw_h, bg);
            if (font)
                Draw_Text_Simple(renderer, font, label, kw_x + 2, kw_y, {0xFF, 0xFF, 0xFF, 0xFF});
            kw_x += kw_w + 2;
        }
    }
}
