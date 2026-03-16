using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public class ColorCombinationSelection : ManaSelection
{
	private readonly HashSet<ManaColor> _colors = new HashSet<ManaColor>();

	public override IEnumerable<ManaColor> SelectableColors => _colors;

	public override uint MaxColorCount => 1u;

	public override bool HasConstantCount => true;

	public override void Init(List<ManaGroup> manaGroups)
	{
		base.Init(manaGroups);
		foreach (ManaGroup manaGroup in manaGroups)
		{
			_colors.UnionWith(manaGroup.DistinctColors);
		}
	}

	public override uint CountForColor(ManaColor color)
	{
		return 1u;
	}
}
