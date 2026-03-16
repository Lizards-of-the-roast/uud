using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class GlobalCommandCardHolder : ZoneCardHolderBase, ICommandCardHolder, ICardHolder
{
	private readonly Dictionary<uint, ICardHolder> _playerIdToCardHolderMap = new Dictionary<uint, ICardHolder>();

	private readonly Dictionary<DuelScene_CDC, ICardHolder> _relativeCardHolderMap = new Dictionary<DuelScene_CDC, ICardHolder>();

	public IEnumerable<ICardHolder> GetAllSubCardHolders()
	{
		foreach (KeyValuePair<uint, ICardHolder> item in _playerIdToCardHolderMap)
		{
			yield return item.Value;
		}
	}

	public void AddSubCardHolderForPlayer(ICardHolder cardHolder, uint playerId)
	{
		_playerIdToCardHolderMap[playerId] = cardHolder;
		if (cardHolder is CardHolderBase cardHolderBase)
		{
			cardHolderBase.transform.SetParent(base.transform);
		}
	}

	public ICardHolder GetSubCardHolderForPlayer(uint playerId)
	{
		if (!_playerIdToCardHolderMap.TryGetValue(playerId, out var value))
		{
			return null;
		}
		return value;
	}

	public bool RemoveSubCardHolderForPlayer(uint playerId)
	{
		return _playerIdToCardHolderMap.Remove(playerId);
	}

	public override void AddCard(DuelScene_CDC cardView)
	{
		if (!(cardView == null))
		{
			uint valueOrDefault = (cardView.Model?.Owner?.InstanceId).GetValueOrDefault();
			if (_playerIdToCardHolderMap.TryGetValue(valueOrDefault, out var value))
			{
				_relativeCardHolderMap[cardView] = value;
				value.AddCard(cardView);
			}
		}
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		if (_relativeCardHolderMap.TryGetValue(cardView, out var value))
		{
			value.RemoveCard(cardView);
		}
	}

	protected override void OnDestroy()
	{
		_playerIdToCardHolderMap.Clear();
		_relativeCardHolderMap.Clear();
		base.OnDestroy();
	}
}
