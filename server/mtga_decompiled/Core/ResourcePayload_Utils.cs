using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Resource;
using GreClient.Rules;
using MovementSystem;
using UnityEngine;
using Wotc.Mtga.Duel;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public static class ResourcePayload_Utils
{
	public static void PlayManaAnimation(MtgMana mana, Vector3 sourcePos, Vector3 destPos, System.Action onComplete, GameManager gameManager, MtgEntity sourceEntity, MtgEntity sinkEntity, CounterType counterType)
	{
		AssetLookupSystem assetLookupSystem = gameManager.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.CounterType = counterType;
		assetLookupSystem.Blackboard.ManaMovement = new ManaMovementData(sourceEntity, sinkEntity, mana);
		if (sourceEntity != null)
		{
			gameManager.ViewManager.TryGetCardView(sourceEntity.InstanceId, out var cardView);
			if ((bool)cardView)
			{
				assetLookupSystem.Blackboard.SetCardDataExtensive(cardView.Model);
			}
		}
		AssetLookupTree<BirthVFX> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<BirthVFX>();
		AssetLookupTree<SustainVFX> assetLookupTree2 = assetLookupSystem.TreeLoader.LoadTree<SustainVFX>();
		AssetLookupTree<HitVFX> assetLookupTree3 = assetLookupSystem.TreeLoader.LoadTree<HitVFX>();
		AssetLookupTree<SinkHitVFX> assetLookupTree4 = assetLookupSystem.TreeLoader.LoadTree<SinkHitVFX>();
		AssetLookupTree<SourceHitVFX> assetLookupTree5 = assetLookupSystem.TreeLoader.LoadTree<SourceHitVFX>();
		BirthVFX payload = assetLookupTree.GetPayload(assetLookupSystem.Blackboard);
		SustainVFX payload2 = assetLookupTree2.GetPayload(assetLookupSystem.Blackboard);
		HitVFX payload3 = assetLookupTree3.GetPayload(assetLookupSystem.Blackboard);
		SourceHitVFX payload4 = assetLookupTree5.GetPayload(assetLookupSystem.Blackboard);
		HashSet<SinkHitVFX> hashSet = new HashSet<SinkHitVFX>();
		assetLookupTree4.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet);
		if (payload == null && payload2 == null && payload3 == null && assetLookupTree4 == null && assetLookupTree5 == null)
		{
			onComplete?.Invoke();
			return;
		}
		AssetLookupTree<SplinePayload> assetLookupTree6 = assetLookupSystem.TreeLoader.LoadTree<SplinePayload>();
		AssetLookupTree<SourceOffsetPayload> assetLookupTree7 = assetLookupSystem.TreeLoader.LoadTree<SourceOffsetPayload>();
		AssetLookupTree<SinkOffsetPayload> assetLookupTree8 = assetLookupSystem.TreeLoader.LoadTree<SinkOffsetPayload>();
		AssetLookupTree<BirthSFX> assetLookupTree9 = assetLookupSystem.TreeLoader.LoadTree<BirthSFX>();
		AssetLookupTree<SustainSFX> assetLookupTree10 = assetLookupSystem.TreeLoader.LoadTree<SustainSFX>();
		AssetLookupTree<HitSFX> assetLookupTree11 = assetLookupSystem.TreeLoader.LoadTree<HitSFX>();
		SplinePayload payload5 = assetLookupTree6.GetPayload(assetLookupSystem.Blackboard);
		SourceOffsetPayload payload6 = assetLookupTree7.GetPayload(assetLookupSystem.Blackboard);
		SinkOffsetPayload payload7 = assetLookupTree8.GetPayload(assetLookupSystem.Blackboard);
		BirthSFX payload8 = assetLookupTree9.GetPayload(assetLookupSystem.Blackboard);
		SustainSFX payload9 = assetLookupTree10.GetPayload(assetLookupSystem.Blackboard);
		HitSFX payload10 = assetLookupTree11.GetPayload(assetLookupSystem.Blackboard);
		Transform projectileTransform = new GameObject("Resource Projectile: " + mana.Color).transform;
		projectileTransform.position = sourcePos;
		if (payload6 != null)
		{
			projectileTransform.position += payload6.Offset;
		}
		SplineEventData splineEventData = new SplineEventData();
		if (payload8 != null)
		{
			splineEventData.Events.Add(new SplineEventAudio(0f, payload8.SfxData.AudioEvents, projectileTransform.gameObject));
		}
		else
		{
			splineEventData.Events.Add(new SplineEventAudio(0f, new List<AudioEvent>
			{
				new AudioEvent(WwiseEvents.sfx_mana_swish.EventName)
			}, projectileTransform.gameObject));
		}
		if (payload != null)
		{
			foreach (VfxData vfxData in payload.VfxDatas)
			{
				splineEventData.Events.Add(new SplineEventPayloadVFXALT(vfxData, null, projectileTransform, gameManager.VfxProvider));
			}
		}
		if (payload9 != null)
		{
			splineEventData.Events.Add(new SplineEventAudio(0f, payload9.SfxData.AudioEvents, projectileTransform.gameObject));
		}
		if (payload2 != null)
		{
			foreach (VfxData vfxData2 in payload2.VfxDatas)
			{
				splineEventData.Events.Add(new SplineEventPayloadVFXALT(vfxData2, null, projectileTransform, gameManager.VfxProvider));
			}
		}
		if (hashSet.Count > 0)
		{
			projectileTransform.gameObject.AddComponent<AkStopAllOnDisable>();
			Transform entityCardTransform = GetEntityCardTransform(gameManager.ViewManager.GetEntity(sinkEntity.InstanceId), gameManager);
			foreach (SinkHitVFX item in hashSet)
			{
				if (item.SfxData != null)
				{
					_ = projectileTransform;
					splineEventData.Events.Add(new SplineEventAudio(1f, item.SfxData.AudioEvents, projectileTransform.gameObject));
				}
			}
			foreach (SinkHitVFX item2 in hashSet)
			{
				foreach (VfxData vfxData3 in item2.VfxDatas)
				{
					splineEventData.Events.Add(new SplineEventPayloadVFXALT(vfxData3, null, entityCardTransform, gameManager.VfxProvider));
				}
			}
		}
		if (payload4 != null)
		{
			projectileTransform.gameObject.AddComponent<AkStopAllOnDisable>();
			Transform entityCardTransform2 = GetEntityCardTransform(gameManager.ViewManager.GetEntity(sourceEntity.InstanceId), gameManager);
			if (payload4.SfxData != null)
			{
				_ = projectileTransform;
				splineEventData.Events.Add(new SplineEventAudio(1f, payload4.SfxData.AudioEvents, projectileTransform.gameObject));
			}
			foreach (VfxData vfxData4 in payload4.VfxDatas)
			{
				splineEventData.Events.Add(new SplineEventPayloadVFXALT(vfxData4, null, entityCardTransform2, gameManager.VfxProvider));
			}
		}
		if (payload10 != null)
		{
			projectileTransform.gameObject.AddComponent<AkStopAllOnDisable>();
			Transform transform = projectileTransform;
			if (payload3 != null && payload3.VfxDatas.Count > 0)
			{
				transform = gameManager.VfxProvider.ResolveSpaceIntoTransforms(payload3.VfxDatas[0].SpaceData.Space, null, transform, payload3.VfxDatas[0].PlayOnStackChildren).FirstOrDefault();
			}
			splineEventData.Events.Add(new SplineEventAudio(1f, payload10.SfxData.AudioEvents, transform.gameObject));
		}
		else
		{
			splineEventData.Events.Add(new SplineEventCallback(1f, delegate
			{
				AudioManager.SetSwitch("color", AudioManager.GetColorKey(mana.Color), projectileTransform.gameObject);
				AudioManager.PlayAudio(WwiseEvents.sfx_mana_hit, projectileTransform.gameObject);
				AudioManager.PlayAudio(WwiseEvents.sfx_mana_payout, projectileTransform.gameObject);
			}));
		}
		if (payload3 != null)
		{
			foreach (VfxData vfxData5 in payload3.VfxDatas)
			{
				splineEventData.Events.Add(new SplineEventPayloadVFXALT(vfxData5, null, projectileTransform, gameManager.VfxProvider));
			}
		}
		ISplineMovementSystem splineSystem = gameManager.SplineMovementSystem;
		splineEventData.Events.Add(new SplineEventCallback(1f, delegate
		{
			onComplete?.Invoke();
			projectileTransform.gameObject.AddOrGetComponent<SelfCleanup>().SetLifetime(0.1f, SelfCleanup.CleanupType.Destroy, onlyWhenChildless: true);
			splineSystem.RemoveTemporaryGoal(projectileTransform);
		}));
		IdealPoint endPoint = new IdealPoint(destPos, Quaternion.identity, Vector3.one);
		if (payload7 != null)
		{
			endPoint.Position += payload7.Offset;
		}
		SplineMovementData splineMovementData = null;
		string text = payload5?.SplineDataRef.RelativePath;
		if (!string.IsNullOrEmpty(text))
		{
			splineMovementData = gameManager.SplineCache.Get(text);
		}
		if (splineMovementData == null)
		{
			splineMovementData = ScriptableObject.CreateInstance<SplineMovementData>();
			splineMovementData.Spline = SplineData.Parabolic;
		}
		splineSystem.AddTemporaryGoal(projectileTransform, endPoint, allowInteractions: false, splineMovementData, splineEventData);
	}

	private static Transform GetEntityCardTransform(IEntityView entity, GameManager gameManager)
	{
		if (entity is DuelScene_CDC duelScene_CDC && gameManager.ViewManager.TryGetCardView(duelScene_CDC.InstanceId, out var cardView) && cardView.IsVisible)
		{
			return cardView.EffectsRoot;
		}
		return null;
	}
}
