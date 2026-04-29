using System.Collections.Generic;
using UnityEngine;

namespace Synthesis;

public class SynthesizerRendering : MonoBehaviour
{
    public Texture2D _EmissiveTex;
    
    private List<Renderer> renderers;
    private List<Material> materials;
    private MaterialPropertyBlock block;
    
    private void Start()
    {
        block = new MaterialPropertyBlock();
    }

    public void CacheMaterials(Component obj)
    {
        renderers = new List<Renderer>(obj.GetComponentsInChildren<Renderer>());
        foreach (Renderer renderer in renderers)
        {
            materials = new List<Material>(renderer.materials);
            foreach (Material mat in materials)
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
    
    public void DestroyMaterials()
    {
        if (materials == null) return;
        
        foreach (Material material in materials)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
        materials.Clear();
    }
}