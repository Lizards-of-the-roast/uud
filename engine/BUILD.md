# Building

## Installing dependencie

```bash
# this is what i use on Debian 13 (trixie)
sudo apt install cmake ninja-build clang-18 libc++-18-dev libc++abi-18-dev liblua5.4-dev pkg-config
```

## Building

```bash
# debug
cmake --preset debug
cmake --build --preset debug

# release
cmake --preset release
cmake --build --preset release
```

## Running tests

```bash
# debug
ctest --test-dir build/debug

# release
ctest --test-dir build/release
```
