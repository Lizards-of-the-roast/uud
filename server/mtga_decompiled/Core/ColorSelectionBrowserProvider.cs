using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

public class ColorSelectionBrowserProvider : ICardBrowserProvider, IDuelSceneBrowserProvider
{
	public string BrowserHeaderText { get; private set; }

	public IReadOnlyList<ManaColor> ManaColors { get; private set; }

	public uint ColorCount { get; private set; }

	public uint SourceId { get; private set; }

	public bool CanCancel { get; private set; }

	public bool ApplyTargetOffset => false;

	public bool ApplySourceOffset => false;

	public bool ApplyControllerOffset => false;

	public ColorSelectionBrowserProvider(string headerText, IReadOnlyList<ManaColor> manaColors, uint colorCount, bool canCancel, uint sourceId = 0u)
	{
		BrowserHeaderText = headerText;
		ManaColors = manaColors ?? Array.Empty<ManaColor>();
		ColorCount = colorCount;
		CanCancel = canCancel;
		SourceId = sourceId;
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.ColorSection;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return null;
	}

	public string GetCardHolderLayoutKey()
	{
		return "ColorSection";
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
	}
}
