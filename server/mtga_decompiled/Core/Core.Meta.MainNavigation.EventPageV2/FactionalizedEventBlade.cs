using System;
using System.Collections.Generic;
using EventPage;
using UnityEngine;
using Wizards.MDN;
using Wizards.Unification.Models.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.EventPageV2;

public class FactionalizedEventBlade : MonoBehaviour
{
	[SerializeField]
	private Transform _buttonsContainer;

	[SerializeField]
	private FactionalizedEventBladeItem _eventBladeItemPrefab;

	[SerializeField]
	private Localize _title;

	private List<FactionalizedEventBladeItem> _items = new List<FactionalizedEventBladeItem>();

	private Action<string> _onItemClicked;

	private Action<string> _onItemHover;

	private Action<string> _onItemHoverOff;

	public Action<string> OnFactionSelect;

	public void Init(EventContext context, Dictionary<string, FactionSealedUXInfo> factions, SharedEventPageClasses sharedClasses)
	{
		_onItemClicked = null;
		_onItemHover = null;
		_onItemHoverOff = null;
		_buttonsContainer.transform.DestroyChildren();
		_items.Clear();
		_title.SetText("Events/FactionSealed/ChooseYourFaction", null, "Events/FactionSealed/ChooseYourFaction");
		foreach (string key in factions.Keys)
		{
			FactionalizedEventBladeItem item = UnityEngine.Object.Instantiate(_eventBladeItemPrefab, _buttonsContainer);
			item.Init(context, factions[key], sharedClasses);
			item.Button.OnClick.AddListener(delegate
			{
				_onClicked(item.FactionName);
			});
			item.Button.OnMouseover.AddListener(delegate
			{
				_onHover(item.FactionName);
			});
			item.Button.OnMouseoff.AddListener(delegate
			{
				_onHoverOff(item.FactionName);
			});
			_items.Add(item);
		}
	}

	public void SelectRandomFaction()
	{
		_items[UnityEngine.Random.Range(0, _items.Count)].Button.OnClick.Invoke();
	}

	private void OnEnable()
	{
		foreach (FactionalizedEventBladeItem item in _items)
		{
			item.SetActive(active: false);
		}
	}

	private void OnDestroy()
	{
		_onItemClicked = null;
		_onItemHover = null;
		_onItemHoverOff = null;
	}

	private void _onClicked(string factionName)
	{
		foreach (FactionalizedEventBladeItem item in _items)
		{
			item.SetActive(item.FactionName == factionName);
		}
		OnFactionSelect?.Invoke(factionName);
		_onItemClicked?.Invoke(factionName);
	}

	private void _onHover(string factionName)
	{
		_onItemHover?.Invoke(factionName);
	}

	private void _onHoverOff(string factionName)
	{
		_onItemHoverOff?.Invoke(factionName);
	}

	public FactionalizedEventBlade SetOnItemClickedCallback(Action<string> onItemClicked)
	{
		_onItemClicked = (Action<string>)Delegate.Combine(_onItemClicked, onItemClicked);
		return this;
	}

	public FactionalizedEventBlade SetOnItemHoverCallback(Action<string> onItemHover)
	{
		_onItemHover = (Action<string>)Delegate.Combine(_onItemHover, onItemHover);
		return this;
	}

	public FactionalizedEventBlade SetOnItemHoverOffCallback(Action<string> onItemHoverOff)
	{
		_onItemHoverOff = (Action<string>)Delegate.Combine(_onItemHoverOff, onItemHoverOff);
		return this;
	}
}
