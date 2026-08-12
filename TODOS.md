# Myriad 4.x TODOs

## Homestead hammer + cultivator inventory

Shipped in-repo:

- [`config/balrond-hammerconfig.json`](config/balrond-hammerconfig.json) — 8-tab sorter (reload **O**)
- [`config/wackysDatabase/Pieces/`](config/wackysDatabase/Pieces/) — **212** Clutter/Railway overlays → `piecehammer: Hammer` (restart or WackyDB reload)
- [`config/PlantEverything/ExtraResources.json`](config/PlantEverything/ExtraResources.json) + PE/Farming cfgs

Rules: allowlist lanes; Balrond-disabled vanilla → neither; plants on cultivator only.

Next: in-game smoke (Hammer has Clutter+Rails; Cultivator ExtraResources). No further inventory authorship queued.

## Compat / content

- **JC + RF inventory chrome** — live profile has JC `Color Item Names` / `Display Socket Background` = **Off** (RF owns rarity). Not shipped in pack `config/` (avoid full JC cfg). Need a minimal authored override or MyriadJewels force if we want this for Thunderstore installs.
- **Custom jewels** — in-repo [`src/MyriadJewels/`](src/MyriadJewels/) → ship [`plugins/MyriadJewels/`](plugins/MyriadJewels/); copy into profile `BepInEx/plugins/`. See [`src/MyriadJewels/README.md`](src/MyriadJewels/README.md). No Thunderstore package / EpicJewels pin.
- **Tenacity** — only if Balrond combat proves too hard
