using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads;

public class LinkedInfoText_ColorValue : LinkedInfoTextEnumValue
{
	protected override TypeCategory Category => TypeCategory.Color;
}
