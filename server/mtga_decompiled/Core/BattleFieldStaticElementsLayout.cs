using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.PlayerNameViews;

public class BattleFieldStaticElementsLayout : MonoBehaviour
{
	protected Camera _camera;

	[Header("Opponent")]
	[SerializeField]
	protected RectTransform _opponentAvatarContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentAvatarLayoutData;

	[SerializeField]
	protected RectTransform _opponentManaPoolContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentManaPoolLayoutData;

	[SerializeField]
	protected RectTransform _opponentCounterPoolContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentCounterPoolLayoutData;

	[SerializeField]
	protected RectTransform _opponentHandContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentHandLayoutData;

	[SerializeField]
	protected RectTransform _opponentGraveyardContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentGraveyardLayoutData;

	[SerializeField]
	protected RectTransform _opponentExileContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentExileLayoutData;

	[SerializeField]
	protected RectTransform _opponentLibraryContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentLibraryLayoutData;

	[SerializeField]
	protected RectTransform _opponentFrameContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentFrameLayoutData;

	[SerializeField]
	protected RectTransform _opponentHpContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentHpLayoutData;

	[SerializeField]
	protected RectTransform _opponentTurnFrameContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentTurnFrameLayoutData;

	[SerializeField]
	protected RectTransform _opponentCommandContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentCommandLayoutData;

	[SerializeField]
	protected RectTransform _opponentRankAnchorPoint;

	[SerializeField]
	protected RectTransform _opponentUserNameAnchorPoint;

	[SerializeField]
	protected RectTransform _opponentTimeoutDisplayAnchorPoint;

	[SerializeField]
	protected RectTransform _opponentMatchTimerAnchorPoint;

	[SerializeField]
	protected RectTransform _opponentWinPipsAnchorPoint;

	[Header("Local Player")]
	[SerializeField]
	protected RectTransform _localAvatarContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localAvatarLayoutData;

	[SerializeField]
	protected RectTransform _localManaPoolContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localManaPoolLayoutData;

	[SerializeField]
	protected RectTransform _localCounterPoolContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localCounterPoolLayoutData;

	[SerializeField]
	protected RectTransform _localHandContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localHandLayoutData;

	[SerializeField]
	protected RectTransform _localGraveyardContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localGraveyardLayoutData;

	[SerializeField]
	protected RectTransform _localExileContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localExileLayoutData;

	[SerializeField]
	protected RectTransform _localLibraryContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localLibraryLayoutData;

	[SerializeField]
	protected RectTransform _localFrameContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localFrameLayoutData;

	[SerializeField]
	protected RectTransform _localHpContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localHpLayoutData;

	[SerializeField]
	protected RectTransform _localTurnFrameContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localTurnFrameLayoutData;

	[SerializeField]
	protected RectTransform _localCommandContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localCommandLayoutData;

	[SerializeField]
	protected RectTransform _stackContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _stackLayoutData;

	[SerializeField]
	protected RectTransform _promptButtonsAnchorPoint;

	[SerializeField]
	protected RectTransform _localRankAnchorPoint;

	[SerializeField]
	protected RectTransform _localUserNameAnchorPoint;

	[SerializeField]
	protected RectTransform _localTimeoutDisplayAnchorPoint;

	[SerializeField]
	protected RectTransform _localMatchTimerAnchorPoint;

	[SerializeField]
	protected RectTransform _localWinPipsAnchorPoint;

	[SerializeField]
	protected RectTransform _fullControlAnchorPoint;

	private readonly Dictionary<ICardHolder, Transform> _cardHolders = new Dictionary<ICardHolder, Transform>();

	private readonly List<(DuelScene_AvatarView avatar, GREPlayerNum owner)> _players = new List<(DuelScene_AvatarView, GREPlayerNum)>();

	protected virtual void Awake()
	{
		_localAvatarLayoutData = _localAvatarContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localManaPoolLayoutData = _localManaPoolContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localCounterPoolLayoutData = _localCounterPoolContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localHandLayoutData = _localHandContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_stackLayoutData = _stackContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localGraveyardLayoutData = _localGraveyardContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localExileLayoutData = _localExileContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localLibraryLayoutData = _localLibraryContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localFrameLayoutData = _localFrameContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localHpLayoutData = _localHpContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localTurnFrameLayoutData = _localTurnFrameContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_localCommandLayoutData = _localCommandContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentAvatarLayoutData = _opponentAvatarContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentManaPoolLayoutData = _opponentManaPoolContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentCounterPoolLayoutData = _opponentCounterPoolContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentHandLayoutData = _opponentHandContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentGraveyardLayoutData = _opponentGraveyardContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentExileLayoutData = _opponentExileContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentLibraryLayoutData = _opponentLibraryContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentFrameLayoutData = _opponentFrameContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentHpLayoutData = _opponentHpContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentTurnFrameLayoutData = _opponentTurnFrameContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentCommandLayoutData = _opponentCommandContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		if (ScreenEventController.Instance != null)
		{
			ScreenEventController.Instance.OnScreenChanged += RefreshRegisteredElements;
		}
	}

	public void SetCamera(Camera cam)
	{
		_camera = cam;
	}

	private void OnDestroy()
	{
		if (ScreenEventController.Instance != null)
		{
			ScreenEventController.Instance.OnScreenChanged -= RefreshRegisteredElements;
		}
		_players.Clear();
		_cardHolders.Clear();
	}

	public void AddCardHolder(ICardHolder cardHolder)
	{
		if (cardHolder is CardHolderBase { transform: var transform })
		{
			_cardHolders[cardHolder] = transform;
			StartCoroutine(UpdateCardHolderPosition(transform, cardHolder.CardHolderType, cardHolder.PlayerNum));
		}
	}

	public void RemoveCardHolder(ICardHolder cardHolder)
	{
		if (_cardHolders.Remove(cardHolder))
		{
			RefreshRegisteredElements();
		}
	}

	public void RegisterAvatar(DuelScene_AvatarView avatar, GREPlayerNum type)
	{
		_players.Add((avatar, type));
		StartCoroutine(UpdateAvatarPosition(avatar, type));
	}

	private void RefreshRegisteredElements()
	{
		foreach (var player in _players)
		{
			StartCoroutine(UpdateAvatarPosition(player.avatar, player.owner, refresh: true));
		}
		foreach (KeyValuePair<ICardHolder, Transform> cardHolder in _cardHolders)
		{
			ICardHolder key = cardHolder.Key;
			Transform value = cardHolder.Value;
			StartCoroutine(UpdateCardHolderPosition(value, key.CardHolderType, key.PlayerNum, refresh: true));
		}
	}

	public virtual IEnumerator UpdateCardHolderPosition(Transform cardHolder, CardHolderType cardHolderType, GREPlayerNum owner, bool refresh = false)
	{
		yield break;
	}

	public virtual IEnumerator UpdateAvatarPosition(DuelScene_AvatarView avatar, GREPlayerNum owner, bool refresh = false)
	{
		yield break;
	}

	public virtual IEnumerator UpdatePromptButtonsAnchorPosition(RectTransform promptButtonsRect)
	{
		yield break;
	}

	public virtual IEnumerator UpdateUserNamesAnchorPosition(IReadOnlyList<PlayerNameViewData> playerNames)
	{
		yield break;
	}

	public virtual IEnumerator UpdateFullControlAnchorPosition(RectTransform fullControlRect)
	{
		yield break;
	}

	public virtual IEnumerator UpdateTimerPositions(TimerManager timerManager)
	{
		yield break;
	}

	protected void UpdateWorldSpaceElementPosition(Transform targetTransform, RectTransform layoutRect, BattleFieldStaticLayoutWorldSpaceElementData layoutElementData)
	{
		Vector3 position = _camera.transform.position;
		Vector3 normalized = (GetRectWorldCenter(layoutRect) - position).normalized;
		OffsetData offsets = layoutElementData.Offsets;
		targetTransform.position = GetLineIntersectionWithXzPlane(position, normalized, layoutElementData.TargetPlaneY);
		targetTransform.localPosition += offsets.PositionOffset;
		if (offsets.RotationIsWorld)
		{
			targetTransform.rotation *= Quaternion.Euler(offsets.RotationOffset);
		}
		else
		{
			targetTransform.localRotation *= Quaternion.Euler(offsets.RotationOffset);
		}
		if (offsets.ScaleIsWorld)
		{
			Vector3 lossyScale = targetTransform.lossyScale;
			targetTransform.localScale = new Vector3(offsets.ScaleMultiplier.x / lossyScale.x, offsets.ScaleMultiplier.y / lossyScale.y, offsets.ScaleMultiplier.z / lossyScale.z);
		}
		else
		{
			Vector3 localScale = targetTransform.localScale;
			targetTransform.localScale = new Vector3(localScale.x * offsets.ScaleMultiplier.x, localScale.y * offsets.ScaleMultiplier.y, localScale.z * offsets.ScaleMultiplier.z);
		}
	}

	protected void UpdateScreenSpaceElementPosition(RectTransform targetTransform, RectTransform layoutRect, bool isTargetScreenSpaceOverlay = false)
	{
		RectTransform component = targetTransform.parent.GetComponent<RectTransform>();
		if (component == null)
		{
			Debug.LogError("RectTransform not found in parent!");
			return;
		}
		Vector2 rectLocalPositionInAnotherRect = GetRectLocalPositionInAnotherRect(layoutRect, isFromRectOverlay: false, component, isTargetScreenSpaceOverlay);
		targetTransform.localPosition = rectLocalPositionInAnotherRect;
	}

	private static Vector3 GetRectWorldCenter(RectTransform rect)
	{
		Vector3[] array = new Vector3[4];
		rect.GetWorldCorners(array);
		return (array[0] + array[2]) * 0.5f;
	}

	private static Vector3 GetLineIntersectionWithXzPlane(Vector3 lineOrigin, Vector3 lineDirection, float planeY)
	{
		float num = (planeY - lineOrigin.y) / lineDirection.y;
		return lineOrigin + num * lineDirection;
	}

	private Vector2 GetRectLocalPositionInAnotherRect(RectTransform fromRect, bool isFromRectOverlay, RectTransform toRect, bool isToRectOverlay)
	{
		Vector2 vector = new Vector2(fromRect.rect.width * fromRect.pivot.x + fromRect.rect.xMin, fromRect.rect.height * fromRect.pivot.y + fromRect.rect.yMin);
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(isFromRectOverlay ? null : _camera, fromRect.position);
		screenPoint += vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(toRect, screenPoint, isToRectOverlay ? null : _camera, out var localPoint);
		Vector2 vector2 = new Vector2(toRect.rect.width * toRect.pivot.x + toRect.rect.xMin, toRect.rect.height * toRect.pivot.y + toRect.rect.yMin);
		return localPoint + toRect.anchoredPosition - vector2;
	}
}
