#pragma once

#include <cstdint>
#include <deque>
#include <functional>
#include <memory>
#include <mutex>
#include <unordered_map>
#include <vector>

#include "mtg/game_state.pb.h"

namespace mtg::engine {

class EventBroadcaster {
public:
    using Callback = std::function<void(const proto::GameEvent&)>;

    void emit(proto::GameEvent event);
    void emit_to_player(uint64_t player_id, proto::GameEvent event);

    [[nodiscard]] auto subscribe(std::shared_ptr<Callback> callback) -> uint64_t;
    [[nodiscard]] auto subscribe_player(uint64_t player_id, std::shared_ptr<Callback> callback)
        -> uint64_t;
    void unsubscribe(uint64_t subscription_id);

    [[nodiscard]] auto get_events_since(uint64_t sequence_number) const
        -> std::vector<proto::GameEvent>;
    [[nodiscard]] auto next_sequence() -> uint64_t;

private:
    mutable std::mutex mutex_;
    uint64_t next_sequence_{1};
    uint64_t next_sub_id_{1};

    struct Subscription {
        uint64_t id;
        uint64_t player_id;
        std::weak_ptr<Callback> callback;
    };
    static constexpr size_t max_event_log_size{10000};

    std::vector<Subscription> subscribers_;
    std::deque<proto::GameEvent> event_log_;
};

}  // namespace mtg::engine
