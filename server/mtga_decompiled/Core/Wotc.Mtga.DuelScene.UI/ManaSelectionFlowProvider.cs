using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI;

public class ManaSelectionFlowProvider : IManaSelectorProvider
{
	private readonly List<ManaColorSelector.ManaProducedData> _validSelections = new List<ManaColorSelector.ManaProducedData>();

	private readonly List<ManaColor> _selected = new List<ManaColor>();

	private readonly ManaSelectionFlow _flow;

	private readonly ICardDataAdapter _model;

	public IEnumerable<ManaColorSelector.ManaProducedData> ValidSelections => _validSelections;

	public int ValidSelectionCount => _validSelections.Count;

	public IReadOnlyCollection<ManaColor> SelectedColors => _selected;

	public int CurrentSelection { get; private set; }

	public uint MaxSelections => _flow.MaxSelections;

	public bool AllSelectionsComplete => _flow.CanSubmit();

	public bool WillTap => _flow.GetActiveTree().WillTap;

	public uint? CurrentConstantCount => GetConstantCountForSelection(CurrentSelection);

	public ManaSelectionFlowProvider(ManaSelectionFlow flow, ICardDataAdapter model)
	{
		_flow = flow;
		_model = model;
		_validSelections = GetValidSelectionsFromTree(flow.GetActiveTree(), model);
		_selected.AddRange(flow.ExtraColors);
	}

	private List<ManaColorSelector.ManaProducedData> GetValidSelectionsFromTree(IManaSelectionTree tree, ICardDataAdapter model)
	{
		List<ManaColorSelector.ManaProducedData> list = new List<ManaColorSelector.ManaProducedData>();
		if (tree == null || tree.SelectableColors == null)
		{
			return list;
		}
		foreach (ManaColor selectableColor in tree.SelectableColors)
		{
			uint countOfColor = tree.AmountForColor(selectableColor);
			list.Add(new ManaColorSelector.ManaProducedData
			{
				PrimaryColor = selectableColor,
				CountOfColor = countOfColor
			});
		}
		ManaProviderUtils.Sort(ref list, model);
		return list;
	}

	public uint? GetConstantCountForSelection(int index)
	{
		return _flow.GetTreeAtIndex(index)?.ConstantManaCount();
	}

	public uint? GetBranchingSelectionCount(int index, ManaColor color)
	{
		return _flow.GetTreeAtIndex(index)?.BranchingSelectionCount(color);
	}

	public ManaColorSelector.ManaProducedData GetElementAt(int index)
	{
		if (_validSelections.Count > index)
		{
			return _validSelections[index];
		}
		return default(ManaColorSelector.ManaProducedData);
	}

	public bool ContainsColor(ManaColor color)
	{
		return _validSelections.Exists((ManaColorSelector.ManaProducedData x) => x.PrimaryColor == color);
	}

	public void Select(ManaColor color)
	{
		IManaSelectionTree manaSelectionTree = _flow?.GetActiveTree();
		CurrentSelection++;
		if (manaSelectionTree == null)
		{
			return;
		}
		for (int i = 0; i < _flow.GetActiveTree().AmountForColor(color); i++)
		{
			_selected.Add(color);
		}
		manaSelectionTree.Next(color);
		if (manaSelectionTree.AllSelectionsComplete)
		{
			if (!_flow.CanSubmit())
			{
				manaSelectionTree = _flow.GetNextTree();
				if (manaSelectionTree == null)
				{
					Debug.LogError("ManaSelectionFlow can't submit but there is no next tree!");
				}
				else
				{
					SetupNextSelection();
				}
			}
		}
		else
		{
			SetupNextSelection();
		}
	}

	private void SetupNextSelection()
	{
		_validSelections.Clear();
		_validSelections.AddRange(GetValidSelectionsFromTree(_flow.GetActiveTree(), _model));
		if (_validSelections.Count == 1)
		{
			ManaColor primaryColor = _validSelections[0].PrimaryColor;
			Select(primaryColor);
		}
	}

	public void Cleanup()
	{
		CurrentSelection = 0;
		_selected.Clear();
		_validSelections.Clear();
	}
}
