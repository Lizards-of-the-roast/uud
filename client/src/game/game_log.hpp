#pragma once

#include <cstdint>
#include <string>
#include <vector>

struct Log_Entry {
    std::string text;
    uint32_t color = 0xFFFFFFFF;  // packed RGBA
};

class Game_Log {
public:
    void Add(const std::string &text, uint32_t color = 0xFFFFFFFF) {
        entries_.push_back({text, color});
        if (entries_.size() > 200)
            entries_.erase(entries_.begin());
    }
    void Clear() { entries_.clear(); }
    const std::vector<Log_Entry> &Entries() const { return entries_; }

private:
    std::vector<Log_Entry> entries_;
};
