using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using Pooling;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class AbilityHangerView : MonoBehaviour
{
	private const uint RELAYOUT_JIGGLE_COUNT = 3u;

	[SerializeField]
	private AbilityHangerItem _itemPrefab;

	[SerializeField]
	private AbilityHangerItem _NPEitemPrefab;

	[SerializeField]
	private Color _defaultColor = Color.white;

	[SerializeField]
	private Color _addedColor = new Color(47f / 85f, 0.7647059f, 1f);

	[SerializeField]
	private Color _perpetualColor = new Color(0.77254903f, 0.49803922f, 1f);

	[SerializeField]
	private Color _cardStyleColor = new Color(39f / 85f, 0.77254903f, 0.9372549f);

	[HideInInspector]
	[SerializeField]
	public AssetReference QuoteBadgeReference = new AssetReference();

	[HideInInspector]
	[SerializeField]
	public AssetReference CardStyleBadgeReference = new AssetReference();

	[Header("Children")]
	[SerializeField]
	private VerticalLayoutGroup _layoutGroup;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private GameObject _scrollEllipsis;

	[SerializeField]
	private Transform _pointerLeft;

	[SerializeField]
	private Transform _pointerRight;

	[Header("Auto Scrolling")]
	[SerializeField]
	private bool _isDragScroll = true;

	[SerializeField]
	private GraphicRaycaster _manualScrollRaycaster;

	[SerializeField]
	private float _scrollDelay = 1f;

	[SerializeField]
	private float _scrollSpeed = 20f;

	private Coroutine _scrollCoroutine;

	private Coroutine _delayRebuild;

	private float _scrollSize;

	private readonly List<AbilityHangerItem> _activeHangerItems = new List<AbilityHangerItem>(10);

	private readonly Dictionary<string, AbilityHangerItem> _abilityHangerCache = new Dictionary<string, AbilityHangerItem>(10);

	private uint _pendingLayoutCalls;

	private IUnityObjectPool _unityObjectPool;

	private AssetLookupSystem _assetLookupSystem;

	private IClientLocProvider _locManager;

	public const int HANGER_SECTION_CARDSTYLE = -5;

	public const int HANGER_SECTION_TYPE = -2;

	public const int HANGER_SECTION_COUNTER = -1;

	public const int HANGER_SECTION_QUOTE = 5;

	public List<AbilityHangerItem> HangerItems => _activeHangerItems;

	private void OnDisable()
	{
		if (_scrollCoroutine != null)
		{
			StopCoroutine(_scrollCoroutine);
			_scrollCoroutine = null;
		}
		if (_delayRebuild != null)
		{
			StopCoroutine(_delayRebuild);
			_delayRebuild = null;
		}
	}

	private void OnDestroy()
	{
		CleanupAllHangers();
		_unityObjectPool = null;
		foreach (AbilityHangerItem value in _abilityHangerCache.Values)
		{
			Object.Destroy(value.gameObject);
		}
		_abilityHangerCache.Clear();
	}

	private IEnumerator DelayRebuild()
	{
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_layoutGroup.transform);
	}

	private void LateUpdate()
	{
		if (_pendingLayoutCalls == 0)
		{
			return;
		}
		_pendingLayoutCalls--;
		if (_scrollRect != null && _scrollEllipsis != null)
		{
			_scrollSize = _scrollRect.content.rect.size.y - _scrollRect.viewport.rect.size.y;
			if (_scrollSize > 0f)
			{
				_scrollEllipsis.gameObject.SetActive(value: true);
				if (_scrollCoroutine == null)
				{
					_scrollCoroutine = StartCoroutine(AutoScroll());
				}
				if (_isDragScroll && _manualScrollRaycaster != null)
				{
					_manualScrollRaycaster.enabled = true;
				}
			}
			else
			{
				_scrollSize = 0f;
				_scrollEllipsis.gameObject.SetActive(value: false);
				if (_scrollCoroutine != null)
				{
					StopCoroutine(_scrollCoroutine);
					_scrollCoroutine = null;
				}
				if (_manualScrollRaycaster != null)
				{
					_manualScrollRaycaster.enabled = false;
				}
			}
		}
		if (_delayRebuild == null)
		{
			_delayRebuild = StartCoroutine(DelayRebuild());
		}
	}

	public void Init(IUnityObjectPool unityObjectPool, IClientLocProvider localizationManager, AssetLookupSystem assetLookupSystem)
	{
		_unityObjectPool = unityObjectPool;
		_locManager = localizationManager;
		_assetLookupSystem = assetLookupSystem;
	}

	public void SetDragScroll(bool isDragScroll)
	{
		_isDragScroll = isDragScroll;
		if (isDragScroll && _scrollRect.verticalScrollbar != null)
		{
			_scrollRect.verticalScrollbar.targetGraphic.enabled = true;
		}
	}

	public void SetHangerSide(HangerSide side)
	{
		if (_pointerLeft != null && _pointerRight != null)
		{
			_pointerLeft.gameObject.SetActive(side == HangerSide.Right);
			_pointerRight.gameObject.SetActive(side == HangerSide.Left);
		}
	}

	public void CreateHangerQuote(string quote, int section = 0)
	{
		if (!string.IsNullOrEmpty(QuoteBadgeReference.RelativePath) && quote.StartsWith("\""))
		{
			quote = quote.Substring(1);
		}
		CreateHangerItem("", "", quote, QuoteBadgeReference.RelativePath, convertSymbols: false, section, addedItem: false, useNPEBattlefieldItem: false, perpetualItem: false, useAddendumMarkup: true);
	}

	public void CreateHangerCardStyle()
	{
		if (!string.IsNullOrEmpty(CardStyleBadgeReference.RelativePath))
		{
			CreateHangerItem(_locManager.GetLocalizedText("AbilityHanger/SpecialHangers/CardStyle/Header"), _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/CardStyle/Body"), null, CardStyleBadgeReference.RelativePath, _cardStyleColor, convertSymbols: false, -5);
		}
	}

	public void CreateHangerItem(string header, bool convertHeaderSymbols, string body, bool convertBodySymbols, string addendum, bool convertAddendumSymbols, string badgePath = null, int section = 0, bool addedItem = false, bool useNPEBattlefieldItem = false, bool perpetualItem = false)
	{
		if (GetHangerItem(header, body, out var hangerItem, useNPEBattlefieldItem))
		{
			hangerItem.SetText(header, body, addendum, convertHeaderSymbols, convertBodySymbols, convertAddendumSymbols);
			hangerItem.SetBadge(badgePath);
			hangerItem.SetColor(perpetualItem ? _perpetualColor : (addedItem ? _addedColor : _defaultColor));
			AddHangerItem(hangerItem, section);
		}
	}

	public void CreateHangerItem(string header, string body, string addendum, string badgePath = null, bool convertSymbols = true, int section = 0, bool addedItem = false, bool useNPEBattlefieldItem = false, bool perpetualItem = false, bool useAddendumMarkup = false)
	{
		if (GetHangerItem(header, body, out var hangerItem, useNPEBattlefieldItem))
		{
			hangerItem.SetText(header, body, addendum, convertSymbols, useAddendumMarkup);
			hangerItem.SetBadge(badgePath);
			hangerItem.SetColor(perpetualItem ? _perpetualColor : (addedItem ? _addedColor : _defaultColor));
			AddHangerItem(hangerItem, section);
		}
	}

	public void CreateHangerItem(string header, string body, string addendum, string badgePath, Color color, bool convertSymbols = true, int section = 0, bool useNPEBattlefieldItem = false, bool useAddendumMarkup = false)
	{
		if (GetHangerItem(header, body, out var hangerItem, useNPEBattlefieldItem))
		{
			hangerItem.SetText(header, body, addendum, convertSymbols, useAddendumMarkup);
			hangerItem.SetBadge(badgePath);
			hangerItem.SetColor((color != default(Color)) ? color : _defaultColor);
			AddHangerItem(hangerItem, section);
		}
	}

	public void CreateHangerItem(string header, string body, string addendum, string badgePath, IEntityView arrowThisEntityView, IEntityView arrowThatEntityView, Transform parent, bool arrowPointsToThat = true, bool useAddendumMarkup = false)
	{
		if (GetHangerItem(header, body, out var hangerItem))
		{
			hangerItem.SetText(header, body, addendum, convertSymbols: true, useAddendumMarkup);
			hangerItem.SetBadge(badgePath);
			hangerItem.SetColor(_defaultColor);
			hangerItem.SetArrow(arrowThisEntityView, arrowThatEntityView, parent, arrowPointsToThat);
			AddHangerItem(hangerItem, 0);
		}
	}

	private bool GetHangerItem(string header, string body, out AbilityHangerItem hangerItem, bool useNPEBattlefieldItem = false)
	{
		string key = header + ": " + body;
		AbilityHangerItem abilityHangerItem = (useNPEBattlefieldItem ? _NPEitemPrefab : _itemPrefab);
		if (!_abilityHangerCache.TryGetValue(key, out hangerItem))
		{
			if (IsDuplicateOfCounterHanger(body))
			{
				return false;
			}
			hangerItem = Object.Instantiate(abilityHangerItem.gameObject).GetComponent<AbilityHangerItem>();
			_abilityHangerCache[key] = hangerItem;
		}
		else
		{
			if (_activeHangerItems.Contains(hangerItem))
			{
				return false;
			}
			hangerItem.gameObject.SetActive(value: true);
		}
		hangerItem.transform.SetParent(_layoutGroup.transform);
		hangerItem.Init(_assetLookupSystem, _unityObjectPool);
		return true;
	}

	private void AddHangerItem(AbilityHangerItem hangerItem, int section)
	{
		int i = 0;
		int num = 0;
		for (; i < _activeHangerItems.Count && section >= _activeHangerItems[i].Section; i++)
		{
			if (section == _activeHangerItems[i].Section)
			{
				num++;
			}
		}
		int count = num + 1;
		hangerItem.UpdateStyle(num, count);
		if (num > 0)
		{
			_activeHangerItems[i - 1].UpdateStyle(num - 1, count);
		}
		hangerItem.Section = section;
		hangerItem.gameObject.SetActive(value: true);
		hangerItem.transform.ZeroOut();
		hangerItem.transform.SetSiblingIndex(i);
		_activeHangerItems.Insert(i, hangerItem);
		_pendingLayoutCalls = 3u;
	}

	private bool IsDuplicateOfCounterHanger(string body)
	{
		foreach (AbilityHangerItem activeHangerItem in _activeHangerItems)
		{
			string bodyText = activeHangerItem.BodyText;
			if (activeHangerItem.Section == -1 && !string.IsNullOrWhiteSpace(bodyText) && string.Equals(body, bodyText))
			{
				return true;
			}
		}
		return false;
	}

	public void CleanupAllHangers()
	{
		while (_activeHangerItems.Count > 0)
		{
			CleanupHangerItem(_activeHangerItems[0]);
		}
	}

	private void CleanupHangerItem(AbilityHangerItem hangerItem)
	{
		if (_activeHangerItems.Contains(hangerItem))
		{
			_activeHangerItems.Remove(hangerItem);
		}
		hangerItem.Cleanup();
		hangerItem.gameObject.SetActive(value: false);
	}

	private IEnumerator AutoScroll()
	{
		float scrollPosition = 1f;
		_scrollRect.verticalNormalizedPosition = scrollPosition;
		yield return new WaitForSeconds(_scrollDelay);
		while (scrollPosition > 0f)
		{
			scrollPosition -= _scrollSpeed * Time.deltaTime / _scrollSize;
			_scrollRect.verticalNormalizedPosition = ((scrollPosition < 0.5f) ? (2f * scrollPosition * scrollPosition) : (-1f + (4f - 2f * scrollPosition) * scrollPosition));
			yield return null;
		}
		_scrollCoroutine = null;
	}

	public void StopAutoScroll()
	{
		if (_isDragScroll && _scrollCoroutine != null)
		{
			StopCoroutine(_scrollCoroutine);
		}
	}
}
