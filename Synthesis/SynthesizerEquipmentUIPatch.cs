using HarmonyLib;
using UnityEngine;

namespace Synthesis;

[HarmonyPatch]
internal static class SynthesizerEquipmentUIPatch
{
    [HarmonyPatch(typeof(uGUI_Equipment), nameof(uGUI_Equipment.Awake))]
    [HarmonyPrefix]
    public static void AwakePrefix(uGUI_Equipment __instance)
    {
        //add a new slot for the synthesizer
        foreach (var slot in __instance.GetComponentsInChildren<uGUI_EquipmentSlot>())
        {
            if (slot.slot != "DecoySlot1") continue;
            
            uGUI_EquipmentSlot slotClone = Object.Instantiate(slot.gameObject, slot.transform.parent).GetComponent<uGUI_EquipmentSlot>();
            slotClone.slot = SynthesizerAuthoring.EquipmentSlot1Name;
            slotClone.gameObject.name = slotClone.slot;
            
            slotClone.transform.localPosition = Vector3.zero;
            slotClone.transform.localRotation = slot.transform.localRotation;
            slotClone.transform.localScale = slot.transform.localScale * 1.5f;
            return;
        }
    }
}
