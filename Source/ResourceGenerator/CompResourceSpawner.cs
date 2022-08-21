using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ResourceGenerator;

public class CompResourceSpawner : ThingComp
{
    private int ticksUntilSpawn;

    public CompProperties_ResourceSpawner PropsSpawner => (CompProperties_ResourceSpawner)props;

    private bool PowerOn
    {
        get
        {
            var comp = parent.GetComp<CompPowerTrader>();
            return comp is { PowerOn: true };
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!respawningAfterLoad)
        {
            ResetCountdown();
        }
    }

    public override void CompTick()
    {
        TickInterval(1);
    }

    public override void CompTickRare()
    {
        TickInterval(250);
    }

    private void TickInterval(int interval)
    {
        if (!parent.Spawned)
        {
            return;
        }

        var comp = parent.GetComp<CompCanBeDormant>();
        if (comp != null)
        {
            if (!comp.Awake)
            {
                return;
            }
        }
        else if (parent.Position.Fogged(parent.Map))
        {
            return;
        }

        if (PropsSpawner.requiresPower && !PowerOn)
        {
            return;
        }

        ticksUntilSpawn -= interval;
        CheckShouldSpawn();
    }

    private void CheckShouldSpawn()
    {
        if (ticksUntilSpawn > 0)
        {
            return;
        }

        ResetCountdown();
        TryDoSpawn();
    }

    public bool TryDoSpawn()
    {
        if (!parent.Spawned)
        {
            return false;
        }

        var product = ((ResourceGenerator)parent).CurrentProduct;
        var amount = ((ResourceGenerator)parent).CurrentAmount;
        if (PropsSpawner.spawnMaxAdjacent >= 0)
        {
            var num = 0;
            for (var i = 0; i < 9; i++)
            {
                var c = parent.Position + GenAdj.AdjacentCellsAndInside[i];
                if (!c.InBounds(parent.Map))
                {
                    continue;
                }

                var thingList = c.GetThingList(parent.Map);
                foreach (var currentThing in thingList)
                {
                    if (currentThing.def != product)
                    {
                        continue;
                    }

                    num += currentThing.stackCount;
                    if (num >= PropsSpawner.spawnMaxAdjacent)
                    {
                        return false;
                    }
                }
            }
        }

        if (!CompSpawner.TryFindSpawnCell(parent, product, amount, out var center))
        {
            return false;
        }

        var thing = ThingMaker.MakeThing(product);
        thing.stackCount = amount;

        if (PropsSpawner.inheritFaction && thing.Faction != parent.Faction)
        {
            thing.SetFaction(parent.Faction);
        }

        GenPlace.TryPlaceThing(thing, center, parent.Map, ThingPlaceMode.Direct, out var t);
        if (PropsSpawner.spawnForbidden)
        {
            t.SetForbidden(true);
        }

        if (ResourceGeneratorMod.instance.Settings.ShowNotification && PropsSpawner.showMessageIfOwned &&
            parent.Faction == Faction.OfPlayer)
        {
            Messages.Message("MessageCompSpawnerSpawnedItem".Translate(product.LabelCap), thing,
                MessageTypeDefOf.PositiveEvent);
        }

        return true;
    }

    public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
    {
        foreach (var intVec in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
        {
            if (!intVec.Walkable(parent.Map))
            {
                continue;
            }

            var edifice = intVec.GetEdifice(parent.Map);
            if (edifice != null && thingToSpawn.IsEdifice())
            {
                continue;
            }

            if (edifice is Building_Door { FreePassage: false } ||
                parent.def.passability != Traversability.Impassable &&
                !GenSight.LineOfSight(parent.Position, intVec, parent.Map))
            {
                continue;
            }

            var noGoodCell = false;
            var thingList = intVec.GetThingList(parent.Map);
            foreach (var thing in thingList)
            {
                if (thing.def.category != ThingCategory.Item || thing.def == thingToSpawn &&
                    thing.stackCount <=
                    thingToSpawn.stackLimit - spawnCount)
                {
                    continue;
                }

                noGoodCell = true;
                break;
            }

            if (noGoodCell)
            {
                continue;
            }

            result = intVec;
            return true;
        }

        result = IntVec3.Invalid;
        return false;
    }

    private void ResetCountdown()
    {
        ticksUntilSpawn = ResourceGeneratorMod.instance.Settings.GenerationTime.RandomInRange;
    }

    public override void PostExposeData()
    {
        var str = PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : PropsSpawner.saveKeysPrefix + "_";
        Scribe_Values.Look(ref ticksUntilSpawn, str + "ticksUntilSpawn");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (Prefs.DevMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Spawn " + ((ResourceGenerator)parent).CurrentProduct.label,
                icon = TexCommand.DesirePower,
                action = delegate
                {
                    ResetCountdown();
                    TryDoSpawn();
                }
            };
        }
    }

    public override string CompInspectStringExtra()
    {
        if (PropsSpawner.writeTimeLeftToSpawn && (!PropsSpawner.requiresPower || PowerOn))
        {
            return "NextSpawnedItemIn"
                       .Translate(GenLabel.ThingLabel(((ResourceGenerator)parent).CurrentProduct, null,
                           ((ResourceGenerator)parent).CurrentAmount))
                       .Resolve() +
                   ": " + ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
        }

        return null;
    }
}