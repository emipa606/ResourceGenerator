﻿using Verse;

namespace ResourceGenerator;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class ResourceGeneratorSettings : ModSettings
{
    public int DefaultLimit;
    public IntRange GenerationTime;
    public float GenerationValue;
    public bool ShowConfirmation = true;
    public bool ShowNotification = true;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref DefaultLimit, "DefaultLimit");
        Scribe_Values.Look(ref GenerationTime, "GenerationTime");
        Scribe_Values.Look(ref GenerationValue, "GenerationValue");
        Scribe_Values.Look(ref ShowNotification, "ShowNotification", true);
        Scribe_Values.Look(ref ShowConfirmation, "ShowConfirmation", true);
    }
}