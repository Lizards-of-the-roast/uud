using System;
using GreClient.CardData;
using MTGA.KeyboardManager;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga;

public class CardRolloverZoomDelegator : ICardRolloverZoom
{
	private readonly View_CardRolloverZoom _cardRolloverZoom;

	private readonly StaticCardRolloverZoom _cardDetailsZoom;

	private CardRolloverZoomBase _cardZoom
	{
		get
		{
			if (!PlatformUtils.IsHandheld())
			{
				return _cardRolloverZoom;
			}
			return _cardDetailsZoom;
		}
	}

	public Action<ICardDataAdapter> OnRolloverStart
	{
		get
		{
			return _cardZoom.OnRolloverStart;
		}
		set
		{
			_cardZoom.OnRolloverStart = value;
		}
	}

	public Action<Meta_CDC> OnRollover
	{
		get
		{
			return _cardZoom.OnRollover;
		}
		set
		{
			_cardZoom.OnRollover = value;
		}
	}

	public Action<Meta_CDC> OnRolloff
	{
		get
		{
			return _cardZoom.OnRolloff;
		}
		set
		{
			_cardZoom.OnRolloff = value;
		}
	}

	public ICardDataAdapter LastRolloverModel => _cardZoom.LastRolloverModel;

	public bool IsActive
	{
		get
		{
			if (_cardRolloverZoom == null && _cardDetailsZoom == null)
			{
				return false;
			}
			if (_cardRolloverZoom == null)
			{
				return _cardDetailsZoom.IsActive;
			}
			if (_cardDetailsZoom == null)
			{
				return _cardRolloverZoom.IsActive;
			}
			if (_cardRolloverZoom.IsActive)
			{
				return _cardDetailsZoom.IsActive;
			}
			return false;
		}
		set
		{
			if (_cardRolloverZoom != null)
			{
				_cardRolloverZoom.IsActive = value;
			}
			if (_cardDetailsZoom != null)
			{
				_cardDetailsZoom.IsActive = value;
			}
		}
	}

	public CardRolloverZoomDelegator(View_CardRolloverZoom desktopView, StaticCardRolloverZoom mobileView)
	{
		_cardRolloverZoom = desktopView;
		_cardDetailsZoom = mobileView;
	}

	public void Destroy()
	{
		_cardRolloverZoom?.Destroy();
		_cardDetailsZoom?.Destroy();
	}

	public void Initialize(CardViewBuilder cardViewBuilder, CardDatabase cardDatabase, IClientLocProvider locManager, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, KeyboardManager keyboardManager, DeckFormat currentEventFormat)
	{
		_cardRolloverZoom?.Initialize(cardViewBuilder, cardDatabase, locManager, unityObjectPool, genericObjectPool, keyboardManager, currentEventFormat);
		_cardDetailsZoom.Initialize(cardViewBuilder, cardDatabase, locManager, unityObjectPool, genericObjectPool, keyboardManager, currentEventFormat);
	}

	public bool CardRolledOver(ICardDataAdapter model, Bounds cardColliderBounds, HangerSituation hangerSituation = default(HangerSituation), Vector2 offset = default(Vector2))
	{
		if (_cardRolloverZoom != null)
		{
			return _cardRolloverZoom.CardRolledOver(model, cardColliderBounds, hangerSituation, offset);
		}
		if (_cardDetailsZoom != null)
		{
			return _cardDetailsZoom.CardRolledOver(model, cardColliderBounds, hangerSituation, offset);
		}
		return false;
	}

	public void CardRolledOff(ICardDataAdapter model, bool alwaysRollOff = false)
	{
		_cardRolloverZoom?.CardRolledOff(model, alwaysRollOff);
		_cardDetailsZoom?.CardRolledOff(model, alwaysRollOff);
	}

	public void CardPointerDown(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null, HangerSituation hangerSituation = default(HangerSituation))
	{
		_cardRolloverZoom?.CardPointerDown(inputButton, model, metaCardView, hangerSituation);
		_cardDetailsZoom?.CardPointerDown(inputButton, model, metaCardView, hangerSituation);
	}

	public void CardPointerUp(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null)
	{
		_cardRolloverZoom?.CardPointerUp(inputButton, model, metaCardView);
		_cardDetailsZoom?.CardPointerUp(inputButton, model, metaCardView);
	}

	public bool CardScrolled(Vector2 scrollDelta)
	{
		if (_cardRolloverZoom != null)
		{
			return _cardRolloverZoom.CardScrolled(scrollDelta);
		}
		if (_cardDetailsZoom != null)
		{
			return _cardDetailsZoom.CardScrolled(scrollDelta);
		}
		return false;
	}

	public void Close()
	{
		_cardRolloverZoom?.Close();
		_cardDetailsZoom?.Close();
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (_cardRolloverZoom != null)
		{
			return _cardRolloverZoom.HandleKeyDown(curr, mods);
		}
		if (_cardDetailsZoom != null)
		{
			return _cardDetailsZoom.HandleKeyDown(curr, mods);
		}
		return false;
	}

	public void Cleanup()
	{
		_cardRolloverZoom?.Cleanup();
		_cardDetailsZoom?.Cleanup();
	}
}
