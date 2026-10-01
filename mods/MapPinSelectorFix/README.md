# MapPinSelectorFix

Manual **position** and **scale** for the Valheim large-map pin selector column (`IconPanel`, death/boss filters, ping).

Use this when the pin icons sit off the right edge of the screen (common on ultrawide, or after other map UI mods). No automatic resolution math — you set the offsets.

## Config

`BepInEx/config/myriad.mappinselectorfix.cfg`

| Key | Default | Notes |
|-----|---------|--------|
| `Enabled` | `true` | Master switch |
| `OffsetX` | `-240` | Negative = left (toward map) |
| `OffsetY` | `0` | Positive = up |
| `Scale` | `1` | Size multiplier |
| `IncludeFilterPanel` | `true` | Death/boss column |
| `IncludePingPanel` | `true` | Ping icon |

Tweaks apply on next large-map open, or live via Configuration Manager while the map is open.

## Build

```bash
cp src/MapPinSelectorFix/Directory.Build.props.user.example \
   src/MapPinSelectorFix/Directory.Build.props.user
# edit ValheimDir / BepInExDir if needed
dotnet build -c Release src/MapPinSelectorFix
# ships to plugins/MapPinSelectorFix/MapPinSelectorFix.dll
```

From the repo root:

```bash
dotnet build -c Release mods/MapPinSelectorFix/src/MapPinSelectorFix
# or: python3 scripts/build_map_pin_selector_fix.py --build
```
