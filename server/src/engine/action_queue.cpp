#include "engine/action_queue.hpp"

#include <spdlog/spdlog.h>

namespace mtg::engine {

auto ActionQueue::submit(ActionData action) -> bool {
    {
        std::lock_guard const lock{mutex_};
        if (pending_.size() >= max_pending) {
            spdlog::warn("ActionQueue: queue full ({} pending), rejecting action '{}'",
                         pending_.size(), action.action_type);
            return false;
        }
        pending_.push_back(std::move(action));
    }
    cv_.notify_one();
    if (shared_notify_) {
        shared_notify_->generation.fetch_add(1, std::memory_order_release);
        shared_notify_->cv.notify_all();
    }
    return true;
}

auto ActionQueue::wait_for(std::chrono::seconds timeout) -> std::optional<ActionData> {
    std::unique_lock lock{mutex_};
    if (!cv_.wait_for(lock, timeout, [this] { return !pending_.empty(); })) {
        return std::nullopt;
    }
    auto action = std::move(pending_.front());
    pending_.pop_front();
    return action;
}

auto ActionQueue::try_take() -> std::optional<ActionData> {
    std::lock_guard const lock{mutex_};
    if (pending_.empty()) {
        return std::nullopt;
    }
    auto action = std::move(pending_.front());
    pending_.pop_front();
    return action;
}

auto ActionQueue::try_take_if(const std::string& action_type) -> std::optional<ActionData> {
    std::lock_guard const lock{mutex_};
    for (auto it = pending_.begin(); it != pending_.end(); ++it) {
        if (it->action_type == action_type) {
            auto action = std::move(*it);
            pending_.erase(it);
            return action;
        }
    }
    return std::nullopt;
}

void ActionQueue::clear() {
    std::lock_guard const lock{mutex_};
    pending_.clear();
}

}  // namespace mtg::engine
