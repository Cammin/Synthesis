using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Synthesis;

public class Synthesizer : MonoBehaviour, IConstructable, IHandTarget
{
	private const string SlotId = SynthesizerAuthoring.EquipmentSlot1Name;
	
	public float TimeBegin
	{
		get => _data.TimeBegin;
		set => _data.TimeBegin = value;
	}
	public bool IsSynthesizing
	{
		get => _data.IsSynthesizing;
		set => _data.IsSynthesizing = value;
	}
	public Dictionary<string, string> SerializedEquipment
	{
		get => _data.SerializedEquipment;
		set => _data.SerializedEquipment = value;
	}

	private SynthesizerInstanceData _data;
	private SynthesizerDrillableHandler _drillableHandle;
	private SynthesizerAudio _sfx;
	private PrefabIdentifier _id;
	
	private string _idString;
	private Equipment _equipment;
	private Matrix _matrix;
	private bool _isInitializingEquipment;
	
	private void Awake()
	{
		_drillableHandle = GetComponent<SynthesizerDrillableHandler>();
		_sfx = GetComponent<SynthesizerAudio>();
		_id = GetComponent<PrefabIdentifier>();
	}
	
	private void Start()
	{
		_idString = _id.Id;
		_data = SynthesizerSaveData.Instance.GetOrAddNew(_idString);
		ChildObjectIdentifier storageRoot = transform.Find(SynthesizerAuthoring.StorageRootName).GetComponent<ChildObjectIdentifier>();
		
		_isInitializingEquipment = true;
		_equipment = new Equipment(gameObject, storageRoot.transform);
		_equipment.SetLabel(ModLocalization.SynthesizerStorageLabel);
		_equipment.onEquip += OnMatrixAdded;
		_equipment.onUnequip += OnMatrixRemoved;
		_equipment.isAllowedToRemove += IsAllowedToRemove;
		_equipment.compatibleSlotDelegate = (EquipmentType type, out string slot) =>
		{
			slot = SlotId;
			return type == SynthesizerAuthoring.SynthesizerEquipmentType;
		};
		
		if (SerializedEquipment != null)
		{
			StorageHelper.TransferEquipment(storageRoot.gameObject, SerializedEquipment, _equipment);
		}
		else
		{
			_equipment.AddSlot(SlotId);
		}
		_isInitializingEquipment = false;
		
		//3 possible scenarios:
		//- no drillable:	!isSynthesizing && !HasMatrix	return!
		//- being made:	isSynthesizing && HasMatrix	Set drillable! set minable NO! Set Sfx loop
		//- drillable exists:	!isSynthesizing && HasMatrix	Set drillable! set minable YES! (it is already like this, so no need to set) 
		
		if (!_matrix)
		{
			//there's no possible scenario where we should be synthesizing and also having no matrix
			if (IsSynthesizing)
			{
				Plugin.Logger.LogError($"Should never happen!!! We are synthesizing and also having no matrix!");
			}
			return;
		}
		
		//init the state if we were currently in-progress
		if (IsSynthesizing)
		{
			_drillableHandle.SetDrillableMinable(false);
			_sfx.PlayLoop();
		}
	}

	private void Update()
	{
		if (IsSynthesizing)
		{
			UpdateDrillableProgress();
		}
	}
	
	private void OnMatrixAdded(string slot, InventoryItem item)
	{
		_matrix = item.item.GetComponent<Matrix>();
		_drillableHandle.SetDrillable(_matrix, OnCompletelyDrilled);
		UpdateDrillableProgress();
		
		if (!_isInitializingEquipment)
		{
			_data.SerializedEquipment = _equipment.SaveEquipment();
			BeginSynthesis(0);
		}
	}
	private void OnMatrixRemoved(string slot, InventoryItem item)
	{
		if (item.item.isDestroyed)
		{
			return;
		}
		
		_matrix = null;
		_data.SerializedEquipment = null;
		
		//interrupt synthesis!
		_drillableHandle.ClearDrillable(OnCompletelyDrilled);
		IsSynthesizing = false;
		_sfx.StopLoop();
		_sfx.PlayEnd();
	}
	
	public void OnCompletelyDrilled(Drillable drillable)
	{
		BeginSynthesis(5);
	}

	private void BeginSynthesis(float delay)
	{
		if (IsSynthesizing) return;
		
		IsSynthesizing = true;
		TimeBegin = DayNightCycle.main.timePassedAsFloat + delay;
		Invoke(nameof(OnSynthesisBegin), delay);
	}
	
	public void OnSynthesisBegin()
	{
		//if the synthesis was interrupted
		if (!IsSynthesizing) return;
		
		_drillableHandle.RestoreDrillable();
		_drillableHandle.SetDrillableMinable(false);
		
		UpdateDrillableProgress();
		
		_sfx.PlayStart();
		_sfx.PlayLoop();
	}

	private void UpdateDrillableProgress()
	{
		if (IsSynthesizing && _matrix == null)
		{
			Plugin.Logger.LogError("UpdateDrillableProgress Matrix is null!");
			return;
		}
		
		float progress = 1f;
		if (IsSynthesizing)
		{
			progress = GetProgress();
		}
		
		_drillableHandle.UpdateDrillableVisuals(progress);
		
		//is it complete?
		if (IsSynthesizing && progress >= 1f)
		{
			IsSynthesizing = false;
			_drillableHandle.SetDrillableMinable(true);
		
			_sfx.StopLoop();
			_sfx.PlayEnd();
		}
	}
	
	private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
	{
		//removing the matrix clears the drillable.
		//block removing the matrix if there is a completed Drillable on the pedestal.
		//so there's no disappointment if the matrix is removed.
		/*if (!IsSynthesizing)
		{
			return false;
		}*/
		return true;
	}
	
	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		if (_matrix != null)
		{
			reason = Language.main.Get(ModLocalization.SynthesizerDeconstructNotEmptyError);
			return false;
		}
		reason = null;
		return true;
	}

	private bool _isConstructed;
	
	public void OnConstructedChanged(bool constructed)
	{
		//puts true/false whether it's constructed when pre-existing in the world
		//puts false when initially building
		//puts false when initially deconstructing
		//puts true when completed building
		
		_isConstructed = constructed;
	}
	private void OnDestroy()
	{
		if (!_isConstructed)
		{
			SynthesizerSaveData.Instance.DeleteInstance(_idString);
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!enabled) return;
		
		HandReticle main = HandReticle.main;
		main.SetIcon(HandReticle.IconType.Hand);
		main.SetText(HandReticle.TextType.Hand, ModLocalization.SynthesizerHand, translate: true, GameInput.Button.LeftHand);
		main.SetText(HandReticle.TextType.HandSubscript, GetSubscript(), translate: true);

		if (IsSynthesizing)
		{
			main.SetProgress(GetProgress());
			main.SetIcon(HandReticle.IconType.Progress, 1.5f);
		}
	}

	private float GetProgress()
	{
		float duration = _matrix.SynthesisDuration;
		float timePassed = DayNightCycle.main.timePassedAsFloat - TimeBegin;
		return Mathf.Clamp01(timePassed / duration);
	}

	private string GetSubscript()
	{
		if (!_matrix)
		{
			return ModLocalization.SynthesizerHandSubscriptEmpty;
		}

		if (!IsSynthesizing)
		{
			return ModLocalization.SynthesizerHandSubscriptDestroyDrillable;
		}

		return string.Empty;
	}

	public void OnHandClick(GUIHand hand)
	{
		if (!enabled) return;

		PDA pda = Player.main.GetPDA();
		if (pda.isInUse) return;
        
		Inventory.main.SetUsedStorage(_equipment);
		pda.Open(PDATab.Inventory, transform);
	}
}