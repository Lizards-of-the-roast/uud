using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Core.Meta.MainNavigation.BoosterChamber;

public class CardDataAndRevealStatus
{
	public Action OnRevealed;

	public CardData CardData;

	public CardData RebalancedCardData;

	public List<string> Tags = new List<string>();

	public int factionCollationId;

	public string factionTag;

	public bool InFinalPosition;

	public bool NeedsAnticipation;

	public bool AutoReveal;

	private bool _revealed;

	public bool Revealed
	{
		get
		{
			return _revealed;
		}
		set
		{
			if (_revealed != value)
			{
				_revealed = value;
				if (_revealed && OnRevealed != null)
				{
					OnRevealed();
					OnRevealed = null;
				}
			}
		}
	}
}
