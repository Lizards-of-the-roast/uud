using System.Collections.Generic;
using System.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public class StandardSelection : ManaSelection
{
	private readonly Dictionary<ManaColor, uint> _colorCounts;

	private bool _hasConstantCount = true;

	public override IEnumerable<ManaColor> SelectableColors => _colorCounts.Keys;

	public override uint MaxColorCount => _colorCounts.Values.Max();

	public override bool HasConstantCount => _hasConstantCount;

	public StandardSelection()
	{
		_colorCounts = new Dictionary<ManaColor, uint>();
	}

	public override void Init(List<ManaGroup> manaGroups)
	{
		base.Init(manaGroups);
		GenerateCounts(manaGroups);
	}

	public override void Prune(IEnumerable<(Action, ManaPaymentOption)> paymentOptions)
	{
		base.Prune(paymentOptions);
		Init(_manaGroups);
	}

	private void GenerateCounts(IEnumerable<ManaGroup> manaGroups)
	{
		_colorCounts.Clear();
		foreach (ManaGroup manaGroup in manaGroups)
		{
			foreach (ManaColor distinctColor in manaGroup.DistinctColors)
			{
				uint num = manaGroup.Count(distinctColor);
				if (!_colorCounts.ContainsKey(distinctColor))
				{
					_colorCounts.Add(distinctColor, num);
				}
				else if (_colorCounts[distinctColor] < num)
				{
					_colorCounts[distinctColor] = num;
				}
			}
		}
		if (_colorCounts.Count > 0)
		{
			uint firstCount = _colorCounts.First().Value;
			_hasConstantCount = _colorCounts.Values.All((uint x) => x == firstCount);
		}
	}

	public override uint CountForColor(ManaColor color)
	{
		if (_colorCounts.ContainsKey(color))
		{
			return _colorCounts[color];
		}
		return 1u;
	}
}
