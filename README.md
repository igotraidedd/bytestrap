<div align="center">

# Bytestrap

### roblox but it actually runs well

</div>

> [!CAUTION]
> this is an unofficial fork of fishstrap/bloxstrap. not affiliated with roblox. use at your own risk

fork of fishstrap/bloxstrap that's all about getting more fps and less ping. mess with fastflags, rendering, network stuff, whatever you need to make roblox not run like a slideshow.

> [!NOTE]
> windows 10+ only

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

plus everything else from bloxstrap/fishstrap

---

## install

grab the latest release from [here](https://github.com/bytestrap/bytestrap/releases), run it, done. configure stuff in the app before launching roblox.

---

## building it yourself

you need the [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) and windows 10+

```
git clone https://github.com/bytestrap/bytestrap.git
cd bytestrap
dotnet build Bytestrap.sln -c Release
```

output goes to `Bloxstrap/bin/Release/net6.0-windows/`

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
