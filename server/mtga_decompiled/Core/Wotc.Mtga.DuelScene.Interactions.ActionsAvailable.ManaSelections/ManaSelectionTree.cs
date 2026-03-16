using System.Collections.Generic;
using System.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public class ManaSelectionTree<T> : IManaSelectionTree where T : ManaSelection, new()
{
	protected readonly List<T> _selections = new List<T>();

	protected T _currentSelection;

	protected ManaSourceCollection _collection;

	public uint SourceId => _collection.SourceId;

	public uint SelectionCount => _collection.SelectionCount;

	public bool AllSelectionsComplete => _currentSelection?.SelectedCount == SelectionCount;

	public IEnumerable<ManaColor> SelectableColors => _currentSelection.SelectableColors;

	public IEnumerable<ManaColor> SelectionTotal => _currentSelection.SelectionTotal;

	public bool WillTap => _collection.WillTap;

	public uint? BranchingSelectionCount(ManaColor color)
	{
		return null;
	}

	public uint? ConstantManaCount()
	{
		if (_currentSelection.HasConstantCount)
		{
			return _currentSelection.MaxColorCount;
		}
		return null;
	}

	public uint AmountForColor(ManaColor color)
	{
		return _currentSelection.CountForColor(color);
	}

	public ManaSelectionTree(ManaSourceCollection collection)
	{
		_collection = collection;
		_currentSelection = new T();
		_selections.Add(_currentSelection);
		_currentSelection.Init(_collection.ManaGroups.ToList());
	}

	public void Prune(IManaSelectionTree previousTree = null)
	{
		_currentSelection.Prune(previousTree.GetAllPaymentOptions());
	}

	public (Action, ManaPaymentOption) GetPaymentOption()
	{
		return _currentSelection.GetManaAction();
	}

	public IEnumerable<(Action, ManaPaymentOption)> GetAllPaymentOptions()
	{
		return _currentSelection.GetAllManaActions();
	}

	public void Next(ManaColor color)
	{
		_currentSelection.Select(color);
		int count = _selections.Count;
		T val = new T();
		val.Init(_currentSelection);
		_selections.Add(val);
		_currentSelection = _selections[count];
	}

	public void Undo()
	{
		int index = _selections.Count - 1;
		_selections.RemoveAt(index);
		index = _selections.Count - 1;
		_currentSelection = _selections[index];
		_currentSelection.ClearSelection();
	}
}
