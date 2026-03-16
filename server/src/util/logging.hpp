#pragma once

#include <string>

namespace mtg::util {

void init_logging(const std::string& level = "info", bool json_format = false);

}  // namespace mtg::util
