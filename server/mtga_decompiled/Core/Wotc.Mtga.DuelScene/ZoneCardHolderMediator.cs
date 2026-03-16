using System;
using System.Collections.Generic;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene;

public class ZoneCardHolderMediator : IDisposable
{
	private readonly ISignalListen<CardHolderCreatedSignalArgs> _cardHolderCreatedEvent;

	private readonly ISignalListen<CardHolderDeletedSignalArgs> _cardHolderDeletedEvent;

	private readonly WorkflowController _workflowController;

	private readonly BrowserManager _browserManager;

	private readonly IHighlightProvider _highlightProvider;

	private readonly HashSet<ICardHolder> _subscribed = new HashSet<ICardHolder>();

	public ZoneCardHolderMediator(ISignalListen<CardHolderCreatedSignalArgs> cardHolderCreatedEvent, ISignalListen<CardHolderDeletedSignalArgs> cardHolderDeletedEvent, WorkflowController workflowController, BrowserManager browserManager, IHighlightProvider highlightProvider)
	{
		_cardHolderCreatedEvent = cardHolderCreatedEvent;
		_cardHolderDeletedEvent = cardHolderDeletedEvent;
		_workflowController = workflowController;
		_browserManager = browserManager;
		_highlightProvider = highlightProvider;
		_cardHolderCreatedEvent.Listeners += OnCardHolderCreated;
		_cardHolderDeletedEvent.Listeners += OnCardHolderDeleted;
	}

	private void OnCardHolderCreated(CardHolderCreatedSignalArgs createdSignalArgs)
	{
		ICardHolder cardHolder = createdSignalArgs.CardHolder;
		if (!_subscribed.Contains(cardHolder))
		{
			Subscribe(cardHolder);
		}
	}

	private void Subscribe(ICardHolder cardHolder)
	{
		if (cardHolder is StackCardHolder stackCardHolder)
		{
			_browserManager.BrowserShown += stackCardHolder.OnBrowserShown;
			_browserManager.BrowserHidden += stackCardHolder.OnBrowserHidden;
			_subscribed.Add(cardHolder);
		}
		else if (cardHolder is IBattlefieldCardHolder battlefieldCardHolder)
		{
			_highlightProvider.HighlightsUpdated += battlefieldCardHolder.LayoutNow;
			_subscribed.Add(cardHolder);
		}
		else if (cardHolder is GraveyardCardHolder graveyardCardHolder)
		{
			_workflowController.InteractionApplied += graveyardCardHolder.OnInteractionApplied;
			_workflowController.InteractionCleared += graveyardCardHolder.OnInteractionCleared;
			_subscribed.Add(cardHolder);
		}
		else if (cardHolder is PlayerExileCardHolder playerExileCardHolder)
		{
			_workflowController.InteractionApplied += playerExileCardHolder.OnInteractionApplied;
			_subscribed.Add(cardHolder);
		}
	}

	private void OnCardHolderDeleted(CardHolderDeletedSignalArgs deletedSignalArgs)
	{
		ICardHolder cardHolder = deletedSignalArgs.CardHolder;
		if (_subscribed.Remove(cardHolder))
		{
			Unsubscribe(cardHolder);
		}
	}

	private void Unsubscribe(ICardHolder cardHolder)
	{
		if (cardHolder is StackCardHolder stackCardHolder)
		{
			_browserManager.BrowserShown -= stackCardHolder.OnBrowserShown;
			_browserManager.BrowserHidden -= stackCardHolder.OnBrowserHidden;
		}
		else if (cardHolder is IBattlefieldCardHolder battlefieldCardHolder)
		{
			_highlightProvider.HighlightsUpdated -= battlefieldCardHolder.LayoutNow;
		}
		else if (cardHolder is GraveyardCardHolder graveyardCardHolder)
		{
			_workflowController.InteractionApplied -= graveyardCardHolder.OnInteractionApplied;
			_workflowController.InteractionCleared -= graveyardCardHolder.OnInteractionCleared;
		}
		else if (cardHolder is PlayerExileCardHolder playerExileCardHolder)
		{
			_workflowController.InteractionApplied -= playerExileCardHolder.OnInteractionApplied;
		}
	}

	public void Dispose()
	{
		_cardHolderCreatedEvent.Listeners -= OnCardHolderCreated;
		_cardHolderDeletedEvent.Listeners -= OnCardHolderDeleted;
		foreach (ICardHolder item in _subscribed)
		{
			Unsubscribe(item);
		}
		_subscribed.Clear();
	}
}
