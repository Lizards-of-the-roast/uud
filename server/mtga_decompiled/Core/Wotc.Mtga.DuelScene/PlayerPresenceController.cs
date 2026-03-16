using System;
using System.Collections.Generic;
using System.Timers;

namespace Wotc.Mtga.DuelScene;

public class PlayerPresenceController : IPlayerPresenceController, IDisposable
{
	private readonly ICardViewProvider _cardViewProvider;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly Timer _resetTimer = new Timer
	{
		Interval = 3000.0,
		AutoReset = false
	};

	private readonly List<DuelScene_CDC> _hoveredCards = new List<DuelScene_CDC>();

	private uint _hoveredId;

	private bool _isDirty;

	public PlayerPresenceController(ICardViewProvider cardViewProvider, ICardHolderProvider cardHolderProvider)
	{
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_resetTimer.Elapsed += OnResetTimerElapsed;
	}

	public void SetHoveredCardId(uint hoveredId)
	{
		_isDirty = _isDirty || hoveredId != _hoveredId;
		_hoveredId = hoveredId;
		if (_hoveredId != 0)
		{
			_resetTimer.Stop();
			_resetTimer.Start();
		}
	}

	public void Update(IEnumerable<DuelScene_CDC> allCards)
	{
		_isDirty |= _hoveredCards.Exists((DuelScene_CDC x) => x == null);
		if (!_isDirty)
		{
			return;
		}
		_isDirty = false;
		if (_cardViewProvider.TryGetCardView(_hoveredId, out var cardView))
		{
			_hoveredCards.Clear();
			if (_cardHolderProvider.TryGetCardHolder(GREPlayerNum.Opponent, CardHolderType.Hand, out var cardHolder) && cardHolder.CardViews.Contains(cardView))
			{
				_hoveredCards.AddRange(cardHolder.CardViews);
			}
			else
			{
				_hoveredCards.Add(cardView);
			}
			{
				foreach (DuelScene_CDC allCard in allCards)
				{
					if ((bool)allCard)
					{
						allCard.SetOpponentHoverState(_hoveredCards.Contains(allCard));
					}
				}
				return;
			}
		}
		foreach (DuelScene_CDC hoveredCard in _hoveredCards)
		{
			if ((bool)hoveredCard)
			{
				hoveredCard.SetOpponentHoverState(isMousedOver: false);
			}
		}
		_hoveredCards.Clear();
	}

	private void OnResetTimerElapsed(object sender, ElapsedEventArgs e)
	{
		SetHoveredCardId(0u);
		_resetTimer.Stop();
	}

	public void Dispose()
	{
		_resetTimer.Elapsed -= OnResetTimerElapsed;
		_hoveredId = 0u;
		_hoveredCards.Clear();
		_resetTimer.Dispose();
	}
}
