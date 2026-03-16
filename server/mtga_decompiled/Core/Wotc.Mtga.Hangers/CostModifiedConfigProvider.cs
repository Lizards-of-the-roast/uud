using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class CostModifiedConfigProvider : IHangerConfigProvider
{
	private class GenericManaComparer : IComparer<ManaQuantity>
	{
		public int Compare(ManaQuantity lhs, ManaQuantity rhs)
		{
			ManaColor color = lhs.Color;
			ManaColor color2 = rhs.Color;
			bool value = color == ManaColor.X;
			int num = (color2 == ManaColor.X).CompareTo(value);
			if (num != 0)
			{
				return num;
			}
			bool value2 = color == ManaColor.Generic;
			return (color2 == ManaColor.Generic).CompareTo(value2);
		}
	}

	private readonly IComparer<ManaQuantity> _genericManaSorter = new GenericManaComparer();

	private readonly IAbilityDataProvider _abilityDataProvider;

	private List<MtgAction> _actionCache = new List<MtgAction>();

	private List<HangerConfig> _hangerCache = new List<HangerConfig>();

	public CostModifiedConfigProvider(IAbilityDataProvider abilityDataProvider)
	{
		_abilityDataProvider = abilityDataProvider ?? NullAbilityDataProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		_actionCache.Clear();
		_hangerCache.Clear();
		foreach (ActionInfo action in model.Actions)
		{
			_actionCache.Add(MtgAction.Convert(action, _abilityDataProvider));
		}
		CostModifiedHanger.ActionTypeFilter(ref _actionCache);
		CostModifiedHanger.NullAbilityFilter(ref _actionCache);
		CostModifiedHanger.ManaCostFilter(ref _actionCache);
		foreach (MtgAction item in _actionCache)
		{
			List<ManaQuantity> list = new List<ManaQuantity>(item.ActionCost);
			list.Sort(_genericManaSorter);
			_hangerCache.Add(CostModifiedHanger.CreateConfig(list));
		}
		return _hangerCache;
	}
}
