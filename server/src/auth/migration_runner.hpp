#pragma once

#include <string>
#include <vector>

#include <pqxx/pqxx>

namespace mtg::auth {

struct Migration {
    int version;
    std::string name;
    std::string sql;
};

class MigrationRunner {
public:
    explicit MigrationRunner(pqxx::connection& conn);

    [[nodiscard]] auto run_pending() -> int;

private:
    void ensure_migrations_table();
    [[nodiscard]] auto current_version() -> int;

    pqxx::connection& conn_;
    static auto all_migrations() -> const std::vector<Migration>&;
};

}  // namespace mtg::auth
