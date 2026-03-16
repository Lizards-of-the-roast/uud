using System;
using System.Collections.Generic;
using Core.Shared.Code.CardFilters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtgo.Gre.External.Messaging;

public class DeckFilterBar : MonoBehaviour
{
	public TMP_InputField SearchInput;

	public Button ClearSearchButton;

	[SerializeField]
	private List<GameObject> _colorFiltersParent;

	private CardFilterView[] _allViews;

	private string _searchText;

	private bool _suppressValueChangeEvents;

	public event Action<CardFilterType, bool> FilterValueChanged;

	public event Action<string> SearchTextChanged;

	private void Awake()
	{
		_allViews = GetComponentsInChildren<CardFilterView>(includeInactive: true);
		CardFilterView[] allViews = _allViews;
		foreach (CardFilterView filterView in allViews)
		{
			filterView.FilterToggle.onValueChanged.AddListener(delegate
			{
				if (!_suppressValueChangeEvents)
				{
					CardFilterView cardFilterView = filterView;
					this.FilterValueChanged?.Invoke(cardFilterView.FilterType, cardFilterView.FilterToggle.isOn);
				}
			});
		}
		SearchInput.onValueChanged.AddListener(delegate
		{
			SearchInput.textComponent.color = UnityEngine.Color.white;
		});
		SearchInput.onEndEdit.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
			UpdateSearchText();
		});
		ClearSearchButton.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
			SearchInput.text = string.Empty;
			UpdateSearchText();
		});
	}

	private void OnEnable()
	{
	}

	private void UpdateSearchText()
	{
		if (_searchText != SearchInput.text)
		{
			_searchText = SearchInput.text;
			this.SearchTextChanged?.Invoke(_searchText);
		}
	}

	public List<ManaColor> GetFilterColors()
	{
		List<ManaColor> list = new List<ManaColor>();
		foreach (GameObject item in _colorFiltersParent)
		{
			if (item.GetComponent<Toggle>().isOn)
			{
				switch (item.GetComponent<CardFilterView>().FilterType)
				{
				case CardFilterType.White:
					list.Add(ManaColor.White);
					break;
				case CardFilterType.Blue:
					list.Add(ManaColor.Blue);
					break;
				case CardFilterType.Black:
					list.Add(ManaColor.Black);
					break;
				case CardFilterType.Red:
					list.Add(ManaColor.Red);
					break;
				case CardFilterType.Green:
					list.Add(ManaColor.Green);
					break;
				}
			}
		}
		return list;
	}
}
