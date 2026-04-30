using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis;

/// <summary>
/// Responsible for only vfx/shaders.
/// When we setup a new drillable, destroy the mats of the old one and cache the new.
/// </summary>
public class SynthesizerRendering : MonoBehaviour
{
    public Texture2D _EmissiveTex;
    
    private List<Renderer> renderers;
    private List<Material> materials;
    private MaterialPropertyBlock block;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
    }
    
    private void OnDestroy()
    {
        TryDestroyMaterials();
    }

    public void CacheNew(Component obj)
    {
        TryDestroyMaterials();

        renderers = new List<Renderer>(obj.GetComponentsInChildren<Renderer>());
        materials = new List<Material>();
        foreach (Renderer renderer in renderers)
        {
            var mats = renderer.materials;
            materials.AddRange(mats);
            
            foreach (Material mat in mats)
            {
                mat.EnableKeyword("FX_BUILDING");
                mat.SetTexture(ShaderPropertyID._EmissiveTex, _EmissiveTex);
                mat.SetColor(ShaderPropertyID._BorderColor, new Color(0.75f, 1f, 0.9f, 1f));
                mat.SetFloat(ShaderPropertyID._Cutoff, 0.42f);
                mat.SetVector(ShaderPropertyID._BuildParams, new Vector4(2f, 0.7f, 3f, -0.25f));
                mat.SetFloat(ShaderPropertyID._NoiseStr, 0.25f);
                mat.SetFloat(ShaderPropertyID._NoiseThickness, 0.49f);
                mat.SetFloat(ShaderPropertyID._BuildLinear, 1f);
                mat.SetFloat(ShaderPropertyID._MyCullVariable, 0f);
            }
        }
        UpdateDrillableVisuals(obj.transform.position, 0);
    }
    
    public void UpdateDrillableVisuals(Vector3 pos, float progress)
    {
        float min = pos.y - 0.5f;
        float max = pos.y + 2.5f;
		
        foreach (Renderer renderer in renderers)
        {
            renderer.GetPropertyBlock(block);
            block.SetFloat(ShaderPropertyID._Built, progress);
            block.SetFloat(ShaderPropertyID._minYpos, min);
            block.SetFloat(ShaderPropertyID._maxYpos, max);
            renderer.SetPropertyBlock(block);
        }
    }

    private void TryDestroyMaterials()
    {
        if (materials == null) return;
        
        foreach (Material material in materials)
        {
            Destroy(material);
        }
        materials.Clear();
    }
}