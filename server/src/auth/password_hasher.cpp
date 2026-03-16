#include "auth/password_hasher.hpp"

#include <array>
#include <cstring>
#include <iomanip>
#include <sstream>
#include <vector>

#include <openssl/evp.h>
#include <openssl/hmac.h>
#include <openssl/rand.h>

namespace mtg::auth {

namespace {

constexpr uint32_t pbkdf2_iterations = 600'000;
constexpr size_t salt_length = 16;
constexpr size_t hash_length = 32;
constexpr std::string_view prefix = "pbkdf2-sha256:";

auto bytes_to_hex(const unsigned char* data, size_t len) -> std::string {
    std::ostringstream ss;
    for (size_t i = 0; i < len; ++i) {
        ss << std::hex << std::setw(2) << std::setfill('0') << static_cast<int>(data[i]);
    }
    return ss.str();
}

auto hex_to_bytes(const std::string& hex) -> std::vector<unsigned char> {
    std::vector<unsigned char> bytes;
    bytes.reserve(hex.size() / 2);
    for (size_t i = 0; i + 1 < hex.size(); i += 2) {
        try {
            auto byte = static_cast<unsigned char>(std::stoul(hex.substr(i, 2), nullptr, 16));
            bytes.push_back(byte);
        } catch (...) {
            return {};
        }
    }
    return bytes;
}

auto constant_time_compare(const unsigned char* a, const unsigned char* b, size_t len) -> bool {
    volatile unsigned char result = 0;
    for (size_t i = 0; i < len; ++i) {
        result |= a[i] ^ b[i];
    }
    return result == 0;
}

auto pbkdf2_derive(const std::string& password, const unsigned char* salt, size_t salt_len,
                   uint32_t iterations, unsigned char* out, size_t out_len) -> bool {
    return PKCS5_PBKDF2_HMAC(password.data(), password.size(), salt, salt_len, iterations,
                             EVP_sha256(), out_len, out) == 1;
}

}  // namespace

auto PasswordHasher::hash(const std::string& password) -> std::string {
    std::array<unsigned char, salt_length> salt{};
    RAND_bytes(salt.data(), salt.size());

    std::array<unsigned char, hash_length> derived{};
    pbkdf2_derive(password, salt.data(), salt_length, pbkdf2_iterations, derived.data(),
                  hash_length);

    return std::string(prefix) + std::to_string(pbkdf2_iterations) + ":" +
           bytes_to_hex(salt.data(), salt.size()) + ":" +
           bytes_to_hex(derived.data(), derived.size());
}

auto PasswordHasher::verify(const std::string& password, const std::string& stored_hash) -> bool {
    if (stored_hash.starts_with("pbkdf2-sha256:")) {
        auto after_prefix = stored_hash.substr(14);

        auto first_colon = after_prefix.find(':');
        if (first_colon == std::string::npos) {
            return false;
        }
        auto second_colon = after_prefix.find(':', first_colon + 1);
        if (second_colon == std::string::npos) {
            return false;
        }

        int iterations_raw = 0;
        try {
            iterations_raw = std::stoi(after_prefix.substr(0, first_colon));
        } catch (...) {
            return false;
        }
        if (iterations_raw <= 0) {
            return false;
        }
        auto iterations = static_cast<uint32_t>(iterations_raw);

        auto salt_hex = after_prefix.substr(first_colon + 1, second_colon - first_colon - 1);
        auto hash_hex = after_prefix.substr(second_colon + 1);

        auto salt = hex_to_bytes(salt_hex);
        auto stored_derived = hex_to_bytes(hash_hex);
        if (salt.empty() || stored_derived.empty()) {
            return false;
        }

        std::vector<unsigned char> computed(stored_derived.size());
        if (!pbkdf2_derive(password, salt.data(), salt.size(), iterations, computed.data(),
                           computed.size())) {
            return false;
        }

        return constant_time_compare(computed.data(), stored_derived.data(), stored_derived.size());
    }

    return false;
}

auto PasswordHasher::needs_rehash(const std::string& stored_hash) -> bool {
    return !stored_hash.starts_with("pbkdf2-sha256:");
}

}  // namespace mtg::auth
