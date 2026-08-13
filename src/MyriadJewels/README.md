# MyriadJewels

In-pack BepInEx plugin that registers seven additive Jewelcrafting stones for Myriad. Stock JC gems are untouched.

Config lives in `Assets/MyriadJewels.yaml` (sockets, synergy conditions, biome spawns). Power arrays are tier 1 / 2 / 3.

**Pack JC layout:** Megingjord (utility belt) is for stock JC utility gems only — wisplight, wishbone, comfortable, etc. Myriad stones use armor, weapons, trinket, and JC ring/neck items. Pack ships `config/org.bepinex.plugins.jewelcrafting.cfg` with Wisplight Gem, Wishbone Gem, Ring Slot, and Necklace Slot enabled.

## Stones

| Stone | Color key | Role |
|-------|-----------|------|
| Howlite | `howlite` | Familiars & hunt |
| Carnelian | `carnelian` | Stamina / eitr economy |
| Bloodstone | `bloodstone` | Battle Flow — Overdrive vs Edge |
| Pyrite | `pyrite` | On-hit procs (no F/F/P/vamp clones) |
| Hematite | `hematite` | Typed damage adds + resists |
| Tourmaline | `tourmaline` | DualWield / majsters / 2H (no shield) |
| Moss Agate | `mossagate` | Homestead / Well Fed / profession skills |

### Howlite — familiars & hunt

| Effect | Slot | Power (T1–T3) |
|--------|------|----------------|
| Spirit Wolf | Cloak | 15 / 25 / 40% familiar strength |
| Combat Crow | Trinket | 15 / 25 / 40% familiar strength |
| Summon Potency | Head | 10 / 18 / 28% familiar HP/damage |
| Trophy Hunter | JC necklace | 5 / 10 / 15% drop chance; same for trophy weight cut |

Chest does **not** get Summon Potency. Familiar duration is synergy-only (`Familiar Bond`).

### Carnelian — stamina / eitr economy

| Effect | Slot | Power |
|--------|------|-------|
| Second Wind | Legs | 10 / 18 / 28% stamina regen delay cut |
| Combat Breath | JC necklace, Shield | 8 / 14 / 22% attack/block stam cost cut |
| Eitr Reservoir | Head | +15 / 30 / 50 max eitr |
| Clear Head | Magic | 10 / 18 / 28% eitr regen delay cut |

### Bloodstone — Battle Flow

Two lanes: **Overdrive** (spend/cycle adrenaline) vs **Edge** (hold high adrenaline). Synergies `Manic Engine` / `Crest Sovereign` are mutually exclusive (red vs yellow majority).

| Effect | Slot | Lane | Power |
|--------|------|------|-------|
| Manic Cycle | Weapon | Overdrive | 10 / 18 / 28% adren gain/decay |
| Surge Detonate | Chest | Overdrive | 12 / 20 / 32% Surge burst |
| Overcharge Lean | Trinket | Overdrive | 10 / 18 / 28% Overcharge amp |
| High-Water Anchor | Cloak | Edge | 12 / 20 / 30% decay resist while high |
| Crest Amp | Head | Edge | 8 / 14 / 22% combat power while high |
| Trinket Resonance | JC necklace | Edge | 8 / 14 / 22% BF trinket scaling while high |

### Pyrite — procs

| Effect | Slot | Damage/restore · chance |
|--------|------|-------------------------|
| Lightning Proc | Weapon | 20 / 35 / 50% · 8 / 12 / 18% |
| Spirit Hex | Magic | 20 / 35 / 50% · 8 / 12 / 18% |
| Physical Burst | Axe, Club | 25 / 40 / 55% · 8 / 12 / 18% |
| Stamina Siphon | Shield | 8 / 14 / 22 stam · 12 / 18 / 25% |
| Eitr Siphon | ElementalMagic | 6 / 10 / 16 eitr · 10 / 15 / 22% |
| Adrenal Spike | Trinket | 5 / 8 / 12 adren · 10 / 15 / 22% |

### Hematite — typed adds & resists

**Adds** (chance to add typed % damage):

| Effect | Slot | Power · chance |
|--------|------|----------------|
| Add Slash | Sword | 8 / 14 / 22% · 30 / 45 / 55% |
| Add Blunt | Club | 8 / 14 / 22% · 30 / 45 / 55% |
| Add Pierce | Spear, Knife | 8 / 14 / 22% · 30 / 45 / 55% |
| Add Lightning | ElementalMagic | 8 / 14 / 22% · 30 / 45 / 55% |
| Add Spirit | BloodMagic | 8 / 14 / 22% · 30 / 45 / 55% |
| Add Chop Pick | Tool | 10 / 16 / 24% · 30 / 45 / 55% |

**Resists** (damage taken reduced):

| Effect | Slot | Power |
|--------|------|-------|
| Resist Slash | Chest | 3 / 6 / 10% |
| Resist Blunt | Legs | 3 / 6 / 10% |
| Resist Pierce | Cloak | 3 / 6 / 10% |
| Resist Fire | Head | 3 / 6 / 10% |
| Resist Frost | Shield | 3 / 6 / 10% |
| Resist Lightning | JC necklace | 3 / 6 / 10% |
| Resist Poison | JC ring | 3 / 6 / 10% |
| Resist Spirit | Trinket | 3 / 6 / 10% |

### Tourmaline — DW / majsters / 2H

| Effect | Slot | Power |
|--------|------|-------|
| Twin Blade Aptitude | Head | 5 / 9 / 14% DualWield |
| Off-Hand Catch-Up | Legs | 8 / 14 / 22% off-hand penalty ease |
| Paired Rhythm | Weapon | 8 / 14 / 22% brief AS after dual hits |
| Claw Rake | Knife (2H claws) | 10 / 16 / 24% fist secondary |
| Kick Thunder | Axe | 10 / 16 / 24% kick stagger/blunt |
| Sledge Quake | Club | 10 / 16 / 24% sledge special |
| Great Weapon Poise | Chest | 10 / 16 / 24% 2H stagger threshold |
| Great Weapon Economy | Cloak | 8 / 14 / 22% 2H attack stam cut |
| Great Weapon Special | JC necklace | 10 / 16 / 24% 2H secondary |
| Great Weapon Commit | Trinket | 8 / 14 / 22% damage after 2H secondary |

### Moss Agate — homestead

| Effect | Slot | Power |
|--------|------|-------|
| Well Fed Grace | Head | 8 / 14 / 22% homestead bonuses while Well Fed |
| Fieldcraft | Tool | 5 / 9 / 14% farming/mining/lumber/forage |
| Outrider | Trinket | 5 / 9 / 14% ranch/sail/packhorse |
| Craftmaster | JC necklace | 5 / 9 / 14% blacksmith/build |
| Expert Runeforger | Cloak | 5 / 9 / 14% Runeforging |
| Expert Atgeir | Chest | 5 / 9 / 14% polearms |
| Expert Unarmed | Legs | 5 / 9 / 14% fists |
| Expert Sledge | Club | 5 / 9 / 14% clubs / 2H blunt |

## Synergies (18)

Conditions use socketed gem counts (`howlite`, `bloodstone`, …) and stock JC color counts (`red`, `yellow`, `black`, `orange`, `purple`, `green`).

| Synergy | Conditions | Effect |
|---------|------------|--------|
| Familiar Bond | howlite > 3 | +20% familiar duration/potency |
| Moon Hunt | howlite ≥ 2 · black ≥ 3 | +15% trophy chance at night |
| Manic Engine | bloodstone ≥ 4 · red ≥ 2 · red > yellow | +18% Surge / detonate (blocks Crest) |
| Crest Sovereign | bloodstone ≥ 4 · yellow ≥ 2 · yellow > red | +18% Crest threshold/power (blocks Manic) |
| Fused Pulse | bloodstone ≥ 3 · orange ≥ 2 | +15% eitr return while high adren |
| Opposed Hearts | bloodstone ≥ 3 · purple ≥ 3 | On Surge +12% AS; when low +DR |
| Twin Tempest | tourmaline ≥ 4 · black < 2 | +15% DW off-hand / rhythm |
| Great Oath | tourmaline ≥ 4 · black ≥ 2 | +15% 2H special / commit |
| Warpath | tourmaline ≥ 3 · bloodstone ≥ 3 | Stance hits +4 adrenaline |
| Second Lung | carnelian ≥ 4 | +12% Second Wind / Combat Breath |
| Orange Reservoir | carnelian ≥ 3 · orange ≥ 3 | +10% Eitr Reservoir / Clear Head |
| Cascade | pyrite ≥ 4 | +8% Pyrite proc chances |
| Iron Lattice | hematite ≥ 5 | +4% Hematite resists |
| Rooted Craft | mossagate ≥ 4 | +8% Fieldcraft / Outrider / Craftmaster |
| Green Thumb | mossagate ≥ 2 · green ≥ 2 | +10% Well Fed Grace |
| Full Larder | mossagate ≥ 3 · purple ≥ 2 | −12% food drain while Well Fed |
| Rune Grove | mossagate ≥ 3 · orange ≥ 2 | +10% Runeforging gain |
| Pack & Pulse | howlite ≥ 2 · bloodstone ≥ 2 | On Surge: familiars +20% power briefly |

## Layout

| Path | Role |
|------|------|
| `src/MyriadJewels/` | Source (this folder) |
| `plugins/MyriadJewels/` | Committed Release DLL (ships with pack) |
| `Assets/MyriadJewels.yaml` | Sockets, synergies, gem spawns |

## Build

Requires Valheim Managed assemblies and a BepInEx profile with Jewelcrafting 2.0.1. Set local paths in `Directory.Build.props.user` (gitignored, next to this README):

```xml
<Project>
  <PropertyGroup>
    <ValheimDir>/path/to/Valheim</ValheimDir>
    <BepInExDir>/path/to/profile/BepInEx</BepInExDir>
  </PropertyGroup>
</Project>
```

```bash
cd src/MyriadJewels
dotnet build -c Release
```

Release output is copied to `plugins/MyriadJewels/MyriadJewels.dll`.

## Install

Copy `plugins/MyriadJewels/` into the profile `BepInEx/plugins/` (or include `plugins/` in the pack zip next to `config/`).
