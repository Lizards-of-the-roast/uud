using System;
using System.Collections;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class ListMetaCardView_Expanding : MetaCardView
{
	public enum TagDisplayType
	{
		Default,
		SideboardA,
		SideboardB,
		Companion
	}

	[Serializable]
	public class TagDisplayKeyValuePair
	{
		public TagDisplayType Key;

		public GameObject Value;
	}

	[SerializeField]
	private TMP_Text _quantityText;

	[SerializeField]
	private TMP_Text _nameText;

	[SerializeField]
	private TMP_Text _manaCostPart;

	[SerializeField]
	private Image _frameImage;

	[SerializeField]
	private Image _raycastImage;

	[SerializeField]
	private GameObject _highlightObject;

	[SerializeField]
	private GameObject _pinLineObject;

	[SerializeField]
	private GameObject _selectedTileObject;

	[SerializeField]
	private GameObject _selectedTagObject;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private LayoutElement _layoutElement;

	[SerializeField]
	private CustomButton _tagButton;

	[SerializeField]
	private CustomButton _tileButton;

	private ListMetaCardView_Expanding _draggingCard;

	private Animator _cardTileAnimator;

	private Animator _cardTagAnimator;

	private Animator _unownedAnimator;

	private Animator _disabledAnimator;

	[SerializeField]
	private TagDisplayKeyValuePair[] _tagDisplayObjects;

	private static readonly int Pulse = Animator.StringToHash("Pulse");

	private static readonly int NotOwned = Animator.StringToHash("NotOwned");

	private static readonly int Invalid = Animator.StringToHash("Invalid");

	private static readonly int Disabled = Animator.StringToHash("Disabled");

	private static readonly int Red = Animator.StringToHash("Red");

	private static readonly int Cosmetic = Animator.StringToHash("Cosmetic");

	private bool _styleModeEnabled;

	public int Quantity { get; private set; }

	public string SkinCode => base.Card?.Instance?.SkinCode;

	public ListMetaCardViewDisplayInformation DisplayInformation { get; set; }

	public GameObject HighlightObject => _highlightObject;

	public GameObject PinLineObject => _pinLineObject;

	public GameObject SelectedTileObject => _selectedTileObject;

	public GameObject SelectedTagObject => _selectedTagObject;

	public CanvasGroup CanvasGroup => _canvasGroup;

	public LayoutElement LayoutElement => _layoutElement;

	public CustomButton TagButton => _tagButton;

	public CustomButton TileButton => _tileButton;

	public Action<MetaCardView> OnAddClicked { get; set; }

	public Action<MetaCardView> OnRemoveClicked { get; set; }

	public void SetQuantity(int quantity)
	{
		if (Quantity > 0 && quantity > 0)
		{
			if (quantity > Quantity)
			{
				_cardTileAnimator.SetTrigger(Pulse);
			}
			else if (Quantity > quantity)
			{
				_cardTagAnimator.SetTrigger(Pulse);
			}
		}
		Quantity = quantity;
		_quantityText.text = quantity + "x";
	}

	private void OnEnable()
	{
		StartCoroutine(Coroutine_UpdateAnimators());
	}

	private IEnumerator Coroutine_UpdateAnimators()
	{
		yield return null;
		if (_unownedAnimator.isActiveAndEnabled)
		{
			_unownedAnimator.SetBool(NotOwned, ShowUnCollectedTreatment || ShowBannedTreatment);
			_unownedAnimator.SetBool(Invalid, ShowInvalidTreatment);
		}
		if (_disabledAnimator.isActiveAndEnabled)
		{
			_disabledAnimator.SetBool(Disabled, ShowUnCollectedTreatment || ShowBannedTreatment || ShowDisabledTreatment);
			_disabledAnimator.SetBool(Red, value: false);
		}
		if (_cardTileAnimator.isActiveAndEnabled)
		{
			_cardTileAnimator.SetBool(Cosmetic, _styleModeEnabled && SkinCode != null);
		}
	}

	public void UpdateTreatment()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(Coroutine_UpdateAnimators());
		}
	}

	public void SetName(string name)
	{
		_nameText.text = CardUtilities.RemoveHiragana(name);
	}

	public void SetNameColor(Color color)
	{
		_nameText.color = color;
	}

	public void SetManaCost(string manaCost)
	{
		_manaCostPart.SetText(manaCost);
	}

	public void SetFrameSprite(Sprite frameSprite)
	{
		_frameImage.sprite = frameSprite;
	}

	private void Awake()
	{
		_tagButton.OnClick.AddListener(TagButton_OnClick);
		_tileButton.OnClick.AddListener(TileButton_OnClick);
		_cardTagAnimator = _tagButton.GetComponent<Animator>();
		_cardTileAnimator = _tileButton.GetComponent<Animator>();
		_unownedAnimator = _tagButton.GetComponent<Animator>();
		_disabledAnimator = _tileButton.GetComponent<Animator>();
	}

	public void SetColliderEnabled(bool enabled)
	{
		_raycastImage.raycastTarget = enabled;
	}

	protected override void UpdateHighlight()
	{
		if (base.Holder == null || base.Holder.CustomHighlightHandler == null)
		{
			Default_HighlightHandler();
		}
		else
		{
			base.Holder.CustomHighlightHandler(this, base.IsMouseOver, base.IsMouseDown, base.IsDragging);
		}
	}

	private void Default_HighlightHandler()
	{
		bool active = false;
		if (base.Holder == null || base.Holder.ShowHighlight(this))
		{
			if (base.IsMouseOver && base.IsMouseDown)
			{
				active = !base.IsDragging;
			}
			else if (base.IsMouseOver)
			{
				active = true;
			}
			else if (base.IsMouseDown)
			{
				active = false;
			}
		}
		if (_highlightObject != null)
		{
			_highlightObject.UpdateActive(active);
		}
	}

	protected override Bounds GetBounds()
	{
		return _raycastImage.rectTransform.GetBounds();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
	}

	public void EventMouseDown()
	{
		base.IsMouseDown = true;
		if (base.Holder != null && base.Holder.RolloverZoomView != null)
		{
			base.Holder.RolloverZoomView.CardPointerDown(PointerEventData.InputButton.Left, base.VisualCard, this, HangerSituation);
			return;
		}
		CardZoomTrigger component = GetComponent<CardZoomTrigger>();
		if (component != null && component.ZoomView != null)
		{
			component.ZoomView.CardPointerDown(PointerEventData.InputButton.Left, base.VisualCard, this, HangerSituation);
		}
	}

	public void EventRightClick()
	{
		if (base.Holder != null && base.Holder.RolloverZoomView != null)
		{
			base.Holder.RolloverZoomView.CardPointerUp(PointerEventData.InputButton.Right, base.VisualCard, this);
			return;
		}
		CardZoomTrigger component = GetComponent<CardZoomTrigger>();
		if (component != null && component.ZoomView != null)
		{
			component.ZoomView.CardPointerUp(PointerEventData.InputButton.Right, base.VisualCard, this);
		}
	}

	public void EventMouseUp()
	{
		base.IsMouseDown = false;
		if (base.Holder != null && base.Holder.RolloverZoomView != null)
		{
			base.Holder.RolloverZoomView.CardPointerUp(PointerEventData.InputButton.Left, base.VisualCard, this);
			return;
		}
		CardZoomTrigger component = GetComponent<CardZoomTrigger>();
		if (component != null && component.ZoomView != null)
		{
			GetComponent<CardZoomTrigger>().ZoomView.CardPointerUp(PointerEventData.InputButton.Left, base.VisualCard, this);
		}
	}

	protected override void BeginDragCard(PointerEventData eventData)
	{
		base.IsDragging = true;
		_draggingCard = UnityEngine.Object.Instantiate(this, GetComponentInParent<Canvas>().transform, worldPositionStays: true);
		_draggingCard._canvasGroup.interactable = false;
		_draggingCard._canvasGroup.blocksRaycasts = false;
		RectTransform component = GetComponent<RectTransform>();
		RectTransform component2 = _draggingCard.GetComponent<RectTransform>();
		component2.anchoredPosition = new Vector2(0.5f, 0.5f);
		component2.anchorMin = new Vector2(0.5f, 0.5f);
		component2.anchorMax = new Vector2(0.5f, 0.5f);
		component2.sizeDelta = component.sizeDelta;
		component2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, component.rect.width);
		component2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, component.rect.height);
		component2.position = component.position;
		component2.localScale = component.localScale;
		StoreDragFields(eventData, _draggingCard.transform);
		_canvasGroup.alpha = 0f;
	}

	protected override void DragCard()
	{
		ApplyDragFields(_draggingCard.transform);
	}

	protected override void EndDragCard()
	{
		base.IsDragging = false;
		if (!(_draggingCard == null))
		{
			UnityEngine.Object.Destroy(_draggingCard.gameObject);
			_draggingCard = null;
			_canvasGroup.alpha = 1f;
		}
	}

	private void TagButton_OnClick()
	{
		OnAddClicked?.Invoke(this);
	}

	private void TileButton_OnClick()
	{
		OnRemoveClicked?.Invoke(this);
	}

	public void SetDisplayInformation(ListMetaCardViewDisplayInformation displayInformation)
	{
		DisplayInformation = displayInformation;
		HangerSituation = new HangerSituation
		{
			ContextualHangers = displayInformation.ContextualHangers
		};
		ShowUnCollectedTreatment = displayInformation.Unowned;
		ShowBannedTreatment = displayInformation.Banned;
		ShowInvalidTreatment = displayInformation.Invalid;
		SetQuantity((int)displayInformation.Quantity);
	}

	public void SetTagDisplayType(TagDisplayType target)
	{
		TagDisplayKeyValuePair[] tagDisplayObjects = _tagDisplayObjects;
		foreach (TagDisplayKeyValuePair tagDisplayKeyValuePair in tagDisplayObjects)
		{
			tagDisplayKeyValuePair.Value.SetActive(tagDisplayKeyValuePair.Key == target);
		}
	}

	public override void SetSelected(bool isSelected)
	{
		if (_selectedTileObject != null)
		{
			_selectedTileObject.SetActive(isSelected);
		}
	}

	public void SetTagHighlight(bool isHighlighted)
	{
		_selectedTagObject.gameObject.SetActive(isHighlighted);
	}

	public void SetDisabled(bool isDisabled)
	{
		if (ShowDisabledTreatment != isDisabled)
		{
			ShowDisabledTreatment = isDisabled;
			StartCoroutine(Coroutine_UpdateAnimators());
		}
	}

	public void SetStyleMode()
	{
		_tagButton.gameObject.UpdateActive(active: false);
		_nameText.enableAutoSizing = true;
		_manaCostPart.gameObject.UpdateActive(active: false);
		_styleModeEnabled = true;
		if (_cardTileAnimator.isActiveAndEnabled)
		{
			_cardTileAnimator.SetBool(Cosmetic, value: true);
		}
	}

	public void Cleanup()
	{
		if (base.DraggingCard == this)
		{
			base.DraggingCard = null;
			EndDragCard();
		}
		if (base.IsMouseOver && base.Holder != null && base.Holder.RolloverZoomView != null)
		{
			base.Holder.RolloverZoomView.CardRolledOff(base.VisualCard);
		}
		OnAddClicked = null;
		OnRemoveClicked = null;
	}
}
