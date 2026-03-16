using System;
using System.Collections.Generic;
using System.Text;
using AssetLookupTree;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Duel;

namespace GreClient.CardData.RulesTextOverrider;

public class AbilityTextOverride : IRulesTextOverride
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly uint _titleId;

	private readonly bool _removeLoyaltyPrefix;

	private readonly bool _convertManaSymbols;

	private IDictionary<string, string> _substitutions;

	private readonly List<uint> _abilityGrpIdSet = new List<uint>(1);

	private readonly List<uint> _sourceGrpIdSet = new List<uint>(1);

	private readonly List<TargetSpec> _targetSpecs = new List<TargetSpec>(1);

	private int? _dieRollResult;

	private AssetLookupSystem _assetLookupSystem;

	private Dictionary<uint, MtgCardInstance> _affectorIdToSourceCardInstance = new Dictionary<uint, MtgCardInstance>();

	public AbilityTextOverride(ICardDatabaseAdapter cardDatabase, uint titleId, bool removeLoyaltyPrefix = false, bool convertManaSymbols = true)
	{
		_cardDatabase = cardDatabase;
		_titleId = titleId;
		_removeLoyaltyPrefix = removeLoyaltyPrefix;
		_convertManaSymbols = convertManaSymbols;
	}

	public AbilityTextOverride AddAbility(uint abilityId)
	{
		AddAbilityGrpId(abilityId);
		return this;
	}

	public AbilityTextOverride AddAbility(AbilityPrintingData ability)
	{
		AddAbilityGrpId(ability.Id);
		return this;
	}

	public AbilityTextOverride AddAbility(IEnumerable<uint> abilityIds)
	{
		foreach (uint abilityId in abilityIds)
		{
			AddAbilityGrpId(abilityId);
		}
		return this;
	}

	public AbilityTextOverride AddAbility(IEnumerable<AbilityPrintingData> abilities)
	{
		foreach (AbilityPrintingData ability in abilities)
		{
			AddAbilityGrpId(ability.Id);
		}
		return this;
	}

	public AbilityTextOverride AddSource(uint sourceGrpId)
	{
		AddSourceGrpId(sourceGrpId);
		return this;
	}

	public AbilityTextOverride AddSource(CardPrintingData sourcePrinting)
	{
		AddCardPrintingToSourceSet(sourcePrinting);
		return this;
	}

	public AbilityTextOverride AddSource(IReadOnlyCollection<uint> sourceGrpIds)
	{
		foreach (uint sourceGrpId in sourceGrpIds)
		{
			AddSourceGrpId(sourceGrpId);
		}
		return this;
	}

	public AbilityTextOverride AddSource(MtgCardInstance sourceInstance)
	{
		AddCardInstanceToSourceSet(sourceInstance);
		AddCardInstanceToSourceSet(sourceInstance?.Parent);
		return this;
	}

	public AbilityTextOverride AddSource(IEnumerable<MtgCardInstance> sourceInstances)
	{
		foreach (MtgCardInstance sourceInstance in sourceInstances)
		{
			AddSource(sourceInstance);
		}
		return this;
	}

	public AbilityTextOverride AddTargetSpecs(IReadOnlyCollection<TargetSpec> targetSpecs, AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
		_targetSpecs.AddRange(targetSpecs);
		return this;
	}

	public AbilityTextOverride AddDieRollResult(int? result)
	{
		_dieRollResult = result;
		return this;
	}

	public AbilityTextOverride AddSubstitution(string key, string token)
	{
		_substitutions = _substitutions ?? new Dictionary<string, string>();
		_substitutions["{" + key + "}"] = token;
		return this;
	}

	public string GetOverride(CardTextColorSettings textColorSettings)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _abilityGrpIdSet.Count; i++)
		{
			string abilityText = ((_abilityGrpIdSet[i] == 278) ? _cardDatabase.ClientLocProvider.GetLocalizedText("Card/SiegeAbilityOverrideText") : ((_abilityGrpIdSet[i] == 314) ? _cardDatabase.ClientLocProvider.GetLocalizedText("DuelScene/Browsers/Cloak_ManaCost_Text") : ((_abilityGrpIdSet[i] == 145) ? _cardDatabase.ClientLocProvider.GetLocalizedText("DuelScene/Browsers/Manifest_ManaCost_Text") : ((_abilityGrpIdSet[i] != 351) ? _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(_sourceGrpIdSet, _abilityGrpIdSet[i], Array.Empty<uint>(), _titleId) : _cardDatabase.ClientLocProvider.GetLocalizedText("DuelScene/Browsers/ManifestDread_ManaCost_Text")))));
			abilityText = FormatAbility(abilityText, _abilityGrpIdSet[i], textColorSettings);
			stringBuilder.Append(abilityText);
			if (i < _abilityGrpIdSet.Count - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		if (_substitutions != null)
		{
			foreach (KeyValuePair<string, string> substitution in _substitutions)
			{
				stringBuilder.Replace(substitution.Key, substitution.Value);
			}
		}
		string text = stringBuilder.ToString();
		if (_removeLoyaltyPrefix)
		{
			text = ManaUtilities.RemovePrefixedLoyaltyCostFromAbilityText(text);
		}
		if (_convertManaSymbols)
		{
			text = ManaUtilities.ConvertManaSymbols(text);
		}
		return text;
	}

	private string FormatAbility(string abilityText, uint abilityId, CardTextColorSettings textColorSettings)
	{
		if (AbilityFormatter.IsDungeonRoomCard(GetSourceCard(abilityId)))
		{
			return AbilityFormatter.FormatDungeonRoom(abilityText, textColorSettings);
		}
		if (TargetingColorer.IsMultiTargetAbility(abilityId, _cardDatabase.AbilityDataProvider, _targetSpecs))
		{
			ICardDataAdapter sourceCardData = null;
			if (_targetSpecs.Count > 0 && _affectorIdToSourceCardInstance.TryGetValue(_targetSpecs[0].Affector, out var value))
			{
				sourceCardData = CardDataExtensions.CreateWithDatabase(value, _cardDatabase);
			}
			return AbilityFormatter.FormatAbilityWithTargetSpecs(abilityText, abilityId, _cardDatabase.AbilityDataProvider, _targetSpecs, textColorSettings, _assetLookupSystem, sourceCardData);
		}
		return AbilityFormatter.FormatAbility(abilityText, textColorSettings);
	}

	private CardPrintingData GetSourceCard(uint abilityId)
	{
		foreach (uint item in _sourceGrpIdSet)
		{
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(item);
			if (cardPrintingById == null)
			{
				continue;
			}
			foreach (AbilityPrintingData ability in cardPrintingById.Abilities)
			{
				if (ability.Id == abilityId)
				{
					return cardPrintingById;
				}
			}
			foreach (AbilityPrintingData hiddenAbility in cardPrintingById.HiddenAbilities)
			{
				if (hiddenAbility.Id == abilityId)
				{
					return cardPrintingById;
				}
			}
		}
		return null;
	}

	private void AddCardInstanceToSourceSet(MtgCardInstance cardInstance)
	{
		if (cardInstance == null)
		{
			return;
		}
		AddSourceGrpId(cardInstance.GrpId);
		if (cardInstance.GrpId != cardInstance.BaseGrpId)
		{
			AddSourceGrpId(cardInstance.BaseGrpId);
		}
		AddSourceGrpId(cardInstance.ObjectSourceGrpId);
		foreach (AddedAbilityData abilityAdder in cardInstance.AbilityAdders)
		{
			AddSourceGrpId(abilityAdder.SourceGrpId);
			AddSourceGrpId(abilityAdder.AddedByGrpId);
		}
		foreach (uint abilityOriginalCardGrpId in cardInstance.AbilityOriginalCardGrpIds)
		{
			AddSourceGrpId(abilityOriginalCardGrpId);
		}
		if (cardInstance.MutationParent != null)
		{
			AddSourceGrpId(cardInstance.MutationParent.GrpId);
		}
		foreach (MtgCardInstance mutationChild in cardInstance.MutationChildren)
		{
			AddSourceGrpId(mutationChild.GrpId);
		}
		if (!_affectorIdToSourceCardInstance.ContainsKey(cardInstance.InstanceId))
		{
			_affectorIdToSourceCardInstance[cardInstance.InstanceId] = cardInstance;
		}
	}

	private void AddCardPrintingToSourceSet(CardPrintingData cardPrinting)
	{
		if (cardPrinting == null)
		{
			return;
		}
		AddSourceGrpId(cardPrinting.GrpId);
		foreach (uint linkedAbilityTemplateCardGrpId in cardPrinting.LinkedAbilityTemplateCardGrpIds)
		{
			AddSourceGrpId(linkedAbilityTemplateCardGrpId);
		}
	}

	private void AddAbilityGrpId(uint grpId)
	{
		if (grpId != 0)
		{
			_abilityGrpIdSet.Add(grpId);
		}
	}

	private void AddSourceGrpId(uint grpId)
	{
		if (grpId != 0 && !_sourceGrpIdSet.Contains(grpId))
		{
			_sourceGrpIdSet.Add(grpId);
		}
	}
}
