using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ProtoBuf;
using UnityEngine;

namespace Synthesis;

/// <summary>
/// Responsible for only handling the equipment. 
/// </summary>
[ProtoContract]
public class SynthesizerEquipment : MonoBehaviour, IProtoEventListener, IProtoTreeEventListener 
{
    private const string SlotId = "SynthesizerMatrix";
    
    public ChildObjectIdentifier storageRoot;
    
    [NonSerialized]
    [ProtoMember(1, OverwriteList = true)]
    public Dictionary<string, string> serializedEquipmentSlots;

    private Equipment equipment;

    public bool HasMatrix => equipment.GetItemInSlot(SlotId) != null;
    public Matrix Matrix
    {
        get
        {
            InventoryItem item = equipment.GetItemInSlot(SlotId);
            return item?.item.GetComponent<Matrix>();
        }
    }

    private void Awake()
    {
        Plugin.Logger.LogInfo("SynthesizerEquipment.Awake");
        
        equipment = new Equipment(gameObject, storageRoot.transform);
        equipment.SetLabel(ModLocalization.SynthesizerStorageLabel);
        equipment.AddSlot(SlotId);
        equipment.compatibleSlotDelegate = (EquipmentType type, out string slot) =>
        {
            slot = SlotId;
            return type == SynthesizerAuthoring.SynthesizerEquipment;
        };
        
    }
    
    //Will always run after Awake, and before start.
    //It, however, will NOT always run, like, for example, if it's spawned in for the first time from the builder or a command 
    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        Plugin.Logger.LogInfo("SynthesizerEquipment.OnProtoDeserializeObjectTree");
        
        if (equipment == null)
        {
            Plugin.Logger.LogError("Equipment was null during OnProtoDeserializeObjectTree!!!");
            return;
        }
        
        if (serializedEquipmentSlots != null)
        {
            StorageHelper.TransferEquipment(storageRoot.gameObject, serializedEquipmentSlots, equipment);
            equipment.AddSlot(SlotId);
            serializedEquipmentSlots = null;
        }
    }
    
    public void OnProtoSerialize(ProtobufSerializer serializer)
    {
        serializedEquipmentSlots = equipment.SaveEquipment();
    }
    
    public void Sub(Equipment.OnEquip onEquip, Equipment.OnUnequip onUnequip)
    {
        equipment.onEquip += onEquip;
        equipment.onUnequip += onUnequip;
    }
    
    //via GenericHandTrigger
    [UsedImplicitly]
    public void OnHandHover(HandTargetEventData eventData)
    {
        if (!enabled) return;
		
        HandReticle main = HandReticle.main;
        main.SetIcon(HandReticle.IconType.Hand);
        main.SetText(HandReticle.TextType.Hand, ModLocalization.SynthesizerOpenStorage, translate: true, GameInput.Button.LeftHand);
        main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
    }

    //via GenericHandTrigger
    [UsedImplicitly]
    public void OnHandClick(HandTargetEventData eventData)
    {
        if (!enabled) return;

        PDA pda = Player.main.GetPDA();
        if (pda.isInUse) return;
        
        Inventory.main.SetUsedStorage(equipment);
        pda.Open(PDATab.Inventory, transform);
    }
    
    public void OnProtoDeserialize(ProtobufSerializer serializer) { }
    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer) { }
}