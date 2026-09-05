# Myriad 4.x TODOs

## Homestead hammer + cultivator inventory

Shipped in-repo:

- [`config/balrond-hammerconfig.json`](config/balrond-hammerconfig.json) — 8-tab sorter (reload **O**)
- [`config/wackysDatabase/Pieces/`](config/wackysDatabase/Pieces/) — **42** Railway overlays → `piecehammer: Hammer` (restart or WackyDB reload). Plumga Clutter stays on the **Clutter Bucket**.
- [`config/PlantEverything/ExtraResources.json`](config/PlantEverything/ExtraResources.json) + PE/Farming cfgs

Rules: allowlist lanes; Balrond-disabled vanilla → neither; plants on cultivator only.

Next: in-game smoke (Hammer has Rails; Clutter Bucket has Clutter; Cultivator ExtraResources). No further inventory authorship queued.

## Compat / content

- **JC + RF inventory chrome** — live profile has JC `Color Item Names` / `Display Socket Background` = **Off** (RF owns rarity). Not shipped in pack `config/` (avoid full JC cfg). Need a minimal authored override or MyriadJewels force if we want this for Thunderstore installs.
- **Custom jewels** — [`mods/MyriadJewels/`](mods/MyriadJewels/) Thunderstore package + [`src/MyriadJewels/`](src/MyriadJewels/) source. Modpack depends on `Myriad-MyriadJewels-0.1.4`. Build: `python3 scripts/build_myriad_jewels_mod.py --build`.
- **Tenacity** — only if Balrond combat proves too hard
