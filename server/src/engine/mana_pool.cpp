#include "engine/mana_pool.hpp"

#include "engine/action_queue.hpp"

namespace mtg::engine {

void ManaPool::add(const std::string& color, int amount) {
    if (auto* field = color_field(color)) {
        *field += amount;
    }
}

auto ManaPool::spend(const std::string& color, int amount) -> bool {
    if (!can_pay(color, amount)) {
        return false;
    }
    if (auto* field = color_field(color)) {
        *field -= amount;
        return true;
    }
    return false;
}

auto ManaPool::can_pay(const std::string& color, int amount) const -> bool {
    if (const auto* field = color_field(color)) {
        return *field >= amount;
    }
    return false;
}

auto ManaPool::get(const std::string& color) const -> int {
    if (const auto* field = color_field(color)) {
        return *field;
    }
    return 0;
}

auto ManaPool::total() const -> int {
    return white + blue + black + red + green + colorless;
}

void ManaPool::clear() {
    white = 0;
    blue = 0;
    black = 0;
    red = 0;
    green = 0;
    colorless = 0;
}

auto ManaPool::empty() const -> bool {
    return total() == 0;
}

auto ManaPool::color_field(const std::string& color) -> int* {
    if (color == "W" || color == "White") {
        return &white;
    }
    if (color == "U" || color == "Blue") {
        return &blue;
    }
    if (color == "B" || color == "Black") {
        return &black;
    }
    if (color == "R" || color == "Red") {
        return &red;
    }
    if (color == "G" || color == "Green") {
        return &green;
    }
    if (color == "C" || color == "Colorless") {
        return &colorless;
    }
    return nullptr;
}

auto ManaPool::color_field(const std::string& color) const -> const int* {
    return const_cast<ManaPool*>(this)->color_field(color);
}

auto ManaPool::can_pay_cost(const cle::mana::ManaCost& cost) const -> bool {
    int const remaining_w = white - cost.white;
    int const remaining_u = blue - cost.blue;
    int const remaining_b = black - cost.black;
    int const remaining_r = red - cost.red;
    int const remaining_g = green - cost.green;

    if (remaining_w < 0 || remaining_u < 0 || remaining_b < 0 || remaining_r < 0 ||
        remaining_g < 0) {
        return false;
    }

    int const hybrid_count = static_cast<int>(cost.hybrid_costs.size());
    if (hybrid_count > 10) {
        return false;
    }
    int best_leftover = -1;

    for (int mask = 0; mask < (1 << hybrid_count); ++mask) {
        int rw = remaining_w, ru = remaining_u, rb = remaining_b;
        int rr = remaining_r, rg = remaining_g, rc = colorless;
        bool valid = true;

        for (int h = 0; h < hybrid_count; ++h) {
            const auto& hybrid = cost.hybrid_costs[static_cast<size_t>(h)];
            bool const use_primary = (mask & (1 << h)) != 0;
            auto color = use_primary ? hybrid.primary : hybrid.secondary;

            switch (color) {
                case cle::mana::ManaColor::White:
                    if (--rw < 0) {
                        valid = false;
                    }
                    break;
                case cle::mana::ManaColor::Blue:
                    if (--ru < 0) {
                        valid = false;
                    }
                    break;
                case cle::mana::ManaColor::Black:
                    if (--rb < 0) {
                        valid = false;
                    }
                    break;
                case cle::mana::ManaColor::Red:
                    if (--rr < 0) {
                        valid = false;
                    }
                    break;
                case cle::mana::ManaColor::Green:
                    if (--rg < 0) {
                        valid = false;
                    }
                    break;
                case cle::mana::ManaColor::Colorless:
                    if (--rc < 0) {
                        valid = false;
                    }
                    break;
            }
            if (!valid) {
                break;
            }
        }

        if (valid) {
            int const leftover = rw + ru + rb + rr + rg + rc;
            if (leftover > best_leftover) {
                best_leftover = leftover;
            }
        }
    }

    if (best_leftover < 0) {
        return false;
    }

    return best_leftover >= cost.colorless;
}

auto ManaPool::pay_cost(const ManaPayment& payment) -> bool {
    if (payment.white > white || payment.blue > blue || payment.black > black ||
        payment.red > red || payment.green > green || payment.colorless > colorless) {
        return false;
    }
    white -= payment.white;
    blue -= payment.blue;
    black -= payment.black;
    red -= payment.red;
    green -= payment.green;
    colorless -= payment.colorless;
    return true;
}

auto ManaPool::compute_optimal_payment(const cle::mana::ManaCost& cost) const
    -> std::optional<ManaPayment> {
    if (!can_pay_cost(cost)) {
        return std::nullopt;
    }

    ManaPayment payment;

    payment.white = cost.white;
    payment.blue = cost.blue;
    payment.black = cost.black;
    payment.red = cost.red;
    payment.green = cost.green;

    int remaining_w = white - cost.white;
    int remaining_u = blue - cost.blue;
    int remaining_b = black - cost.black;
    int remaining_r = red - cost.red;
    int remaining_g = green - cost.green;
    int remaining_c = colorless;

    for (const auto& hybrid : cost.hybrid_costs) {
        auto available_for = [&](cle::mana::ManaColor c) -> int {
            switch (c) {
                case cle::mana::ManaColor::White:
                    return remaining_w;
                case cle::mana::ManaColor::Blue:
                    return remaining_u;
                case cle::mana::ManaColor::Black:
                    return remaining_b;
                case cle::mana::ManaColor::Red:
                    return remaining_r;
                case cle::mana::ManaColor::Green:
                    return remaining_g;
                case cle::mana::ManaColor::Colorless:
                    return remaining_c;
            }
            return 0;
        };
        auto spend_from = [&](cle::mana::ManaColor c) {
            switch (c) {
                case cle::mana::ManaColor::White:
                    remaining_w--;
                    payment.white++;
                    break;
                case cle::mana::ManaColor::Blue:
                    remaining_u--;
                    payment.blue++;
                    break;
                case cle::mana::ManaColor::Black:
                    remaining_b--;
                    payment.black++;
                    break;
                case cle::mana::ManaColor::Red:
                    remaining_r--;
                    payment.red++;
                    break;
                case cle::mana::ManaColor::Green:
                    remaining_g--;
                    payment.green++;
                    break;
                case cle::mana::ManaColor::Colorless:
                    remaining_c--;
                    payment.colorless++;
                    break;
            }
        };

        if (available_for(hybrid.primary) >= available_for(hybrid.secondary)) {
            spend_from(hybrid.primary);
        } else {
            spend_from(hybrid.secondary);
        }
    }

    int generic = cost.colorless;
    while (generic > 0 && remaining_c > 0) {
        payment.colorless++;
        remaining_c--;
        generic--;
    }

    while (generic > 0) {
        int* best_remaining = nullptr;
        int* best_payment = nullptr;
        int best_val = 0;

        auto check = [&](int& rem, int& pay) {
            if (rem > best_val) {
                best_val = rem;
                best_remaining = &rem;
                best_payment = &pay;
            }
        };
        check(remaining_w, payment.white);
        check(remaining_u, payment.blue);
        check(remaining_b, payment.black);
        check(remaining_r, payment.red);
        check(remaining_g, payment.green);

        if (best_remaining == nullptr) {
            return std::nullopt;
        }
        (*best_remaining)--;
        (*best_payment)++;
        generic--;
    }

    return payment;
}

}  // namespace mtg::engine
