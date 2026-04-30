using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using UnityEngine;
using UnityEngine.U2D;
using UWE;

namespace Synthesis;

public static class MatrixAuthoring
{
    public static AssetBundle ModelBundle; 
    public static AssetBundle UiBundle;

    public static List<MatrixAuthor> Authors;

    public static void Register()
    {
        //ModelBundle = Plugin.LoadBundle("drillarmanimations");
        UiBundle = Plugin.LoadBundle("synthesis_ui");
        
        Authors = new List<MatrixAuthor>()
        {
            new MatrixAuthorAluminumOxide(),
            new MatrixAuthorCopper(),
            new MatrixAuthorDiamond(),
            new MatrixAuthorGold(),
            new MatrixAuthorKyanite(),
            new MatrixAuthorLead(),
            new MatrixAuthorLithium(),
            new MatrixAuthorMagnetite(),
            new MatrixAuthorNickel(),
            new MatrixAuthorQuartz(),
            new MatrixAuthorSalt(),
            new MatrixAuthorSilver(),
            new MatrixAuthorSulphur(),
            new MatrixAuthorTitanium(),
            new MatrixAuthorUraninite(),
        };

        SpriteAtlas atlas = UiBundle.LoadAsset<SpriteAtlas>("ui");
        foreach (MatrixAuthor author in Authors)
        {
            author.Register(atlas);
        }
        
        ConsoleCommandsHandler.RegisterConsoleCommand(nameof(MatrixLoot), typeof(MatrixAuthoring), nameof(MatrixLoot), null);
    }

    public static void MatrixLoot()
    {
        foreach (MatrixAuthor author in Authors)
        {
            CraftData.AddToInventory(author.Info.TechType);
        }
    }
}

public abstract class MatrixAuthor
{
    public PrefabInfo Info { get; private set; }
    public abstract string ClassId { get; }
    public abstract TechType Resource { get; }
    public abstract TechType Drillable { get; }
    public abstract int CraftAmount { get; }
    
    public void Register(SpriteAtlas atlas)
    {
        Sprite iconSprite = atlas.GetSprite($"Icon_Matrix_{Resource}");
        if (iconSprite == null)
        {
            Plugin.Logger.LogError($"Failed loading the iconSprite for {Resource}");
        }

        //Register PrefabInfo here so it executes based on the current language
        Info = PrefabInfo
            .WithTechType(ClassId)
            .WithIcon(iconSprite);
        
        CustomPrefab prefab = new(Info);

        prefab.SetUnlock(Resource);
        prefab.SetEquipment(SynthesizerAuthoring.SynthesizerEquipmentType);
        
        prefab.SetRecipe(new RecipeData(new Ingredient(TechType.PrecursorIonCrystal, 1), new Ingredient(Resource, CraftAmount)))
            .WithFabricatorType(CompressorAuthoring.CompressorCraftType)
            .WithStepsToFabricatorTab()
            .WithCraftingTime(5);
        
        prefab.SetGameObject(SetupObj());
        prefab.Register();
    }

    private PrefabTemplate SetupObj()
    {
        CloneTemplate cloneTemplate = new(Info, TechType.VehicleHullModule1)
        {
            ModifyPrefabAsync = DoModifyPrefabAsync
        };
        return cloneTemplate;

        IEnumerator DoModifyPrefabAsync(GameObject obj)
        {
            obj.GetComponent<TechTag>().type = Info.TechType;
            
            Matrix matrix = obj.AddComponent<Matrix>();
            matrix.Resource = Resource;
            matrix.Drillable = Drillable;
            
            yield break;
        }
    }
}

public class MatrixAuthorAluminumOxide : MatrixAuthor
{
    public override string ClassId => "AluminumOxideMatrix";
    public override TechType Resource => TechType.AluminumOxide;
    public override TechType Drillable => TechType.DrillableAluminiumOxide;
    public override int CraftAmount => 100;
}
public class MatrixAuthorCopper : MatrixAuthor
{
    public override string ClassId => "CopperMatrix";
    public override TechType Resource => TechType.Copper;
    public override TechType Drillable => TechType.DrillableCopper;
    public override int CraftAmount => 100;
}
public class MatrixAuthorDiamond : MatrixAuthor
{
    public override string ClassId => "DiamondMatrix";
    public override TechType Resource => TechType.Diamond;
    public override TechType Drillable => TechType.DrillableDiamond;
    public override int CraftAmount => 100;
}
public class MatrixAuthorGold : MatrixAuthor
{
    public override string ClassId => "GoldMatrix";
    public override TechType Resource => TechType.Gold;
    public override TechType Drillable => TechType.DrillableGold;
    public override int CraftAmount => 100;
}
public class MatrixAuthorKyanite : MatrixAuthor
{
    public override string ClassId => "KyaniteMatrix";
    public override TechType Resource => TechType.Kyanite;
    public override TechType Drillable => TechType.DrillableKyanite;
    public override int CraftAmount => 100;
}
public class MatrixAuthorLead : MatrixAuthor
{
    public override string ClassId => "LeadMatrix";
    public override TechType Resource => TechType.Lead;
    public override TechType Drillable => TechType.DrillableLead;
    public override int CraftAmount => 100;
}
public class MatrixAuthorLithium : MatrixAuthor
{
    public override string ClassId => "LithiumMatrix";
    public override TechType Resource => TechType.Lithium;
    public override TechType Drillable => TechType.DrillableLithium;
    public override int CraftAmount => 100;
}
public class MatrixAuthorMagnetite : MatrixAuthor
{
    public override string ClassId => "MagnetiteMatrix";
    public override TechType Resource => TechType.Magnetite;
    public override TechType Drillable => TechType.DrillableMagnetite;
    public override int CraftAmount => 100;
}
public class MatrixAuthorNickel : MatrixAuthor
{
    public override string ClassId => "NickelMatrix";
    public override TechType Resource => TechType.Nickel;
    public override TechType Drillable => TechType.DrillableNickel;
    public override int CraftAmount => 100;
}
public class MatrixAuthorQuartz : MatrixAuthor
{
    public override string ClassId => "QuartzMatrix";
    public override TechType Resource => TechType.Quartz;
    public override TechType Drillable => TechType.DrillableQuartz;
    public override int CraftAmount => 100;
}
public class MatrixAuthorSalt : MatrixAuthor
{
    public override string ClassId => "SaltMatrix";
    public override TechType Resource => TechType.Salt;
    public override TechType Drillable => TechType.DrillableSalt;
    public override int CraftAmount => 100;
}
public class MatrixAuthorSilver : MatrixAuthor
{
    public override string ClassId => "SilverMatrix";
    public override TechType Resource => TechType.Silver;
    public override TechType Drillable => TechType.DrillableSilver;
    public override int CraftAmount => 100;
}
public class MatrixAuthorSulphur : MatrixAuthor
{
    public override string ClassId => "SulphurMatrix";
    public override TechType Resource => TechType.Sulphur;
    public override TechType Drillable => TechType.DrillableSulphur;
    public override int CraftAmount => 100;
}
public class MatrixAuthorTitanium : MatrixAuthor
{
    public override string ClassId => "TitaniumMatrix";
    public override TechType Resource => TechType.Titanium;
    public override TechType Drillable => TechType.DrillableTitanium;
    public override int CraftAmount => 100;
}
public class MatrixAuthorUraninite : MatrixAuthor
{
    public override string ClassId => "UraniumMatrix";
    public override TechType Resource => TechType.UraniniteCrystal;
    public override TechType Drillable => TechType.DrillableUranium;
    public override int CraftAmount => 100;
}
