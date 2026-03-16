#include "util/logging.hpp"

#include <spdlog/pattern_formatter.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <spdlog/spdlog.h>

namespace mtg::util {

namespace {
class JsonEscapedPayload : public spdlog::custom_flag_formatter {
public:
    void format(const spdlog::details::log_msg& msg, [[maybe_unused]] const std::tm& tm,
                spdlog::memory_buf_t& dest) override {
        std::string_view payload{msg.payload.data(), msg.payload.size()};
        for (char c : payload) {
            switch (c) {
                case '"':
                    dest.push_back('\\');
                    dest.push_back('"');
                    break;
                case '\\':
                    dest.push_back('\\');
                    dest.push_back('\\');
                    break;
                case '\n':
                    dest.push_back('\\');
                    dest.push_back('n');
                    break;
                case '\r':
                    dest.push_back('\\');
                    dest.push_back('r');
                    break;
                case '\t':
                    dest.push_back('\\');
                    dest.push_back('t');
                    break;
                default:
                    dest.push_back(c);
                    break;
            }
        }
    }

    [[nodiscard]] auto clone() const -> std::unique_ptr<custom_flag_formatter> override {
        return spdlog::details::make_unique<JsonEscapedPayload>();
    }
};
}  // namespace

void init_logging(const std::string& level, bool json_format) {
    auto console = spdlog::stdout_color_mt("console");
    spdlog::set_default_logger(console);

    if (level == "trace") {
        spdlog::set_level(spdlog::level::trace);
    } else if (level == "debug") {
        spdlog::set_level(spdlog::level::debug);
    } else if (level == "warn") {
        spdlog::set_level(spdlog::level::warn);
    } else if (level == "error") {
        spdlog::set_level(spdlog::level::err);
    } else {
        spdlog::set_level(spdlog::level::info);
    }

    if (json_format) {
        auto formatter = std::make_unique<spdlog::pattern_formatter>();
        formatter->add_flag<JsonEscapedPayload>('*');
        formatter->set_pattern(
            R"({"time":"%Y-%m-%dT%H:%M:%S.%e","level":"%l","thread":%t,"logger":"%n","msg":"%*"})");
        spdlog::set_formatter(std::move(formatter));
    } else {
        spdlog::set_pattern("[%Y-%m-%d %H:%M:%S.%e] [%^%l%$] [%t] %v");
    }
}

}  // namespace mtg::util
