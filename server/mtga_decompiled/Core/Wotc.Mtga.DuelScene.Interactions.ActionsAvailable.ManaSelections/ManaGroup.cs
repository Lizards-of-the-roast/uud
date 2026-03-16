using System.Collections.Generic;
using System.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public struct ManaGroup
{
	public readonly (Action, ManaPaymentOption) ManaAction;

	private readonly Dictionary<ManaColor, uint> _colorCounts;

	public IEnumerable<ManaColor> DistinctColors => _colorCounts.Keys;

	public ManaGroup(IEnumerable<ManaInfo> manaInfos, (Action, ManaPaymentOption) manaAction)
	{
		ManaAction = manaAction;
		_colorCounts = new Dictionary<ManaColor, uint>();
		foreach (ManaInfo manaInfo in manaInfos)
		{
			ManaColor color = manaInfo.Color;
			AddColor(color, manaInfo.Count);
		}
	}

	internal void AddColor(ManaColor color, uint count)
	{
		if (_colorCounts.ContainsKey(color))
		{
			_colorCounts[color] += count;
		}
		else
		{
			_colorCounts.Add(color, count);
		}
	}

	public bool ColorsMatch(ManaGroup colorGroup)
	{
		return ColorsMatch(colorGroup, considerCount: true);
	}

	public bool ColorsMatch(ManaGroup colorGroup, bool considerCount)
	{
		if (considerCount)
		{
			return _colorCounts.SequenceEqual(colorGroup._colorCounts);
		}
		return DistinctColors.SequenceEqual(colorGroup.DistinctColors);
	}

	public bool ColorsMatchOutOfOrder(ManaGroup colorGroup)
	{
		foreach (ManaColor distinctColor in colorGroup.DistinctColors)
		{
			if (!Contains(distinctColor))
			{
				return false;
			}
		}
		return true;
	}

	public bool Contains(ManaColor color)
	{
		return _colorCounts.ContainsKey(color);
	}

	public uint Count(ManaColor color)
	{
		if (Contains(color))
		{
			return _colorCounts[color];
		}
		return 0u;
	}

	public readonly uint TotalColorCount()
	{
		uint num = 0u;
		foreach (KeyValuePair<ManaColor, uint> colorCount in _colorCounts)
		{
			num += colorCount.Value;
		}
		return num;
	}

	public void Remove(ManaColor selected)
	{
		if (_colorCounts.ContainsKey(selected))
		{
			_colorCounts[selected]--;
			if (_colorCounts[selected] == 0)
			{
				_colorCounts.Remove(selected);
			}
		}
	}
}
