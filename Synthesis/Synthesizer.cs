using System;
using JetBrains.Annotations;
using ProtoBuf;
using UnityEngine;

namespace Synthesis;

[ProtoContract]
public class Synthesizer : MonoBehaviour, IObstacle, IHandTarget
{
	[NonSerialized]
	[ProtoMember(1)]
	public float timeBegin = -1f;

	[NonSerialized]
	[ProtoMember(2)]
	public bool isSynthesizing;
	
	private SynthesizerDrillableHandler _drillableHandle;
	private SynthesizerEquipment _equipmentHandle;
	private SynthesizerAudio _sfx;

	private void Awake()
	{
		_drillableHandle = GetComponent<SynthesizerDrillableHandler>();
		_equipmentHandle = GetComponent<SynthesizerEquipment>();
		_sfx = GetComponent<SynthesizerAudio>();
		
		_equipmentHandle.OnAwake(OnMatrixAdded, OnMatrixRemoved, IsAllowedToRemove);
	}
	
	private void Start()
	{
		//3 possible scenarios:
		//- no drillable:		!isSynthesizing && !HasMatrix	return!
		//- being made:			 isSynthesizing && HasMatrix	Set drillable! set minable NO! Set Sfx loop
		//- drillable exists:	!isSynthesizing && HasMatrix	Set drillable! set minable YES! (it is already like this, so no need to set) 
		
		Matrix matrix = _equipmentHandle.Matrix;
		if (!matrix)
		{
			//there's no possible scenario where we should be synthesizing and also having no matrix
			if (isSynthesizing)
			{
				Plugin.Logger.LogInfo($"Panic!!! We are synthesizing and also having no matrix!");
			}
			return;
		}
		
		_drillableHandle.SetDrillable(matrix.Drillable, OnCompletelyDrilled);

		if (isSynthesizing)
		{
			_drillableHandle.SetDrillableMinable(false);
			_sfx.PlayLoop();
		}
	}

	private void Update()
	{
		if (isSynthesizing)
		{
			UpdateDrillableProgress();
		}
	}
	
	// Will also potentially call when loading proto, which is useful
	private void OnMatrixAdded(string slot, InventoryItem item)
	{
		Plugin.Logger.LogInfo($"OnMatrixAdded {item.techType}");
		
		_drillableHandle.SetDrillable(item.techType, OnCompletelyDrilled);
		BeginSynthesis(0);
	}
	private void OnMatrixRemoved(string slot, InventoryItem item)
	{
		Plugin.Logger.LogInfo($"OnMatrixRemoved {item.techType}");
		
		_drillableHandle.ClearDrillable(OnCompletelyDrilled);
		OnSynthesisInterrupted();
	}
	
	public void OnCompletelyDrilled(Drillable drillable)
	{
		BeginSynthesis(5);
	}

	private void BeginSynthesis(float delay)
	{
		if (isSynthesizing) return;
		
		isSynthesizing = true;
		timeBegin = DayNightCycle.main.timePassedAsFloat + delay;
		Invoke(nameof(OnSynthesisBegin), delay);
	}
	
	public void OnSynthesisBegin()
	{
		_drillableHandle.RestoreDrillable();
		_drillableHandle.SetDrillableMinable(false);
		
		UpdateDrillableProgress();
		
		_sfx.PlayStart();
		_sfx.PlayLoop();
	}

	private void UpdateDrillableProgress()
	{
		float progress = 1f;
		if (isSynthesizing)
		{
			float timePassed = DayNightCycle.main.timePassedAsFloat - timeBegin;
			progress = Mathf.Clamp01(timePassed / 5);
		}
		
		_drillableHandle.UpdateDrillableVisuals(progress);
		
		if (isSynthesizing && progress >= 1f)
		{
			OnSynthesisComplete();
		}
	}
	
	public void OnSynthesisComplete()
	{
		isSynthesizing = false;
		_drillableHandle.SetDrillableMinable(true);
		
		UpdateDrillableProgress();
		
		_sfx.StopLoop();
		_sfx.PlayEnd();
	}
	
	public void OnSynthesisInterrupted()
	{
		isSynthesizing = false;
		
		_sfx.StopLoop();
		_sfx.PlayEnd();
	}
	
	private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
	{
		//removing the matrix clears the drillable.
		//block removing the matrix if there is a completed drillable on the pedestal.
		//so there's no disappointment if the matrix is removed.
		if (!isSynthesizing)
		{
			ErrorMessage.AddMessage(Language.main.Get(ModLocalization.SynthesizerEquipmentCantRemove));
			return false;
		}
		return true;
	}
	
	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		if (_equipmentHandle.HasMatrix)
		{
			reason = Language.main.Get(ModLocalization.SynthesizerDeconstructNotEmptyError);
			return false;
		}
		reason = null;
		return true;
	}
	
	//via GenericHandTrigger
	[UsedImplicitly]
	public void OnHandHover(HandTargetEventData eventData)
	{
		
	}

	//via GenericHandTrigger
	[UsedImplicitly]
	public void OnHandClick(HandTargetEventData eventData)
	{
		
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!enabled) return;
		
		HandReticle main = HandReticle.main;
		main.SetIcon(HandReticle.IconType.Hand);
		main.SetText(HandReticle.TextType.Hand, ModLocalization.SynthesizerOpenStorage, translate: true, GameInput.Button.LeftHand);
		main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
	}

	public void OnHandClick(GUIHand hand)
	{
		if (!enabled) return;

		PDA pda = Player.main.GetPDA();
		if (pda.isInUse) return;
        
		_equipmentHandle.SetUsedStorage();
		pda.Open(PDATab.Inventory, transform);
	}
}