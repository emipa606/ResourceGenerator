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
        if (ResourceGeneratorMod.Instance.Settings.GenerationValue == 0)
        {
            ResourceGeneratorMod.Instance.Settings.GenerationValue =
                ThingDefOf.Steel.GetStatValueAbstract(StatDefOf.MarketValue) * 75;
        }

        if (ResourceGeneratorMod.Instance.Settings.GenerationTime == default)
        {
            ResourceGeneratorMod.Instance.Settings.GenerationTime = new IntRange(120000, 120000);
        }

        ResourceGeneratorMod.SteelWorth = ThingDefOf.Steel.GetStatValueAbstract(StatDefOf.MarketValue);
        ResourceGeneratorMod.ComponentWorth =
            ThingDefOf.ComponentIndustrial.GetStatValueAbstract(StatDefOf.MarketValue);
        ResourceGeneratorMod.WoodWorth = ThingDefOf.WoodLog.GetStatValueAbstract(StatDefOf.MarketValue);

        ValidResources = [];
        ValidResources.AddRange(DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.IsStuff));
        ValidResources.AddRange(DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.mineable).Select(def => def.building.mineableThing));
        Log.Message($"[ResourceGenerator]: Added {ValidResources.Count} resources as possible to generate");
    }
}