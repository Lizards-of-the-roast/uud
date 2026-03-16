using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public class AbilityHangerBaseConfigProvider : IAbilityHangerConfigProvider
{
	private readonly IHangerLookupProvider _hangerLookup;

	private readonly ICardDataProvider _cardProvider;

	private readonly IGreLocProvider _greLoc;

	private readonly IClientLocProvider _clientLoc;

	private readonly ICardTitleProvider _cardTitleProvider;

	private readonly IObjectPool _genericPool;

	private readonly IAbilityHangerTextOverride[] _hangerOverrides;

	private readonly HashSet<uint> _abilitiesAdded = new HashSet<uint>();

	private readonly HashSet<string> _layersAdded = new HashSet<string>();

	public AbilityHangerBaseConfigProvider(AssetLookupSystem assetLookupSystem, ICardDatabaseAdapter cardDatabase, IClientLocProvider locManager, IObjectPool genericPool)
		: this(new ALTQueryProvider(assetLookupSystem, genericPool), cardDatabase.CardDataProvider, cardDatabase.CardTitleProvider, cardDatabase.GreLocProvider, locManager, genericPool)
	{
	}

	public AbilityHangerBaseConfigProvider(IHangerLookupProvider queryProvider, ICardDataProvider cardProvider, ICardTitleProvider cardTitleProvider, IGreLocProvider greLocProvider, IClientLocProvider locManager, IObjectPool genericPool)
	{
		_hangerLookup = queryProvider;
		_cardProvider = cardProvider;
		_clientLoc = locManager;
		_greLoc = greLocProvider;
		_cardTitleProvider = cardTitleProvider;
		_genericPool = genericPool;
		_hangerOverrides = new IAbilityHangerTextOverride[2]
		{
			new PartyCountOverride(_greLoc, _clientLoc),
			new DeliriumHangerOverride(_greLoc, _clientLoc)
		};
	}

	public IEnumerable<HangerConfig> GetHangerConfigsForCard(ICardDataAdapter cardData, CardHolderType cardHolder, CDCViewMetadata metaData)
	{
		foreach (var (ability, state) in HangerUtilities.GetAllAbilities(cardData, _cardProvider))
		{
			foreach (HangerConfig item in GetHangerConfigsForAbility(cardData, cardHolder, metaData, ability, state))
			{
				yield return item;
			}
		}
	}

	public IEnumerable<HangerConfig> GetHangerConfigsForAbility(ICardDataAdapter cardData, CardHolderType cardHolder, CDCViewMetadata metaData, AbilityPrintingData ability, AbilityState state = AbilityState.Normal)
	{
		_hangerLookup.FillCardBlackboard(cardData, cardHolder, metaData);
		HashSet<HangerConfig> configs = _genericPool.PopObject<HashSet<HangerConfig>>();
		QueryHangerAbilities(configs, cardData, ability, 0, state);
		foreach (HangerConfig item in configs)
		{
			yield return item;
		}
		configs.Clear();
		_genericPool.PushObject(configs, tryClear: false);
	}

	private void QueryHangerAbilities(HashSet<HangerConfig> configs, ICardDataAdapter model, AbilityPrintingData ability, int section = 0, AbilityState state = AbilityState.Normal)
	{
		if (_hangerLookup.ShouldShowAbilityHanger(ability) && !_abilitiesAdded.Contains(ability.Id))
		{
			configs.UnionWith(RunHangerQuery(model, ability, AbilityType.None, section, state));
		}
		foreach (AbilityPrintingData hiddenAbility in ability.HiddenAbilities)
		{
			if (ability.Id != hiddenAbility.Id && ability.BaseId != hiddenAbility.Id)
			{
				QueryHangerAbilities(configs, model, hiddenAbility, section + 1);
			}
		}
		foreach (AbilityType referencedAbilityType in ability.ReferencedAbilityTypes)
		{
			if ((!ability.ReferencedAbilities.TryGetById((uint)referencedAbilityType, out var printing) || _hangerLookup.ShouldShowAbilityHanger(printing)) && _hangerLookup.ShouldShowAbilityHanger(referencedAbilityType))
			{
				configs.UnionWith(RunHangerQuery(model, ability, referencedAbilityType, section + 1));
			}
		}
		if (ability.ReferencedAbilities.Count <= 4)
		{
			foreach (AbilityPrintingData referencedAbility in ability.ReferencedAbilities)
			{
				if (_hangerLookup.ShouldShowAbilityHanger(referencedAbility) && ability.Id != referencedAbility.Id && ability.BaseId != referencedAbility.Id)
				{
					AbilityPrintingData abilityPrintingData;
					if (referencedAbility.BaseId == 0)
					{
						AbilityPrintingRecord record = referencedAbility.Record;
						uint? baseId = referencedAbility.Id;
						abilityPrintingData = new AbilityPrintingData(referencedAbility, new AbilityPrintingRecord(record, null, baseId));
					}
					else
					{
						abilityPrintingData = referencedAbility;
					}
					AbilityPrintingData ability2 = abilityPrintingData;
					QueryHangerAbilities(configs, model, ability2, section + 1);
				}
			}
		}
		foreach (AbilityPrintingData modalAbilityChild in ability.ModalAbilityChildren)
		{
			if (ability.Id != modalAbilityChild.Id && ability.BaseId != modalAbilityChild.Id)
			{
				QueryHangerAbilities(configs, model, modalAbilityChild, section + 1);
			}
		}
	}

	private IEnumerable<HangerConfig> RunHangerQuery(ICardDataAdapter model, AbilityPrintingData ability, AbilityType abilityType = AbilityType.None, int section = 0, AbilityState state = AbilityState.Normal)
	{
		if (ability == null || !_hangerLookup.ShouldShowAbilityHanger(ability))
		{
			yield break;
		}
		foreach (HangerPayload item in _hangerLookup.QueryHangers(ability, abilityType))
		{
			if (!item.Layers.Exists(_layersAdded, (string layer, HashSet<string> addedLayers) => !string.IsNullOrEmpty(layer) && addedLayers.Contains(layer)))
			{
				AbilityBadgeData badgeData = _hangerLookup.GetBadgeData(item.Data, item.Layers, ability, model);
				(string Header, string BodyText) hangerText = GetHangerText(model, ability, badgeData, item.Layers);
				string headerText = hangerText.Header;
				string bodyText = hangerText.BodyText;
				string addendumText = GetAddendumText(model, ability, state);
				HangerTextParameters(model, badgeData, ref headerText, ref bodyText, out var convertSymbols);
				_abilitiesAdded.Add(ability.Id);
				_layersAdded.UnionWith(item.Layers);
				yield return new HangerConfig(headerText, bodyText, addendumText, badgeData?.IconSpritePath, convertSymbols, GetColor(model, ability, state), section);
			}
		}
		static HangerColor GetColor(ICardDataAdapter cardDataAdapter, AbilityPrintingData ability2, AbilityState abilityState)
		{
			if (abilityState == AbilityState.Added)
			{
				return HangerColor.Added;
			}
			if (cardDataAdapter != null && cardDataAdapter.Instance.HasPerpetualAddedAbility(ability2))
			{
				return HangerColor.Perpetual;
			}
			return HangerColor.None;
		}
	}

	private (string Header, string BodyText) GetHangerText(ICardDataAdapter model, AbilityPrintingData ability, AbilityBadgeData badgeData, IReadOnlyCollection<string> layers)
	{
		IAbilityHangerTextOverride[] hangerOverrides = _hangerOverrides;
		foreach (IAbilityHangerTextOverride abilityHangerTextOverride in hangerOverrides)
		{
			if (abilityHangerTextOverride.CanUse(model, ability, layers))
			{
				return abilityHangerTextOverride.GetText(model);
			}
		}
		string item = GetHangerLoc(badgeData.LocTitle, _greLoc, _clientLoc);
		string text = GetHangerLoc(badgeData.LocTerm, _greLoc, _clientLoc);
		if (!string.IsNullOrEmpty(badgeData.LocAddendum))
		{
			text = text + "\n" + _clientLoc.GetLocalizedText(badgeData.LocAddendum);
		}
		return (Header: item, BodyText: text);
		static string GetHangerLoc(string loc, IGreLocProvider greLoc, IClientLocProvider clientLoc)
		{
			if (!string.IsNullOrEmpty(loc))
			{
				if (!uint.TryParse(loc, out var result))
				{
					return clientLoc.GetLocalizedText(loc);
				}
				return greLoc.GetLocalizedText(result);
			}
			return string.Empty;
		}
	}

	private string GetAddendumText(ICardDataAdapter model, AbilityPrintingData ability, AbilityState state)
	{
		if (state == AbilityState.Added && model.Instance != null)
		{
			AddedAbilityData addedAbilityData = model.Instance.AbilityAdders.Find((AddedAbilityData x) => x.AbilityId == ability.Id);
			if (addedAbilityData.AddedByGrpId != 0)
			{
				uint addedByGrpId = addedAbilityData.AddedByGrpId;
				if (_cardProvider.GetCardPrintingById(addedByGrpId) != null)
				{
					return _cardTitleProvider.GetCardTitle(addedByGrpId);
				}
			}
		}
		return string.Empty;
	}

	protected virtual void HangerTextParameters(ICardDataAdapter cardData, AbilityBadgeData badgeData, ref string headerText, ref string bodyText, out bool convertSymbols)
	{
		convertSymbols = true;
		if (badgeData.Ability.Id == 136774 && cardData.ActiveAbilityWords.Exists((AbilityWordData x) => x.AbilityWord == "LinkInfoAmount"))
		{
			string additionalDetail = cardData.ActiveAbilityWords.Find("LinkInfoAmount", (AbilityWordData x, string t) => x.AbilityWord == t).AdditionalDetail;
			bodyText = string.Format(_clientLoc.GetLocalizedText(badgeData.LocTerm), additionalDetail);
		}
		if (badgeData.Ability.BaseIdNumeral != 0)
		{
			headerText = headerText.Replace("{numeral}", badgeData.Ability.BaseIdNumeral.ToString());
			bodyText = bodyText.Replace("{numeral}", badgeData.Ability.BaseIdNumeral.ToString());
		}
	}

	public void Cleanup()
	{
		_abilitiesAdded.Clear();
		_layersAdded.Clear();
	}
}
