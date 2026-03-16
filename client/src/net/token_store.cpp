#include "token_store.hpp"

#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iostream>

std::mutex Token_Store::mutex_;

std::string Token_Store::Get_Path() {
    const char *home = std::getenv("HOME");
    if (!home)
        home = ".";
    return std::string(home) + "/.uud/tokens";
}

bool Token_Store::Save(const std::string &access_token, const std::string &refresh_token,
                       const std::string &server_address) {
    std::lock_guard lock(mutex_);
    std::string path = Get_Path();
    std::filesystem::create_directories(std::filesystem::path(path).parent_path());

    std::ofstream file(path, std::ios::trunc);
    if (!file.is_open()) {
        std::cerr << "token_store: failed to write " << path << '\n';
        return false;
    }

    file << access_token << '\n' << refresh_token << '\n' << server_address << '\n';
    return file.good();
}

std::optional<Stored_Tokens> Token_Store::Load() {
    std::lock_guard lock(mutex_);
    std::string path = Get_Path();
    std::ifstream file(path);
    if (!file.is_open())
        return std::nullopt;

    Stored_Tokens tokens;
    if (!std::getline(file, tokens.access_token) || tokens.access_token.empty())
        return std::nullopt;
    if (!std::getline(file, tokens.refresh_token) || tokens.refresh_token.empty())
        return std::nullopt;
    if (!std::getline(file, tokens.server_address) || tokens.server_address.empty())
        return std::nullopt;

    return tokens;
}

void Token_Store::Clear() {
    std::lock_guard lock(mutex_);
    std::string path = Get_Path();
    std::filesystem::remove(path);
}
