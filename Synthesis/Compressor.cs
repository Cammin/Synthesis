using UnityEngine;

namespace Synthesis;

public class Compressor : GhostCrafter
{
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Deinitialize()
    {
        base.Deinitialize();
    }

    public override void Craft(TechType techType, float duration)
    {
        base.Craft(techType, duration);
    }

    public override void OnCraftingBegin(TechType techType, float duration)
    {
        base.OnCraftingBegin(techType, duration);
    }

    public override void OnCraftingEnd()
    {
        base.OnCraftingEnd();
    }

    public override void OnStateChanged(bool crafting)
    {
        base.OnStateChanged(crafting);
    }

    public override void OnProgress(float progress)
    {
        base.OnProgress(progress);
    }

    public override void OnOpenedChanged(bool opened)
    {
        Plugin.Logger.LogInfo("[Compressor] OnOpenedChanged: " + opened);
        base.OnOpenedChanged(opened);
    }

    public override void OnCraftedItemPickup(GameObject item)
    {
        base.OnCraftedItemPickup(item);
    }

    public override void OnItemChanged(TechType techType)
    {
        base.OnItemChanged(techType);
    }
}