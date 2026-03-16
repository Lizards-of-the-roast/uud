using System;
using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace EventPage.CampaignGraph.ColorMastery;

public class ColorMasteryPanel : MonoBehaviour
{
	[SerializeField]
	private Transform _buttonsContainer;

	[SerializeField]
	private ColorMasteryBannerItem _colorMasteryBannerItemPrefab;

	[SerializeField]
	private Localize _title;

	private List<ColorMasteryBannerItem> _items = new List<ColorMasteryBannerItem>();

	private Action<string> _onItemClicked;

	private Action<string> _onItemHover;

	private Action<string> _onItemHoverOff;

	public ColorMasteryPanel Init(IColorChallengeStrategy strategy, string activeTrack, AssetLookupSystem assetLookupSystem)
	{
		_onItemClicked = null;
		_onItemHover = null;
		_onItemHoverOff = null;
		_buttonsContainer.transform.DestroyChildren();
		_items.Clear();
		_title.SetText("Events/Event_Title_" + strategy.TemplateKey);
		foreach (IColorChallengeTrack value in strategy.Tracks.Values)
		{
			ColorMasteryBannerItem item = UnityEngine.Object.Instantiate(_colorMasteryBannerItemPrefab, _buttonsContainer);
			item.SetTrack(assetLookupSystem, value);
			item.Button.OnClick.AddListener(delegate
			{
				_onClicked(item.EventName);
			});
			item.Button.OnMouseover.AddListener(delegate
			{
				_onHover(item.EventName);
			});
			item.Button.OnMouseoff.AddListener(delegate
			{
				_onHoverOff(item.EventName);
			});
			if (value.Name == activeTrack)
			{
				item.SetActive(active: true);
			}
			_items.Add(item);
		}
		return this;
	}

	private void OnDestroy()
	{
		_onItemClicked = null;
		_onItemHover = null;
		_onItemHoverOff = null;
	}

	public void SetEvent(IColorChallengeStrategy strategy, string activeEvent, AssetLookupSystem assetLookupSystem)
	{
		foreach (ColorMasteryBannerItem item in _items)
		{
			if (strategy.Tracks.TryGetValue(item.EventName, out var value))
			{
				item.SetTrack(assetLookupSystem, value);
				item.SetActive(value.Name == activeEvent);
			}
		}
	}

	public void UpdateEvent()
	{
		foreach (ColorMasteryBannerItem item in _items)
		{
			item.UpdateEvent();
		}
	}

	private void _onClicked(string colorName)
	{
		foreach (ColorMasteryBannerItem item in _items)
		{
			item.SetActive(item.EventName == colorName);
		}
		_onItemClicked?.Invoke(colorName);
	}

	private void _onHover(string colorName)
	{
		_onItemHover?.Invoke(colorName);
	}

	private void _onHoverOff(string colorName)
	{
		_onItemHoverOff?.Invoke(colorName);
	}

	public ColorMasteryPanel SetOnItemClickedCallback(Action<string> onItemClicked)
	{
		_onItemClicked = (Action<string>)Delegate.Combine(_onItemClicked, onItemClicked);
		return this;
	}

	public ColorMasteryPanel SetOnItemHoverCallback(Action<string> onItemHover)
	{
		_onItemHover = (Action<string>)Delegate.Combine(_onItemHover, onItemHover);
		return this;
	}

	public ColorMasteryPanel SetOnItemHoverOffCallback(Action<string> onItemHoverOff)
	{
		_onItemHoverOff = (Action<string>)Delegate.Combine(_onItemHoverOff, onItemHoverOff);
		return this;
	}
}
