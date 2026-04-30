using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis;

/// <summary>
/// Responsible only for "spawning" a drillable, and notifying when it's been completely mined.
/// </summary>
public class SynthesizerDrillableHandler : MonoBehaviour
{
    public SynthesizerRendering Render;
    public List<Drillable> drillables;
    
    private Dictionary<TechType, Drillable> drillablesDict;
    private Drillable drillable;

    private void Awake()
    {
        drillablesDict = new Dictionary<TechType, Drillable>(drillables.Count);
        foreach (Drillable element in drillables)
        {
            drillablesDict.Add(element.GetDominantResourceType(), element);
        }
    }

    /// <summary>
    /// Activate the desired drillable, sub it. unsub the old one.
    /// </summary>
    public void SetDrillable(TechType type, Drillable.OnDrilled sub)
    {
        SetActiveDrillable(type);
        SetNewDrillable(drillablesDict[type], sub);
    }

    public void SetActiveDrillable(TechType type)
    {
        foreach (KeyValuePair<TechType, Drillable> pair in drillablesDict)
        {
            drillablesDict[pair.Key].gameObject.SetActive(pair.Key == type);
        }
    }
	
    private void SetNewDrillable(Drillable newDrillable, Drillable.OnDrilled sub)
    {
        if (drillable == newDrillable)
        {
            Plugin.Logger.LogError("Tried SetNewDrillable but its the same one!");
            return;
        }
		
        if (drillable)
        {
            ClearDrillable(sub);
        }
		
        if (!newDrillable) return;

        drillable = newDrillable;
        drillable.onDrilled += sub;
        
        Render.CacheNew(drillable);
    }

    public void ClearDrillable(Drillable.OnDrilled unsubThis)
    {
        if (!drillable)
        {
            Plugin.Logger.LogError("Tried clearing drillable when it is already cleared");
            return;
        }
        drillable.onDrilled -= unsubThis;
        drillable.gameObject.SetActive(false);
        drillable = null;
    }

    public void RestoreDrillable()
    {
        if (!drillable)
        {
            Plugin.Logger.LogError("Tried restoring drillable when we have none");
            return;
        }
        drillable.Restore();
    }
    
    public void SetMinable(bool enable)
    {
        if (drillable == null)
        {
            Plugin.Logger.LogError("Tried SetMineable but drillable is currently null!");
            return;
        }
        //We'd turn off colliders so it may fully show itself, but is not interactable yet
        Collider[] colliders = drillable.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = enable;
        }
    }
    
    public void UpdateDrillableVisuals(float progress)
    {
        Render.UpdateDrillableVisuals(drillable.transform.position, progress);
    }
}