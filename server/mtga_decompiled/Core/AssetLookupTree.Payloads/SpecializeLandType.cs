using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads;

public class SpecializeLandType : ILocParameterProvider
{
	private const string KEY = "specializeLandType";

	public string GetKey()
	{
		return "specializeLandType";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		if (TryGetSpecializeColor(filledBB, out var specializeColor))
		{
			IGreLocProvider greLocProvider = filledBB?.CardDatabase?.GreLocProvider;
			if (greLocProvider != null)
			{
				paramValue = greLocProvider.GetLocalizedText(LandTitleIdForCardColor(specializeColor.Value));
				return true;
			}
		}
		paramValue = string.Empty;
		return false;
	}

	private uint LandTitleIdForCardColor(CardColor cardColor)
	{
		return cardColor switch
		{
			CardColor.White => 648u, 
			CardColor.Blue => 652u, 
			CardColor.Black => 653u, 
			CardColor.Red => 1250u, 
			CardColor.Green => 647u, 
			_ => 0u, 
		};
	}

	private bool TryGetSpecializeColor(IBlackboard filledBB, out CardColor? specializeColor)
	{
		specializeColor = filledBB?.CardData?.Instance?.PendingSpecializationColor;
		return specializeColor.HasValue;
	}
}
