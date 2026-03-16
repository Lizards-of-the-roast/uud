#pragma once

#include <format>
#include <random>
#include <string>

namespace mtg::util {

inline auto generate_uuid() -> std::string {
    static thread_local std::mt19937_64 rng{std::random_device{}()};
    std::uniform_int_distribution<uint64_t> dist;
    uint64_t a = dist(rng);
    uint64_t b = dist(rng);
    return std::format("{:016x}{:016x}", a, b);
}

}  // namespace mtg::util
