using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardLayout_MultiLayout : ICardLayout
{
	private readonly List<ICardLayout> _layouts;

	private IGroupedCardProvider _cardGroupProvider;

	private readonly List<Vector3> _layoutCenters;

	private readonly List<Quaternion> _layoutRotations;

	public CardLayout_MultiLayout(List<ICardLayout> layouts, List<Vector3> layoutCenters, List<Quaternion> layoutRotations, IGroupedCardProvider cardGroupProvider)
	{
		_layouts = layouts;
		_cardGroupProvider = cardGroupProvider;
		_layoutCenters = layoutCenters;
		_layoutRotations = layoutRotations;
	}

	public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		List<List<DuelScene_CDC>> cardGroups = _cardGroupProvider.GetCardGroups();
		List<CardLayoutData> allData2 = new List<CardLayoutData>();
		List<List<CardLayoutData>> list = new List<List<CardLayoutData>>();
		for (int i = 0; i < cardGroups.Count; i++)
		{
			Vector3 center2 = _layoutCenters[i];
			center2 += center;
			_layouts[i].GenerateData(cardGroups[i], ref allData2, center2, _layoutRotations[i]);
			list.Add(allData2.ToList());
			allData2.Clear();
		}
		foreach (List<CardLayoutData> item in list)
		{
			allData.AddRange(item.ToList());
		}
	}

	public ICardLayout GetLayout(int layoutIndex)
	{
		return _layouts[layoutIndex];
	}

	public IEnumerable<ICardLayout> GetLayouts()
	{
		return _layouts;
	}
}
