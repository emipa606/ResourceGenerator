using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResourceGenerator;

public class ResourceGenerator : Building
{
    private static readonly Vector3 iconSize = new Vector3(0.5f, 1f, 0.5f);
    private static readonly Vector3 iconOffset = new Vector3(0.3f, 1.1f, 0.3f);
    private int currentAmount;
    private Color currentColor;
    private Texture2D currentIcon;
    private ThingDef currentProduct;
    private CompResourceSpawner spawner;

    public CompResourceSpawner Spawner
    {
        get
        {
            if (spawner == null)
            {
                spawner = GetComp<CompResourceSpawner>();
            }

            return spawner;
        }
        set => spawner = value;
    }

    public ThingDef CurrentProduct
    {
        get
        {
            if (currentProduct == null)
            {
                currentProduct = ThingDefOf.Steel;
            }

            return currentProduct;
        }
        set
        {
            currentProduct = value;
            CurrentIcon = null;
            CurrentColor = default;
        }
    }

    public int CurrentAmount
    {
        get
        {
            if (currentAmount != 0)
            {
                return currentAmount;
            }

            currentAmount = amountToSpawn(CurrentProduct);
            return currentAmount;
        }
        set => currentAmount = value;
    }

    public Texture2D CurrentIcon
    {
        get
        {
            if (currentIcon != null)
            {
                return currentIcon;
            }

            currentIcon = getIcon(CurrentProduct);
            return currentIcon;
        }
        set => currentIcon = value;
    }

    public Color CurrentColor
    {
        get
        {
            if (currentColor != default)
            {
                return currentColor;
            }

            currentColor = getColor(CurrentProduct);
            return currentColor;
        }
        set => currentColor = value;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref currentProduct, "currentProduct");
        Scribe_Values.Look(ref currentAmount, "currentAmount");
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        setProduct(CurrentProduct, false);
    }

    public override void Draw()
    {
        base.Draw();
        var iconLocation = DrawPos + iconOffset;
        var color = Color.white;
        var icon = Main.NoPower;
        if (GetComp<CompPowerTrader>().PowerOn)
        {
            color = CurrentColor;
            icon = CurrentIcon;
        }

        color.a *= 0.7f;
        Matrix4x4 matrix = default;
        matrix.SetTRS(iconLocation, Quaternion.identity, iconSize);
        var material = MaterialPool.MatFrom(icon, ShaderDatabase.Transparent, color);
        Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        yield return new Command_Action
        {
            action = selectProduct,
            hotKey = KeyBindingDefOf.Misc1,
            defaultDesc = "ReGe.product".Translate(),
            icon = CurrentIcon,
            iconAngle = CurrentProduct.uiIconAngle,
            iconOffset = CurrentProduct.uiIconOffset,
            defaultIconColor = CurrentColor,
            defaultLabel = CurrentProduct.LabelCap
        };
    }

    public override void Tick()
    {
        base.Tick();
        if (GenTicks.TicksGame % GenTicks.TickRareInterval == 0 && GetComp<CompPowerTrader>().PowerOn)
        {
            FleckMaker.ThrowSmoke(DrawPos, Map, 1f);
        }
    }

    private void setProduct(ThingDef thingToSet, bool reset)
    {
        CurrentProduct = thingToSet;
        CurrentAmount = amountToSpawn(CurrentProduct);

        if (reset)
        {
            Spawner.PostSpawnSetup(false);
        }
    }


    private void selectProduct()
    {
        var list = new List<FloatMenuOption>();
        foreach (var thingDef in Main.ValidResources)
        {
            try
            {
                if (thingDef.stackLimit <= 1 ||
                    thingDef.researchPrerequisites?.Any(projectDef => !projectDef.IsFinished) == true ||
                    thingDef.recipeMaker?.researchPrerequisites?.Any(projectDef => !projectDef.IsFinished) == true ||
                    thingDef.recipeMaker?.researchPrerequisite?.IsFinished == false)
                {
                    continue;
                }

                var textToAdd = thingDef.LabelCap;
                list.Add(new FloatMenuOption(textToAdd, delegate
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "ReGe.change.confirm".Translate(), delegate { setProduct(thingDef, true); }));
                    }, getIcon(thingDef),
                    getColor(thingDef), MenuOptionPriority.Default, null, null, 29f));
            }
            catch
            {
                // ignored
            }
        }

        if (list.Count == 0)
        {
            list.Add(new FloatMenuOption("ReGe.norecipes".Translate(), null, MenuOptionPriority.Default, null, null,
                29f));
        }

        var sortedList = list.OrderBy(option => option.Label).ToList();
        Find.WindowStack.Add(new FloatMenu(sortedList));
    }

    private static int amountToSpawn(ThingDef thingDef)
    {
        var itemWorh = thingDef.GetStatValueAbstract(StatDefOf.MarketValue);
        return (int)Math.Round(ResourceGeneratorMod.instance.Settings.GenerationValue / itemWorh);
    }

    private static Texture2D getIcon(ThingDef thingDef)
    {
        if (thingDef.uiIcon == null || thingDef.uiIcon == BaseContent.BadTex)
        {
            return BaseContent.BadTex;
        }

        return thingDef.IsStuff
            ? Widgets.GetIconFor(thingDef, thingDef)
            : Widgets.GetIconFor(thingDef);
    }

    private static Color getColor(ThingDef thingDef)
    {
        if (thingDef.uiIcon == null || thingDef.uiIcon == BaseContent.BadTex)
        {
            return Color.white;
        }

        return thingDef.uiIconColor;
    }
}