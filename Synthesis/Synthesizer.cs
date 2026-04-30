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
public class Synthesizer : MonoBehaviour, IObstacle
{
	[NonSerialized]
	[ProtoMember(2)]
	public float timeBegin = -1f;

	[NonSerialized]
	[ProtoMember(3)]
	public bool isSynthesizing;
	
	public SynthesizerAudio sfx;
	public SynthesizerRendering Render;
	public SynthesizerEquipment _equip;
	public SynthesizerDrillableHandler drillableHandler;
	
	private void Start()
	{
		
		_equip.Sub(OnMatrixAdded, OnMatrixRemoved);
		
		
		if (!isSynthesizing) return;

		Matrix matrix = _equip.Matrix;

		drillableHandler.SetDrillable(matrix.Drillable, OnCompletelyDrilled);
		drillableHandler.SetMinable(false);
		
		sfx.PlayLoop();
	}
	
	private void OnMatrixAdded(string slot, InventoryItem item)
	{
		Plugin.Logger.LogInfo("OnMatrixAdded");
	}
	private void OnMatrixRemoved(string slot, InventoryItem item)
	{
		Plugin.Logger.LogInfo("OnMatrixRemoved");
	}

	private void Update()
	{
		if (isSynthesizing)
		{
			UpdateDrillableProgress();
		}
	}
	
	public void OnCompletelyDrilled(Drillable drillable)
	{
		const float timeBeforeMakingNewOne = 5f;
		
		timeBegin = DayNightCycle.main.timePassedAsFloat + timeBeforeMakingNewOne;
		isSynthesizing = true;
		Invoke(nameof(OnSynthesisBegin), timeBeforeMakingNewOne);
	}
	
	public void OnSynthesisBegin()
	{
		drillableHandler.RestoreDrillable();
		drillableHandler.SetMinable(false);
		
		UpdateDrillableProgress();
		
		sfx.PlayStart();
		sfx.PlayLoop();
	}

	private void UpdateDrillableProgress()
	{
		float progress = 1f;
		if (isSynthesizing)
		{
			float timePassed = DayNightCycle.main.timePassedAsFloat - timeBegin;
			progress = Mathf.Clamp01(timePassed / 5);
		}
		
		drillableHandler.UpdateDrillableVisuals(progress);
		
		if (isSynthesizing && progress >= 1f)
		{
			OnSynthesisComplete();
		}
	}
	
	public void OnSynthesisComplete()
	{
		isSynthesizing = false;
		drillableHandler.SetMinable(true);
		
		UpdateDrillableProgress();
		
		sfx.StopLoop();
		sfx.PlayEnd();
	}
	
	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		if (_equip.HasMatrix)
		{
			reason = Language.main.Get(ModLocalization.SynthesizerDeconstructNotEmptyError);
			return false;
		}
		reason = null;
		return true;
	}
}