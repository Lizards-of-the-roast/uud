using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class ManaTypeSpinner : MonoBehaviour
{
	[SerializeField]
	private Image _background;

	[SerializeField]
	private TMP_Text _defaultTextEntry;

	[SerializeField]
	private RectTransform _contentRoot;

	[SerializeField]
	private VerticalLayoutGroup _layoutGroup;

	[SerializeField]
	private Button _upButton;

	[SerializeField]
	private Button _downButton;

	[SerializeField]
	private float _scrollSpeed = 120f;

	private float _targetPosition;

	public event Action<ManaTypeSpinner> UpEvent;

	public event Action<ManaTypeSpinner> DownEvent;

	public void Init(CastingTimeOption_ManaTypeWorkflow.SelectionPair selectionPair)
	{
		SetOptions(selectionPair.Options.Select((ManaColor x) => ManaQuantity.MakeMana(1u, x)).ToArray());
		SetIndex(selectionPair.SelectedIndex, instant: true);
		_background.gameObject.UpdateActive(active: true);
		_upButton.gameObject.UpdateActive(active: true);
		_upButton.gameObject.UpdateActive(active: true);
		_upButton.onClick.AddListener(OnUpClick);
		_downButton.onClick.AddListener(OnDownClick);
	}

	public void Init(ManaQuantity option)
	{
		SetOptions(new ManaQuantity[1] { option });
		SetIndex(0, instant: true);
		_background.gameObject.UpdateActive(active: false);
		_upButton.gameObject.UpdateActive(active: false);
		_downButton.gameObject.UpdateActive(active: false);
	}

	public void Cleanup()
	{
		SetUpArrowInteractable(interactable: true);
		SetDownArrowInteractable(interactable: true);
		_upButton.onClick.RemoveAllListeners();
		_downButton.onClick.RemoveAllListeners();
	}

	private void Start()
	{
		_scrollSpeed = Mathf.Abs(_scrollSpeed);
		_scrollSpeed = Mathf.Max(_scrollSpeed, 10f);
	}

	private void Update()
	{
		Vector3 localPosition = _contentRoot.localPosition;
		if (localPosition.y > _targetPosition)
		{
			localPosition.y = Math.Max(_targetPosition, localPosition.y - _scrollSpeed * Time.smoothDeltaTime);
			_contentRoot.localPosition = localPosition;
		}
		else if (localPosition.y < _targetPosition)
		{
			localPosition.y = Math.Min(_targetPosition, localPosition.y + _scrollSpeed * Time.smoothDeltaTime);
			_contentRoot.localPosition = localPosition;
		}
	}

	private void OnUpClick()
	{
		this.UpEvent?.Invoke(this);
	}

	private void OnDownClick()
	{
		this.DownEvent?.Invoke(this);
	}

	public void SetOptions(IReadOnlyList<ManaQuantity> manaOptions)
	{
		int num = manaOptions.Count - _contentRoot.childCount;
		for (int i = 0; i < num; i++)
		{
			UnityEngine.Object.Instantiate(_defaultTextEntry, _contentRoot);
		}
		for (int j = 0; j < _contentRoot.childCount; j++)
		{
			Transform child = _contentRoot.GetChild(j);
			if (j < manaOptions.Count)
			{
				ManaQuantity manaQuantity = manaOptions[j];
				string sourceText = ManaUtilities.ConvertManaSymbols(ManaUtilities.ConvertToOldSchoolManaTextParams(manaQuantity));
				child.GetComponent<TMP_Text>().SetText(sourceText);
				child.gameObject.UpdateActive(active: true);
			}
			else
			{
				child.gameObject.UpdateActive(active: false);
			}
		}
	}

	public void SetIndex(int index, bool instant = false)
	{
		float num = (_targetPosition = (110f + _layoutGroup.spacing) * (float)index);
		if (instant)
		{
			_contentRoot.localPosition = num * Vector3.up;
		}
	}

	public void SetUpArrowInteractable(bool interactable)
	{
		_upButton.interactable = interactable;
	}

	public void SetDownArrowInteractable(bool interactable)
	{
		_downButton.interactable = interactable;
	}
}
