using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using TMPro;
using UnityEngine;

public class ColumnCardQuantityAdjust : MonoBehaviour
{
	[Serializable]
	public class MultipleQuantityAdjustMapping
	{
		public int _cardQuantity;

		public float _interval;

		public int _quantityAdjust;

		public MultipleQuantityAdjustMapping(int cardQuantity, float interval, int quantityAdjust)
		{
			_cardQuantity = cardQuantity;
			_interval = interval;
			_quantityAdjust = quantityAdjust;
		}
	}

	[SerializeField]
	private List<MultipleQuantityAdjustMapping> quantityAdjustMappings = new List<MultipleQuantityAdjustMapping>
	{
		new MultipleQuantityAdjustMapping(1, 0.25f, 1)
	};

	[SerializeField]
	private CustomTouchButton _leftButton;

	[SerializeField]
	private CustomTouchButton _rightButton;

	[SerializeField]
	private TMP_Text _quantityText;

	[SerializeField]
	private GameObject _uiParent;

	[SerializeField]
	private float _disappearDelay = 3f;

	[SerializeField]
	private Vector3 _cardWorldPosOffset = new Vector3(0f, 0f, 0.5f);

	public Action<CardData> CardQuantityIncrease;

	public Action<CardData> CardQuantityDecrease;

	public Func<uint, bool> CanAddCard;

	public Action OnTimeout;

	private StaticColumnMetaCardView _currentCardView;

	private Coroutine _showQuantityAdjustCoroutine;

	private Coroutine _adjustMultipleCardQuantityCoroutine;

	private float _currentTimerTime;

	private bool _onClickAndHeld;

	public CardData CurrentCardData
	{
		get
		{
			if (_currentCardView == null)
			{
				return null;
			}
			return _currentCardView.Card;
		}
	}

	private void Start()
	{
		_uiParent.gameObject.SetActive(value: false);
		_leftButton.OnClick.AddListener(OnLeftPressed);
		_leftButton.OnClickAndHold.AddListener(OnLeftClickAndHold);
		_leftButton.OnClickUp.AddListener(OnLeftClickUp);
		_rightButton.OnClick.AddListener(OnRightPressed);
		_rightButton.OnClickAndHold.AddListener(OnRightClickAndHold);
		_rightButton.OnClickUp.AddListener(OnRightClickUp);
		quantityAdjustMappings.Sort((MultipleQuantityAdjustMapping x, MultipleQuantityAdjustMapping y) => x._cardQuantity.CompareTo(y._cardQuantity));
	}

	private void OnDestroy()
	{
		_leftButton.OnClick.RemoveListener(OnLeftPressed);
		_leftButton.OnClickAndHold.RemoveListener(OnLeftClickAndHold);
		_leftButton.OnClickUp.RemoveListener(OnLeftClickUp);
		_rightButton.OnClick.RemoveListener(OnRightPressed);
		_rightButton.OnClickAndHold.RemoveListener(OnRightClickAndHold);
		_rightButton.OnClickUp.RemoveListener(OnRightClickUp);
	}

	private void OnLeftPressed()
	{
		if (_currentCardView != null)
		{
			_currentTimerTime = 0f;
			CardQuantityDecrease?.Invoke(CurrentCardData);
		}
	}

	private void OnLeftClickAndHold()
	{
		if (_adjustMultipleCardQuantityCoroutine != null)
		{
			StopCoroutine(_adjustMultipleCardQuantityCoroutine);
		}
		_onClickAndHeld = true;
		_adjustMultipleCardQuantityCoroutine = StartCoroutine(AdjustMultipleCardQuality_Coroutine(CardQuantityDecrease, _leftButton));
	}

	private void OnLeftClickUp()
	{
		_onClickAndHeld = false;
	}

	private void OnRightPressed()
	{
		if (_currentCardView != null)
		{
			_currentTimerTime = 0f;
			CardQuantityIncrease?.Invoke(CurrentCardData);
		}
	}

	private void OnRightClickAndHold()
	{
		if (_adjustMultipleCardQuantityCoroutine != null)
		{
			StopCoroutine(_adjustMultipleCardQuantityCoroutine);
		}
		_onClickAndHeld = true;
		_adjustMultipleCardQuantityCoroutine = StartCoroutine(AdjustMultipleCardQuality_Coroutine(CardQuantityIncrease, _rightButton));
	}

	private void OnRightClickUp()
	{
		_onClickAndHeld = false;
	}

	private IEnumerator AdjustMultipleCardQuality_Coroutine(Action<CardData> quantityAdjustAction, CustomTouchButton touchButton)
	{
		while (_onClickAndHeld && !(_currentCardView == null))
		{
			MultipleQuantityAdjustMapping currentQuantityMapping = GetCurrentQuantityMapping();
			if (currentQuantityMapping == null)
			{
				break;
			}
			_currentTimerTime = 0f;
			for (int i = 0; i < currentQuantityMapping._quantityAdjust; i++)
			{
				quantityAdjustAction?.Invoke(CurrentCardData);
				if (_currentCardView.Quantity < currentQuantityMapping._cardQuantity)
				{
					currentQuantityMapping = GetCurrentQuantityMapping();
					if (currentQuantityMapping == null)
					{
						yield break;
					}
				}
				if (!touchButton.Interactable || _currentCardView.Quantity <= 1)
				{
					_onClickAndHeld = false;
					yield break;
				}
			}
			yield return new WaitForSeconds(currentQuantityMapping._interval);
		}
	}

	private MultipleQuantityAdjustMapping GetCurrentQuantityMapping()
	{
		if (_currentCardView == null || quantityAdjustMappings.Count == 0)
		{
			return null;
		}
		if (quantityAdjustMappings.Count == 1)
		{
			return quantityAdjustMappings.First();
		}
		MultipleQuantityAdjustMapping result = quantityAdjustMappings.First();
		foreach (MultipleQuantityAdjustMapping quantityAdjustMapping in quantityAdjustMappings)
		{
			if (quantityAdjustMapping._cardQuantity == _currentCardView.Quantity)
			{
				return quantityAdjustMapping;
			}
			if (quantityAdjustMapping._cardQuantity > _currentCardView.Quantity)
			{
				return result;
			}
			result = quantityAdjustMapping;
		}
		return result;
	}

	private void HighlightHandler(MetaCardView cardView, bool isMouseOver, bool isMouseDown, bool isDragging)
	{
		if (cardView != _currentCardView)
		{
			((CDCMetaCardView)cardView).Default_HighlightHandler();
		}
	}

	public void Show(StaticColumnMetaCardView cardView)
	{
		if (_showQuantityAdjustCoroutine != null)
		{
			StopCoroutine(_showQuantityAdjustCoroutine);
			CleanUpCardView();
		}
		_showQuantityAdjustCoroutine = StartCoroutine(Show_Coroutine(cardView));
	}

	private IEnumerator Show_Coroutine(StaticColumnMetaCardView cardView)
	{
		_currentCardView = cardView;
		_currentCardView.ForceHideQuantity = true;
		_currentCardView.AllowRollOver = false;
		cardView.Holder.CustomHighlightHandler = HighlightHandler;
		_currentCardView.CardView.UpdateHighlight(HighlightType.Selected);
		_currentCardView.CardView.UpdateVisuals();
		Refresh();
		_uiParent.SetActive(value: true);
		_currentTimerTime = 0f;
		while (_currentTimerTime < _disappearDelay && _currentCardView != null && _currentCardView.CardView != null)
		{
			Bounds bounds = _currentCardView.CardView.Collider.bounds;
			Vector3 position = new Vector3(bounds.center.x, bounds.center.y + bounds.size.y / 2f, bounds.center.z + bounds.size.z) + _cardWorldPosOffset;
			_uiParent.transform.position = position;
			_currentTimerTime += Time.deltaTime;
			yield return null;
		}
		if (_currentTimerTime >= _disappearDelay)
		{
			OnTimeout?.Invoke();
		}
		else
		{
			Hide();
		}
	}

	private void CleanUpCardView()
	{
		if (!(_currentCardView == null))
		{
			_currentCardView.Holder.CustomHighlightHandler = null;
			_currentCardView.ForceHideQuantity = false;
			_currentCardView.AllowRollOver = true;
			_currentCardView = null;
		}
	}

	public void Hide()
	{
		if (_showQuantityAdjustCoroutine != null)
		{
			StopCoroutine(_showQuantityAdjustCoroutine);
		}
		if (_adjustMultipleCardQuantityCoroutine != null)
		{
			StopCoroutine(_adjustMultipleCardQuantityCoroutine);
		}
		CleanUpCardView();
		_uiParent.SetActive(value: false);
	}

	public void Refresh()
	{
		if (!(_currentCardView == null))
		{
			_quantityText.text = $"x{_currentCardView.Quantity}";
			_leftButton.Interactable = true;
			_rightButton.Interactable = CanAddCard != null && CanAddCard(_currentCardView.Card.GrpId);
		}
	}
}
