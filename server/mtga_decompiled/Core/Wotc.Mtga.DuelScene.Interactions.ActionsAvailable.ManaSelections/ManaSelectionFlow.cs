using System;
using System.Collections.Generic;
using System.Linq;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public class ManaSelectionFlow
{
	internal class ManaOrganizer
	{
		private readonly IAbilityDataProvider _abilityDataProvider;

		private static readonly IReadOnlyList<uint> ExtraManaAbilityIds = new List<uint> { 176328u };

		internal ManaOrganizer(IAbilityDataProvider abilityDataProvider)
		{
			_abilityDataProvider = abilityDataProvider;
		}

		internal IReadOnlyCollection<ManaSourceCollection> ToCollections(IEnumerable<Wotc.Mtgo.Gre.External.Messaging.Action> actions)
		{
			Dictionary<uint, ManaSourceCollection> dictionary = new Dictionary<uint, ManaSourceCollection>();
			foreach (Wotc.Mtgo.Gre.External.Messaging.Action action in actions)
			{
				foreach (ManaPaymentOption manaPaymentOption in action.ManaPaymentOptions)
				{
					(Wotc.Mtgo.Gre.External.Messaging.Action, ManaPaymentOption) manaAction = (action, manaPaymentOption);
					foreach (IGrouping<uint, ManaInfo> item in from y in manaPaymentOption.Mana
						group y by y.SrcInstanceId)
					{
						if (item.Count() == 0)
						{
							continue;
						}
						uint key = item.Key;
						List<ManaInfo> list = item.ToList();
						if (list.Exists((ManaInfo m) => ExtraManaAbilityIds.Contains(m.AbilityGrpId)))
						{
							List<ManaInfo> list2;
							(list2, list) = list.Partition((ManaInfo m) => ExtraManaAbilityIds.Contains(m.AbilityGrpId));
							AddToGroups(dictionary, list2[0].AbilityGrpId, list2, manaAction);
						}
						AddToGroups(dictionary, key, list, manaAction);
					}
				}
			}
			return dictionary.Values;
		}

		private void AddToGroups(Dictionary<uint, ManaSourceCollection> groups, uint sourceId, List<ManaInfo> manaList, (Wotc.Mtgo.Gre.External.Messaging.Action, ManaPaymentOption) manaAction)
		{
			if (!groups.ContainsKey(sourceId))
			{
				groups.Add(sourceId, new ManaSourceCollection(_abilityDataProvider, sourceId, manaList, manaAction));
			}
			else
			{
				groups[sourceId].AddManaGroup(manaList, manaAction);
			}
		}
	}

	private readonly uint _sourceId;

	private int _currentTreeIndex;

	private readonly List<IManaSelectionTree> _selectionTrees = new List<IManaSelectionTree>();

	private readonly List<ManaColor> _extraColors = new List<ManaColor>();

	private readonly ManaOrganizer _manaOrganizer;

	public IReadOnlyList<ManaColor> ExtraColors => _extraColors;

	public uint MaxSelections { get; private set; }

	public ManaSelectionFlow(uint sourceId, IAbilityDataProvider abilityDataProvider)
	{
		_sourceId = sourceId;
		_manaOrganizer = new ManaOrganizer(abilityDataProvider);
	}

	public void CreateTrees(IEnumerable<Wotc.Mtgo.Gre.External.Messaging.Action> actions)
	{
		IEnumerable<ManaSourceCollection> manaCollections = OrganizeManaIntoCollections(actions);
		CreateTreesFromManaCollections(manaCollections, _extraColors, _selectionTrees);
	}

	public void CreateSourceTree(IEnumerable<Wotc.Mtgo.Gre.External.Messaging.Action> actions, uint sourceId)
	{
		ManaSourceCollection item = OrganizeManaIntoCollections(actions).FirstOrDefault((ManaSourceCollection x) => x.SourceId == sourceId);
		List<ManaSourceCollection> manaCollections = new List<ManaSourceCollection> { item };
		CreateTreesFromManaCollections(manaCollections, _extraColors, _selectionTrees);
	}

	public IEnumerable<ManaSourceCollection> OrganizeManaIntoCollections(IEnumerable<Wotc.Mtgo.Gre.External.Messaging.Action> actions)
	{
		return _manaOrganizer.ToCollections(actions);
	}

	public void CreateTreesFromManaCollections(IEnumerable<ManaSourceCollection> manaCollections, List<ManaColor> extraColors, List<IManaSelectionTree> selectionTreeList)
	{
		ManaSourceCollection manaSourceCollection = manaCollections.Find(_sourceId, (ManaSourceCollection m, uint paramSourceId) => m.SourceId == paramSourceId);
		uint totalSelectionCount = 0u;
		List<ManaSourceCollection> list = null;
		foreach (ManaSourceCollection manaCollection in manaCollections)
		{
			if (manaCollection == manaSourceCollection)
			{
				continue;
			}
			if (manaCollection.DetermineIfGlobalExtra(manaSourceCollection))
			{
				AddToExtraColors(manaCollection, extraColors);
			}
			else if (manaCollection.DetermineIfSelectionExtra(manaSourceCollection))
			{
				AddToExistingCollection(manaCollection, manaSourceCollection);
			}
			else if (manaCollection.DetermineIfAdditionalProducedMana(manaSourceCollection))
			{
				if (list == null)
				{
					list = new List<ManaSourceCollection>();
				}
				list.Add(manaCollection);
			}
			else
			{
				CreateSelectionTree(manaCollection, selectionTreeList, ref totalSelectionCount);
			}
		}
		if (list != null)
		{
			foreach (ManaSourceCollection item in list)
			{
				CreateSelectionTree(item, selectionTreeList, ref totalSelectionCount);
			}
		}
		if (manaSourceCollection.HasNoOptions() && manaCollections.Count() > 1)
		{
			AddToExtraColors(manaSourceCollection, extraColors);
		}
		else
		{
			CreateSelectionTree(0, manaSourceCollection, selectionTreeList, ref totalSelectionCount);
		}
		MaxSelections = totalSelectionCount;
	}

	private void AddToExtraColors(ManaSourceCollection collection, List<ManaColor> extraColors)
	{
		foreach (ManaColor distinctColor in collection.DistinctColors)
		{
			for (int i = 0; i < collection.GetFirstGroup().Count(distinctColor); i++)
			{
				extraColors.Add(distinctColor);
			}
		}
	}

	private void AddToExistingCollection(ManaSourceCollection collectionToAdd, ManaSourceCollection existingCollection)
	{
		foreach (ManaGroup manaGroup in collectionToAdd.ManaGroups)
		{
			existingCollection.AddToExistingGroups(manaGroup);
		}
	}

	private void CreateSelectionTree(ManaSourceCollection collection, List<IManaSelectionTree> selectionTreeList, ref uint totalSelectionCount)
	{
		IManaSelectionTree tree = SelectionTreeFactory.GetTree(collection);
		selectionTreeList.Add(tree);
		totalSelectionCount += tree.SelectionCount;
	}

	private void CreateSelectionTree(int insertIndex, ManaSourceCollection collection, List<IManaSelectionTree> selectionTreeList, ref uint totalSelectionCount)
	{
		IManaSelectionTree tree = SelectionTreeFactory.GetTree(collection);
		selectionTreeList.Insert(insertIndex, tree);
		totalSelectionCount += tree.SelectionCount;
	}

	public bool CanSubmit()
	{
		foreach (IManaSelectionTree selectionTree in _selectionTrees)
		{
			if (!selectionTree.AllSelectionsComplete)
			{
				return false;
			}
		}
		return true;
	}

	public bool Submit(Action<GreInteraction> submitInteraction)
	{
		if (CanSubmit())
		{
			(Wotc.Mtgo.Gre.External.Messaging.Action, ManaPaymentOption) paymentOption = _selectionTrees.Last().GetPaymentOption();
			if (paymentOption.Item1 != null && paymentOption.Item2 != null)
			{
				Wotc.Mtgo.Gre.External.Messaging.Action action = new Wotc.Mtgo.Gre.External.Messaging.Action(paymentOption.Item1);
				action.ManaPaymentOptions.Clear();
				action.ManaPaymentOptions.Add(paymentOption.Item2);
				submitInteraction(new GreInteraction(action));
				return true;
			}
		}
		return false;
	}

	public bool SubmitSourceOnly(Action<GreInteraction> submitInteraction)
	{
		if (CanSubmit())
		{
			(Wotc.Mtgo.Gre.External.Messaging.Action, ManaPaymentOption) paymentOption = _selectionTrees.Last().GetPaymentOption();
			if (paymentOption.Item1 != null)
			{
				Wotc.Mtgo.Gre.External.Messaging.Action greAction = new Wotc.Mtgo.Gre.External.Messaging.Action(paymentOption.Item1);
				submitInteraction(new GreInteraction(greAction));
				return true;
			}
		}
		return false;
	}

	public IManaSelectionTree GetTreeAtIndex(int index)
	{
		if (_selectionTrees.Count > index)
		{
			return _selectionTrees[index];
		}
		return null;
	}

	public IManaSelectionTree GetActiveTree()
	{
		return _selectionTrees[_currentTreeIndex];
	}

	public IManaSelectionTree GetNextTree()
	{
		IManaSelectionTree previousTree = _selectionTrees[_currentTreeIndex];
		_currentTreeIndex++;
		if (_selectionTrees.Count > _currentTreeIndex)
		{
			IManaSelectionTree manaSelectionTree = _selectionTrees[_currentTreeIndex];
			manaSelectionTree.Prune(previousTree);
			return manaSelectionTree;
		}
		_currentTreeIndex--;
		return null;
	}
}
