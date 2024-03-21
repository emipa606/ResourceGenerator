using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResourceGenerator;

[StaticConstructorOnStartup]
public class Main
{
    public static readonly HashSet<ThingDef> ValidResources;
    public static readonly Texture2D NoPower = ContentFinder<Texture2D>.Get("UI/Buttons/Abandon", false);

    static Main()
    {
        if (ResourceGeneratorMod.instance.Settings.GenerationValue == 0)
        {
            ResourceGeneratorMod.instance.Settings.GenerationValue =
                ThingDefOf.Steel.GetStatValueAbstract(StatDefOf.MarketValue) * 75;
        }

        if (ResourceGeneratorMod.instance.Settings.GenerationTime == default)
        {
            ResourceGeneratorMod.instance.Settings.GenerationTime = new IntRange(120000, 120000);
        }

        ResourceGeneratorMod.steelWorth = ThingDefOf.Steel.GetStatValueAbstract(StatDefOf.MarketValue);
        ResourceGeneratorMod.componentWorth =
            ThingDefOf.ComponentIndustrial.GetStatValueAbstract(StatDefOf.MarketValue);
        ResourceGeneratorMod.woodWorth = ThingDefOf.WoodLog.GetStatValueAbstract(StatDefOf.MarketValue);

        ValidResources = [];
        ValidResources.AddRange(DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.IsStuff));
        ValidResources.AddRange(DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.mineable).Select(def => def.building.mineableThing));
        Log.Message($"[ResourceGenerator]: Added {ValidResources.Count} resources as possible to generate");
    }
}