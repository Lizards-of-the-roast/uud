using System;
using System.Collections.Generic;
using Pooling;

namespace Wotc.Mtga.DuelScene;

public class CardHolderRelativeToCameraMediator : IDisposable
{
	private readonly IObjectPool _objectPool;

	private readonly ICameraAdapter _cameraAdapter;

	private readonly ISignalListen<CameraViewportChangedSignalArgs> _viewportChangedSignal;

	private readonly ISignalListen<CardHolderCreatedSignalArgs> _cardHolderCreatedSignal;

	private readonly ISignalListen<CardHolderDeletedSignalArgs> _cardHolderDeletedSignal;

	private readonly HashSet<RelativeToCameraViewport> _toPosition;

	public CardHolderRelativeToCameraMediator(IObjectPool objectPool, ICameraAdapter cameraAdapter, ICardHolderProvider cardHolderProvider, ISignalListen<CameraViewportChangedSignalArgs> viewportChangedSignal, ISignalListen<CardHolderCreatedSignalArgs> cardHolderCreatedSignal, ISignalListen<CardHolderDeletedSignalArgs> cardHolderDeletedSignal)
	{
		_objectPool = objectPool;
		_cameraAdapter = cameraAdapter;
		_viewportChangedSignal = viewportChangedSignal;
		_cardHolderCreatedSignal = cardHolderCreatedSignal;
		_cardHolderDeletedSignal = cardHolderDeletedSignal;
		_toPosition = _objectPool.PopObject<HashSet<RelativeToCameraViewport>>();
		_viewportChangedSignal.Listeners += OnViewportChanged;
		_cardHolderCreatedSignal.Listeners += OnCardHolderCreated;
		_cardHolderDeletedSignal.Listeners += OnCardHolderDeleted;
		if (cardHolderProvider.TryGetCardHolder(GREPlayerNum.Invalid, CardHolderType.Examine, out var cardHolder) && cardHolder is ExamineViewCardHolder examineViewCardHolder && examineViewCardHolder.TryGetComponent<RelativeToCameraViewport>(out var component))
		{
			component.Reposition(_cameraAdapter);
			_toPosition.Add(component);
		}
	}

	private void OnCardHolderCreated(CardHolderCreatedSignalArgs args)
	{
		OnCardHolderCreated(args.CardHolder);
	}

	private void OnCardHolderCreated(ICardHolder cardHolder)
	{
		if (cardHolder is CardHolderBase cardHolderBase)
		{
			RelativeToCameraViewport component = cardHolderBase.GetComponent<RelativeToCameraViewport>();
			if (!(component == null))
			{
				component.Reposition(_cameraAdapter);
				_toPosition.Add(component);
			}
		}
	}

	private void OnCardHolderDeleted(CardHolderDeletedSignalArgs args)
	{
		OnCardHolderDeleted(args.CardHolder);
	}

	private void OnCardHolderDeleted(ICardHolder cardHolder)
	{
		if (cardHolder is CardHolderBase cardHolderBase)
		{
			RelativeToCameraViewport component = cardHolderBase.GetComponent<RelativeToCameraViewport>();
			if (!(component == null))
			{
				_toPosition.Remove(component);
			}
		}
	}

	private void OnViewportChanged(CameraViewportChangedSignalArgs args)
	{
		Reposition();
	}

	private void Reposition()
	{
		foreach (RelativeToCameraViewport item in _toPosition)
		{
			item.Reposition(_cameraAdapter);
		}
	}

	public void Dispose()
	{
		_toPosition.Clear();
		_objectPool.PushObject(_toPosition, tryClear: false);
		_cardHolderCreatedSignal.Listeners -= OnCardHolderCreated;
		_cardHolderDeletedSignal.Listeners -= OnCardHolderDeleted;
		_viewportChangedSignal.Listeners -= OnViewportChanged;
	}
}
