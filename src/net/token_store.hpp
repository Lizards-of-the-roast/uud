#pragma once

#include <mutex>
#include <optional>
#include <string>

struct Stored_Tokens {
    std::string access_token;
    std::string refresh_token;
    std::string server_address;
};

struct Token_Store {
    static bool Save(const std::string &access_token, const std::string &refresh_token,
                     const std::string &server_address);
    static std::optional<Stored_Tokens> Load();
    static void Clear();

   private:
    static std::mutex mutex_;
    static std::string Get_Path();
};
