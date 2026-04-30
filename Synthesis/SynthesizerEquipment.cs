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
    
    [NonSerialized]
    [ProtoMember(1, OverwriteList = true)]
    private Dictionary<string, string> _protoEquipment;

    private ChildObjectIdentifier _storageRoot;
    private Equipment _equipment;

    public bool HasMatrix => _equipment.GetItemInSlot(SlotId) != null;
    public Matrix Matrix => _equipment.GetItemInSlot(SlotId)?.item.GetComponent<Matrix>();

    public void OnAwake(Equipment.OnEquip onEquip, Equipment.OnUnequip onUnequip, IsAllowedToRemove isAllowedToRemove)
    {
        Plugin.Logger.LogInfo("SynthesizerEquipment.Awake");
        _storageRoot = transform.Find(SynthesizerAuthoring.StorageRootName).GetComponent<ChildObjectIdentifier>();
        
        _equipment = new Equipment(gameObject, _storageRoot.transform);
        _equipment.SetLabel(ModLocalization.SynthesizerStorageLabel);
        _equipment.AddSlot(SlotId);
        _equipment.onEquip += onEquip;
        _equipment.onUnequip += onUnequip;
        _equipment.isAllowedToRemove += isAllowedToRemove;
        _equipment.compatibleSlotDelegate = (EquipmentType type, out string slot) =>
        {
            slot = SlotId;
            return type == SynthesizerAuthoring.SynthesizerEquipmentType;
        };
    }
    
    //Will always run after Awake, and before start.
    //It, however, will NOT always run, like, for example, if it's spawned in for the first time from the builder or a command 
    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        Plugin.Logger.LogInfo("SynthesizerEquipment.OnProtoDeserializeObjectTree");
        
        if (_equipment == null)
        {
            Plugin.Logger.LogError("Equipment was null during OnProtoDeserializeObjectTree!!!");
            return;
        }
        
        if (_protoEquipment != null)
        {
            StorageHelper.TransferEquipment(_storageRoot.gameObject, _protoEquipment, _equipment);
            _equipment.AddSlot(SlotId);
            _protoEquipment = null;
        }
    }
    
    public void OnProtoSerialize(ProtobufSerializer serializer)
    {
        _protoEquipment = _equipment.SaveEquipment();
    }
    
    public void OnProtoDeserialize(ProtobufSerializer serializer) { }
    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer) { }

    public void SetUsedStorage()
    {
        Inventory.main.SetUsedStorage(_equipment);
    }
}