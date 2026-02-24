#pragma once

#include <cstdint>
#include <string>

#include <cle/triggers/trigger_type.hpp>

namespace cle::triggers {

struct TriggerEvent {
    TriggerType type;
    uint64_t source_id{0};
    uint64_t target_id{0};
    uint64_t player_id{0};
    int amount{0};
    std::string extra_data;
};

}  // namespace cle::triggers
