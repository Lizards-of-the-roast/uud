using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI;

public class ManaColorListProvider : IManaSelectorProvider
{
	private readonly List<ManaColorSelector.ManaProducedData> _validSelections;

	private readonly List<ManaColor> _selected = new List<ManaColor>();

	private readonly uint _maxSelection;

	private readonly SelectionValidationType _validationType;

	public IEnumerable<ManaColorSelector.ManaProducedData> ValidSelections => _validSelections;

	public int ValidSelectionCount => _validSelections.Count;

	public IReadOnlyCollection<ManaColor> SelectedColors => _selected;

	public int CurrentSelection { get; private set; }

	public uint MaxSelections => _maxSelection;

	public bool AllSelectionsComplete => CurrentSelection == MaxSelections;

	public bool WillTap { get; private set; }

	public uint? CurrentConstantCount => GetConstantCountForSelection(CurrentSelection);

	public ManaColorListProvider(IReadOnlyList<ManaColor> colors, uint maxSelection, SelectionValidationType validationType, ICardDataAdapter model)
	{
		_maxSelection = maxSelection;
		_validSelections = ConvertToManaProduced(colors);
		ManaProviderUtils.Sort(ref _validSelections, model);
		_validationType = validationType;
	}

	private static List<ManaColorSelector.ManaProducedData> ConvertToManaProduced(IEnumerable<ManaColor> colors)
	{
		List<ManaColorSelector.ManaProducedData> list = new List<ManaColorSelector.ManaProducedData>();
		foreach (ManaColor color in colors)
		{
			list.Add(new ManaColorSelector.ManaProducedData
			{
				PrimaryColor = color,
				CountOfColor = 1u
			});
		}
		return list;
	}

	public uint? GetConstantCountForSelection(int index)
	{
		return 1u;
	}

	public uint? GetBranchingSelectionCount(int index, ManaColor color)
	{
		return null;
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
		CurrentSelection++;
		_selected.Add(color);
		int index = _validSelections.FindIndex((ManaColorSelector.ManaProducedData c) => c.PrimaryColor == color);
		SelectionValidationType validationType = _validationType;
		if (validationType != SelectionValidationType.NonRepeatable)
		{
			_ = 2;
		}
		else
		{
			_validSelections.RemoveAt(index);
		}
	}

	public void Cleanup()
	{
		CurrentSelection = 0;
		_selected.Clear();
		_validSelections.Clear();
	}
}
