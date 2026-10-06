# Bytestrap for Linux

The Linux port of Bytestrap: the same fast-flag engine and performance
profiles as the Windows app, as a dependency-free .NET 8 CLI that manages
Roblox installs running under Wine/Proton runners (plain Wine, Vinegar,
Bottles, Sober, Steam Proton).

> [!NOTE]
> Roblox has no native Linux build, so Bytestrap doesn't install or patch
> Roblox itself here — it manages `ClientAppSettings.json` flags across your
> Wine prefixes and launches the game through your runner with a tuned
> environment.
>
> To actually **play** in 2026, use [Sober](https://sober.vinegarhq.org/)
> (`flatpak install flathub org.vinegarhq.Sober`) - Roblox's Hyperion
> anti-cheat blocks the stock Windows Player under Wine. For **Studio**, use
> [Vinegar](https://vinegarhq.org/) from Flathub. `bytestrap status` shows
> which of these you have installed. Bytestrap's flag management targets any
> Wine-layout install (Studio via Vinegar, custom prefixes, ...).

---

## Install

You need the [.NET 8 SDK](https://dot.net) to build (one-time).

```bash
git clone https://github.com/bytestrap/bytestrap.git
cd bytestrap/bytestrap

# user install (~/.local) - framework-dependent, tiny download
./linux/install.sh

# ...or self-contained (bundles the runtime, works without dotnet installed)
./linux/install.sh --self-contained

# ...or system-wide
sudo ./linux/install.sh --system

# make sure it's on PATH, then:
export PATH="$PATH:$HOME/.local/bin"
bytestrap status
```

Manual build without the script:

```bash
dotnet publish Bytestrap.Linux/Bytestrap.Linux.csproj -c Release -o out
./out/bytestrap status
```

The port has **zero NuGet dependencies** — it builds offline with just the SDK.

> [!TIP]
> On Arch? Everything's in the official repos (`dotnet-sdk-8.0`, `flatpak`,
> `wine` from multilib) — see the step-by-step in the main
> [README](README.md#arch-linux-btw).

---

## Quickstart

```bash
bytestrap status                 # detect runner + installs + flag count
bytestrap preset apply max-fps   # one-click performance profile
bytestrap launch                 # launch Roblox via wine/proton
```

Flags are stored in a master copy (`~/.local/share/bytestrap/ClientAppSettings.json`)
and deployed to every `Versions/*/ClientSettings/` dir on every change —
the equivalent of the Windows app's bypass flag method.

---

## Commands

```
bytestrap status [--json] [--prefix DIR]   runner / sober / installs / flags
bytestrap versions                         list detected Roblox versions
bytestrap apply                            deploy master flags to all installs

bytestrap flags list [prefix] [--json]     list flags (optional filter)
bytestrap flags get <name>                 print one flag (preset key or FFlag)
bytestrap flags set <name> <value>         set a flag (auto-deploys)
bytestrap flags unset <name>               remove a flag (auto-deploys)
bytestrap flags clear [prefix] [--yes]     remove flags (auto-deploys)

bytestrap preset list [--json]             show performance presets
bytestrap preset show <name> [--json]      show what a preset sets
bytestrap preset apply <name>              apply a preset (auto-deploys)

bytestrap export <file.json>               back up flags
bytestrap import <file.json> [--replace]   restore flags (merge by default)

bytestrap launch [uri] [--studio]          launch player/studio
bytestrap launch --dry-run                 show the launch plan without running
bytestrap kill                             kill Roblox processes
bytestrap clean [--deep]                   delete logs (and downloads cache)

bytestrap config show|get|set|path         manage settings
bytestrap install-desktop                  add an app-menu launcher
```

Global options: `--prefix DIR` (use this Wine prefix), `--runner BIN`
(use this runner), `--json` (machine output), `--no-apply` (edit the
master copy without deploying).

Names accept friendly preset keys (`Performance.UnlockFPS`) or raw FFlags
(`DFIntTaskSchedulerTargetFps`) interchangeably:

```bash
bytestrap flags set Performance.UnlockFPS 240
bytestrap flags set DFIntTaskSchedulerTargetFps 240   # same thing
```

---

## Presets

| preset | what it does |
|---|---|
| `max-fps` | uncapped FPS, lowest visuals, no shadows/postfx/grass, throttled render |
| `low-ping` | tuned MTU/send rates/physics, network prediction on |
| `ultra-low-latency` | aggressive low-ping + uncapped FPS + no telemetry |
| `balanced` | 240 FPS cap, medium visuals, mild network tuning |
| `max-graphics` | max textures, long render distance, MSAA x4 |

Values match the Windows app's quick presets, and `export`/`import` JSON is
interchangeable with the Windows performance-config export.

---

## Config

Settings live in `~/.config/bytestrap/settings.json`:

| key | default | meaning |
|---|---|---|
| `prefix` | auto | Wine prefix to use (overrides `$WINEPREFIX`) |
| `runner` | auto | runner binary (`wine`, `wine64`, `proton`, path, …) |
| `runnerArgs` | | extra args before the Roblox exe |
| `launchArgs` | | extra args after the launch URI |
| `gamemode` | true | prefix launch with `gamemoderun` when available |
| `esync` | true | set `WINEESYNC=1` |
| `fsync` | true | set `WINEFSYNC=1` |

```bash
bytestrap config set runner /usr/bin/wine64
bytestrap config set prefix /mnt/games/wine-roblox
```

Per-run overrides: `--prefix`, `--runner`, and `$WINEPREFIX` all beat the file.

Extra prefixes to deploy flags into and extra env vars can be added by
editing `settings.json` (`extraWinePrefixes`, `extraEnv`).

---

## How detection works

Prefixes are scanned in priority order — explicit config/`$WINEPREFIX`
first, then:

1. `~/.wine` (stock Wine)
2. `~/.local/share/vinegar/prefixes/*` (+ flatpak path)
3. Bottles (`~/.local/share/bottles/bottles/*`, + flatpak path)
4. Sober flatpak app data
5. Steam `compatdata/*/pfx` (Proton prefixes)

Inside each prefix, every `drive_c/users/*/AppData/Local/Roblox/Versions/version-*`
containing `RobloxPlayerBeta.exe`/`RobloxStudioBeta.exe` is picked up.

Runners resolve in order: `config runner` → `wine`/`wine64`/`proton`/`umu-run`
on PATH → `Proton*/proton` under Steam/compatibilitytools.d.

Launch environment: `WINEPREFIX`, `WINEESYNC`/`WINEFSYNC`, GL shader disk
cache, `mesa_glthread`, `DXVK_ASYNC`, plus your `extraEnv`, optionally under
`gamemoderun`. Inspect it any time with `bytestrap launch --dry-run`.

---

## `roblox://` links (optional)

To open `roblox://` links from your browser with Bytestrap:

```bash
# ~/.local/share/applications/bytestrap-handler.desktop
[Desktop Entry]
Type=Application
Name=Bytestrap URL handler
Exec=bytestrap launch %u
NoDisplay=true
MimeType=x-scheme-handler/roblox;x-scheme-handler/roblox-studio;
```

```bash
xdg-mime default bytestrap-handler.desktop x-scheme-handler/roblox
```

---

## Troubleshooting

- **`No Roblox installs found`** — to play, install Sober
  (`flatpak install flathub org.vinegarhq.Sober`); for flag management of a
  Wine-layout install (e.g. Studio via Vinegar), pass the prefix explicitly:
  `bytestrap --prefix /path/to/prefix status`.
- **`No Wine/Proton runner found`** — install `wine64` (or your runner of
  choice) and make sure it's on PATH, or `bytestrap config set runner …`.
- **Game launches but flags don't apply** — run `bytestrap apply` and check
  for errors; some runners reset the prefix on launch, in which case re-run
  `bytestrap apply` (or add it to your launch routine).
- **Stutter on first launch** — shader cache is cold; `__GL_SHADER_DISK_CACHE`
  is enabled by default so it improves after the first session.
- **esync/fsync warnings from wine** — your kernel/wine lacks support; turn
  them off: `bytestrap config set esync false` (same for `fsync`).

---

## Dev notes

- `Bytestrap.Core` (`net8.0`, no packages): preset table, profiles,
  ClientAppSettings IO, install detection, runner. Reusable from any UI.
- `Bytestrap.Linux` (`net8.0` console): the CLI. `Bytestrap.Linux.sln` builds both.
- Keep `FlagPresets` in sync with the Windows `FastFlagManager.PresetFlags`
  table, and `PerformanceProfiles` with the Windows quick presets.
