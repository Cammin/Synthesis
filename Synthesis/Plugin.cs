

using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;

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
    }
}