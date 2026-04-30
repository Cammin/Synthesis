using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis;

/// <summary>
/// Responsible only for "spawning" a drillable, and notifying when it's been completely mined.
/// </summary>
public class SynthesizerDrillableHandler : MonoBehaviour
{
    private SynthesizerRendering _render;
    private Dictionary<TechType, Drillable> _drillablesDict;
    private Drillable _drillable;

    private void Awake()
    {
        _render = GetComponent<SynthesizerRendering>();
        
        var drillables = GetComponentsInChildren<Drillable>(true);
        _drillablesDict = new Dictionary<TechType, Drillable>(drillables.Length);
        foreach (Drillable element in drillables)
        {
            _drillablesDict.Add(element.GetDominantResourceType(), element);
        }
    }

    /// <summary>
    /// Initially, all drillables are inactive.
    /// Setting one will enable it.
    /// Setting a different one will disable the old one.
    /// </summary>
    public void SetDrillable(TechType type, Drillable.OnDrilled sub)
    {
        if (!_drillablesDict.TryGetValue(type, out Drillable newDrillable))
        {
            Plugin.Logger.LogError($"Tried SetDrillable {type} but it's not a drillable");
            return;
        }

        if (!newDrillable)
        {
            Plugin.Logger.LogError($"Tried SetNewDrillable {type} but the drillable was null!");
            return;
        }
        
        if (_drillable)
        {
            if (_drillable == newDrillable)
            {
                Plugin.Logger.LogError($"Tried SetNewDrillable {type} but it's the same one as before!");
                return;
            }
            
            ClearDrillable(sub);
        }
        
        _drillable.gameObject.SetActive(true);
        _drillable = newDrillable;
        _drillable.onDrilled += sub;
        
        _render.CacheNew(_drillable);
        UpdateDrillableVisuals(0);
    }

    public void ClearDrillable(Drillable.OnDrilled unsubThis)
    {
        if (!_drillable)
        {
            Plugin.Logger.LogError("Tried clearing drillable when it is already cleared");
            return;
        }
        RestoreDrillable();
        _drillable.onDrilled -= unsubThis;
        _drillable.gameObject.SetActive(false);
        _drillable = null;
    }

    public void RestoreDrillable()
    {
        if (!_drillable)
        {
            Plugin.Logger.LogError("Tried RestoreDrillable when we have none");
            return;
        }
        _drillable.Restore();
    }
    
    public void SetDrillableMinable(bool enable)
    {
        if (_drillable == null)
        {
            Plugin.Logger.LogError("Tried SetMineable but drillable is currently null!");
            return;
        }
        //We'd turn off colliders so it may fully show itself, but is not interactable yet
        Collider[] colliders = _drillable.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = enable;
        }
    }
    
    public void UpdateDrillableVisuals(float progress)
    {
        _render.UpdateDrillableVisuals(_drillable.transform.position, progress);
    }
}