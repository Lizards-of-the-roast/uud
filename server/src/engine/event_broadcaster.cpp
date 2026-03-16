#include "engine/event_broadcaster.hpp"

#include <spdlog/spdlog.h>

namespace mtg::engine {

void EventBroadcaster::emit(proto::GameEvent event) {
    std::vector<std::shared_ptr<Callback>> callbacks;
    {
        std::lock_guard const lock{mutex_};
        event.set_sequence_number(next_sequence_++);
        while (event_log_.size() >= max_event_log_size) {
            event_log_.pop_front();
        }
        event_log_.push_back(event);
        for (const auto& sub : subscribers_) {
            if (auto cb = sub.callback.lock()) {
                callbacks.push_back(std::move(cb));
            }
        }
    }
    for (const auto& cb : callbacks) {
        try {
            (*cb)(event);
        } catch (const std::exception& e) {
            spdlog::warn("EventBroadcaster: callback threw: {}", e.what());
        }
    }
}

void EventBroadcaster::emit_to_player(uint64_t player_id, proto::GameEvent event) {
    std::vector<std::shared_ptr<Callback>> callbacks;
    {
        std::lock_guard const lock{mutex_};
        event.set_sequence_number(next_sequence_++);
        while (event_log_.size() >= max_event_log_size) {
            event_log_.pop_front();
        }
        event_log_.push_back(event);
        for (const auto& sub : subscribers_) {
            if (sub.player_id == 0 || sub.player_id == player_id) {
                if (auto cb = sub.callback.lock()) {
                    callbacks.push_back(std::move(cb));
                }
            }
        }
    }
    for (const auto& cb : callbacks) {
        try {
            (*cb)(event);
        } catch (const std::exception& e) {
            spdlog::warn("EventBroadcaster: callback threw: {}", e.what());
        }
    }
}

uint64_t EventBroadcaster::subscribe(std::shared_ptr<Callback> callback) {
    std::lock_guard const lock{mutex_};
    uint64_t const id = next_sub_id_++;
    subscribers_.push_back({.id = id, .player_id = 0, .callback = callback});
    return id;
}

uint64_t EventBroadcaster::subscribe_player(uint64_t player_id,
                                             std::shared_ptr<Callback> callback) {
    std::lock_guard const lock{mutex_};
    uint64_t const id = next_sub_id_++;
    subscribers_.push_back({.id = id, .player_id = player_id, .callback = callback});
    return id;
}

void EventBroadcaster::unsubscribe(uint64_t subscription_id) {
    std::lock_guard const lock{mutex_};
    std::erase_if(subscribers_,
                  [subscription_id](const Subscription& s) { return s.id == subscription_id; });
}

auto EventBroadcaster::next_sequence() -> uint64_t {
    std::lock_guard const lock{mutex_};
    return next_sequence_++;
}

auto EventBroadcaster::get_events_since(uint64_t sequence_number) const
    -> std::vector<proto::GameEvent> {
    std::lock_guard const lock{mutex_};
    std::vector<proto::GameEvent> result;
    for (const auto& event : event_log_) {
        if (event.sequence_number() > sequence_number) {
            result.push_back(event);
        }
    }
    return result;
}

}  // namespace mtg::engine
