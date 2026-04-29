using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using ProtoBuf;
using UnityEngine;
using Vector4 = UnityEngine.Vector4;

namespace Synthesis;

[ProtoContract]
public class Synthesizer : MonoBehaviour, IProtoEventListener, IProtoTreeEventListener, IObstacle
{
	private const string SlotId = "SynthesizerMatrix";
	
	[NonSerialized]
	[ProtoMember(2)]
	public float timeBegin = -1f;

	[NonSerialized]
	[ProtoMember(3)]
	public bool isSynthesizing;
	
	[NonSerialized]
	[ProtoMember(5, OverwriteList = true)]
	public Dictionary<string, string> serializedEquipmentSlots;
	
	[NonSerialized]
	[ProtoMember(6)]
	public string serializedDrillableId;
	
	
	public ChildObjectIdentifier storageRoot;
	public SynthesizerAudio sfx;
	public SynthesizerRendering Render;

	
	private PrefabIdentifier drillableId;
	private Drillable drillable;
	private Equipment equipment;
	
	private Matrix GetEquippedMatrix()
	{
		InventoryItem item = equipment.GetItemInSlot(SlotId);
		return item?.item.GetComponent<Matrix>();
	}
	private bool HasMatrix()
	{
		return equipment.GetItemInSlot(SlotId) != null;
	}

	private void EnsureEquipment()
	{
		if (equipment != null) return;

		equipment = new Equipment(gameObject, storageRoot.transform);
		equipment.SetLabel(ModLocalization.SynthesizerStorageLabel);
		//equipment.isAllowedToAdd = IsAllowedToAdd;
		//equipment.isAllowedToRemove = IsAllowedToRemove;
		equipment.compatibleSlotDelegate = GetCompatibleSlot;
		equipment.onEquip += OnEquip;
		equipment.onUnequip += OnUnequip;
		UnlockSlot();
	}

	private bool GetCompatibleSlot(EquipmentType itemType, out string slot)
	{
		slot = SlotId;
		return itemType == SynthesizerAuthoring.SynthesizerEquipment;
	}

	private void OnEquip(string slot, InventoryItem item)
	{
		
	}

	private void OnUnequip(string slot, InventoryItem item)
	{
		
	}

	private void Awake()
	{
		EnsureEquipment();
	}

	private void Start()
	{
		//todo check for actual drillable that was pre-existing based on what is in storage upon loading in
		TryContinueFromSave();
	}

	private void UnlockSlot()
	{
		equipment.AddSlot(SlotId);
	}
	
	private void TryContinueFromSave()
	{
		EnsureEquipment();
		if (serializedEquipmentSlots != null)
		{
			StorageHelper.TransferEquipment(storageRoot.gameObject, serializedEquipmentSlots, equipment);
			
			equipment.RestoreEquipment();
			
			serializedEquipmentSlots = null;
		}
		UnlockSlot();
		
		if (serializedSlots != null)
		{
			Dictionary<string, InventoryItem> items = StorageHelper.ScanItems(equipmentRoot.transform);
			equipment.RestoreEquipment(serializedSlots, items);
			serializedSlots = null;
			UnlockDefaultEquipmentSlots();
		}
		
		if (!isSynthesizing) return;
		
		//try getting the current drillable on top of this pedestal!!
		
		if (drillable == null)
		{
			Plugin.Logger.LogError("Tried TryContinueFromSave but drillable is currently null!");
			return;
		}
		
		if (drillable)
		{
			SetNewDrillable(drillable);
		}

		sfx.PlayLoop();
		SetDrillableColliders(false);
	}
	
	private void OnDestroy()
	{
		DestroyDrillable();
	}

	//called from a GenericHandTrigger
	public void OnHandHover(HandTargetEventData eventData)
	{
		if (!enabled) return;
		
		HandReticle main = HandReticle.main;
		main.SetIcon(HandReticle.IconType.Hand);
		main.SetText(HandReticle.TextType.Hand, ModLocalization.SynthesizerOpenStorage, translate: true, GameInput.Button.LeftHand);
		main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
	}

	//called from a GenericHandTrigger
	public void OnHandClick(HandTargetEventData eventData)
	{
		if (!enabled) return;
		
		PDA pda = Player.main.GetPDA();
		if (!pda.isInUse)
		{
			Inventory.main.SetUsedStorage(equipment);
			pda.Open(PDATab.Inventory, transform);
		}
	}
	
	private void Update()
	{
		if (isSynthesizing)
		{
			UpdateDrillableProgress();
		}
	}
	

	public void OnDrilled(Drillable _)
	{
		const float timeBeforeMakingNewOne = 5f;
		
		timeBegin = DayNightCycle.main.timePassedAsFloat + timeBeforeMakingNewOne;
		isSynthesizing = true;
		Invoke(nameof(OnSynthesisBegin), timeBeforeMakingNewOne);
	}

	public void OnSynthesisBegin()
	{
		drillable.Restore();
		SetDrillableColliders(false);
		
		UpdateDrillableProgress();
		
		sfx.PlayStart();
		sfx.PlayLoop();
	}

	public void OnSynthesisComplete()
	{
		isSynthesizing = false;
		SetDrillableColliders(true);
		
		UpdateDrillableProgress();
		
		sfx.StopLoop();
		sfx.PlayEnd();
	}

	private void SetDrillableColliders(bool enable)
	{
		if (drillable == null)
		{
			Plugin.Logger.LogError("Tried SetDrillableColliders but drillable is currently null!");
			return;
		}
		//We'd turn off colliders so it may fully show itself, but is not interactable yet
		Collider[] colliders = drillable.GetComponentsInChildren<Collider>();
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = enable;
		}
	}

	private void UpdateDrillableProgress()
	{
		float progress = 1f;
		if (isSynthesizing)
		{
			float timePassed = DayNightCycle.main.timePassedAsFloat - timeBegin;
			progress = Mathf.Clamp01(timePassed / 5);
		}

		Render.UpdateDrillableVisuals(drillable.transform.position, progress);
		
		if (isSynthesizing && progress >= 1f)
		{
			OnSynthesisComplete();
		}
	}
	
	
	public void SetNewDrillable(Drillable newDrillable)
	{
		if (drillable)
		{
			DestroyDrillable();
		}
		
		if (!newDrillable) return;

		drillable = newDrillable;
		
		Render.CacheMaterials(drillable);
		UpdateDrillableProgress();
	}

	private void DestroyDrillable()
	{
		if (!drillable)
		{
			Plugin.Logger.LogWarning("Tried destroying drillable when it already destroyed");
			return;
		}
		drillable.onDrilled -= OnDrilled;
		Render.DestroyMaterials();
		Destroy(drillable.gameObject);
		drillable = null;
	}
	
	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		if (HasMatrix())
		{
			reason = Language.main.Get(ModLocalization.SynthesizerDeconstructNotEmptyError);
			return false;
		}
		reason = null;
		return true;
	}
	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		serializedEquipmentSlots = equipment.SaveEquipment();
		serializedDrillableId = drillableId.Id;
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
		
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		EnsureEquipment();
		if (serializedEquipmentSlots != null)
		{
			StorageHelper.TransferEquipment(storageRoot.gameObject, serializedEquipmentSlots, equipment);
			serializedEquipmentSlots = null;
		}
		UnlockSlot();

		if (serializedDrillableId != null)
		{
			UniqueIdentifier.
		}
		
	}

	
}