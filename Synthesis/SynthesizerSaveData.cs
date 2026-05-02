using System.Collections.Generic;
using Nautilus.Handlers;
using Nautilus.Json;

namespace Synthesis;

public class SynthesizerInstanceData
{
    public float TimeBegin { get; set; } = -1f;
    public bool IsSynthesizing { get; set; }
    public Dictionary<string, string> SerializedEquipment { get; set; }
}

public class SynthesizerSaveData : SaveDataCache
{
    public static SynthesizerSaveData Instance { get; private set; }
    
    public Dictionary<string, SynthesizerInstanceData> SynthesizerInstances { get; set; } = new Dictionary<string, SynthesizerInstanceData>();

    public static void Register()
    {
        Instance = SaveDataHandler.RegisterSaveDataCache<SynthesizerSaveData>();
    }
    
    public SynthesizerInstanceData GetOrAddNew(string id)
    {
        if (!SynthesizerInstances.ContainsKey(id))
        {
            SynthesizerInstances.Add(id, new SynthesizerInstanceData());
        }
        return SynthesizerInstances[id];
    }
    
    public void DeleteInstance(string id)
    {
        if (!SynthesizerInstances.Remove(id))
        {
            Plugin.Logger.LogWarning($"Tried to delete SynthesizerInstanceData \"{id}\" that doesn't exist");
        }
    }
}