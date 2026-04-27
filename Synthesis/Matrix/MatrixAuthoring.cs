using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using UnityEngine;
using UnityEngine.U2D;
using UWE;

namespace Synthesis;

public static class MatrixAuthoring
{
    public static AssetBundle ModelBundle; 
    public static AssetBundle UiBundle;

    public static void Register()
    {
        //ModelBundle = Plugin.LoadBundle("drillarmanimations");
        UiBundle = Plugin.LoadBundle("synthesis_ui");
        
        SpriteAtlas atlas = UiBundle.LoadAsset<SpriteAtlas>("ui");
        
        new MatrixAuthorTitanium().Register(atlas);
        new MatrixAuthorCopper().Register(atlas);
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
        prefab.SetEquipment(Plugin.SynthesizerModule);
        SetupRecipe(prefab);
        SetupObj(prefab);
        prefab.Register();
    }

    private void SetupObj(CustomPrefab prefab)
    {
        CloneTemplate cloneTemplate = new(Info, TechType.VehicleHullModule1)
        {
            ModifyPrefabAsync = DoModifyPrefabAsync
        };

        prefab.SetGameObject(cloneTemplate);
        return;

        IEnumerator DoModifyPrefabAsync(GameObject obj)
        {
            obj.GetComponent<TechTag>().type = Info.TechType;
            
            Matrix matrix = obj.AddComponent<Matrix>();
            matrix.Resource = Resource;
            matrix.Drillable = Drillable;
            
            yield break;
        }
    }
    
    private void SetupRecipe(CustomPrefab prefab)
    {
        RecipeData recipe = new RecipeData
        {
            craftAmount = 1,
            Ingredients = new List<Ingredient>() { new Ingredient(Resource, CraftAmount) }
        };
        prefab.SetRecipe(recipe)
            .WithFabricatorType(Plugin.CompressorModule)
            .WithCraftingTime(5);
    }
}

public class MatrixAuthorTitanium : MatrixAuthor
{
    public override string ClassId => "TitaniumMatrix";
    public override TechType Resource => TechType.Titanium;
    public override TechType Drillable => TechType.DrillableTitanium;
    public override int CraftAmount => 100;
}
public class MatrixAuthorCopper : MatrixAuthor
{
    public override string ClassId => "CopperMatrix";
    public override TechType Resource => TechType.Copper;
    public override TechType Drillable => TechType.DrillableCopper;
    public override int CraftAmount => 100;
}