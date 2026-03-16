using System.Collections.Generic;
using System.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public class BranchingManaSelectionTree : IManaSelectionTree
{
	private readonly BranchSelection _selection;

	private IManaSelectionTree _chosenTree;

	private readonly Dictionary<ManaColor, IManaSelectionTree> _treesByColor = new Dictionary<ManaColor, IManaSelectionTree>();

	private readonly HashSet<uint> _treeSelectionCounts = new HashSet<uint>();

	public uint SourceId { get; private set; }

	public bool AllSelectionsComplete
	{
		get
		{
			if (_chosenTree == null)
			{
				return false;
			}
			return _chosenTree.AllSelectionsComplete;
		}
	}

	public bool WillTap { get; private set; }

	public IEnumerable<ManaColor> SelectableColors
	{
		get
		{
			if (_chosenTree == null)
			{
				return _selection.SelectableColors;
			}
			return _chosenTree.SelectableColors;
		}
	}

	public IEnumerable<ManaColor> SelectionTotal
	{
		get
		{
			if (_chosenTree == null)
			{
				return _selection.SelectionTotal;
			}
			return _chosenTree.SelectionTotal;
		}
	}

	public uint SelectionCount { get; private set; }

	public BranchingManaSelectionTree(IEnumerable<IManaSelectionTree> trees)
	{
		Dictionary<ManaColor, uint> dictionary = new Dictionary<ManaColor, uint>();
		foreach (IManaSelectionTree tree in trees)
		{
			SourceId = tree.SourceId;
			WillTap |= tree.WillTap;
			_treeSelectionCounts.Add(tree.SelectionCount);
			foreach (ManaColor selectableColor in tree.SelectableColors)
			{
				if (!_treesByColor.ContainsKey(selectableColor))
				{
					_treesByColor.Add(selectableColor, tree);
					dictionary.Add(selectableColor, tree.AmountForColor(selectableColor));
				}
			}
		}
		_selection = new BranchSelection(dictionary);
		SelectionCount = _treeSelectionCounts.Max();
	}

	public uint? BranchingSelectionCount(ManaColor color)
	{
		if (_chosenTree != null)
		{
			return _chosenTree.BranchingSelectionCount(color);
		}
		if (_treeSelectionCounts.Count == 1)
		{
			return null;
		}
		if (_treesByColor.TryGetValue(color, out var value))
		{
			return value.SelectionCount;
		}
		return null;
	}

	public uint AmountForColor(ManaColor color)
	{
		if (_chosenTree == null)
		{
			return _selection.CountForColor(color);
		}
		return _chosenTree.AmountForColor(color);
	}

	public uint? ConstantManaCount()
	{
		if (_chosenTree != null)
		{
			return _chosenTree.ConstantManaCount();
		}
		if (_selection.HasConstantCount)
		{
			return _selection.MaxColorCount;
		}
		return null;
	}

	public (Action, ManaPaymentOption) GetPaymentOption()
	{
		if (_chosenTree == null)
		{
			return _treesByColor.Values.First().GetPaymentOption();
		}
		return _chosenTree.GetPaymentOption();
	}

	public IEnumerable<(Action, ManaPaymentOption)> GetAllPaymentOptions()
	{
		if (_chosenTree == null)
		{
			return _treesByColor.Values.SelectMany((IManaSelectionTree t) => t.GetAllPaymentOptions());
		}
		return _chosenTree.GetAllPaymentOptions();
	}

	public void Prune(IManaSelectionTree previousTree = null)
	{
		if (_chosenTree != null)
		{
			_chosenTree.Prune(previousTree);
			return;
		}
		foreach (IManaSelectionTree value in _treesByColor.Values)
		{
			value.Prune(previousTree);
		}
	}

	public void Next(ManaColor color)
	{
		IManaSelectionTree value;
		if (_chosenTree != null)
		{
			_chosenTree.Next(color);
		}
		else if (_treesByColor.TryGetValue(color, out value))
		{
			value.Next(color);
			_chosenTree = value;
		}
	}

	public void Undo()
	{
		if (SelectionTotal.Count() == 0)
		{
			_chosenTree = null;
			_selection.ClearSelection();
		}
		else if (_chosenTree != null)
		{
			_chosenTree.Undo();
		}
	}
}
