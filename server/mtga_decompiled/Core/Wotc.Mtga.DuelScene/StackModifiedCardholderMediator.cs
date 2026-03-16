using System;
using System.Collections.Generic;
using Pooling;

namespace Wotc.Mtga.DuelScene;

public class StackModifiedCardholderMediator : IDisposable
{
	private readonly IObjectPool _objectPool;

	private readonly ISignalListen<CardHolderCreatedSignalArgs> _cardHolderCreatedEvent;

	private readonly ISignalListen<CardHolderDeletedSignalArgs> _cardHolderDeletedEvent;

	private readonly HashSet<ICardHolder> _toLayout;

	private StackCardHolder _stack;

	public StackModifiedCardholderMediator(ISignalListen<CardHolderCreatedSignalArgs> cardHolderCreatedEvent, ISignalListen<CardHolderDeletedSignalArgs> cardHolderDeletedEvent, IObjectPool objectPool)
	{
		_objectPool = objectPool;
		_toLayout = _objectPool.PopObject<HashSet<ICardHolder>>();
		_cardHolderCreatedEvent = cardHolderCreatedEvent;
		_cardHolderDeletedEvent = cardHolderDeletedEvent;
		_cardHolderCreatedEvent.Listeners += OnCardHolderCreated;
		_cardHolderDeletedEvent.Listeners += OnCardHolderDeleted;
	}

	private void OnCardHolderCreated(CardHolderCreatedSignalArgs args)
	{
		ICardHolder cardHolder = args.CardHolder;
		if (cardHolder != null)
		{
			if (cardHolder is StackCardHolder stack)
			{
				_stack = stack;
				_stack.CardAdded += OnStackModified;
				_stack.CardRemoved += OnStackModified;
			}
			else if (cardHolder is GraveyardCardHolder || cardHolder is IExileCardHolder || cardHolder is ICommandCardHolder)
			{
				_toLayout.Add(cardHolder);
			}
		}
	}

	private void OnCardHolderDeleted(CardHolderDeletedSignalArgs args)
	{
		if (args != null)
		{
			_toLayout.Remove(args.CardHolder);
		}
	}

	private void OnStackModified(DuelScene_CDC cardView)
	{
		foreach (ICardHolder item in _toLayout)
		{
			if (item is IExileCardHolder exileCardHolder)
			{
				foreach (ICardHolder allSubCardHolder in exileCardHolder.GetAllSubCardHolders())
				{
					allSubCardHolder.LayoutNow();
				}
			}
			else if (item is ICommandCardHolder commandCardHolder)
			{
				foreach (ICardHolder allSubCardHolder2 in commandCardHolder.GetAllSubCardHolders())
				{
					allSubCardHolder2.LayoutNow();
				}
			}
			else
			{
				item.LayoutNow();
			}
		}
	}

	public void Dispose()
	{
		_cardHolderDeletedEvent.Listeners -= OnCardHolderDeleted;
		_cardHolderCreatedEvent.Listeners -= OnCardHolderCreated;
		_toLayout.Clear();
		_objectPool.PushObject(_toLayout, tryClear: false);
		if (!(_stack == null))
		{
			_stack.CardAdded -= OnStackModified;
			_stack.CardRemoved -= OnStackModified;
			_stack = null;
		}
	}
}
