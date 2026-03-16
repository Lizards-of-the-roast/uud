#pragma once

#include <string>

namespace mtg::auth {

class PasswordHasher {
public:
    [[nodiscard]] static auto hash(const std::string& password) -> std::string;
    [[nodiscard]] static auto verify(const std::string& password, const std::string& hash) -> bool;
    [[nodiscard]] static auto needs_rehash(const std::string& hash) -> bool;
};

}  // namespace mtg::auth
