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
        foreach (Drillable drillable in drillables)
        {
            var resource = drillable.GetDominantResourceType();
            var drillableType = MatrixAuthoring.ResourceToDrillable(resource);
            if (drillableType == TechType.None)
            {
                Plugin.Logger.LogError($"Tried registering a drillable as none?");
                continue;
            }
            
            _drillablesDict.Add(drillableType, drillable);
        }
    }

    /// <summary>
    /// Initially, all drillables are inactive.
    /// Setting one will enable it.
    /// Setting a different one will disable the old one.
    /// </summary>
    public void SetDrillable(Matrix matrix, Drillable.OnDrilled sub)
    {
        TechType drillableType = matrix.Drillable;
        if (!_drillablesDict.TryGetValue(drillableType, out Drillable newDrillable))
        {
            Plugin.Logger.LogError($"Tried SetDrillable {drillableType} but it's not a drillable type");
            return;
        }

        if (!newDrillable)
        {
            Plugin.Logger.LogError($"Tried SetDrillable {drillableType} but the drillable was null!");
            return;
        }
        
        if (_drillable)
        {
            if (_drillable == newDrillable)
            {
                Plugin.Logger.LogError($"Tried SetDrillable {drillableType} but it's the same one as before!");
                return;
            }
            
            ClearDrillable(sub);
        }
        
        _drillable = newDrillable;
        _drillable.gameObject.SetActive(true);
        _drillable.onDrilled += sub;
        
        _render.CacheNew(_drillable, matrix.ShaderYMin, matrix.ShaderYMax);
    }

    public void ClearDrillable(Drillable.OnDrilled unsubThis)
    {
        if (!_drillable)
        {
            Plugin.Logger.LogWarning("Tried clearing drillable when it is already cleared");
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
            Plugin.Logger.LogWarning("Tried RestoreDrillable when we have none");
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
        if (!_drillable)
        {
            Plugin.Logger.LogError("Tried UpdateDrillableVisuals but drillable is currently null!");
            return;
        }

        if (!_render)
        {
            Plugin.Logger.LogError("Tried UpdateDrillableVisuals but render is currently null!");
            return;
        }
        
        _render.UpdateDrillableVisuals(_drillable.transform.position, progress);
    }
}