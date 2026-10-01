# Myriad's Valheim Mods

Custom Valheim mods and Thunderstore packs from [MyriadSecurity](https://github.com/MyriadSecurity).

This repo holds **first-party plugins** plus the **modpack overlays** we ship for servers and clients.

## Custom mods

| Mod | Folder | What it does |
|-----|--------|----------------|
| **MyriadJewels** | [`mods/MyriadJewels/`](mods/MyriadJewels/) | Extra Jewelcrafting stones (Howlite, Carnelian, Bloodstone, …) |
| **MapPinSelectorFix** | [`mods/MapPinSelectorFix/`](mods/MapPinSelectorFix/) | Manual OffsetX / OffsetY / Scale for the large-map pin selector |

## Modpacks

| Pack | Folder | What it does |
|------|--------|----------------|
| **Myriad Lite** | [`packs/Lite/`](packs/Lite/) | Vanilla+ server pack (shared mods + configs) |
| **Lite Client Mods** | [`packs/Lite_Client_Mods/`](packs/Lite_Client_Mods/) | Optional client-only extras for Lite |
| **Myriad** | [`packs/Myriad/`](packs/Myriad/) | Full Balrond / Amazing Nature pack |

## Layout

```
mods/          # BepInEx plugins we write and ship
packs/         # Thunderstore pack manifests + config overlays
Releases/      # Local build zips (gitignored)
scripts/       # Local build / deploy tooling (gitignored)
```

## Build

**Plugins** (from repo root):

```bash
# MapPinSelectorFix
cp mods/MapPinSelectorFix/src/MapPinSelectorFix/Directory.Build.props.user.example \
   mods/MapPinSelectorFix/src/MapPinSelectorFix/Directory.Build.props.user
dotnet build -c Release mods/MapPinSelectorFix/src/MapPinSelectorFix

# MyriadJewels
dotnet build -c Release mods/MyriadJewels/src/MyriadJewels
```

**Pack zips** (local tooling under `scripts/`):

```bash
python3 scripts/build_thunderstore_zip.py --root packs/Lite
python3 scripts/build_map_pin_selector_fix.py --build
python3 scripts/build_myriad_jewels_mod.py --build
```

## Install

Use [r2modman](https://thunderstore.io/c/valheim/p/ebkr/r2modman/). Packs go on the server and all clients (except **Lite Client Mods**, which is client-only). Custom mods install as normal Thunderstore dependencies or from a local zip under `Releases/`.
