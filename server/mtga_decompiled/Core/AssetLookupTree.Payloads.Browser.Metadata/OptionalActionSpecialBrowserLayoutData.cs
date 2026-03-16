namespace AssetLookupTree.Payloads.Browser.Metadata;

public class OptionalActionSpecialBrowserLayoutData
{
	public SourceCardType SourceCardType;

	public uint GrpId;

	public int LocId;

	public string SubheaderLocKey;

	public bool YesOnLeft;

	public string YesText;

	public string NoText;

	public OptionalActionSpecialBrowserLayoutData()
	{
		SourceCardType = SourceCardType.None;
		GrpId = 0u;
		LocId = 0;
		SubheaderLocKey = null;
		YesOnLeft = true;
		YesText = null;
		NoText = null;
	}

	public OptionalActionSpecialBrowserLayoutData(SourceCardType sourceCardType, uint grpId, int locId, string subheaderLocKey, bool yesOnLeft, string yesText, string noText)
	{
		SourceCardType = sourceCardType;
		GrpId = grpId;
		LocId = locId;
		SubheaderLocKey = subheaderLocKey;
		YesOnLeft = yesOnLeft;
		YesText = yesText;
		NoText = noText;
	}
}
