#pragma once

#include <mutex>
#include <optional>
#include <queue>

template <typename T>
struct Async_Queue {
    void Push(T value) {
        std::lock_guard lock(mutex_);
        queue_.push(std::move(value));
    }

    std::optional<T> Poll() {
        std::lock_guard lock(mutex_);
        if (queue_.empty())
            return std::nullopt;
        T value = std::move(queue_.front());
        queue_.pop();
        return value;
    }

    bool Empty() {
        std::lock_guard lock(mutex_);
        return queue_.empty();
    }

    void Clear() {
        std::lock_guard lock(mutex_);
        while (!queue_.empty())
            queue_.pop();
    }

   private:
    std::mutex mutex_;
    std::queue<T> queue_;
};
