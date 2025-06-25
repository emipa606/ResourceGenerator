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
    public static ResourceGeneratorMod Instance;

    public static float SteelWorth;
    public static float ComponentWorth;
    public static float WoodWorth;
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
        Instance = this;
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
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        listingStandard.Gap();
        listingStandard.Label(
            "ReGe.GenerationTime.label".Translate(Settings.GenerationTime.min.ToStringTicksToPeriod(),
                Settings.GenerationTime.max.ToStringTicksToPeriod()), -1f,
            "ReGe.GenerationTime.tooltip".Translate());
        listingStandard.IntRange(ref Settings.GenerationTime, 1, 7 * GenDate.TicksPerDay);
        listingStandard.Label("ReGe.GenerationValue.label".Translate(), -1,
            "ReGe.GenerationValue.tooltip".Translate());
        Settings.GenerationValue = Widgets.HorizontalSlider(listingStandard.GetRect(20),
            Settings.GenerationValue, 0,
            2500f,
            false, Settings.GenerationValue.ToStringMoney(), null, null, 1);
        listingStandard.Gap();
        listingStandard.Label("ReGe.ExampleTitle.label".Translate());
        listingStandard.Label(
            "ReGe.ExampleSteel.label".Translate((int)Math.Round(Instance.Settings.GenerationValue / SteelWorth)));
        listingStandard.Label(
            "ReGe.ExampleComponent.label".Translate(
                (int)Math.Round(Instance.Settings.GenerationValue / ComponentWorth)));
        listingStandard.Label(
            "ReGe.ExampleWood.label".Translate((int)Math.Round(Instance.Settings.GenerationValue / WoodWorth)));
        if (listingStandard.ButtonText("ReGe.Reset.label".Translate()))
        {
            Settings.GenerationTime = new IntRange(120000, 120000);
            Settings.GenerationValue = SteelWorth * 75;
        }

        listingStandard.CheckboxLabeled("ReGe.ShowNotification.label".Translate(), ref Settings.ShowNotification);
        listingStandard.CheckboxLabeled("ReGe.ShowConfirmation.label".Translate(), ref Settings.ShowConfirmation);
        Settings.DefaultLimit = (int)listingStandard.SliderLabeled(
            "ReGe.DefaultLimit.label".Translate(Settings.DefaultLimit == 0
                ? "ReGe.unlimited".Translate()
                : Settings.DefaultLimit), Settings.DefaultLimit, 0, 1000f,
            tooltip: "ReGe.DefaultLimit.tooltip".Translate());

        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("ReGe.Version.label".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }
}