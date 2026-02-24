#pragma once

#include <string>

namespace cle::lua {

enum class CardLoadError { FileNotFound, ScriptError, InvalidReturnType, NullCard };

struct CardLoadErrorInfo {
    CardLoadError code;
    std::string detail;
};

}  // namespace cle::lua
