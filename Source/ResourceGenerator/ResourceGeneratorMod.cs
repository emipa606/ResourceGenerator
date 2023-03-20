using System;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResourceGenerator;

[StaticConstructorOnStartup]
internal class ResourceGeneratorMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static ResourceGeneratorMod instance;

    public static float steelWorth;
    public static float componentWorth;
    public static float woodWorth;
    private static string currentVersion;

    /// <summary>
    ///     The private settings
    /// </summary>
    public readonly ResourceGeneratorSettings Settings;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public ResourceGeneratorMod(ModContentPack content) : base(content)
    {
        instance = this;
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Settings = GetSettings<ResourceGeneratorSettings>();
    }


    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Resource Generator";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        listing_Standard.Gap();
        listing_Standard.Label(
            "ReGe.GenerationTime.label".Translate(Settings.GenerationTime.min.ToStringTicksToPeriod(),
                Settings.GenerationTime.max.ToStringTicksToPeriod()), -1f,
            "ReGe.GenerationTime.tooltip".Translate());
        listing_Standard.IntRange(ref Settings.GenerationTime, 1, 7 * GenDate.TicksPerDay);
        listing_Standard.Label("ReGe.GenerationValue.label".Translate(), -1,
            "ReGe.GenerationValue.tooltip".Translate());
        Settings.GenerationValue = Widgets.HorizontalSlider_NewTemp(listing_Standard.GetRect(20),
            Settings.GenerationValue, 0,
            1000f,
            false, Settings.GenerationValue.ToStringMoney(), null, null, 1);
        listing_Standard.Gap();
        listing_Standard.Label("ReGe.ExampleTitle.label".Translate());
        listing_Standard.Label(
            "ReGe.ExampleSteel.label".Translate((int)Math.Round(instance.Settings.GenerationValue / steelWorth)));
        listing_Standard.Label(
            "ReGe.ExampleComponent.label".Translate(
                (int)Math.Round(instance.Settings.GenerationValue / componentWorth)));
        listing_Standard.Label(
            "ReGe.ExampleWood.label".Translate((int)Math.Round(instance.Settings.GenerationValue / woodWorth)));
        if (listing_Standard.ButtonText("ReGe.Reset.label".Translate()))
        {
            Settings.GenerationTime = new IntRange(120000, 120000);
            Settings.GenerationValue = steelWorth * 75;
        }

        listing_Standard.CheckboxLabeled("ReGe.ShowNotification.label".Translate(), ref Settings.ShowNotification);
        listing_Standard.CheckboxLabeled("ReGe.ShowConfirmation.label".Translate(), ref Settings.ShowConfirmation);

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("ReGe.Version.label".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }
}