using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads;

public class SpecializeColorProvider : ILocParameterProvider
{
	private const string KEY = "specializeColor";

	public string GetKey()
	{
		return "specializeColor";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		if (TryGetSpecializeColor(filledBB, out var specializeColor))
		{
			ICardDatabaseAdapter cardDatabase = filledBB.CardDatabase;
			if (cardDatabase != null)
			{
				paramValue = cardDatabase.GreLocProvider.GetLocalizedTextForEnumValue(ConvertCardColortoColor(specializeColor));
				return true;
			}
		}
		paramValue = string.Empty;
		return false;
	}

	private bool TryGetSpecializeColor(IBlackboard filledBB, out CardColor? specializeColor)
	{
		specializeColor = filledBB?.CardData?.Instance?.PendingSpecializationColor;
		return specializeColor.HasValue;
	}

	private Color ConvertCardColortoColor(CardColor? cardColor)
	{
		if (cardColor.HasValue)
		{
			return cardColor.Value switch
			{
				CardColor.White => Color.White, 
				CardColor.Blue => Color.Blue, 
				CardColor.Black => Color.Black, 
				CardColor.Red => Color.Red, 
				CardColor.Green => Color.Green, 
				_ => Color.None, 
			};
		}
		return Color.None;
	}
}
