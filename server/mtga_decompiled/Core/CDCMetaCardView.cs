using System.Collections.Generic;
using GreClient.CardData;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class CDCMetaCardView : MetaCardView
{
	[SerializeField]
	private float _cardScale = 0.527f;

	[SerializeField]
	private bool playSleeveFX = true;

	[SerializeField]
	private string _dragLayerName = "Zoom";

	[SerializeField]
	protected GameObject _tagObject;

	[SerializeField]
	protected GameObject _tagObjectFaction;

	[SerializeField]
	protected List<TMP_Text> _tagTextItems = new List<TMP_Text>();

	protected Meta_CDC _cardView;

	protected int _cardNumberNew;

	protected Collider _cardCollider;

	protected int _originalLayer = -1;

	protected HighlightType _baseHighlight;

	public static readonly Color BANNED_COLOR_VALUE = new Color(1f, 0f, 0.0667f, 0.5f);

	public static readonly Color GRAY_OUT_COLOR_VALUE = new Color(0f, 0f, 0f, 0.5f);

	public static readonly Color SUGGESTED_COLOR_VALUE = new Color(0f, 0f, 1f, 0.2f);

	protected static int TAG_STATE_PARAM = Animator.StringToHash("TAG_TopText");

	protected CardDatabase _cardDatabase;

	protected CardViewBuilder _cardViewBuilder;

	protected IUnityObjectPool _objectPool;

	protected Animator _animator;

	public Meta_CDC CardView => _cardView;

	public virtual void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_objectPool = Pantry.Get<IUnityObjectPool>();
		if (_cardView == null)
		{
			_cardView = _cardViewBuilder.CreateMetaCdc(CardDataExtensions.CreateBlank(), base.transform);
			_cardCollider = _cardView.GetComponentInChildren<Collider>();
		}
		else
		{
			_cardView.Root.localPosition = Vector3.zero;
			_cardCollider.enabled = true;
			if (_originalLayer != -1)
			{
				_cardView.transform.SetLayerRecursive(_originalLayer);
			}
			_cardView.UpdateHighlight(HighlightType.None);
		}
		Transform obj = _cardView.transform;
		obj.localPosition = Vector3.zero;
		obj.localScale = _cardScale * Vector3.one;
		obj.localRotation = Quaternion.Euler(Vector3.zero);
		_cardView.PartsRoot.localPosition = Vector3.zero;
		_animator = base.gameObject.GetComponent<Animator>();
	}

	public void InitWithData(CardData data, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		Init(cardDatabase, cardViewBuilder);
		SetData(data);
	}

	public void SetDataAndWait(CardData data, CardHolderType cardHolderType = CardHolderType.None, uint waitFrames = 0u)
	{
		_cardView.SetUpdateDelay(waitFrames);
		SetData(data, cardHolderType);
	}

	public void SetData(CardData data, CardHolderType cardHolderType = CardHolderType.None, CardData visualData = null)
	{
		base.Card = data;
		base.VisualCard = visualData ?? data;
		_cardView.SetModel(base.VisualCard, updateVisuals: true, cardHolderType);
		if (base.Holder != null && base.Holder.ShowHighlight(this))
		{
			bool flag = (base.IsDragging = false);
			bool isMouseDown = (base.IsMouseOver = flag);
			base.IsMouseDown = isMouseDown;
		}
	}

	public void Default_HighlightHandler()
	{
		if (_cardView == null)
		{
			Debug.LogWarning("Trying to update highlight on card view without CDC. Returning early.");
		}
		else if (base.IsMouseOver && base.IsMouseDown)
		{
			if (base.IsDragging)
			{
				_cardView.UpdateHighlight(_baseHighlight);
			}
			else
			{
				_cardView.UpdateHighlight(HighlightType.Selected);
			}
		}
		else if (base.IsMouseOver)
		{
			_cardView.UpdateHighlight(HighlightType.Hot);
		}
		else if (base.IsMouseDown)
		{
			_cardView.UpdateHighlight(_baseHighlight);
		}
		else
		{
			_cardView.UpdateHighlight(_baseHighlight);
		}
	}

	public virtual void ActivateFirstTag(bool isFirst)
	{
		_tagObject.SetActive(isFirst);
	}

	public virtual void ActivateFactionTag(bool isFaction, string factionTag)
	{
		_tagObjectFaction.gameObject.GetComponentInChildren<TMP_Text>().text = factionTag;
		_tagObjectFaction.SetActive(isFaction);
	}

	public virtual void ActivateTags(List<string> tagsToShow)
	{
		if (tagsToShow != null && tagsToShow.Count <= 0)
		{
			if (_tagObject != null)
			{
				_tagObject.SetActive(value: false);
			}
			return;
		}
		if (_tagObject != null)
		{
			_tagObject.SetActive(value: true);
		}
		else
		{
			SimpleLog.LogError("Missing/null _tagObject on " + GetType().Name);
		}
		if (_animator != null)
		{
			_animator.enabled = true;
		}
		else
		{
			SimpleLog.LogError("Missing/null _animator on " + GetType().Name);
		}
		if (_tagTextItems != null)
		{
			if (tagsToShow.Count > _tagTextItems.Count)
			{
				SimpleLog.LogError($"We can't show more than {_tagTextItems.Count} tags!");
				return;
			}
			for (int i = 0; i < tagsToShow.Count; i++)
			{
				_tagTextItems[i].text = tagsToShow[i];
			}
		}
		else
		{
			SimpleLog.LogError("Missing/null _tagTextItems on " + GetType().Name);
		}
		if (_animator != null)
		{
			_animator.SetInteger(TAG_STATE_PARAM, tagsToShow.Count);
		}
	}

	public virtual void UpdateNumberNew(uint grpId)
	{
		_cardNumberNew = 0;
		if (WrapperController.Instance != null)
		{
			WrapperController.Instance.InventoryManager.CardsToTagNew?.TryGetValue(grpId, out _cardNumberNew);
			ActivateFirstTag(_cardNumberNew > 0);
		}
	}

	protected override void UpdateHighlight()
	{
		if (!(base.Holder == null))
		{
			if (base.Holder.CustomHighlightHandler == null)
			{
				Default_HighlightHandler();
			}
			else
			{
				base.Holder.CustomHighlightHandler(this, base.IsMouseOver, base.IsMouseDown, base.IsDragging);
			}
		}
	}

	protected override Bounds GetBounds()
	{
		return _cardCollider.bounds;
	}

	protected override void BeginDragCard(PointerEventData eventData)
	{
		if (IsCardViewEnabled())
		{
			_originalLayer = _cardView.gameObject.layer;
			_cardView.transform.SetLayerRecursive(_dragLayerName);
			_cardCollider.enabled = false;
			StoreDragFields(eventData, _cardView.Root);
		}
	}

	protected override void DragCard()
	{
		if (IsCardViewEnabled())
		{
			ApplyDragFields(_cardView.Root);
		}
	}

	protected override void EndDragCard()
	{
		if (IsCardViewEnabled() && !(_cardView == null))
		{
			if (_originalLayer != -1)
			{
				_cardView.transform.SetLayerRecursive(_originalLayer);
			}
			_cardCollider.enabled = true;
			RestoreDragOriginalPosition(_cardView.Root);
		}
	}

	public void UpdateVisuals()
	{
		_cardView.UpdateVisuals();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (CardView == null)
		{
			return;
		}
		CardView.GetSleeveFXPayload(CardView.Model, CardHolderType.None, out var sleeveFXPayload, out var prefabFilePath);
		CDCPart_AnimatedCardback componentInChildren = GetComponentInChildren<CDCPart_AnimatedCardback>();
		if (sleeveFXPayload == null && componentInChildren == null)
		{
			base.OnPointerEnter(eventData);
			return;
		}
		if (sleeveFXPayload != null && prefabFilePath != null)
		{
			base.OnPointerEnter(eventData);
			if (playSleeveFX && CardView.EffectsRoot.childCount <= 1)
			{
				GameObject gameObject = _objectPool.PopObject(prefabFilePath);
				gameObject.transform.SetParent(CardView.EffectsRoot);
				gameObject.transform.localPosition = sleeveFXPayload.OffsetData.PositionOffset;
				gameObject.transform.localEulerAngles = sleeveFXPayload.OffsetData.RotationOffset;
				gameObject.transform.localScale = sleeveFXPayload.OffsetData.ScaleMultiplier;
				gameObject.AddOrGetComponent<SelfCleanup>().SetLifetime(sleeveFXPayload.CleanUpAfterSeconds);
				AudioManager.PlayAudio(sleeveFXPayload.AudioEvent, gameObject);
			}
		}
		if (componentInChildren != null)
		{
			componentInChildren.OnPointerEnter(eventData);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		CDCPart_AnimatedCardback componentInChildren = GetComponentInChildren<CDCPart_AnimatedCardback>();
		if (componentInChildren != null)
		{
			componentInChildren.OnPointerExit(eventData);
		}
		base.OnPointerExit(eventData);
	}

	public virtual void Cleanup()
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
		if ((bool)_cardView)
		{
			_cardViewBuilder.DestroyCDC(_cardView);
		}
		_cardView = null;
		_cardCollider = null;
	}
}
