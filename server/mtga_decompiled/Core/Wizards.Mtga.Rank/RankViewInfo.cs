using System.Collections.Generic;
using Core.Shared.Code.ClientModels;

namespace Wizards.Mtga.Rank;

public class RankViewInfo
{
	public Client_RankDefinition RankDefinition;

	public string RankImageAssetPath;

	public string RankFormatLocText;

	public string RankTierLocText;

	public Dictionary<string, string> RankTierLocTextParams;

	public string RankMythicPlacementText;

	public bool IsMythic;

	public bool ShowText;

	public bool ShowPips;

	public bool ShowToolTip;

	public bool ShowBacker;

	public bool UseNativeImageSize;

	public int MaxPips;

	public int Steps;
}
