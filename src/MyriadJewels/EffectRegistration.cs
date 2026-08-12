using Jewelcrafting;
using MyriadJewels.Effects.Bloodstone;
using MyriadJewels.Effects.Carnelian;
using MyriadJewels.Effects.Hematite;
using MyriadJewels.Effects.Howlite;
using MyriadJewels.Effects.MossAgate;
using MyriadJewels.Effects.Pyrite;
using MyriadJewels.Effects.Synergies;
using MyriadJewels.Effects.Tourmaline;

namespace MyriadJewels;

internal static class EffectRegistration
{
	internal static void Register()
	{
		// Howlite
		API.AddGemEffect<SpiritWolf.Config>("Spirit Wolf", "Summons a combat wolf familiar.", "Spirit wolf familiar strength $1%.");
		API.AddGemEffect<CombatCrow.Config>("Combat Crow", "Summons an attacking crow familiar.", "Combat crow familiar strength $1%.");
		API.AddGemEffect<SummonPotency.Config>("Summon Potency", "Familiar HP and damage up.", "Familiar power increased by $1%.");
		API.AddGemEffect<TrophyHunter.Config>("Trophy Hunter", "Trophy drops come easier.", "$1% trophy drop chance; $2% trophy weight reduction.");

		// Carnelian
		API.AddGemEffect<SecondWind.Config>("Second Wind", "Stamina recovers sooner after use.", "Stamina regen delay reduced by $1%.");
		API.AddGemEffect<CombatBreath.Config>("Combat Breath", "Attack and block cost less stamina.", "Attack and block stamina cost reduced by $1%.");
		API.AddGemEffect<EitrReservoir.Config>("Eitr Reservoir", "Increases maximum eitr.", "Maximum eitr increased by $1.");
		API.AddGemEffect<ClearHead.Config>("Clear Head", "Eitr returns faster after casting.", "Eitr regen delay reduced by $1%.");

		// Bloodstone
		API.AddGemEffect<ManicCycle.Config>("Manic Cycle", "Faster adrenaline gain and decay.", "Adrenaline gain/decay rate +$1%.");
		API.AddGemEffect<SurgeDetonate.Config>("Surge Detonate", "Surge spends adrenaline for burst.", "On Surge, convert adrenaline into $1% burst.");
		API.AddGemEffect<OverchargeLean.Config>("Overcharge Lean", "Lean into Overcharge risk/reward.", "Overcharge effects amplified by $1%.");
		API.AddGemEffect<HighWaterAnchor.Config>("High-Water Anchor", "Resist decay while adrenaline is high.", "While high, adrenaline decay reduced by $1%.");
		API.AddGemEffect<CrestAmp.Config>("Crest Amp", "Power while adrenaline is high.", "While adrenaline ≥ threshold: +$1% combat power.");
		API.AddGemEffect<TrinketResonance.Config>("Trinket Resonance", "Battle Flow trinkets scale harder while high.", "BF trinket scaling +$1% while high.");

		// Pyrite
		API.AddGemEffect<LightningProc.Config>("Lightning Proc", "Chance on hit for lightning burst.", "$2% chance to deal $1% lightning damage.");
		API.AddGemEffect<SpiritHex.Config>("Spirit Hex", "Chance on hit for spirit damage.", "$2% chance to deal $1% spirit damage.");
		API.AddGemEffect<PhysicalBurst.Config>("Physical Burst", "Chance on hit for physical burst.", "$2% chance to deal $1% physical burst.");
		API.AddGemEffect<StaminaSiphon.Config>("Stamina Siphon", "Chance to restore stamina on block/hit.", "$2% chance to restore $1 stamina.");
		API.AddGemEffect<EitrSiphon.Config>("Eitr Siphon", "Chance to restore eitr on hit.", "$2% chance to restore $1 eitr.");
		API.AddGemEffect<AdrenalSpike.Config>("Adrenal Spike", "Chance on hit for adrenaline.", "$2% chance to gain $1 adrenaline.");

		// Hematite — typed
		API.AddGemEffect<AddSlash.Config>("Add Slash", "Chance to add slash damage.", "$2% chance to add $1% slash.");
		API.AddGemEffect<AddBlunt.Config>("Add Blunt", "Chance to add blunt damage.", "$2% chance to add $1% blunt.");
		API.AddGemEffect<AddPierce.Config>("Add Pierce", "Chance to add pierce damage.", "$2% chance to add $1% pierce.");
		API.AddGemEffect<AddLightning.Config>("Add Lightning", "Chance to add lightning damage.", "$2% chance to add $1% lightning.");
		API.AddGemEffect<AddSpirit.Config>("Add Spirit", "Chance to add spirit damage.", "$2% chance to add $1% spirit.");
		API.AddGemEffect<AddChopPick.Config>("Add Chop Pick", "Chance to add chop/pickaxe damage.", "$2% chance to add $1% tool damage.");

		// Hematite — resists
		API.AddGemEffect<ResistSlash.Config>("Resist Slash", "Reduces slash damage taken.", "Slash damage taken reduced by $1%.");
		API.AddGemEffect<ResistBlunt.Config>("Resist Blunt", "Reduces blunt damage taken.", "Blunt damage taken reduced by $1%.");
		API.AddGemEffect<ResistPierce.Config>("Resist Pierce", "Reduces pierce damage taken.", "Pierce damage taken reduced by $1%.");
		API.AddGemEffect<ResistFire.Config>("Resist Fire", "Reduces fire damage taken.", "Fire damage taken reduced by $1%.");
		API.AddGemEffect<ResistFrost.Config>("Resist Frost", "Reduces frost damage taken.", "Frost damage taken reduced by $1%.");
		API.AddGemEffect<ResistLightning.Config>("Resist Lightning", "Reduces lightning damage taken.", "Lightning damage taken reduced by $1%.");
		API.AddGemEffect<ResistPoison.Config>("Resist Poison", "Reduces poison damage taken.", "Poison damage taken reduced by $1%.");
		API.AddGemEffect<ResistSpirit.Config>("Resist Spirit", "Reduces spirit damage taken.", "Spirit damage taken reduced by $1%.");

		// Tourmaline
		API.AddGemEffect<TwinBladeAptitude.Config>("Twin Blade Aptitude", "DualWield skill up.", "DualWield skill +$1%.");
		API.AddGemEffect<OffHandCatchUp.Config>("Off-Hand Catch-Up", "Softer off-hand penalty while dual.", "Off-hand penalty eased by $1%.");
		API.AddGemEffect<PairedRhythm.Config>("Paired Rhythm", "After dual hits, short attack speed.", "After dual hits: +$1% attack speed briefly.");
		API.AddGemEffect<ClawRake.Config>("Claw Rake", "Fist secondary assist.", "Fist secondary power +$1%.");
		API.AddGemEffect<KickThunder.Config>("Kick Thunder", "2H axe kick stagger.", "Kick stagger / blunt +$1%.");
		API.AddGemEffect<SledgeQuake.Config>("Sledge Quake", "Sledge special AOE/stagger.", "Sledge special power +$1%.");
		API.AddGemEffect<GreatWeaponPoise.Config>("Great Weapon Poise", "While 2H: stagger threshold up.", "2H stagger threshold +$1%.");
		API.AddGemEffect<GreatWeaponEconomy.Config>("Great Weapon Economy", "While 2H: attack stam cost down.", "2H attack stamina cost -$1%.");
		API.AddGemEffect<GreatWeaponSpecial.Config>("Great Weapon Special", "While 2H: secondary power up.", "2H secondary power +$1%.");
		API.AddGemEffect<GreatWeaponCommit.Config>("Great Weapon Commit", "While 2H: damage after heavy lands.", "After 2H secondary: +$1% damage briefly.");

		// Moss Agate
		API.AddGemEffect<WellFedGrace.Config>("Well Fed Grace", "Bonus while Nature Well Fed.", "While Well Fed: +$1% to homestead bonuses.");
		API.AddGemEffect<Fieldcraft.Config>("Fieldcraft", "Farming, mining, lumber, foraging.", "Profession skills +$1%.");
		API.AddGemEffect<Outrider.Config>("Outrider", "Ranching, sailing, packhorse.", "Travel/ranch skills +$1%.");
		API.AddGemEffect<Craftmaster.Config>("Craftmaster", "Blacksmithing and building.", "Craft skills +$1%.");
		API.AddGemEffect<ExpertRuneforger.Config>("Expert Runeforger", "Runeforging skill.", "Runeforging +$1%.");
		API.AddGemEffect<ExpertAtgeir.Config>("Expert Atgeir", "Polearm skill.", "Polearms +$1%.");
		API.AddGemEffect<ExpertUnarmed.Config>("Expert Unarmed", "Fists skill.", "Unarmed +$1%.");
		API.AddGemEffect<ExpertSledge.Config>("Expert Sledge", "Sledge / 2H blunt skill.", "Clubs +$1%.");

		// Synergies
		API.AddGemEffect<FamiliarBond.Config>("Familiar Bond", "Familiar duration and potency.", "Familiar duration/potency +$1%.");
		API.AddGemEffect<MoonHunt.Config>("Moon Hunt", "Night trophy hunt.", "At night: trophy chance +$1%.");
		API.AddGemEffect<ManicEngine.Config>("Manic Engine", "Overdrive Surge amp.", "Surge / detonate power +$1%.");
		API.AddGemEffect<CrestSovereign.Config>("Crest Sovereign", "Edge crest amp.", "Crest threshold/power +$1%.");
		API.AddGemEffect<FusedPulse.Config>("Fused Pulse", "High adren eitr return.", "While high adren: eitr return +$1%.");
		API.AddGemEffect<OpposedHearts.Config>("Opposed Hearts", "Surge AS / low DR.", "On Surge: AS +$1%; when low: DR +$2%.");
		API.AddGemEffect<TwinTempest.Config>("Twin Tempest", "DualWield cadence amp.", "Off-hand / rhythm ease +$1%.");
		API.AddGemEffect<GreatOath.Config>("Great Oath", "2H commit amp.", "2H special / commit +$1%.");
		API.AddGemEffect<Warpath.Config>("Warpath", "Stance hits feed adrenaline.", "Stance hits grant +$1 adrenaline.");
		API.AddGemEffect<SecondLung.Config>("Second Lung", "Economy sockets amp.", "Second Wind / Combat Breath +$1%.");
		API.AddGemEffect<OrangeReservoir.Config>("Orange Reservoir", "Eitr reservoir play.", "Eitr Reservoir / Clear Head +$1%.");
		API.AddGemEffect<Cascade.Config>("Cascade", "Pyrite proc chance up.", "Pyrite proc chances +$1%.");
		API.AddGemEffect<IronLattice.Config>("Iron Lattice", "Hematite resists amp.", "Hematite resists +$1%.");
		API.AddGemEffect<RootedCraft.Config>("Rooted Craft", "Profession sockets amp.", "Fieldcraft / Outrider / Craftmaster +$1%.");
		API.AddGemEffect<GreenThumb.Config>("Green Thumb", "Well Fed + green.", "Well Fed Grace +$1%.");
		API.AddGemEffect<FullLarder.Config>("Full Larder", "Food drain while Well Fed.", "Food drain -$1% while Well Fed.");
		API.AddGemEffect<RuneGrove.Config>("Rune Grove", "Runeforge gain.", "Runeforging gain +$1%.");
		API.AddGemEffect<PackAndPulse.Config>("Pack & Pulse", "Surge echoes to familiars.", "On Surge: familiars gain +$1% power briefly.");
	}
}
