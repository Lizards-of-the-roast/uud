using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public class ManaSourceCollection
{
	public readonly uint SourceId;

	private readonly List<ManaGroup> _manaGroups;

	private readonly HashSet<ManaColor> _distinctColors;

	private readonly Dictionary<SelectionType, ManaSourceCollection> _branchingCollections;

	private readonly IAbilityDataProvider _abilityDataProvider;

	public uint SelectionCount { get; private set; }

	public SelectionType SelectionType { get; private set; }

	public IEnumerable<ManaGroup> ManaGroups => _manaGroups;

	public IEnumerable<ManaColor> DistinctColors => _distinctColors;

	public bool WillTap { get; private set; }

	public IReadOnlyDictionary<SelectionType, ManaSourceCollection> BranchingCollections => _branchingCollections;

	public ManaSourceCollection(IAbilityDataProvider abilityDataProvider, uint sourceId, IEnumerable<ManaInfo> manaInfo, (Wotc.Mtgo.Gre.External.Messaging.Action, ManaPaymentOption) manaAction)
	{
		_abilityDataProvider = abilityDataProvider;
		_manaGroups = new List<ManaGroup>();
		_distinctColors = new HashSet<ManaColor>();
		_branchingCollections = new Dictionary<SelectionType, ManaSourceCollection>();
		SourceId = sourceId;
		SelectionType = DetermineSelectionType(manaInfo);
		AddManaGroup(manaInfo, manaAction);
		SelectionCount = DetermineSelectionCount(_manaGroups[0]);
		WillTap = CheckForTapAbility(abilityDataProvider, manaInfo);
	}

	private bool CheckForTapAbility(IAbilityDataProvider abilityDataProvider, IEnumerable<ManaInfo> manaInfos)
	{
		foreach (ManaInfo manaInfo in manaInfos)
		{
			AbilityPrintingData abilityPrintingById = abilityDataProvider.GetAbilityPrintingById(manaInfo.AbilityGrpId);
			if (abilityPrintingById != null && abilityPrintingById.PaymentType == AbilityPaymentType.TapSymbol)
			{
				return true;
			}
		}
		return false;
	}

	private SelectionType DetermineSelectionType(IEnumerable<ManaInfo> manaInfos)
	{
		using (IEnumerator<ManaInfo> enumerator = manaInfos.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				ManaInfo current = enumerator.Current;
				if (current.Color == ManaColor.AnyColor)
				{
					return SelectionType.AnyColor;
				}
				if (_abilityDataProvider.TryGetAbilityRecordById(current.AbilityGrpId, out var record) && record.Tags != null && record.Tags.Contains(MetaDataTag.SpecificManaColorCombination))
				{
					return SelectionType.SpecificColorCombination;
				}
			}
		}
		return SelectionType.Standard;
	}

	private uint DetermineSelectionCount(ManaGroup firstColorGroup)
	{
		return SelectionType switch
		{
			SelectionType.Standard => (uint)DistinctColors.Count(), 
			SelectionType.AnyColor => firstColorGroup.Count(ManaColor.AnyColor), 
			SelectionType.SpecificColorCombination => firstColorGroup.TotalColorCount(), 
			_ => throw new ArgumentException($"No count defined for {SelectionType}, did you forget to add a case here?"), 
		};
	}

	public bool DetermineIfGlobalExtra(ManaSourceCollection sourceCollection)
	{
		if (_manaGroups.Count == 0)
		{
			return true;
		}
		if (SelectionType == SelectionType.AnyColor && _manaGroups.Count == 1)
		{
			return false;
		}
		if (_manaGroups.Count != sourceCollection._manaGroups.Count)
		{
			return false;
		}
		ManaGroup colorGroup = _manaGroups[0];
		foreach (ManaGroup manaGroup in _manaGroups)
		{
			if (!manaGroup.ColorsMatch(colorGroup))
			{
				return false;
			}
		}
		return true;
	}

	public bool DetermineIfSelectionExtra(ManaSourceCollection sourceCollection)
	{
		foreach (ManaGroup manaGroup in _manaGroups)
		{
			if (!sourceCollection.HasSameManaGroup(manaGroup))
			{
				return false;
			}
		}
		return true;
	}

	public bool DetermineIfAdditionalProducedMana(ManaSourceCollection sourceCollection)
	{
		foreach (ManaGroup manaGroup in _manaGroups)
		{
			if (!sourceCollection.HasSimilarManaGroup(manaGroup))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasNoOptions()
	{
		if (_branchingCollections.Count > 0 || _manaGroups.Count() <= 1)
		{
			return false;
		}
		for (int i = 0; i < _manaGroups.Count() - 1; i++)
		{
			if (!_manaGroups[i].ColorsMatch(_manaGroups[i + 1]))
			{
				return false;
			}
		}
		return true;
	}

	public void AddManaGroup(IEnumerable<ManaInfo> manaInfos, (Wotc.Mtgo.Gre.External.Messaging.Action, ManaPaymentOption) manaAction)
	{
		SelectionType selectionType = DetermineSelectionType(manaInfos);
		ManaSourceCollection value;
		if (selectionType == SelectionType)
		{
			ManaGroup item = new ManaGroup(manaInfos, manaAction);
			_manaGroups.Add(item);
			_distinctColors.UnionWith(item.DistinctColors);
		}
		else if (_branchingCollections.TryGetValue(selectionType, out value))
		{
			value.AddManaGroup(manaInfos, manaAction);
		}
		else
		{
			_branchingCollections.Add(selectionType, new ManaSourceCollection(_abilityDataProvider, SourceId, manaInfos, manaAction));
		}
	}

	internal void AddToExistingGroups(ManaGroup group)
	{
		ManaGroup manaGroup = _manaGroups.FirstOrDefault(delegate(ManaGroup x)
		{
			if (x.ColorsMatch(group, considerCount: false))
			{
				var (action, manaPaymentOption) = x.ManaAction;
				var (action2, manaPaymentOption2) = group.ManaAction;
				if (action == action2)
				{
					return manaPaymentOption == manaPaymentOption2;
				}
				return false;
			}
			return false;
		});
		if (manaGroup.Equals(default(ManaGroup)) || manaGroup.DistinctColors == null)
		{
			return;
		}
		foreach (ManaColor distinctColor in group.DistinctColors)
		{
			manaGroup.AddColor(distinctColor, group.Count(distinctColor));
		}
	}

	internal void CombineCollections(ManaSourceCollection collection)
	{
		_manaGroups.AddRange(collection._manaGroups);
	}

	public ManaGroup GetFirstGroup()
	{
		return _manaGroups[0];
	}

	public bool HasSameManaGroup(ManaGroup group)
	{
		return _manaGroups.Exists(group, delegate(ManaGroup current, ManaGroup compareTo)
		{
			if (current.ColorsMatch(compareTo, considerCount: false))
			{
				var (action, manaPaymentOption) = current.ManaAction;
				var (action2, manaPaymentOption2) = compareTo.ManaAction;
				if (action == action2)
				{
					return manaPaymentOption == manaPaymentOption2;
				}
				return false;
			}
			return false;
		});
	}

	private bool HasSimilarManaGroup(ManaGroup group)
	{
		return _manaGroups.Exists(group, delegate(ManaGroup current, ManaGroup compareTo)
		{
			if (current.ColorsMatchOutOfOrder(compareTo))
			{
				var (action, manaPaymentOption) = current.ManaAction;
				var (action2, manaPaymentOption2) = compareTo.ManaAction;
				if (action == action2)
				{
					return manaPaymentOption == manaPaymentOption2;
				}
				return false;
			}
			return false;
		});
	}
}
