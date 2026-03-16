using System.Collections;
using DG.Tweening;
using GreClient.CardData;
using MTGA.KeyboardManager;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;

public class View_CardRolloverZoom : CardRolloverZoomBase
{
	[SerializeField]
	private float _waitSeconds = 1f;

	[SerializeField]
	private string _zoomLayerName;

	private float _xSpacingBetweenZoomCards = 0.1f;

	[SerializeField]
	private RectTransform _screenBoundsRect;

	[SerializeField]
	private AbilityHangerBase _abilityHanger;

	[SerializeField]
	private FaceHanger _faceHanger;

	[SerializeField]
	private float _faceHangerScale = 0.1f;

	[SerializeField]
	private bool _animateCards = true;

	[Tooltip("This is old. Use Screen Bounds Rect instead. Keeping to maintain any stragglers.")]
	private Vector2 _screenBounds = new Vector2(8.755f, 4.86f);

	private Bounds _lastRolloverCardColliderBounds;

	private Vector2 _lastRolloverOffset;

	private Vector3[] _lastBoundsPoints;

	public override void Initialize(CardViewBuilder cardViewBuilder, CardDatabase cardDatabase, IClientLocProvider locManager, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, KeyboardManager keyboardManager, DeckFormat currentEventFormat)
	{
		_unityObjectPool = unityObjectPool;
		_cardViewBuilder = cardViewBuilder;
		_cardDatabase = cardDatabase;
		_keyboardManager = keyboardManager;
		_keyboardManager?.Subscribe(this);
		_cardParent = new GameObject("CardParent").transform;
		_cardParent.SetParent(base.transform);
		_cardParent.ZeroOut();
		_cardParent.localPosition = Vector3.zero;
		_zoomCard = CreateCardView(_cardParent, _zoomLayerName);
		if (_screenBoundsRect == null)
		{
			_screenBoundsRect = GetComponentInParent<Canvas>()?.transform as RectTransform;
		}
		IFaceInfoGenerator faceInfoGenerator = null;
		if ((bool)_faceHanger)
		{
			faceInfoGenerator = FaceInfoGeneratorFactory.HoverGenerator(_cardDatabase, _cardViewBuilder.AssetLookupSystem, genericObjectPool);
		}
		if ((bool)_abilityHanger)
		{
			Canvas component = _abilityHanger.GetComponent<Canvas>();
			if ((bool)component)
			{
				component.overrideSorting = true;
				component.sortingOrder = 2;
			}
			_abilityHanger.gameObject.SetLayer(LayerMask.NameToLayer("Zoom"));
			_abilityHanger.transform.SetParent(_cardParent, worldPositionStays: false);
			_abilityHanger.Init(_cardDatabase, _cardViewBuilder.AssetLookupSystem, _unityObjectPool, genericObjectPool, faceInfoGenerator, locManager, currentEventFormat);
		}
		if ((bool)_faceHanger)
		{
			Canvas component2 = _faceHanger.GetComponent<Canvas>();
			if ((bool)component2)
			{
				component2.overrideSorting = true;
				component2.sortingOrder = 2;
			}
			_faceHanger.gameObject.SetLayer(LayerMask.NameToLayer("Zoom"));
			Transform obj = _faceHanger.transform;
			obj.SetParent(_cardParent);
			obj.localScale = Vector3.one * _faceHangerScale;
			_faceHanger.Init(faceInfoGenerator, _cardViewBuilder);
		}
	}

	public override bool CardRolledOver(ICardDataAdapter model, Bounds cardColliderBounds, HangerSituation hangerSituation = default(HangerSituation), Vector2 offset = default(Vector2))
	{
		if (_rolloverCoroutine != null)
		{
			StopCoroutine(_rolloverCoroutine);
		}
		if (model != null)
		{
			base.OnRolloverStart?.Invoke(model);
			_lastRolloverModel = model;
			_lastRolloverCardColliderBounds = cardColliderBounds;
			_lastHangerSituation = hangerSituation;
			_lastRolloverOffset = offset;
			_rolloverCoroutine = StartCoroutine(Coroutine_RolloverWait());
			return true;
		}
		return false;
	}

	public override void CardRolledOff(ICardDataAdapter model, bool alwaysRollOff = false)
	{
		if (_lastRolloverModel == model || alwaysRollOff)
		{
			base.OnRolloff?.Invoke(_zoomCard);
			_lastRolloverModel = null;
			_cardParent.gameObject.SetActive(value: false);
			if (_rolloverCoroutine != null)
			{
				StopCoroutine(_rolloverCoroutine);
			}
			if ((bool)_abilityHanger)
			{
				_abilityHanger.DeactivateHanger();
			}
			if ((bool)_faceHanger)
			{
				_faceHanger.DeactivateHanger();
			}
		}
	}

	public override void CardPointerDown(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null, HangerSituation hangerSituation = default(HangerSituation))
	{
	}

	public override void CardPointerUp(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null)
	{
		if (inputButton == PointerEventData.InputButton.Right && _faceHanger != null)
		{
			_faceHanger.ShowNextFace();
		}
	}

	public override bool CardScrolled(Vector2 scrollDelta)
	{
		bool flag = false;
		if ((bool)_zoomCard)
		{
			foreach (CDCPart_TextBox_Rules item in _zoomCard.FindAllParts<CDCPart_TextBox_Rules>(AnchorPointType.Invalid))
			{
				flag |= item.ScrollTextbox(scrollDelta);
			}
		}
		if ((bool)_abilityHanger)
		{
			flag |= _abilityHanger.HandleScroll(scrollDelta);
		}
		if ((bool)_faceHanger)
		{
			flag |= _faceHanger.HandleScroll(scrollDelta);
		}
		return flag;
	}

	public override void Close()
	{
		base.Close();
		if ((bool)_cardParent)
		{
			_cardParent.gameObject.SetActive(value: false);
		}
		if ((bool)_abilityHanger)
		{
			_abilityHanger.DeactivateHanger();
		}
		if ((bool)_faceHanger)
		{
			_faceHanger.DeactivateHanger();
		}
	}

	private IEnumerator Coroutine_RolloverWait()
	{
		while (!base.IsActive)
		{
			yield return new WaitForEndOfFrame();
		}
		if (_waitSeconds > 0f)
		{
			yield return new WaitForSeconds(_waitSeconds);
		}
		_cardParent.gameObject.SetActive(value: true);
		_cardParent.transform.localScale = Vector3.one;
		PrepareZoomCard();
		Vector2 cardSize = GetCardSize(_zoomCard);
		Vector2 vector = new Vector2(cardSize.x, Mathf.Max(Vector2.zero.y, cardSize.y));
		DecorateLastHangerSituation();
		if ((bool)_faceHanger)
		{
			_faceHanger.ActivateHanger(_zoomCard, _lastRolloverModel, _lastHangerSituation);
		}
		if ((bool)_abilityHanger)
		{
			_abilityHanger.ActivateHanger(_zoomCard, _lastRolloverModel, _lastHangerSituation);
		}
		bool flag = (bool)_faceHanger && _faceHanger.Active;
		bool flag2 = (bool)_abilityHanger && _abilityHanger.Active;
		Bounds screenBounds = GetScreenBounds();
		DebugDrawBounds(screenBounds, Color.red, 3);
		Bounds cardBounds = _lastRolloverCardColliderBounds;
		DebugDrawBounds(cardBounds, new Color(0f, 0.25f, 1f), 2, fromLastBounds: true);
		cardBounds.size = _cardParent.parent.TransformVector(vector);
		DebugDrawBounds(cardBounds, new Color(0f, 0.75f, 1f), 4, fromLastBounds: true);
		cardBounds.center += Vector3.Scale(cardBounds.extents + _lastRolloverCardColliderBounds.extents, _lastRolloverOffset);
		DebugDrawBounds(cardBounds, new Color(0.5f, 0.5f, 0f), 2, fromLastBounds: true);
		float num = vector.x;
		bool flag3 = _lastRolloverModel.IsSplitCard();
		if (flag3)
		{
			num *= 0.5f;
		}
		else if (_zoomCard.ActiveScaffold.Shape == ScaffoldShape.Horizontal)
		{
			if (flag)
			{
				num = _faceHanger.GetHangerWidth();
			}
			else if (flag2)
			{
				num = _abilityHanger.GetHangerWidth();
			}
		}
		float cardHalfWidth = num * 0.5f;
		float totalCardHalfWidth = vector.x * 0.5f;
		Vector3 adjustedPosition = cardBounds.center;
		PositionRollover(flag, ref cardBounds, screenBounds, ref adjustedPosition, flag3);
		if (!flag && flag2)
		{
			PositionSingleHanger(_abilityHanger, cardHalfWidth, totalCardHalfWidth, adjustedPosition, screenBounds);
		}
		else if (flag && !flag2)
		{
			PositionSingleHanger(_faceHanger, cardHalfWidth, totalCardHalfWidth, adjustedPosition, screenBounds);
		}
		else if (flag && flag2)
		{
			PositionBothHangers(cardHalfWidth, totalCardHalfWidth, adjustedPosition, screenBounds, num);
		}
		DebugDrawBounds(cardBounds, new Color(0f, 1f, 0.25f), 1, fromLastBounds: true);
		Transform transform = _cardParent.transform;
		transform.position = cardBounds.center;
		if (_animateCards)
		{
			AnimateRolloverZoom(transform, cardBounds);
		}
		_rolloverCoroutine = null;
		yield return StartCoroutine(AnimateIfAnimatedCardBack());
	}

	private void PrepareZoomCard()
	{
		_zoomCard.gameObject.SetActive(value: true);
		_zoomCard.IsMousedOver = true;
		if (base.OnRollover != null)
		{
			base.OnRollover(_zoomCard);
		}
		if (_zoomCard.Model != _lastRolloverModel)
		{
			_zoomCard.SetModel(_lastRolloverModel, updateVisuals: true, CardHolderType.RolloverZoom);
		}
		_zoomCard.SetDimmed(null);
		_zoomCard.ImmediateUpdate();
	}

	private void PositionRollover(bool faceHanger, ref Bounds cardBounds, Bounds screenBounds, ref Vector3 adjustedPosition, bool splitCard)
	{
		Bounds bounds = cardBounds;
		if (faceHanger)
		{
			float x = (splitCard ? 1.5f : 2f);
			Vector3 center = (splitCard ? (cardBounds.center + new Vector3(cardBounds.extents.x / 2f, 0f, 0f)) : (cardBounds.center + new Vector3(cardBounds.extents.x, 0f, 0f)));
			Vector3 size = Vector3.Scale(cardBounds.size, new Vector3(x, 1f, 1f)) + new Vector3(_xSpacingBetweenZoomCards, 0f, 0f);
			bounds = new Bounds(center, size);
		}
		if (bounds.min.x < screenBounds.min.x)
		{
			adjustedPosition.x += screenBounds.min.x - bounds.min.x;
		}
		else if (bounds.max.x > screenBounds.max.x)
		{
			adjustedPosition.x += screenBounds.max.x - bounds.max.x;
		}
		if (bounds.min.y < screenBounds.min.y)
		{
			adjustedPosition.y += screenBounds.min.y - bounds.min.y;
		}
		else if (bounds.max.y > screenBounds.max.y)
		{
			adjustedPosition.y += screenBounds.max.y - bounds.max.y;
		}
		Vector3 vector = (faceHanger ? new Vector3(cardBounds.extents.x, 0f, 0f) : Vector3.zero);
		Bounds bounds2 = new Bounds(adjustedPosition + vector, bounds.size);
		if (bounds2.Intersects(_lastRolloverCardColliderBounds))
		{
			if (screenBounds.center.x < bounds2.center.x)
			{
				adjustedPosition.x += _lastRolloverCardColliderBounds.min.x - bounds2.max.x;
				if (adjustedPosition.x - cardBounds.extents.x < screenBounds.min.x)
				{
					adjustedPosition.x = screenBounds.min.x + cardBounds.extents.x;
				}
			}
			else
			{
				adjustedPosition.x += _lastRolloverCardColliderBounds.max.x - bounds2.min.x;
				if (adjustedPosition.x + cardBounds.extents.x > screenBounds.max.x)
				{
					adjustedPosition.x = screenBounds.max.x - cardBounds.extents.x;
				}
			}
		}
		adjustedPosition.z = _cardParent.transform.position.z;
		cardBounds.center = adjustedPosition;
		DebugDrawBounds(cardBounds, new Color(0.25f, 1f, 0.75f), 2, fromLastBounds: true);
	}

	private void PositionSingleHanger(HangerBase hanger, float cardHalfWidth, float totalCardHalfWidth, Vector3 adjustedPosition, Bounds screenBounds)
	{
		float num = cardHalfWidth + totalCardHalfWidth + _xSpacingBetweenZoomCards;
		Bounds bounds = new Bounds(adjustedPosition + new Vector3(num, 0f, 0f), new Vector3(2f * cardHalfWidth + _xSpacingBetweenZoomCards, 1f, 0f));
		bool flag = bounds.max.x >= screenBounds.max.x || _lastRolloverCardColliderBounds.Intersects(bounds);
		if (flag)
		{
			num *= -1f;
		}
		hanger.IsDisplayedOnLeftSide = flag;
		Vector3 localPosition = Vector3.right * num;
		hanger.transform.localPosition = localPosition;
	}

	private void PositionBothHangers(float cardHalfWidth, float totalCardHalfWidth, Vector3 adjustedPosition, Bounds screenBounds, float cardWidth)
	{
		float num = cardHalfWidth + totalCardHalfWidth + _xSpacingBetweenZoomCards;
		Bounds bounds = new Bounds(adjustedPosition + new Vector3(num, 0f, 0f), new Vector3(2f * cardHalfWidth + _xSpacingBetweenZoomCards, 1f, 0f));
		bool flag = bounds.max.x >= screenBounds.max.x || _lastRolloverCardColliderBounds.Intersects(bounds);
		float num2 = num + cardWidth + _xSpacingBetweenZoomCards;
		float num3 = num2 + cardHalfWidth + _xSpacingBetweenZoomCards * 0.5f;
		bool flag2 = adjustedPosition.x + num3 >= screenBounds.max.x;
		if (flag)
		{
			num *= -1f;
			num2 *= -1f;
		}
		else if (flag2)
		{
			num2 = 0f - num;
		}
		float num4 = num2 - cardHalfWidth + _xSpacingBetweenZoomCards * 0.5f;
		num3 = num2 + cardHalfWidth + _xSpacingBetweenZoomCards * 0.5f;
		if (adjustedPosition.x + num3 >= screenBounds.max.x || num4 <= screenBounds.min.x)
		{
			num2 = 0f - num;
		}
		Vector3 localPosition = Vector3.right * num;
		_faceHanger.IsDisplayedOnLeftSide = flag;
		_faceHanger.transform.localPosition = localPosition;
		Vector3 localPosition2 = Vector3.right * num2;
		_abilityHanger.IsDisplayedOnLeftSide = flag2;
		_abilityHanger.transform.localPosition = localPosition2;
	}

	private void AnimateRolloverZoom(Transform cardParentTransform, Bounds cardBounds)
	{
		DOTween.Kill(cardParentTransform);
		if (_lastRolloverOffset.x > 0f)
		{
			cardParentTransform.Translate(new Vector3(-0.3f, 0f, 0f));
		}
		else if (_lastRolloverOffset.x < 0f)
		{
			cardParentTransform.Translate(new Vector3(0.3f, 0f, 0f));
		}
		else if (_lastRolloverOffset.y < 0f)
		{
			cardParentTransform.Translate(new Vector3(0f, 0.3f, 0f));
		}
		else
		{
			cardParentTransform.Translate(new Vector3(0f, -0.3f, 0f));
		}
		cardParentTransform.DOMove(cardBounds.center, 0.35f);
		cardParentTransform.localScale = Vector3.one * 0.85f;
		cardParentTransform.DOScale(Vector3.one, 0.25f);
		if (_zoomCard.gameObject.activeInHierarchy)
		{
			DOTween.Kill(_zoomCard.transform);
			Transform transform = _zoomCard.transform;
			transform.DOLocalMove(transform.localPosition, 0.15f);
			transform.localPosition = Vector3.Lerp(Vector3.zero, transform.localPosition, 0.5f);
		}
		if ((bool)_faceHanger && _faceHanger.Active)
		{
			DOTween.Kill(_faceHanger.transform);
			Transform transform2 = _faceHanger.transform;
			transform2.DOLocalMove(transform2.localPosition, 0.15f);
			transform2.localPosition = Vector3.Lerp(Vector3.zero, transform2.localPosition, 0.5f);
		}
		if ((bool)_abilityHanger && _abilityHanger.Active)
		{
			DOTween.Kill(_abilityHanger.transform);
			Transform transform3 = _abilityHanger.transform;
			transform3.DOLocalMove(transform3.localPosition, 0.15f);
			transform3.localPosition = Vector3.Lerp(Vector3.zero, transform3.localPosition, 0.5f);
		}
		_zoomCard.GetSleeveFXPayload(_lastRolloverModel, CardHolderType.None, out var sleeveFXPayload, out var prefabFilePath);
		if (sleeveFXPayload != null && prefabFilePath != null)
		{
			GameObject gameObject = _unityObjectPool.PopObject(prefabFilePath);
			gameObject.transform.SetParent(_zoomCard.EffectsRoot);
			gameObject.transform.localPosition = sleeveFXPayload.OffsetData.PositionOffset;
			gameObject.transform.localEulerAngles = sleeveFXPayload.OffsetData.RotationOffset;
			gameObject.transform.localScale = sleeveFXPayload.OffsetData.ScaleMultiplier;
			gameObject.AddOrGetComponent<SelfCleanup>().SetLifetime(sleeveFXPayload.CleanUpAfterSeconds);
			AudioManager.PlayAudio(sleeveFXPayload.AudioEvent, gameObject);
		}
	}

	private IEnumerator AnimateIfAnimatedCardBack()
	{
		CDCPart_AnimatedCardback cardBack = _zoomCard.FindPart<CDCPart_AnimatedCardback>(AnchorPointType.AnimatedCardback);
		if (cardBack != null)
		{
			cardBack.DisableBoxCollider();
			yield return new WaitForEndOfFrame();
			cardBack.SetHoverAnimation();
		}
		CDCPart_ControllerAnimatedCardback animatedCardback = _zoomCard.gameObject.GetComponentInChildren<CDCPart_ControllerAnimatedCardback>();
		if (animatedCardback != null)
		{
			yield return new WaitForEndOfFrame();
			animatedCardback.PlayAnimations();
		}
	}

	private static Vector2 GetCardSize(Meta_CDC cardView)
	{
		return cardView.ActiveScaffold.GetColliderBounds.size;
	}

	private Bounds GetScreenBounds()
	{
		if ((bool)_screenBoundsRect)
		{
			return _screenBoundsRect.GetBounds();
		}
		Transform parent = _cardParent.parent;
		return new Bounds(parent.position, parent.TransformVector(_screenBounds) * 0.5f);
	}

	private void DebugDrawBounds(Bounds bounds, Color color, int thickness = 1, bool fromLastBounds = false)
	{
	}

	protected void OnDestroy()
	{
		Cleanup();
	}

	public override void Cleanup()
	{
		_faceHanger.Cleanup();
		base.Cleanup();
	}
}
