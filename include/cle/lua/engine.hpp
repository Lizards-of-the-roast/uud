#pragma once

#include <expected>
#include <filesystem>
#include <memory>
#include <string>

#include <cle/core/card.hpp>
#include <cle/game/game_context.hpp>
#include <cle/lua/bindings.hpp>
#include <cle/lua/error.hpp>
#include <sol/sol.hpp>

namespace cle::lua {

// PLS dont let cards outlive the engine they were created from
class CardEngine {
public:
    CardEngine() : lua_{std::make_unique<sol::state>()} {
        lua_->open_libraries(sol::lib::base, sol::lib::string, sol::lib::table, sol::lib::math);
        register_all(*lua_);
    }

    [[nodiscard]] auto load_card_from_file(const std::filesystem::path& path)
        -> std::expected<std::shared_ptr<core::Card>, CardLoadErrorInfo> {
        if (!std::filesystem::exists(path)) {
            return std::unexpected(CardLoadErrorInfo{CardLoadError::FileNotFound, path.string()});
        }
        return execute_script([&] { return lua_->script_file(path.string()); });
    }

    [[nodiscard]] auto load_card_from_string(const std::string& lua_source)
        -> std::expected<std::shared_ptr<core::Card>, CardLoadErrorInfo> {
        return execute_script([&] { return lua_->script(lua_source); });
    }

    void set_game_context(std::shared_ptr<game::GameContext> ctx) {
        game_context_ = std::move(ctx);
        (*lua_)["game"] = game_context_.get();
    }

    [[nodiscard]] auto lua_state() -> sol::state& { return *lua_; }

private:
    std::unique_ptr<sol::state> lua_;
    std::shared_ptr<game::GameContext> game_context_;

    template <typename ScriptFn>
    auto execute_script(ScriptFn&& run_script)
        -> std::expected<std::shared_ptr<core::Card>, CardLoadErrorInfo> {
        try {
            auto result = run_script();

            if (!result.valid()) {
                sol::error err = result;
                return std::unexpected(
                    CardLoadErrorInfo{CardLoadError::ScriptError, std::string{err.what()}});
            }

            if (result.get_type() != sol::type::userdata) {
                return std::unexpected(CardLoadErrorInfo{CardLoadError::InvalidReturnType, {}});
            }

            auto card = result.template get<std::shared_ptr<core::Card>>();
            if (!card) {
                return std::unexpected(CardLoadErrorInfo{CardLoadError::NullCard, {}});
            }

            return card;

        } catch (const sol::error& e) {
            return std::unexpected(
                CardLoadErrorInfo{CardLoadError::ScriptError, std::string{e.what()}});
        }
    }
};

}  // namespace cle::lua
