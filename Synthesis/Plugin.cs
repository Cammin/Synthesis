

using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace Synthesis;

[Menu("Synthesis")]
public class ModOptions : ConfigFile
{    

}

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.cammin.synthesis";
    public const string PluginName = "Synthesis";
    public const string PluginVersion = "0.1.0";
    
    public new static ManualLogSource Logger { get; private set; }
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static ModOptions ModConfig { get; } = OptionsPanelHandler.RegisterModOptions<ModOptions>();
    public static string ModPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    private void Awake()
    {
        Logger = base.Logger;
        
        Harmony.CreateAndPatchAll(Assembly, $"{PluginGuid}");
        Logger.LogInfo($"Plugin {PluginGuid} is loaded!");

        //FixSpawnables();
    }

    
    
    public static void FixSpawnables()
    {
        
        
        
        //also need a way to spawn the drillable iron crystal too.
        
        var drillables = new TechType[]
        {
            TechType.DrillableAluminiumOxide,
            TechType.DrillableCopper,
            TechType.DrillableDiamond,
            TechType.DrillableGold,
            TechType.DrillableKyanite, // n/a
            TechType.DrillableLead,
            TechType.DrillableLithium,
            TechType.DrillableMagnetite,
            TechType.DrillableMercury,
            TechType.DrillableNickel,
            TechType.PrecursorIonCrystal,
            TechType.DrillableQuartz,
            TechType.DrillableSalt,
            TechType.DrillableSilver,
            TechType.DrillableSulphur, // n/a 697beac5-e39a-4809-854d-9163da9f997e7WorldEntities/Natural/drillable/DrillableSulphur.prefab
            TechType.DrillableTitanium,
            TechType.DrillableUranium,
        };
    }


    public static AssetBundle LoadBundle(string bundleName)
    {
        return AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly, bundleName);
    }
}