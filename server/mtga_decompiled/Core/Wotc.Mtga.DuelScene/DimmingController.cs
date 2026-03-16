using System;
using System.Collections.Generic;
using Pooling;
using WorkflowVisuals;

namespace Wotc.Mtga.DuelScene;

public class DimmingController : IDimmingController, IDisposable
{
	private readonly IObjectPool _objPool;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly Dictionary<DuelScene_CDC, bool> _workflowDimming;

	private readonly Dictionary<DuelScene_CDC, bool> _browserDimming;

	private bool _dirty;

	private bool _browserDefault;

	private bool _workflowDefault;

	public DimmingController(IObjectPool objPool, ICardViewProvider cardViewProvider)
	{
		_objPool = objPool ?? NullObjectPool.Default;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_workflowDimming = _objPool.PopObject<Dictionary<DuelScene_CDC, bool>>();
		_browserDimming = _objPool.PopObject<Dictionary<DuelScene_CDC, bool>>();
	}

	public void SetDirty()
	{
		_dirty = true;
	}

	public void SetBrowserDimming(Dictionary<DuelScene_CDC, bool> dimming, bool defaultState)
	{
		_dirty = true;
		_browserDimming.Clear();
		_browserDefault = defaultState;
		if (dimming == null)
		{
			return;
		}
		foreach (KeyValuePair<DuelScene_CDC, bool> item in dimming)
		{
			_browserDimming[item.Key] = item.Value;
		}
	}

	public void SetWorkflowDimming(Dimming workflowDimming)
	{
		_dirty = true;
		_workflowDimming.Clear();
		_workflowDefault = workflowDimming.WorkflowActive;
		foreach (KeyValuePair<uint, bool> item in workflowDimming.IdToIsDimmed)
		{
			if (_cardViewProvider.TryGetCardView(item.Key, out var cardView))
			{
				_workflowDimming[cardView] = item.Value;
			}
		}
	}

	private bool GetDimmingForCardView(DuelScene_CDC cardView)
	{
		ICardHolder currentCardHolder = cardView.CurrentCardHolder;
		if ((currentCardHolder.CardHolderType == CardHolderType.Library || currentCardHolder.CardHolderType == CardHolderType.Graveyard) && (currentCardHolder.GetIndexForCard(cardView) != currentCardHolder.CardViews.Count - 1 || cardView.Model.IsDisplayedFaceDown))
		{
			return false;
		}
		if (_browserDimming.Count > 0)
		{
			if (!_browserDimming.TryGetValue(cardView, out var value))
			{
				return _browserDefault;
			}
			return value;
		}
		if (_workflowDimming.Count > 0)
		{
			if (!_workflowDimming.TryGetValue(cardView, out var value2))
			{
				return _workflowDefault;
			}
			return value2;
		}
		if (cardView.Model == null)
		{
			return false;
		}
		if (cardView.CurrentCardHolder.CardHolderType == CardHolderType.Hand || cardView.CurrentCardHolder.CardHolderType == CardHolderType.Stack || cardView.CurrentCardHolder.CardHolderType == CardHolderType.Command || cardView.CurrentCardHolder.CardHolderType == CardHolderType.Graveyard || cardView.CurrentCardHolder.CardHolderType == CardHolderType.Library || cardView.CurrentCardHolder.CardHolderType == CardHolderType.CardBrowserDefault || cardView.CurrentCardHolder.CardHolderType == CardHolderType.CardBrowserViewDismiss)
		{
			return false;
		}
		return cardView.Model.IsTapped;
	}

	public void UpdateDimming(IEnumerable<DuelScene_CDC> cardViews)
	{
		if (!_dirty)
		{
			return;
		}
		foreach (DuelScene_CDC cardView in cardViews)
		{
			if (!(cardView == null))
			{
				cardView.SetDimmedState(GetDimmingForCardView(cardView));
			}
		}
		_dirty = false;
	}

	public void Dispose()
	{
		_workflowDimming.Clear();
		_objPool.PushObject(_workflowDimming, tryClear: false);
		_browserDimming.Clear();
		_objPool.PushObject(_browserDimming, tryClear: false);
	}
}
