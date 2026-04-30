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
    
    public static readonly EquipmentType SynthesizerEquipmentType = EnumHandler.AddEntry<EquipmentType>("SynthesizerModule");
    
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
        
        Synthesizer synthesizer = prefab.AddComponent<Synthesizer>();
        synthesizer.DrillableHandle = prefab.AddComponent<SynthesizerDrillableHandler>();
        synthesizer.EquipmentHandle = prefab.AddComponent<SynthesizerEquipment>();
        synthesizer.Sfx = prefab.AddComponent<SynthesizerAudio>();
        synthesizer.DrillableHandle.Render = prefab.AddComponent<SynthesizerRendering>();
        
        synthesizer.EquipmentHandle.StorageRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
        synthesizer.EquipmentHandle.StorageRoot.ClassId = "SynthesizerStorage";
        
        //Add the sound assets!
        IPrefabRequest anteChamberHandle = PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Doodads/Precursor/Precursor_Prison_Interior_Antechamber.prefab");
        yield return anteChamberHandle;
        if (!anteChamberHandle.TryGetPrefab(out var anteChamberObj))
        {
            Plugin.Logger.LogError($"SynthesizerAuthoring: Failed loading the anteChamber");
            yield break;
        }

        AnteChamber anteChamber = anteChamberObj.GetComponent<AnteChamber>();

        synthesizer.DrillableHandle.Render._EmissiveTex = anteChamber._EmissiveTex;
        
        GameObject loopCopy = Object.Instantiate(anteChamberObj.transform.Find("scannerTr").gameObject, prefab.transform);
        synthesizer.Sfx.sfxLocation = loopCopy.transform;
        synthesizer.Sfx.sfxLoop = loopCopy.GetComponent<FMOD_CustomLoopingEmitter>();
        synthesizer.Sfx.sfxStart = anteChamber.scanSequenceBeginSound;
        synthesizer.Sfx.sfxEnd = anteChamber.scanSequenceEndSound;

        //the synthesizer stores one of each drillable inside of it.
        //chose to do it this way because interacting with the save system in the way where 
        synthesizer.DrillableHandle.Drillables = new List<Drillable>(MatrixAuthoring.Authors.Count);
        foreach (MatrixAuthor matrix in MatrixAuthoring.Authors)
        {
            IPrefabRequest drillableHandle = PrefabDatabase.GetPrefabAsync(matrix.Drillable.ToString());
            yield return drillableHandle;
            if (!anteChamberHandle.TryGetPrefab(out var drillableObj))
            {
                Plugin.Logger.LogError($"SynthesizerAuthoring: Failed loading the drillable {matrix.Drillable}");
                yield break;
            }

            //make new instance copy to keep
            drillableObj = Object.Instantiate(drillableObj, prefab.transform);
            drillableObj.SetActive(false);

            Drillable drillable = drillableObj.GetComponent<Drillable>();
            drillable.deleteWhenDrilled = false;

            Object.Destroy(drillableObj.GetComponent<PrefabIdentifier>());
            Object.Destroy(drillableObj.GetComponent<LargeWorldEntity>());
            Object.Destroy(drillableObj.GetComponent<ResourceTracker>());
            Object.Destroy(drillableObj.GetComponent<EntityTag>());

            synthesizer.DrillableHandle.Drillables.Add(drillable);
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