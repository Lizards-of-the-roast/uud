# mtg-server

```bash
# Debug
cmake --preset debug
cmake --build build/debug/ -j$(nproc)

# Release
cmake --preset release
cmake --build build/release/ -j$(nproc)

# Debug + address sanitiser
cmake --preset asan
cmake --build build/asan/ -j$(nproc)
```
