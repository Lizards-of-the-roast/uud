using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Card.RulesText;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Cards.Text;

public class LinkedInfoTextParser : ITextEntryParser
{
	private readonly IGreLocProvider _greLocManager;

	private readonly IClientLocProvider _clientLocManager;

	private readonly AssetLookupSystem _assetLookupSystem;

	public LinkedInfoTextParser(IGreLocProvider greLocManager, IClientLocProvider clientLocManager, AssetLookupSystem assetLookupSystem)
	{
		_greLocManager = greLocManager ?? NullGreLocManager.Default;
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		string text = string.Join(", ", GetLinkedInfoStrings(card, overrideLang));
		if (!string.IsNullOrWhiteSpace(text))
		{
			yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, text));
		}
	}

	private IEnumerable<string> GetLinkedInfoStrings(ICardDataAdapter card, string overrideLang = null)
	{
		if (card.LinkedInfoText.Count <= 0 || !_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<LinkedInfoTextOverride> overrideTree))
		{
			yield break;
		}
		IBlackboard blackboard = _assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.SetCardDataExtensive(card);
		foreach (LinkedInfoText item in card.LinkedInfoText)
		{
			blackboard.LinkInfo = item.LinkInfo;
			blackboard.LinkedInfoText = item;
			if (overrideTree != null)
			{
				LinkedInfoTextOverride payload = overrideTree.GetPayload(blackboard);
				if (payload != null)
				{
					if (!payload.IgnoreOverride)
					{
						yield return _clientLocManager.GetLocalizedTextForLanguage(payload.LocKey, overrideLang, payload.BuildParameters(blackboard));
					}
					continue;
				}
			}
			yield return _greLocManager.GetLocalizedTextForEnumValue(item.EnumName, item.Value, formatted: true, overrideLang);
		}
	}
}
