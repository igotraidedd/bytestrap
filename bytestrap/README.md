<div align="center">

# Bytestrap

### roblox but it actually runs well

</div>

> [!CAUTION]
> this is an unofficial fork of fishstrap/bloxstrap. not affiliated with roblox. use at your own risk

fork of fishstrap/bloxstrap that's all about getting more fps and less ping. mess with fastflags, rendering, network stuff, whatever you need to make roblox not run like a slideshow.

> [!NOTE]
> windows app needs windows 10+. there's also a linux port (see below)

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
- 🐧 linux port - same flags + profiles as a CLI for wine/proton runners (see [LINUX.md](LINUX.md))

plus everything else from bloxstrap/fishstrap

---

## install

grab the latest release from [here](https://github.com/bytestrap/bytestrap/releases), run it, done. configure stuff in the app before launching roblox.

on linux, build the port instead:

```
./linux/install.sh
bytestrap status
```

---

## building it yourself

**windows app** - you need the [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and windows 10+

```
git clone https://github.com/bytestrap/bytestrap.git
cd bytestrap/bytestrap
git submodule update --init
dotnet build Bytestrap.sln -c Release
```

output goes to `Bloxstrap/bin/Release/net8.0-windows/`

**linux port** - you need the .NET 8.0 SDK, that's it (zero NuGet dependencies)

```
dotnet build Bytestrap.Linux.sln -c Release
# binary: Bytestrap.Linux/bin/Release/net8.0/bytestrap
```

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
