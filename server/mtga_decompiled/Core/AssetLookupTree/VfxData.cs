using System;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.Serialization;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree;

[Serializable]
public class VfxData
{
	public VfxActivationType ActivationType;

	public string LoopingKey;

	public bool CanSurviveZoneTransfer;

	public SpaceData SpaceData;

	public bool ParentToSpace = true;

	public bool IgnoreDedupe;

	public TurnWideDataOptions TurnWideDataOptions;

	public VfxPrefabData PrefabData;

	public OffsetData Offset;

	public bool PlayOnAttachmentStack;

	public bool PlayOnStackChildren;

	public bool HideIfNotTopOfStack;

	public VfxDelayData DelayData;

	[FormerlySerializedAs("OffsetRelativeToBattlefieldHeight")]
	public bool AddParentZPositionToOffset;

	public VfxData()
	{
		ActivationType = VfxActivationType.OneShot;
		SpaceData = new SpaceData();
		ParentToSpace = true;
		IgnoreDedupe = false;
		TurnWideDataOptions = TurnWideDataOptions.None;
		PrefabData = new VfxPrefabData();
		Offset = new OffsetData();
		PlayOnAttachmentStack = false;
		HideIfNotTopOfStack = false;
		PlayOnStackChildren = false;
		AddParentZPositionToOffset = false;
		DelayData = null;
	}

	public AltAssetReference<GameObject> GetRandomPrefab()
	{
		AltAssetReference<GameObject> result = null;
		if (PrefabData.AllPrefabs.Count > 0)
		{
			result = PrefabData.AllPrefabs.SelectRandom();
		}
		return result;
	}

	public bool HasDelay(AssetLookupSystem als, ICardDataAdapter model)
	{
		if (DelayData == null || DelayData.Time == 0f)
		{
			return false;
		}
		if (DelayData.Condition != null)
		{
			als.Blackboard.Clear();
			als.Blackboard.SetCardDataExtensive(model);
			return DelayData.Condition.Execute(als.Blackboard);
		}
		return true;
	}
}
