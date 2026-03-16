using AssetLookupTree;
using GreClient.CardData;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Unity;

public class AbilityHangerItem : MonoBehaviour
{
	private AssetLoader.AssetTracker<Sprite> _badgeTracker = new AssetLoader.AssetTracker<Sprite>("AbilityHangerItemBadge");

	[SerializeField]
	private Image _badgeAnchor;

	[SerializeField]
	private TMP_Text _titleLabel;

	[SerializeField]
	private TMP_Text _definitionLabel;

	[SerializeField]
	private TMP_Text _addendumLabel;

	[SerializeField]
	private Image _backgroundImage;

	[SerializeField]
	private string _linkArrowId = "Link";

	[Header("Hanger Styling")]
	[SerializeField]
	private int _bottomPadding = 5;

	[SerializeField]
	private Color _evenColor;

	[SerializeField]
	private Color _oddColor;

	[SerializeField]
	private Sprite _topSprite;

	[SerializeField]
	private Sprite _midSprite;

	[SerializeField]
	private Sprite _botSprite;

	[SerializeField]
	private Sprite _topBotSprite;

	private FromEntityIntentionBase _arrowMediator;

	private IUnityObjectPool _unityObjectPool;

	private AssetLookupSystem _assetLookupSystem;

	private LayoutGroup _layoutGroup;

	private int _defaultPadding;

	private bool _rebuildLayout;

	public int Section { get; set; }

	public string TitleText { get; private set; }

	public string BodyText { get; private set; }

	public string AddendumText { get; private set; }

	private void Awake()
	{
		_layoutGroup = GetComponent<LayoutGroup>();
		if (_layoutGroup != null)
		{
			_defaultPadding = _layoutGroup.padding.bottom;
		}
		Cleanup();
	}

	private void Update()
	{
		if (_rebuildLayout)
		{
			_rebuildLayout = false;
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		}
	}

	public void Init(AssetLookupSystem assetLookupSystem, IUnityObjectPool unityObjectPool)
	{
		_assetLookupSystem = assetLookupSystem;
		_unityObjectPool = unityObjectPool;
	}

	public void SetText(string header, string body, string addendum, bool convertHeaderSymbols = true, bool convertBodySymbols = true, bool convertAddendumSymbols = true, bool useAddendumMarkup = false)
	{
		TitleText = (convertHeaderSymbols ? ManaUtilities.ConvertManaSymbols(header) : header);
		BodyText = (convertBodySymbols ? ManaUtilities.ConvertManaSymbols(body) : body);
		AddendumText = (convertAddendumSymbols ? ManaUtilities.ConvertManaSymbols(addendum) : addendum);
		if (!string.IsNullOrEmpty(TitleText))
		{
			_titleLabel.gameObject.SetActive(value: true);
			_titleLabel.SetText(TitleText);
		}
		if (!string.IsNullOrEmpty(BodyText))
		{
			_definitionLabel.gameObject.SetActive(value: true);
			_definitionLabel.SetText(BodyText);
		}
		if (!string.IsNullOrEmpty(AddendumText))
		{
			_addendumLabel.gameObject.SetActive(value: true);
			_addendumLabel.fontStyle = ((!useAddendumMarkup) ? FontStyles.Italic : FontStyles.Normal);
			_addendumLabel.SetText(AddendumText);
		}
		base.name = header + ": " + body;
		_rebuildLayout = true;
	}

	public void SetText(string header, string body, string addendum, bool convertSymbols = true, bool useAddendumMarkup = false)
	{
		SetText(header, body, addendum, convertSymbols, convertSymbols, convertSymbols, useAddendumMarkup);
	}

	public void SetBadge(string badgePath)
	{
		if (_badgeTracker.LastPath != badgePath)
		{
			_badgeAnchor.sprite = _badgeTracker.Acquire(badgePath);
			_badgeAnchor.gameObject.SetActive(_badgeAnchor.sprite != null);
			_rebuildLayout = true;
		}
	}

	private void ClearBadge()
	{
		_badgeAnchor.sprite = null;
		_badgeTracker.Cleanup();
		_badgeAnchor.gameObject.SetActive(value: false);
	}

	public void SetColor(Color color)
	{
		_titleLabel.color = color;
		_definitionLabel.color = color;
		_badgeAnchor.color = color;
	}

	public void SetArrow(IEntityView thisEntityView, IEntityView thatEntityView, Transform parent, bool arrowPointsToThat = true)
	{
		EraseLinkArrow();
		if (thisEntityView == null || thatEntityView == null)
		{
			return;
		}
		DreamteckIntentionArrowBehavior dreamteckIntentionArrowBehavior = IntentionLineUtils.CreateIntentionLine(_assetLookupSystem, _linkArrowId, _unityObjectPool);
		if ((object)dreamteckIntentionArrowBehavior != null)
		{
			dreamteckIntentionArrowBehavior.gameObject.GetComponent<Transform>().SetParent(parent, worldPositionStays: true);
			if (arrowPointsToThat)
			{
				_arrowMediator = new ToEntityFromSpellStackIntention().Init(thisEntityView, thatEntityView);
			}
			else
			{
				_arrowMediator = new ToSpellStackFromEntityIntention().Init(thatEntityView, thisEntityView);
			}
			_arrowMediator.ArrowBehavior = dreamteckIntentionArrowBehavior;
		}
	}

	public void Cleanup()
	{
		_titleLabel.gameObject.SetActive(value: false);
		_definitionLabel.gameObject.SetActive(value: false);
		_addendumLabel.gameObject.SetActive(value: false);
		ClearBadge();
		EraseLinkArrow();
	}

	public void UpdateStyle(int index, int count)
	{
		if (!(_backgroundImage == null))
		{
			bool flag = index == 0;
			bool flag2 = index == count - 1;
			bool flag3 = index % 2 == 1;
			_backgroundImage.color = (flag3 ? _oddColor : _evenColor);
			if (flag && flag2)
			{
				SetBackground(_topBotSprite, _bottomPadding);
			}
			else if (flag2)
			{
				SetBackground(_botSprite, _bottomPadding);
			}
			else if (flag)
			{
				SetBackground(_topSprite);
			}
			else
			{
				SetBackground(_midSprite);
			}
		}
	}

	private void SetBackground(Sprite sprite, float padding = 0f)
	{
		_backgroundImage.sprite = sprite;
		_backgroundImage.rectTransform.offsetMin = new Vector2(0f, padding);
		if (_layoutGroup != null)
		{
			_layoutGroup.padding.bottom = _defaultPadding + (int)padding;
		}
	}

	private void EraseLinkArrow()
	{
		if (_arrowMediator != null)
		{
			if (_arrowMediator.ArrowBehavior != null)
			{
				_arrowMediator.ArrowBehavior.gameObject.UpdateActive(active: false);
				_unityObjectPool.PushObject(_arrowMediator.ArrowBehavior.gameObject);
				_arrowMediator.ArrowBehavior = null;
			}
			_arrowMediator = null;
		}
	}
}
