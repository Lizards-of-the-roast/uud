#pragma once

#include <atomic>
#include <chrono>
#include <condition_variable>
#include <cstdint>
#include <deque>
#include <mutex>
#include <optional>
#include <string>
#include <variant>
#include <vector>

namespace mtg::engine {

struct ManaPayment {
    int white{0};
    int blue{0};
    int black{0};
    int red{0};
    int green{0};
    int colorless{0};
};

struct ActionData {
    uint64_t player_id{0};
    std::string prompt_id;
    std::string action_type;
    uint64_t target_id{0};
    std::vector<uint64_t> ids;
    std::vector<int> indices;
    bool flag{false};
    std::string text;
    int x_value{0};
    std::vector<uint64_t> convoke_ids;
    int delve_count{0};
    ManaPayment mana_payment;
};

struct SharedNotify {
    std::mutex mutex;
    std::condition_variable cv;
    std::atomic<uint64_t> generation{0};
};

class ActionQueue {
public:
    static constexpr size_t max_pending = 64;

    void set_shared_notify(std::shared_ptr<SharedNotify> sn) { shared_notify_ = std::move(sn); }

    [[nodiscard]] auto submit(ActionData action) -> bool;
    [[nodiscard]] auto wait_for(std::chrono::seconds timeout) -> std::optional<ActionData>;
    [[nodiscard]] auto try_take() -> std::optional<ActionData>;
    [[nodiscard]] auto try_take_if(const std::string& action_type) -> std::optional<ActionData>;
    void clear();

private:
    std::mutex mutex_;
    std::condition_variable cv_;
    std::deque<ActionData> pending_;
    std::shared_ptr<SharedNotify> shared_notify_;
};

}  // namespace mtg::engine
