# setup

## deps

```bash
sudo apt update && sudo apt upgrade -y

sudo apt install -y \
  build-essential git cmake ninja-build pkg-config \
  autoconf automake libtool curl wget unzip
```

### clang 18

```bash
wget https://apt.llvm.org/llvm.sh
chmod +x llvm.sh
sudo ./llvm.sh 18
sudo apt install -y clang-tools-18 libc++-18-dev libc++abi-18-dev

sudo update-alternatives --install /usr/bin/clang clang /usr/bin/clang-18 100
sudo update-alternatives --install /usr/bin/clang++ clang++ /usr/bin/clang++-18 100
sudo update-alternatives --install /usr/bin/clang-scan-deps clang-scan-deps /usr/bin/clang-scan-deps-18 100
sudo update-alternatives --install /usr/bin/clang-tidy clang-tidy /usr/bin/clang-tidy-18 100
```

### cmake 3.30+

```bash
sudo apt remove cmake
pip install cmake --break-system-packages
```

### server stuff

```bash
sudo apt install -y libssl-dev libpq-dev libyaml-cpp-dev postgresql postgresql-client redis-server
```

then enable both:

```bash
sudo systemctl enable --now postgresql redis-server
```

### client stuff

```bash
sudo apt install -y \
  libwayland-dev libxkbcommon-dev libx11-dev libxext-dev \
  libxrandr-dev libxcursor-dev libxi-dev libxfixes-dev libxss-dev \
  libgl-dev libgles-dev libegl-dev libdbus-1-dev \
  libpulse-dev libasound2-dev libfreetype-dev libharfbuzz-dev \
  libpng-dev libjpeg-dev libwebp-dev
```

### database

```bash
sudo -u postgres psql -c "CREATE DATABASE mtg_db;"
sudo -u postgres psql -c "CREATE USER mtg_user WITH PASSWORD 'password';"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE mtg_db TO mtg_user;"
```

## building

```bash
cmake --preset server-only
cmake --build build/server-only -j$(nproc)

cmake --preset client-only
cmake --build build/client-only -j$(nproc)
```

`-DCMAKE_POLICY_VERSION_MINIMUM=3.5`
