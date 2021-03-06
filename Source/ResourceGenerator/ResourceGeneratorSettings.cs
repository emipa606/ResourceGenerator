using Verse;

namespace ResourceGenerator;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class ResourceGeneratorSettings : ModSettings
{
    public IntRange GenerationTime;
    public float GenerationValue;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref GenerationTime, "GenerationTime");
        Scribe_Values.Look(ref GenerationValue, "GenerationValue");
    }
}