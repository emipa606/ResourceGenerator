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
    public static readonly float ValuePerCycle;

    static Main()
    {
        ValidResources = new HashSet<ThingDef>();
        ValidResources.AddRange(DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.IsStuff));
        ValidResources.AddRange(DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => def.mineable).Select(def => def.building.mineableThing));
        //ValidResources.AddRange(DefDatabase<ThingDef>.AllDefsListForReading
        //    .Where(def => def.recipeMaker != null && def.stackLimit > 1));
        Log.Message($"[ResourceGenerator]: Added {ValidResources.Count} resources as possible to generate");
        ValuePerCycle = ThingDefOf.Steel.GetStatValueAbstract(StatDefOf.MarketValue) * 75;
    }
}