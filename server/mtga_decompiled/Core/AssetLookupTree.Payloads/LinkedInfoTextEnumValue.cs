using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads;

public abstract class LinkedInfoTextEnumValue : ILocParameterProvider
{
	private readonly string KEY;

	protected virtual TypeCategory Category { get; }

	public string GetKey()
	{
		return KEY;
	}

	public LinkedInfoTextEnumValue()
	{
		KEY = LinkedInfoText.GetEnumName(Category).ToLower();
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		LinkedInfoText linkedInfoText = filledBB.LinkedInfoText;
		if (linkedInfoText.Category == Category && !string.IsNullOrEmpty(linkedInfoText.EnumName))
		{
			ICardDatabaseAdapter cardDatabase = filledBB.CardDatabase;
			if (cardDatabase != null)
			{
				paramValue = cardDatabase.GreLocProvider.GetLocalizedTextForEnumValue(linkedInfoText.EnumName, linkedInfoText.Value);
				return true;
			}
		}
		paramValue = string.Empty;
		return false;
	}
}
