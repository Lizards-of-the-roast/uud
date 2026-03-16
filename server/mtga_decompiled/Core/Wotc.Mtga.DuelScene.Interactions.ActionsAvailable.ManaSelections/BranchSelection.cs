using System.Collections.Generic;
using System.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public class BranchSelection : ManaSelection
{
	private readonly uint _maxColorCount;

	private readonly bool _hasConstantCount = true;

	private readonly Dictionary<ManaColor, uint> _colorCounts;

	public override IEnumerable<ManaColor> SelectableColors => _colorCounts.Keys;

	public override uint MaxColorCount => _maxColorCount;

	public override bool HasConstantCount => _hasConstantCount;

	public BranchSelection(Dictionary<ManaColor, uint> colorCounts)
	{
		Init(new List<ManaGroup>());
		_colorCounts = colorCounts;
		_hasConstantCount = true;
		_maxColorCount = colorCounts.Values.Max();
		foreach (uint value in colorCounts.Values)
		{
			if (value != _maxColorCount)
			{
				_hasConstantCount = false;
				break;
			}
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
