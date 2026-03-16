using System;
using AssetLookupTree;
using GreClient.CardData;
using MovementSystem;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;

public class HangerController : IDisposable, IUpdate
{
	private readonly struct HangerPositionDetails
	{
		public readonly Vector3 CardPosition;

		public readonly Vector3 CardLeft;

		public readonly Vector3 CardRight;

		public readonly float CardWidth;

		public readonly Vector3 FrustumLeftPoint;

		public readonly Vector3 FrustumRightPoint;

		public readonly Bounds HoveredBounds;

		public float HalfCardWidth => CardWidth * 0.5f;

		public HangerPositionDetails(Vector3 cardPosition, Vector3 cardLeft, Vector3 cardRight, float cardWidth, Vector3 frustumLeftPoint, Vector3 frustumRightPoint, Bounds hoveredBounds)
		{
			CardPosition = cardPosition;
			CardLeft = cardLeft;
			CardRight = cardRight;
			CardWidth = cardWidth;
			FrustumLeftPoint = frustumLeftPoint;
			FrustumRightPoint = frustumRightPoint;
			HoveredBounds = hoveredBounds;
		}
	}

	private FaceHanger _faceHanger;

	private AbilityHanger _abilityHanger;

	private Camera _mainCamera;

	private ISplineMovementSystem _splineMovementSystem;

	private float _spaceBetweenHangers = 0.25f;

	private float _abilityScale = 1f;

	private Transform _referenceTransform;

	private const float ACTIVATION_TIME_DEFAULT = 0f;

	private const float ACTIVATION_TIME_DELAY = 0.5f;

	private const float PREVENT_OVERLAP_OFFSET = 0.1f;

	private float _activationTime = 1f;

	private float _activationTimer;

	public DuelScene_CDC ActiveCard { get; private set; }

	private HangerSituation _situation { get; set; }

	public void Init(Transform parent, Camera mainCamera, ISplineMovementSystem splineMovementSystem, FaceHanger faceHanger, AbilityHanger abilityHanger, float spaceBetweenHangers = 0.25f, float abilityScale = 1f)
	{
		_mainCamera = mainCamera;
		_splineMovementSystem = splineMovementSystem;
		_spaceBetweenHangers = spaceBetweenHangers;
		_abilityScale = abilityScale;
		_referenceTransform = new GameObject("FaceHangerReferenceTransform").transform;
		_referenceTransform.parent = parent;
		_faceHanger = faceHanger;
		if (_faceHanger != null)
		{
			_faceHanger.gameObject.UpdateActive(active: false);
		}
		_abilityHanger = abilityHanger;
		if (_abilityHanger != null)
		{
			_abilityHanger.gameObject.UpdateActive(active: false);
		}
	}

	public void InitFaceHanger(IFaceInfoGenerator faceInfoGenerator, CardViewBuilder cardViewBuilder)
	{
		if (!(_faceHanger == null))
		{
			_faceHanger.Init(faceInfoGenerator, cardViewBuilder);
		}
	}

	public void InitAbilityHanger(Transform hangerParent, IContext context, AssetLookupSystem assetLookupSystem, IFaceInfoGenerator faceInfoGenerator, DeckFormat eventFormat, NPEDirector npeDirector)
	{
		if (!(_abilityHanger == null))
		{
			_abilityHanger.Init(hangerParent, context, assetLookupSystem, faceInfoGenerator, eventFormat, npeDirector);
		}
	}

	public void ShowHangersForCard(DuelScene_CDC cardView, ICardDataAdapter sourceModel, HangerSituation situation)
	{
		if (ActiveCard != null)
		{
			ClearHangers();
		}
		if (!cardView.Model.IsDisplayedFaceDown || cardView.Model.Instance.FaceDownState.IsFaceDown)
		{
			ActiveCard = cardView;
			_situation = situation;
			_faceHanger.ActivateHanger(cardView, sourceModel, _situation, delayShow: true);
			_abilityHanger.ActivateHanger(cardView, sourceModel, _situation, delayShow: true);
			_activationTimer = 0f;
			_activationTime = (situation.DelayActivation ? 0.5f : 0f);
		}
	}

	public void ShowNextFaceHanger()
	{
		if (_faceHanger.isActiveAndEnabled)
		{
			_faceHanger.ShowNextFace();
		}
	}

	public void ClearHangers()
	{
		ActiveCard = null;
		if ((bool)_faceHanger)
		{
			_faceHanger.DeactivateHanger();
		}
		if ((bool)_abilityHanger)
		{
			_abilityHanger.DeactivateHanger();
		}
	}

	public void OnUpdate(float timeStep)
	{
		if (ActiveCard == null)
		{
			ClearHangers();
		}
		else if (ActiveCard.IsVisible)
		{
			if (_activationTimer < _activationTime)
			{
				_activationTimer += timeStep;
			}
			if (_activationTimer >= _activationTime || ShouldForceUpdateHangers())
			{
				UpdateHangersActive();
			}
			LayoutNow();
		}
	}

	private bool ShouldForceUpdateHangers()
	{
		if ((bool)_faceHanger)
		{
			return _faceHanger.gameObject.activeSelf != _faceHanger.Active;
		}
		if ((bool)_abilityHanger)
		{
			return _abilityHanger.gameObject.activeSelf != _abilityHanger.Active;
		}
		return false;
	}

	private void UpdateHangersActive()
	{
		if ((bool)_faceHanger)
		{
			_faceHanger.gameObject.UpdateActive(_faceHanger.Active);
		}
		if ((bool)_abilityHanger)
		{
			_abilityHanger.gameObject.UpdateActive(_abilityHanger.Active);
		}
	}

	public bool LayoutNow()
	{
		IdealPoint goal = _splineMovementSystem.GetGoal(ActiveCard.transform);
		_referenceTransform.position = goal.Position;
		_referenceTransform.rotation = goal.Rotation;
		_referenceTransform.localScale = goal.Scale;
		Vector3 intersectionPoint;
		Vector3 cameraCenterPoint;
		Rect rect = _mainCamera.FrustumAtPoint(_referenceTransform, out intersectionPoint, out cameraCenterPoint);
		HangerPositionDetails positionDetails = new HangerPositionDetails(_referenceTransform.position, _referenceTransform.TransformDirection(Vector3.left), _referenceTransform.TransformDirection(Vector3.right), ((ActiveCard.ActiveScaffold != null) ? ActiveCard.ActiveScaffold.GetColliderBounds.size.x : 2f) * _referenceTransform.lossyScale.x, cameraCenterPoint + Vector3.right * rect.width * 0.49f, cameraCenterPoint + Vector3.left * rect.width * 0.49f, _situation.HoveredCardBounds);
		if (_abilityHanger.Active && !_faceHanger.Active)
		{
			return TryPositionAbilityHanger(_abilityHanger, positionDetails, _abilityScale);
		}
		if (_faceHanger.Active && !_abilityHanger.Active)
		{
			return TryPositionFaceHanger(positionDetails);
		}
		if (_abilityHanger.Active && _faceHanger.Active)
		{
			return TryPositionBothHangers(positionDetails);
		}
		return true;
	}

	private bool TryPositionAbilityHanger(HangerBase hanger, HangerPositionDetails positionDetails, float customScale = 1f)
	{
		ApplyHangerTransform(hanger, customScale);
		float num = hanger.GetHangerWidth() * 0.5f;
		float num2 = HangerOffset(positionDetails.HalfCardWidth, num);
		if (positionDetails.HoveredBounds == default(Bounds))
		{
			bool flag = positionDetails.CardPosition.x - num2 - num < positionDetails.FrustumRightPoint.x;
			bool result = !flag;
			if (flag)
			{
				num2 *= -1f;
			}
			hanger.IsDisplayedOnLeftSide = flag;
			hanger.transform.position = positionDetails.CardPosition + positionDetails.CardRight * num2;
			return result;
		}
		if (positionDetails.CardPosition.x < positionDetails.HoveredBounds.center.x)
		{
			return TryPositionHanger(hanger, positionDetails);
		}
		if (positionDetails.CardPosition.x + num2 + num > positionDetails.FrustumLeftPoint.x)
		{
			_abilityHanger.IsDisplayedOnLeftSide = false;
			_abilityHanger.transform.position = positionDetails.CardPosition + positionDetails.CardRight * num2;
		}
		else
		{
			_abilityHanger.IsDisplayedOnLeftSide = true;
			_abilityHanger.transform.position = positionDetails.CardPosition - positionDetails.CardRight * num2;
		}
		return true;
	}

	private bool TryPositionFaceHanger(HangerPositionDetails positionDetails)
	{
		ApplyHangerTransform(_faceHanger);
		return TryPositionHanger(_faceHanger, positionDetails);
	}

	private bool TryPositionBothHangers(HangerPositionDetails positionDetails)
	{
		ApplyHangerTransform(_faceHanger);
		float num = _faceHanger.GetHangerWidth() * 0.5f;
		float num2 = HangerOffset(positionDetails.HalfCardWidth, num);
		bool result = TryPositionHanger(_faceHanger, positionDetails);
		ApplyHangerTransform(_abilityHanger, _abilityScale);
		float num3 = _abilityHanger.GetHangerWidth() * 0.5f;
		if (!_faceHanger.IsDisplayedOnLeftSide)
		{
			float num4 = num2 + num + _spaceBetweenHangers + num3;
			if (!(positionDetails.CardPosition.x - num4 - num3 < positionDetails.FrustumRightPoint.x))
			{
				_abilityHanger.IsDisplayedOnLeftSide = false;
				_abilityHanger.transform.position = positionDetails.CardPosition + positionDetails.CardRight * num4;
				return result;
			}
			num4 = positionDetails.HalfCardWidth + _spaceBetweenHangers + num3;
			_abilityHanger.IsDisplayedOnLeftSide = true;
			_abilityHanger.transform.position = positionDetails.CardPosition - positionDetails.CardRight * num4;
			return result;
		}
		float num5 = Mathf.Abs(num2) + num + _spaceBetweenHangers + num3;
		if (!(positionDetails.CardPosition.x + num5 + num3 > positionDetails.FrustumLeftPoint.x))
		{
			_abilityHanger.IsDisplayedOnLeftSide = true;
			_abilityHanger.transform.position = positionDetails.CardPosition - positionDetails.CardRight * num5;
			return result;
		}
		num5 = positionDetails.HalfCardWidth + _spaceBetweenHangers + num3;
		_abilityHanger.IsDisplayedOnLeftSide = false;
		_abilityHanger.transform.position = positionDetails.CardPosition + positionDetails.CardRight * num5;
		return result;
	}

	private void ApplyHangerTransform(HangerBase hanger, float scaleMultiplier = 1f)
	{
		hanger.transform.rotation = _referenceTransform.rotation;
		hanger.transform.localScale = _referenceTransform.lossyScale * 0.1f * scaleMultiplier;
	}

	private float HangerOffset(float halfCardWidth, float hangerHalfWidth)
	{
		return halfCardWidth + _spaceBetweenHangers + hangerHalfWidth;
	}

	private bool HangerOutsideCameraFrustum(float hangerEdgeR, HangerPositionDetails positionDetails)
	{
		return hangerEdgeR < positionDetails.FrustumRightPoint.x;
	}

	private bool HangerOverlapsBattlefieldCDC(float hangerEdgeR, float hangerEdgeL, HangerPositionDetails positionDetails)
	{
		if (positionDetails.HoveredBounds != default(Bounds))
		{
			Bounds hoveredBounds = positionDetails.HoveredBounds;
			float num = hoveredBounds.center.x - hoveredBounds.extents.x;
			float num2 = hoveredBounds.center.x + hoveredBounds.extents.x;
			if (!(hangerEdgeR < num) || !(hangerEdgeL > num))
			{
				if (hangerEdgeR < num2)
				{
					return hangerEdgeL > num2;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool TryPositionHanger(HangerBase hanger, HangerPositionDetails details, float scaleMultiplier = 1f)
	{
		ApplyHangerTransform(hanger, scaleMultiplier);
		float num = hanger.GetHangerWidth() * 0.5f;
		float num2 = HangerOffset(details.HalfCardWidth, num);
		float hangerEdgeR = details.CardPosition.x - num2 - num;
		float hangerEdgeL = details.CardPosition.x - num2 + num;
		bool num3 = HangerOutsideCameraFrustum(hangerEdgeR, details);
		if (num3 || HangerOverlapsBattlefieldCDC(hangerEdgeR, hangerEdgeL, details))
		{
			num2 *= -1f;
			hanger.IsDisplayedOnLeftSide = true;
		}
		else
		{
			hanger.IsDisplayedOnLeftSide = false;
		}
		hanger.transform.position = details.CardPosition + details.CardRight * num2;
		return !num3;
	}

	public void Scroll(Vector2 delta)
	{
		_faceHanger.HandleScroll(delta);
		_abilityHanger.HandleScroll(delta);
	}

	public void Dispose()
	{
		if ((bool)_faceHanger)
		{
			_faceHanger.Cleanup();
			UnityEngine.Object.Destroy(_faceHanger);
		}
		if ((bool)_abilityHanger)
		{
			_abilityHanger.Shutdown();
			UnityEngine.Object.Destroy(_abilityHanger);
		}
	}
}
