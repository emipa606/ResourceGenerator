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

        while (amount > 0)
        {
            var amountToSpawn = amount;
            if (amount > product.stackLimit)
            {
                amountToSpawn = product.stackLimit;
            }

            if (!TryFindSpawnCell(product, amountToSpawn, out var outputTile))
            {
                return false;
            }

            var thing = ThingMaker.MakeThing(product);
            thing.stackCount = amountToSpawn;

            if (PropsSpawner.inheritFaction && thing.Faction != parent.Faction)
            {
                thing.SetFaction(parent.Faction);
            }

            GenPlace.TryPlaceThing(thing, outputTile, parent.Map, ThingPlaceMode.Direct, out var t);
            if (PropsSpawner.spawnForbidden)
            {
                t.SetForbidden(true);
            }

            amount -= amountToSpawn;
        }

        if (ResourceGeneratorMod.instance.Settings.ShowNotification && PropsSpawner.showMessageIfOwned &&
            parent.Faction == Faction.OfPlayer)
        {
            Messages.Message("MessageCompSpawnerSpawnedItem".Translate(product.LabelCap), parent,
                MessageTypeDefOf.PositiveEvent);
        }

        return true;
    }

    public bool TryFindSpawnCell(ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
    {
        result = IntVec3.Invalid;
        if (((ResourceGenerator)parent).IsValidSpawnCell(((ResourceGenerator)parent).OutputTile, thingToSpawn,
                spawnCount))
        {
            result = ((ResourceGenerator)parent).OutputTile;
            return true;
        }

        foreach (var intVec in ((ResourceGenerator)parent).ValidCells)
        {
            if (!((ResourceGenerator)parent).IsValidSpawnCell(intVec, thingToSpawn, spawnCount))
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
        var str = PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : $"{PropsSpawner.saveKeysPrefix}_";
        Scribe_Values.Look(ref ticksUntilSpawn, $"{str}ticksUntilSpawn");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmosExtra in base.CompGetGizmosExtra())
        {
            yield return gizmosExtra;
        }

        yield return new Command_Action
        {
            defaultLabel = "ReGe.RotateOutput.label".Translate(),
            action = delegate
            {
                if (!parent.Spawned)
                {
                    return;
                }

                ((ResourceGenerator)parent).NextOutputTile();
            },
            icon = TexUI.RotLeftTex
        };

        if (Prefs.DevMode)
        {
            yield return new Command_Action
            {
                defaultLabel = $"DEBUG: Spawn {((ResourceGenerator)parent).CurrentProduct.label}",
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
            return $"{"NextSpawnedItemIn"
                .Translate(GenLabel.ThingLabel(((ResourceGenerator)parent).CurrentProduct, null,
                    ((ResourceGenerator)parent).CurrentAmount))
                .Resolve()}: {ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor)}";
        }

        return null;
    }
}