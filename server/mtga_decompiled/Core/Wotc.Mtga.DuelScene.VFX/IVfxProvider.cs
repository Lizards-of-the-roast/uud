using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using MovementSystem;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public interface IVfxProvider
{
	IReadOnlyCollection<SplineEvent> GenerateEtbSplineEvents(DuelScene_CDC cardView, IBattlefieldStack stack, bool playHitEffects, Transform localSpaceOverride = null);

	void GenerateEtbTriggerEvents(DuelScene_CDC cardView, List<SplineEvent> events, IBattlefieldStack stack, Transform localSpaceOverride = null);

	GameObject PlayVFX(VfxData vfxData, ICardDataAdapter effectContext, MtgEntity spaceContext = null, Transform localSpaceOverride = null, string prefabPath = null);

	GameObject PlayAnchoredVFX(VfxData vfxData, AnchorPointType anchorType, ScaffoldingBase scaffold, ICardDataAdapter effectContext, MtgEntity spaceContext = null, Transform localSpaceOverride = null);

	IEnumerable<Transform> ResolveSpaceIntoTransforms(RelativeSpace space, MtgEntity spaceContext, Transform localSpaceOverride = null, bool resolveForStackChildren = false);
}
