public static class AnchorTypeConverter
{
	public static AnchorPointType FromType(CDCAnchorType oldType)
	{
		return oldType switch
		{
			CDCAnchorType.PowerToughness => AnchorPointType.PowerToughness, 
			CDCAnchorType.TextBox => AnchorPointType.TextBox, 
			CDCAnchorType.Icons => AnchorPointType.Icons, 
			CDCAnchorType.CardBase => AnchorPointType.CardBase, 
			CDCAnchorType.LoyaltyBox => AnchorPointType.Loyalty, 
			CDCAnchorType.Highlights => AnchorPointType.Highlights, 
			CDCAnchorType.Counters => AnchorPointType.Counters, 
			CDCAnchorType.CountersReversed => AnchorPointType.CountersReversed, 
			_ => AnchorPointType.Invalid, 
		};
	}

	public static AnchorPointType FromCustom(string oldType)
	{
		switch (oldType)
		{
		case "Expansion Symbol":
			return AnchorPointType.ExpansionSymbol;
		case "Title":
		case "Title Rewards":
			return AnchorPointType.TitleBar;
		case "Double Sided Icon":
			return AnchorPointType.DoubleSidedSymbol;
		case "Type Line":
			return AnchorPointType.TypeLine;
		case "Face Symbol":
			return AnchorPointType.FaceSymbol;
		case "Stank":
			return AnchorPointType.CastableStank;
		case "SubtypeIcon":
			return AnchorPointType.SubTypeSymbol;
		case "Animated Cardback":
			return AnchorPointType.AnimatedCardback;
		case "Guildmark":
			return AnchorPointType.Guildmark;
		case "Artist Credit":
			return AnchorPointType.ArtistCredit;
		case "ArtInFrame":
		case "Rewards Count":
			return AnchorPointType.ArtInFrame;
		case "AdventureOmissionLeftPage":
			return AnchorPointType.AdventureOmissionLeftPage;
		case "AdventureOmissionRightPage":
			return AnchorPointType.AdventureOmissionRightPage;
		case "Split 1":
			return AnchorPointType.LinkedFaceRoot_0;
		case "Split 2":
			return AnchorPointType.LinkedFaceRoot_1;
		default:
			return AnchorPointType.Invalid;
		}
	}
}
