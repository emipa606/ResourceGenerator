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
    private CompFlickable flickableComp;
    private int lastCount;
    private int limit;
    private CompPowerTrader powerTraderComp;
    private CompResourceSpawner spawner;

    public CompFlickable FlickableComp
    {
        get
        {
            if (flickableComp == null)
            {
                flickableComp = GetComp<CompFlickable>();
            }

            return flickableComp;
        }
        set => flickableComp = value;
    }

    public CompPowerTrader PowerTraderComp
    {
        get
        {
            if (powerTraderComp == null)
            {
                powerTraderComp = GetComp<CompPowerTrader>();
            }

            return powerTraderComp;
        }
        set => powerTraderComp = value;
    }

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
            currentAmount = (int)Math.Round(currentAmount * Spawner.PropsSpawner.generationFactor);
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


    private bool ControlIsHeld => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

    private bool ShiftIsHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref currentProduct, "currentProduct");
        Scribe_Values.Look(ref currentAmount, "currentAmount");
        Scribe_Values.Look(ref limit, "limit");
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

    public override string GetInspectString()
    {
        var baseInspectString = base.GetInspectString();

        if (limit == 0)
        {
            return $"{baseInspectString}\n" + "ReGe.unlimited".Translate();
        }

        var limitString = "ReGe.limitedto".Translate(limit);
        limitString += verifyLimit()
            ? "ReGe.limitednotreached".Translate(lastCount)
            : "ReGe.limitedreached".Translate(lastCount);

        return $"{baseInspectString}\n" + limitString;
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
        if (limit > 0)
        {
            yield return new Command_Action
            {
                action = delegate
                {
                    foreach (var selectedObject in Find.Selector.SelectedObjects)
                    {
                        if (selectedObject is not ResourceGenerator resourceGenerator)
                        {
                            continue;
                        }

                        if (ControlIsHeld)
                        {
                            resourceGenerator.DecreaseBy(1);
                            continue;
                        }

                        if (ShiftIsHeld)
                        {
                            resourceGenerator.DecreaseBy(100);
                            continue;
                        }

                        resourceGenerator.DecreaseBy(10);
                    }
                },
                defaultDesc = "ReGe.decreaseLimittt".Translate(),
                icon = TexButton.ReorderDown,
                defaultLabel = "ReGe.decreaseLimit".Translate()
            };
        }

        yield return new Command_Action
        {
            action = delegate
            {
                foreach (var selectedObject in Find.Selector.SelectedObjects)
                {
                    if (selectedObject is not ResourceGenerator resourceGenerator)
                    {
                        continue;
                    }

                    if (ControlIsHeld)
                    {
                        resourceGenerator.IncreaseBy(1);
                        continue;
                    }

                    if (ShiftIsHeld)
                    {
                        resourceGenerator.IncreaseBy(100);
                        continue;
                    }

                    resourceGenerator.IncreaseBy(10);
                }
            },
            defaultDesc = "ReGe.increaseLimittt".Translate(),
            icon = TexButton.ReorderUp,
            defaultLabel = "ReGe.increaseLimit".Translate()
        };
    }

    public void IncreaseBy(int amount)
    {
        limit += amount;
        verifyLimit();
    }

    public void DecreaseBy(int amount)
    {
        limit = Math.Max(0, limit - amount);
        verifyLimit(limit == 0);
    }

    public override void Tick()
    {
        base.Tick();
        if (GenTicks.TicksGame % GenTicks.TickRareInterval != 0)
        {
            return;
        }

        if (PowerTraderComp.PowerOn)
        {
            FleckMaker.ThrowSmoke(DrawPos, Map, 1f);
        }

        verifyLimit();
    }

    private bool verifyLimit(bool reset = false)
    {
        if (CurrentProduct == null)
        {
            return false;
        }

        if (limit == 0)
        {
            if (reset && !FlickableComp.SwitchIsOn)
            {
                FlickableComp.DoFlick();
            }

            return true;
        }

        lastCount = Map.resourceCounter.GetCount(CurrentProduct);
        if (lastCount < limit)
        {
            if (!FlickableComp.SwitchIsOn)
            {
                FlickableComp.DoFlick();
            }

            return true;
        }

        if (FlickableComp.SwitchIsOn)
        {
            FlickableComp.DoFlick();
        }

        return false;
    }

    private void setProduct(ThingDef thingToSet, bool reset)
    {
        var originalProduct = CurrentProduct;
        foreach (var selectedObject in Find.Selector.SelectedObjects)
        {
            if (selectedObject is not ResourceGenerator resourceGenerator)
            {
                continue;
            }

            if (originalProduct != resourceGenerator.CurrentProduct)
            {
                continue;
            }

            if (!reset && resourceGenerator != this)
            {
                continue;
            }

            resourceGenerator.CurrentProduct = thingToSet;
            resourceGenerator.CurrentAmount = amountToSpawn(CurrentProduct);
            if (reset)
            {
                resourceGenerator.Spawner.PostSpawnSetup(false);
            }
        }

        verifyLimit();
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
                if (ResourceGeneratorMod.instance.Settings.ShowConfirmation)
                {
                    list.Add(new FloatMenuOption(textToAdd, delegate
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                                "ReGe.change.confirm".Translate(), delegate { setProduct(thingDef, true); }));
                        }, getIcon(thingDef),
                        getColor(thingDef), MenuOptionPriority.Default, null, null, 29f));
                }
                else
                {
                    list.Add(new FloatMenuOption(textToAdd, delegate { setProduct(thingDef, true); }, getIcon(thingDef),
                        getColor(thingDef), MenuOptionPriority.Default, null, null, 29f));
                }
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