#pragma once

#include <cstdint>
#include <optional>
#include <string>

#include "engine/action_queue.hpp"
#include <cle/mana/mana_cost.hpp>

namespace mtg::engine {

class ManaPool {
public:
    int white{0};
    int blue{0};
    int black{0};
    int red{0};
    int green{0};
    int colorless{0};

    void add(const std::string& color, int amount);

    [[nodiscard]] auto spend(const std::string& color, int amount) -> bool;
    [[nodiscard]] auto can_pay(const std::string& color, int amount) const -> bool;
    [[nodiscard]] auto get(const std::string& color) const -> int;
    [[nodiscard]] auto total() const -> int;

    void clear();

    [[nodiscard]] auto empty() const -> bool;

    [[nodiscard]] auto can_pay_cost(const cle::mana::ManaCost& cost) const -> bool;

    [[nodiscard]] auto pay_cost(const ManaPayment& payment) -> bool;

    [[nodiscard]] auto compute_optimal_payment(const cle::mana::ManaCost& cost) const
        -> std::optional<ManaPayment>;

private:
    [[nodiscard]] auto color_field(const std::string& color) -> int*;
    [[nodiscard]] auto color_field(const std::string& color) const -> const int*;
};

}  // namespace mtg::engine
