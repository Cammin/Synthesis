using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public static CraftTree.Type CompressorCraftType;
    
    public static void Register()
    {
        Info = PrefabInfo.WithTechType("Compressor");
        
        CustomPrefab prefab = new(Info);

        RecipeData recipe = Recipe();

        prefab
            .SetUnlock(TechType.PrecursorIonCrystal)
            .WithCompoundTechsForUnlock(recipe.Ingredients.Select(p => p._techType).ToList())
            .WithPdaGroupCategoryAfter(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.Workbench);

        prefab.CreateFabricator(out CompressorCraftType);
        prefab.SetRecipe(recipe);
        prefab.SetGameObject(SetupObj());
        prefab.Register();
    }

    private static PrefabTemplate SetupObj()
    {
        FabricatorTemplate template = new FabricatorTemplate(Info, CompressorCraftType);
        template.ConstructableFlags = ConstructableFlags.Inside | ConstructableFlags.Wall | ConstructableFlags.Submarine;
        template.FabricatorModel = FabricatorTemplate.Model.Fabricator;
        template.ColorTint = new Color(1f, 0.5f, 0.5f);
        template.ModifyPrefabAsync = DoModifyPrefabAsync;
        return template;

        IEnumerator DoModifyPrefabAsync(GameObject obj)
        {
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
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.CopperWire, 1),
                new Ingredient(TechType.Lead, 5)
            }
        };
    }
}