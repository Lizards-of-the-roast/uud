using System.Collections.Generic;
using Pooling;

namespace Wotc.Mtga.DuelScene.CardView;

public class NoDuplicatesDecorator : IVisualStateCardProvider
{
	private readonly IVisualStateCardProvider _provider;

	private readonly IObjectPool _objPool;

	public NoDuplicatesDecorator(IObjectPool objPool, IVisualStateCardProvider provider)
	{
		_provider = provider ?? new NullVisualCardProvider();
		_objPool = objPool ?? new ObjectPool();
	}

	public IEnumerable<DuelScene_CDC> GetCardViews()
	{
		HashSet<DuelScene_CDC> hashSet = _objPool.PopObject<HashSet<DuelScene_CDC>>();
		foreach (DuelScene_CDC cardView in _provider.GetCardViews())
		{
			if (hashSet.Add(cardView))
			{
				yield return cardView;
			}
		}
		hashSet.Clear();
		_objPool.PushObject(hashSet);
	}
}
