using System.Collections.Generic;
using Pooling;

namespace Wotc.Mtga.DuelScene;

public class RelatedCardIdProvider : IRelatedCardIdProvider
{
	private readonly GameManager _gameManager;

	private readonly IObjectPool _objectPool;

	public RelatedCardIdProvider(GameManager gameManager, IObjectPool objectPool)
	{
		_gameManager = gameManager;
		_objectPool = objectPool ?? NullObjectPool.Default;
	}

	public IEnumerable<uint> GetRelatedIds(DuelScene_CDC card)
	{
		HashSet<uint> relatedIds = _objectPool.PopObject<HashSet<uint>>();
		CardViewUtilities.GetListOfReferencedCards(card, _gameManager, relatedIds);
		foreach (uint item in relatedIds)
		{
			yield return item;
		}
		relatedIds.Clear();
		_objectPool.PushObject(relatedIds, tryClear: false);
	}
}
