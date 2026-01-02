using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResourceGenerator;

public class ResourceGenerator : Building
{
    private static readonly Vector3 iconSize = new(0.5f, 1f, 0.5f);
    private static readonly Vector3 iconOffset = new(0.3f, 1.1f, 0.3f);
    private int currentAmount;
    private ThingDef currentProduct;
    private int lastCount;
    private int limit;
    private IntVec3 outputTile;
    private List<IntVec3> validCells;

    private CompFlickable FlickableComp
    {
        get
        {
            field ??= GetComp<CompFlickable>();

            return field;
        }
    }

    public List<IntVec3> ValidCells
    {
        get
        {
            validCells ??= GenAdj.CellsAdjacent8Way(this).ToList();

            return validCells;
        }
    }

    private CompPowerTrader PowerTraderComp
    {
        get
        {
            field ??= GetComp<CompPowerTrader>();

            return field;
        }
    }

    private CompResourceSpawner Spawner
    {
        get
        {
            field ??= GetComp<CompResourceSpawner>();

            return field;
        }
    }

    public ThingDef CurrentProduct
    {
        get
        {
            currentProduct ??= ThingDefOf.Steel;

            return currentProduct;
        }
        private set
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

            currentAmount = amountToSpawn(CurrentProduct, Spawner.PropsSpawner.generationFactor);
            return currentAmount;
        }
        private set => currentAmount = value;
    }

    public IntVec3 OutputTile
    {
        get
        {
            if (outputTile == IntVec3.Invalid)
            {
                outputTile = ValidCells.RandomElement() - Position;
            }

            if (ValidCells.Contains(outputTile))
            {
                outputTile -= Position;
            }

            return outputTile + Position;
        }
        private set => outputTile = value - Position;
    }

    private Texture2D CurrentIcon
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            field = getIcon(CurrentProduct);
            return field;
        }
        set;
    }

    private Color CurrentColor
    {
        get
        {
            if (field != default)
            {
                return field;
            }

            field = getColor(CurrentProduct);
            return field;
        }
        set;
    }

    private static bool ControlIsHeld => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

    private static bool ShiftIsHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    public override void PreSwapMap()
    {
        base.PreSwapMap();
        validCells = null;
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
        validCells = null;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref currentProduct, "currentProduct");
        Scribe_Values.Look(ref currentAmount, "currentAmount");
        Scribe_Values.Look(ref outputTile, "outputTile");
        Scribe_Values.Look(ref limit, "limit", ResourceGeneratorMod.Instance.Settings.DefaultLimit);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        setProduct(CurrentProduct, false);
        if (respawningAfterLoad || Spawner.Pause)
        {
            return;
        }

        NextOutputTile();
        limit = ResourceGeneratorMod.Instance.Settings.DefaultLimit;
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        var iconLocation = drawLoc + iconOffset;
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

    public override void DrawExtraSelectionOverlays()
    {
        base.DrawExtraSelectionOverlays();
        GenDraw.DrawFieldEdges([OutputTile], Color.green);
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
                            resourceGenerator.decreaseBy(1);
                            continue;
                        }

                        if (ShiftIsHeld)
                        {
                            resourceGenerator.decreaseBy(100);
                            continue;
                        }

                        resourceGenerator.decreaseBy(10);
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
                        resourceGenerator.increaseBy(1);
                        continue;
                    }

                    if (ShiftIsHeld)
                    {
                        resourceGenerator.increaseBy(100);
                        continue;
                    }

                    resourceGenerator.increaseBy(10);
                }
            },
            defaultDesc = "ReGe.increaseLimittt".Translate(),
            icon = TexButton.ReorderUp,
            defaultLabel = "ReGe.increaseLimit".Translate()
        };
    }

    private void increaseBy(int amount)
    {
        limit += amount;
        verifyLimit();
    }

    private void decreaseBy(int amount)
    {
        limit = Math.Max(0, limit - amount);
        verifyLimit(limit == 0);
    }

    protected override void Tick()
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

    public void NextOutputTile()
    {
        var currentIndex = ValidCells.IndexOf(OutputTile);

        foreach (var unused in ValidCells)
        {
            if (currentIndex + 1 == ValidCells.Count)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
            }

            if (!IsValidSpawnCell(ValidCells[currentIndex], CurrentProduct, CurrentAmount))
            {
                continue;
            }

            OutputTile = ValidCells[currentIndex];
            return;
        }
    }

    public bool IsValidSpawnCell(IntVec3 intVec, ThingDef thingToSpawn, int spawnCount)
    {
        if (!intVec.Walkable(Map))
        {
            return false;
        }

        var edifice = intVec.GetEdifice(Map);
        if (edifice != null && thingToSpawn.IsEdifice())
        {
            return false;
        }

        if (edifice is Building_Door { FreePassage: false } ||
            def.passability != Traversability.Impassable &&
            !GenSight.LineOfSight(Position, intVec, Map))
        {
            return false;
        }

        var thingList = intVec.GetThingList(Map);
        foreach (var thing in thingList)
        {
            if (thing.def.category != ThingCategory.Item || thing.def == thingToSpawn &&
                thing.stackCount <= thingToSpawn.stackLimit - spawnCount)
            {
                continue;
            }

            return false;
        }

        return true;
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
            resourceGenerator.CurrentAmount = amountToSpawn(CurrentProduct, Spawner.PropsSpawner.generationFactor);
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

                var amount = amountToSpawn(thingDef, Spawner.PropsSpawner.generationFactor);
                if (amount == 0)
                {
                    continue;
                }

                var textToAdd = $"{thingDef.LabelCap} x{amount}";
                if (ResourceGeneratorMod.Instance.Settings.ShowConfirmation)
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

    private static int amountToSpawn(ThingDef thingDef, float factor)
    {
        var itemWorth = thingDef.GetStatValueAbstract(StatDefOf.MarketValue);
        var valueToGenerate = ResourceGeneratorMod.Instance.Settings.GenerationValue * factor;
        return (int)Math.Floor(valueToGenerate / itemWorth);
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