using HarmonyLib;
using UWE;

namespace Synthesis;

/// <summary>
/// The EntTechData doesn't have entries for a couple drillable prefabs, so we're bringing it back so they can be spawned from CraftData
/// </summary>
[HarmonyPatch(typeof(CraftData))]
public class FixDrillableKyaniteAndSulphur
{
    [HarmonyPatch(nameof(CraftData.PrepareEntTechCache))]
    [HarmonyPostfix]
    public static void PrepareEntTechCache()
    {
        CraftData.entTechMap["drillablekyanite"] = TechType.DrillableKyanite;
        CraftData.entTechMap["drillablesulphur"] = TechType.DrillableSulphur;
    }
}