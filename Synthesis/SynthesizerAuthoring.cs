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
using UWE;

namespace Synthesis;

public static class SynthesizerAuthoring
{
    public static PrefabInfo Info { get; private set; }
    
    public static readonly EquipmentType SynthesizerEquipment = EnumHandler.AddEntry<EquipmentType>("SynthesizerModule");
    
    public static void Register()
    {
        Info = PrefabInfo.WithTechType("Synthesizer");

        
        
        
        CustomPrefab prefab = new(Info);

        prefab
            .SetUnlock(TechType.PlasteelIngot)
            .WithCompoundTechsForUnlock(new List<TechType> { TechType.AdvancedWiringKit, TechType.Magnetite })
            .WithPdaGroupCategoryAfter(TechGroup.ExteriorModules, TechCategory.ExteriorModule, TechType.PowerTransmitter);

        //prefab.CreateFabricator(out CompressorCraftType);
        prefab.SetRecipe(Recipe());
        prefab.SetGameObject(PrefabAsync);
        prefab.Register();
    }

    private static IEnumerator PrefabAsync(IOut<GameObject> arg)
    {
        GameObject prefab = new GameObject("Synthesizer");
        
        //make mesh
        GameObject meshRoot = new GameObject("mesh");
        meshRoot.transform.SetParent(prefab.transform);
        
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pedestal.transform.SetParent(meshRoot.transform);
        pedestal.transform.localPosition = new Vector3(0, 0.15f, 0);
        pedestal.transform.localScale = new Vector3(3, 0.3f, 3);
        
        BoxCollider meshRootCollider = meshRoot.AddComponent<BoxCollider>();
        meshRootCollider.center = pedestal.transform.localPosition;
        meshRootCollider.size = pedestal.transform.localScale;
        Object.Destroy(pedestal.GetComponent<Collider>());

        MaterialUtils.ApplySNShaders(meshRoot, 6);
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Medium);

        Constructable constructable = PrefabUtils.AddConstructable(prefab, Info.TechType, ConstructableFlags.Outside | ConstructableFlags.Ground | ConstructableFlags.Rotatable, meshRoot);
        constructable.placeMaxDistance = 8f;
        constructable.placeMinDistance = 2f;
        constructable.placeDefaultDistance = 4f;
        
        //raise the bounds up slightly so that it doesn't overlap badly with the floor
        ConstructableBounds constructableBounds = prefab.AddComponent<ConstructableBounds>();
        constructableBounds.bounds = new OrientedBounds(meshRootCollider.bounds.center + Vector3.up * 0.05f, Quaternion.identity, meshRootCollider.bounds.size * 0.5f);
        
        GameObject storageRoot = new GameObject("StorageRoot");
        storageRoot.transform.SetParent(prefab.transform, false);
        
        ChildObjectIdentifier storageId = storageRoot.AddComponent<ChildObjectIdentifier>();
        storageId.ClassId = "SynthesizerStorage";
        
        //StorageContainer storage = PrefabUtils.AddStorageContainer(prefab, "StorageRoot", "SynthesizerMatrixStorage", 1, 1, true);
        //todo: add localization strings here
        //storage.storageLabel = "SynthesizerStorage";
        //storage.hoverText = "SynthesizerHover";
        
        Synthesizer synthesizer = prefab.AddComponent<Synthesizer>();
        synthesizer.storageRoot = storageId;
        synthesizer.Render = prefab.AddComponent<SynthesizerRendering>();
        synthesizer.sfx = prefab.AddComponent<SynthesizerAudio>();
        
        //Add the sound assets!
        IPrefabRequest anteChamberHandle = PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Doodads/Precursor/Precursor_Prison_Interior_Antechamber.prefab");
        yield return anteChamberHandle;
        if (anteChamberHandle.TryGetPrefab(out var anteChamberObj))
        {
            AnteChamber anteChamber = anteChamberObj.GetComponent<AnteChamber>();

            synthesizer.Render._EmissiveTex = anteChamber._EmissiveTex;
            
            synthesizer.sfx.sfxStart = anteChamber.scanSequenceBeginSound;
            synthesizer.sfx.sfxEnd = anteChamber.scanSequenceEndSound;

            GameObject loopCopy = Object.Instantiate(anteChamberObj.transform.Find("scannerTr").gameObject, prefab.transform);
            synthesizer.sfx.sfxLocation = loopCopy.transform;
            synthesizer.sfx.sfxLoop = loopCopy.GetComponent<FMOD_CustomLoopingEmitter>();
        }
        else
        {
            Plugin.Logger.LogError($"Failed loading the anteChamber");
        }

        //the synthesizer stores one of each drillable inside of it
        foreach (MatrixAuthor matrix in MatrixAuthoring.Authors)
        {
            IPrefabRequest drillableHandle = PrefabDatabase.GetPrefabAsync(matrix.Drillable.ToString());
            yield return drillableHandle;
            if (anteChamberHandle.TryGetPrefab(out var drillableObj))
            {
                //make new instance copy to keep
                drillableObj = Object.Instantiate(drillableObj, prefab.transform);

                Drillable drillable = drillableObj.GetComponent<Drillable>();
                drillable.deleteWhenDrilled = false;

                //ensure this is good
                Object.Destroy(drillableObj.GetComponent<PrefabIdentifier>());
                Object.Destroy(drillableObj.GetComponent<LargeWorldEntity>());
                Object.Destroy(drillableObj.GetComponent<ResourceTracker>());
                Object.Destroy(drillableObj.GetComponent<EntityTag>());
            }
            else
            {
                Plugin.Logger.LogError($"Failed loading the drillable {matrix.Drillable}");
            }
        }
        
        arg.Set(prefab);
    }
    
    private static RecipeData Recipe()
    {
        return new RecipeData()
        {
            craftAmount = 1,
            Ingredients = new List<Ingredient>()
            {
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Magnetite, 2)
            }
        };
    }

    
}