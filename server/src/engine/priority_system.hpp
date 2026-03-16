#pragma once

#include <cstdint>
#include <vector>

namespace mtg::engine {

class PrioritySystem {
public:
    void set_player_order(std::vector<uint64_t> order);
    void begin_round(uint64_t active_player_id);
    [[nodiscard]] auto current_priority_holder() const -> uint64_t;
    auto pass() -> bool;
    void interrupt();
    [[nodiscard]] auto all_passed() const -> bool;
    void skip_to_next();

private:
    std::vector<uint64_t> player_order_;
    int current_index_{0};
    int consecutive_passes_{0};
};

}  // namespace mtg::engine
