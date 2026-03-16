#pragma once

#include <cstdint>
#include <string>

#include "auth/connection_pool.hpp"

namespace mtg::auth {

struct RatingRecord {
    uint64_t user_id{0};
    int elo{1200};
    int wins{0};
    int losses{0};
};

class RatingStore {
public:
    explicit RatingStore(const std::string& connection_string, int pool_size = 2);

    [[nodiscard]] auto get_rating(uint64_t user_id) -> RatingRecord;

    void update_after_game(uint64_t winner_id, uint64_t loser_id);

private:
    ConnectionPool pool_;
};

}  // namespace mtg::auth
