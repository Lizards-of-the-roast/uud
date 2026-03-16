using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card.RulesText;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Cards.Text;

public class AbilityWordSupplementalTextParser : ITextEntryParser
{
	private readonly IClientLocProvider _clientLocManager;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IAbilityDataProvider _abilityProvider;

	public AbilityWordSupplementalTextParser(IClientLocProvider clientLocManager, IAbilityDataProvider abilityProvider, AssetLookupSystem assetLookupSystem)
	{
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_assetLookupSystem = assetLookupSystem;
		_abilityProvider = abilityProvider ?? NullAbilityDataProvider.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AbilityWordSupplementalTextPayload> supplementalTextTree))
		{
			yield break;
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(card);
		foreach (AbilityWordData activeAbilityWord in card.ActiveAbilityWords)
		{
			if (_abilityProvider.TryGetAbilityPrintingById(activeAbilityWord.AbilityGrpId, out var ability))
			{
				_assetLookupSystem.Blackboard.Ability = ability;
			}
			AbilityWordSupplementalTextPayload payload = supplementalTextTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				string localizedTextForLanguage = _clientLocManager.GetLocalizedTextForLanguage(payload.LocKey, overrideLang, payload.BuildParameters(_assetLookupSystem.Blackboard));
				yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, localizedTextForLanguage));
			}
		}
	}
}
