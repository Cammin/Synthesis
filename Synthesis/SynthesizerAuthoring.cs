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
    
    public const string StorageRootName = "SynthesizerStorageRoot";
    
    public static void Register()
    {
        Info = PrefabInfo.WithTechType("Synthesizer");
        
        CustomPrefab prefab = new(Info);

        prefab
            .SetUnlock(TechType.TitaniumIngot)
            .WithCompoundTechsForUnlock(new List<TechType> { TechType.AdvancedWiringKit, TechType.Magnetite })
            .WithPdaGroupCategoryAfter(TechGroup.ExteriorModules, TechCategory.ExteriorModule, TechType.PowerTransmitter);
        
        prefab.SetRecipe(new RecipeData()
        {
            craftAmount = 1,
            Ingredients = new List<Ingredient>()
            {
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Magnetite, 2)
            }
        });
        
        prefab.SetGameObject(PrefabAsync);
        prefab.Register();
    }

    //this runs when the prefab is first needed. 
    private static IEnumerator PrefabAsync(IOut<GameObject> arg)
    {
        Plugin.Logger.LogInfo($"SynthesizerAuthoring: PrefabAsync");
        
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
        PrefabUtils.AddBasicComponents(prefab, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Far);

        Constructable constructable = PrefabUtils.AddConstructable(prefab, Info.TechType, ConstructableFlags.Outside | ConstructableFlags.Ground | ConstructableFlags.Rotatable, meshRoot);
        constructable.placeMaxDistance = 8f;
        constructable.placeMinDistance = 2f;
        constructable.placeDefaultDistance = 4f;
        
        //raise the bounds up slightly so that it doesn't overlap badly with the floor
        ConstructableBounds constructableBounds = prefab.AddComponent<ConstructableBounds>();
        constructableBounds.bounds = new OrientedBounds(meshRootCollider.bounds.center + Vector3.up * 0.05f, Quaternion.identity, meshRootCollider.bounds.size * 0.5f);
        
        IPrefabRequest anteChamberHandle = PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Doodads/Precursor/Precursor_Prison_Interior_Antechamber.prefab");
        yield return anteChamberHandle;
        if (!anteChamberHandle.TryGetPrefab(out var anteChamberObj))
        {
            Plugin.Logger.LogError($"SynthesizerAuthoring: Failed loading the anteChamber");
            yield break;
        }
        
        foreach (MatrixAuthor matrix in MatrixAuthoring.Authors)
        {
            IPrefabRequest drillableHandle = PrefabDatabase.GetPrefabAsync(matrix.Drillable.ToString());
            yield return drillableHandle;
            if (!drillableHandle.TryGetPrefab(out var drillableObj))
            {
                Plugin.Logger.LogError($"SynthesizerAuthoring: Failed loading the drillable {matrix.Drillable}");
                yield break;
            }

            //make new instance copy to keep in this prefab
            drillableObj = Object.Instantiate(drillableObj, prefab.transform);
            drillableObj.SetActive(false);

            Drillable drillable = drillableObj.GetComponent<Drillable>();
            drillable.deleteWhenDrilled = false;

            //remove a bunch of this because it's part of this prefab's discretion now
            Object.Destroy(drillableObj.GetComponent<PrefabIdentifier>());
            Object.Destroy(drillableObj.GetComponent<LargeWorldEntity>());
            Object.Destroy(drillableObj.GetComponent<ResourceTracker>());
            Object.Destroy(drillableObj.GetComponent<EntityTag>());
        }
        
        AnteChamber anteChamber = anteChamberObj.GetComponent<AnteChamber>();
        var render = prefab.AddComponent<SynthesizerRendering>();
        render._EmissiveTex = anteChamber._EmissiveTex;
        
        GameObject sfxLoopCopy = Object.Instantiate(anteChamberObj.transform.Find("scannerTr").gameObject, prefab.transform);
        var sfx = prefab.AddComponent<SynthesizerAudio>();
        sfx.sfxLocation = sfxLoopCopy.transform;
        sfx.sfxLoop = sfxLoopCopy.GetComponent<FMOD_CustomLoopingEmitter>();
        sfx.sfxStart = anteChamber.scanSequenceBeginSound;
        sfx.sfxEnd = anteChamber.scanSequenceEndSound;
        
        GameObject storageRoot = new GameObject(StorageRootName);
        storageRoot.transform.SetParent(prefab.transform, false);
        var storageId = storageRoot.AddComponent<ChildObjectIdentifier>();
        storageId.ClassId = "SynthesizerStorage";
        
        prefab.AddComponent<SynthesizerEquipment>(); //depends on StorageRoot
        prefab.AddComponent<SynthesizerDrillableHandler>(); // depends on Render, Drillables
        prefab.AddComponent<Synthesizer>(); // depends on DrillableHandle, EquipmentHandle, Sfx
        
        arg.Set(prefab);
    }
}