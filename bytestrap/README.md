<div align="center">

# Bytestrap

### roblox but it actually runs well

</div>

> [!CAUTION]
> this is an unofficial fork of fishstrap/bloxstrap. not affiliated with roblox. use at your own risk

fork of fishstrap/bloxstrap that's all about getting more fps and less ping. mess with fastflags, rendering, network stuff, whatever you need to make roblox not run like a slideshow.

> [!NOTE]
> two flavors: a **windows app** (windows 10+) and a **linux CLI port** (any distro, arch included). details below.

---

## what it does

**performance stuff**
- quick profiles - just pick one (max fps, low ping, balanced, ultra low latency) and go
- fps unlocker - slider goes up to 9999, no more 60fps cap
- low ping mode - tweaks MTU, send rates, heartbeat, network prediction
- kill shadows/postfx/grass/particles - turn off the stuff you don't need
- render throttle - dial back render workload for more fps
- disable telemetry - stops roblox from phoning home

**the cool stuff**
- bypass flag method - writes flags straight to roblox's folders with read-only lock so they stick through updates
- flag editor - edit any fastflag, save profiles/presets
- global settings editor - framerate cap, quality, mouse sens, etc
- channel changer

**other things**
- discord rich presence
- server info (thanks [RoValra](https://www.rovalra.com/))
- works with roblox studio too
- cache cleaner
- custom themes
- mod support (cursors, sounds, etc)

**linux port** - same flag engine + profiles as a fast CLI for wine/proton setups
- `preset apply max-fps`, `flags set Performance.UnlockFPS 240`, `launch`, ...
- auto-detects installs across wine / vinegar / bottles / sober / proton prefixes
- launches with a tuned env (esync/fsync, gamemode, shader cache, dxvk async)
- zero dependencies, `--json` output for scripting

plus everything else from bloxstrap/fishstrap

---

## requirements

**windows app**
- windows 10 or newer, that's it

**linux port**
- to build: [.NET 8 SDK](https://dot.net) or newer (8.0+; anything newer works too)
- to launch roblox: a runner - `wine`/`proton`, and/or the flatpaks below
- to actually play in 2026: [Sober](https://sober.vinegarhq.org/) (roblox's anti-cheat blocks stock wine for the player, so sober is the way to play; use [Vinegar](https://vinegarhq.org/) for studio). bytestrap manages flags for any wine-layout install and tells you what's detected via `bytestrap status`

---

## install

### windows

grab the latest release from [here](https://github.com/bytestrap/bytestrap/releases), run it, done. configure stuff in the app before launching roblox.

### linux

```bash
./linux/install.sh                  # user install to ~/.local
# ./linux/install.sh --self-contained   # bundles .NET, no runtime needed
# sudo ./linux/install.sh --system      # system-wide to /usr/local

export PATH="$PATH:$HOME/.local/bin"
bytestrap status
```

then get something to actually run roblox with:

```bash
flatpak install flathub org.vinegarhq.Sober     # playing
flatpak install flathub org.vinegarhq.Vinegar   # studio
```

### arch linux (btw)

yes, it works on arch. everything's in the official repos:

```bash
# 1. build tools + .NET SDK (8.0 matches the project; latest also builds it)
sudo pacman -S base-devel dotnet-sdk-8.0

# 2. flatpak for the roblox runners
sudo pacman -S flatpak
flatpak install flathub org.vinegarhq.Sober org.vinegarhq.Vinegar

# 3. optional but recommended: wine (multilib) + gamemode
#    enable [multilib] in /etc/pacman.conf first, then:
sudo pacman -Syu wine wine-mono wine_gecko gamemode

# 4. build + install bytestrap
./linux/install.sh
export PATH="$PATH:$HOME/.local/bin"

# 5. go
bytestrap status
bytestrap preset apply max-fps
bytestrap launch
```

notes for arch users:
- `dotnet-sdk-8.0` and `dotnet-sdk` (latest) can both build this repo - the pinned SDK in `global.json` rolls forward to whatever you have.
- no AUR packages needed. if you prefer zero dotnet on your system, use `./linux/install.sh --self-contained` (still needs the SDK at build time, just not at runtime).
- `wine` on arch is 64-bit-capable out of the box (wow64); roblox player itself is 64-bit.

---

## linux CLI reference

```
bytestrap status [--json] [--prefix DIR]   runner / sober / installs / flags overview
bytestrap versions                         list detected roblox versions
bytestrap apply                            deploy master flags to all installs

bytestrap flags list [prefix] [--json]     list flags, optional filter
bytestrap flags get <name>                 print one flag
bytestrap flags set <name> <value>         set a flag (auto-deploys)
bytestrap flags unset <name>               remove a flag (auto-deploys)
bytestrap flags clear [prefix] [--yes]     remove flags (auto-deploys)

bytestrap preset list [--json]             show performance presets
bytestrap preset show <name> [--json]      show what a preset sets
bytestrap preset apply <name>              apply a preset (auto-deploys)

bytestrap export <file.json>               back up flags
bytestrap import <file.json> [--replace]   restore flags (merge by default)

bytestrap launch [uri] [--studio]          launch player/studio via wine/proton
bytestrap launch --dry-run                 show the launch plan without running
bytestrap kill                             kill running roblox processes
bytestrap clean [--deep]                   delete logs (and downloads cache)

bytestrap config show|get|set|path         manage settings
bytestrap install-desktop                  add an app-menu launcher
```

global options: `--prefix DIR` (use this wine prefix), `--runner BIN` (use this runner), `--json` (machine output), `--no-apply` (edit master copy without deploying).

examples:

```bash
bytestrap preset apply ultra-low-latency
bytestrap flags set Performance.UnlockFPS 240
bytestrap flags set DFIntTaskSchedulerTargetFps 240   # same thing, raw name
bytestrap flags list Performance
bytestrap export ~/my-flags.json
bytestrap launch "roblox://placeId=1818"
bytestrap launch --studio --dry-run
```

### presets

| preset | what it does |
|---|---|
| `max-fps` | uncapped FPS, lowest visuals, no shadows/postfx/grass, throttled render |
| `low-ping` | tuned MTU/send rates/physics, network prediction on |
| `ultra-low-latency` | aggressive low-ping + uncapped FPS + no telemetry |
| `balanced` | 240 FPS cap, medium visuals, mild network tuning |
| `max-graphics` | max textures, long render distance, MSAA x4 |

preset values match the windows app, and `export`/`import` JSON is interchangeable with the windows performance-config export.

### flags

names accept friendly preset keys (`Performance.UnlockFPS`) or raw fflags (`DFIntTaskSchedulerTargetFps`) interchangeably. unknown names are set as raw fflags (with a warning), so any new flag works on day one.

flags live in a master copy at `~/.local/share/bytestrap/ClientAppSettings.json` and deploy to every `Versions/*/ClientSettings/` dir on every change - the linux equivalent of the bypass flag method.

### config

settings live in `~/.config/bytestrap/settings.json`:

| key | default | meaning |
|---|---|---|
| `prefix` | auto | wine prefix to use (overrides `$WINEPREFIX`) |
| `runner` | auto | runner binary (`wine`, `wine64`, `proton`, path, ...) |
| `runnerArgs` | | extra args before the roblox exe |
| `launchArgs` | | extra args after the launch URI |
| `gamemode` | true | prefix launch with `gamemoderun` when available |
| `esync` | true | set `WINEESYNC=1` |
| `fsync` | true | set `WINEFSYNC=1` |

```bash
bytestrap config set runner /usr/bin/wine64
bytestrap config set prefix /mnt/games/wine-roblox
```

per-run overrides (`--prefix`, `--runner`, `$WINEPREFIX`) beat the file. extra prefixes + extra env vars (`extraWinePrefixes`, `extraEnv`) can be added by editing the json directly. full deep-dive (detection order, launch env, `roblox://` handler, dev notes) is in [LINUX.md](LINUX.md).

---

## building it yourself

**windows app** - [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), windows 10+

```
git submodule update --init
dotnet build Bytestrap.sln -c Release
```

output goes to `Bloxstrap/bin/Release/net8.0-windows/`

**linux port** - .NET 8.0 SDK or newer, any distro (zero NuGet dependencies)

```
dotnet build Bytestrap.Linux.sln -c Release
# binary: Bytestrap.Linux/bin/Release/net8.0/bytestrap
```

project layout:

```
Bloxstrap/          windows app (wpf, net8.0-windows)
Bytestrap.Core/     cross-platform core: flags, profiles, detection (net8.0)
Bytestrap.Linux/    linux CLI (net8.0)
Bytestrap.sln       windows solution
Bytestrap.Linux.sln linux solution
linux/install.sh    linux build+install script
LINUX.md            linux deep-dive docs
```

---

## troubleshooting

**windows**
- roblox won't start after an update: the app reinstalls/updates it automatically on next launch; if stuck, delete the version folder and relaunch.
- flags not applying: make sure fastflag manager is on in settings, and try the bypass flag method toggle.
- download loops / checksum errors: check the connectivity dialog link, or nuke `Downloads/` in the install folder and retry.

**linux**
- `No Roblox installs found`: install roblox inside your wine prefix first (or sober to play), or pass `--prefix /path/to/prefix`.
- `No Wine/Proton runner found`: install `wine` (arch: enable multilib, `sudo pacman -S wine`) or set one: `bytestrap config set runner /path/to/wine`.
- game launches but ignores flags: run `bytestrap apply`; some runners reset the prefix on launch, so re-apply after.
- `dotnet: command not found`: install the SDK (arch: `sudo pacman -S dotnet-sdk-8.0`), or grab a self-contained build.
- esync/fsync wine warnings: your kernel/wine lacks support - `bytestrap config set esync false` (same for `fsync`).

---

## contributing

check [CONTRIBUTING.md](CONTRIBUTING.md). fork it, branch it, PR it.

---

## license

[MIT](LICENSE)

---

## credits

- [bloxstrap](https://github.com/bloxstraplabs/bloxstrap) - the og
- [fishstrap](https://github.com/returnrqt/fishstrap) - the fork before this fork
- [RoValra](https://www.rovalra.com/) - server info
- [wpf ui](https://github.com/lepoco/wpfui) - ui framework
- [vinegarhq](https://vinegarhq.org/) - sober/vinegar, making roblox-on-linux possible
