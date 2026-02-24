#pragma once

#include <atomic>
#include <cstdint>

namespace cle::core::internal {

inline std::atomic<uint64_t> next_card_instance_id{1};

inline auto generate_card_instance_id() -> uint64_t {
    return next_card_instance_id.fetch_add(1, std::memory_order_relaxed);
}

}  // namespace cle::core::internal
