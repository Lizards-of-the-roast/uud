using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public class IsSameCardDataComparer : IEqualityComparer<ICardDataAdapter>
{
	private readonly GameManager _gameManager;

	public IsSameCardDataComparer(GameManager gameManager)
	{
		_gameManager = gameManager;
	}

	public bool Equals(ICardDataAdapter x, ICardDataAdapter y)
	{
		return CardViewUtilities.IsSame(x, y, _gameManager);
	}

	public int GetHashCode(ICardDataAdapter obj)
	{
		throw new NotImplementedException("Hash code not used in this context.");
	}
}
