#pragma once

#include <atomic>
#include <cstdint>
#include <string>
#include <variant>

#include "async_queue.hpp"

struct Auth_Login_Result {
    bool success;
    std::string error;
    std::string access_token;
    std::string refresh_token;
    uint64_t user_id;
    std::string username;
};

struct Auth_Register_Result {
    bool success;
    std::string error;
    uint64_t user_id;
};

struct Auth_Validate_Result {
    bool valid;
    uint64_t user_id;
    std::string username;
};

struct Auth_Refresh_Result {
    bool success;
    std::string error;
    std::string access_token;
    std::string refresh_token;
};

using Auth_Result = std::variant<Auth_Login_Result, Auth_Register_Result, Auth_Validate_Result,
                                 Auth_Refresh_Result>;

struct Auth_Client {
    void Login(const std::string &server, const std::string &username, const std::string &password);
    void Register(const std::string &server, const std::string &username, const std::string &email,
                  const std::string &password);
    void Validate(const std::string &server, const std::string &token);
    void Refresh(const std::string &server, const std::string &refresh_token);
    void Logout();

    std::optional<Auth_Result> Poll();
    bool In_Flight();

private:
    Async_Queue<Auth_Result> results_;
    std::atomic<bool> in_flight_{false};
};

extern Auth_Client auth_client;
