using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using Nautilus.Extensions;

namespace Synthesis;

public static class CompressorAuthoring
{
    public static PrefabInfo Info { get; private set; }
    
    public static void Register()
    {
        Info = PrefabInfo.WithTechType("Compressor");
        
        CustomPrefab prefab = new(Info);

        prefab
            .SetUnlock(TechType.PlasteelIngot)
            .WithCompoundTechsForUnlock(new List<TechType> { TechType.AdvancedWiringKit, TechType.Magnetite })
            .WithPdaGroupCategoryAfter(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.Workbench);
        
        prefab.SetRecipe(Recipe());
        prefab.SetGameObject(SetupObj());
        prefab.Register();
    }

    private static PrefabTemplate SetupObj()
    {
        FabricatorTemplate template = new FabricatorTemplate(Info, Plugin.CompressorCraftType);
        template.ConstructableFlags = ConstructableFlags.Inside | ConstructableFlags.Wall;
        template.FabricatorModel = FabricatorTemplate.Model.Fabricator;
        template.ColorTint = new Color(1f, 0.5f, 0.5f);
        template.ModifyPrefabAsync = DoModifyPrefabAsync;
        return template;

        IEnumerator DoModifyPrefabAsync(GameObject obj)
        {
            /*GhostCrafter fabricator = obj.GetComponent<GhostCrafter>();
            Compressor compressor = obj.AddComponent<Compressor>();
            compressor.CopyComponent(fabricator);
            Object.DestroyImmediate(fabricator);*/
            
            //var constructable = PrefabUtils.AddConstructable(obj, Info.TechType, ConstructableFlags.Base | ConstructableFlags.Inside | ConstructableFlags.Ground);
            yield break;
        }
    }
    
    private static RecipeData Recipe()
    {
        return new RecipeData()
        {
            craftAmount = 1,
            Ingredients = new List<Ingredient>()
            {
                new Ingredient(TechType.PlasteelIngot, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Magnetite, 2)
            }
        };
    }
}