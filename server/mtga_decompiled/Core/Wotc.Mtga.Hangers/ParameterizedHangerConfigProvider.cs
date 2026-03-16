using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability;
using GreClient.CardData;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

public class ParameterizedHangerConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locManager;

	private readonly ICardDataProvider _cardDataProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IObjectPool _genericPool;

	private readonly IReadOnlyDictionary<ParameterizedInjectors, IParameterizedInjector> _injectors;

	public ParameterizedHangerConfigProvider(IClientLocProvider locManager, AssetLookupSystem assetLookupSystem, IObjectPool genericPool, ICardDataProvider cardDataProvider, Dictionary<ParameterizedInjectors, IParameterizedInjector> injectors)
	{
		_locManager = locManager ?? NullLocProvider.Default;
		_assetLookupSystem = assetLookupSystem;
		_genericPool = genericPool;
		_injectors = injectors ?? DictionaryExtensions.Empty<ParameterizedInjectors, IParameterizedInjector>();
		_cardDataProvider = cardDataProvider;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ParameterizedHangerEntry> parameterizedHangerTree))
		{
			yield break;
		}
		foreach (var abilityData in HangerUtilities.GetAllAbilities(model, _cardDataProvider))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.Ability = abilityData.Ability;
			_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
			ParameterizedHangerEntry parameterizedHangerEntry = parameterizedHangerTree?.GetPayload(_assetLookupSystem.Blackboard);
			if (parameterizedHangerEntry != null)
			{
				yield return GenerateHanger(model, abilityData.Ability, _injectors, parameterizedHangerEntry, _locManager);
			}
			foreach (AbilityPrintingData hiddenAbility in abilityData.Ability.HiddenAbilities)
			{
				_assetLookupSystem.Blackboard.Clear();
				_assetLookupSystem.Blackboard.Ability = hiddenAbility;
				_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
				ParameterizedHangerEntry parameterizedHangerEntry2 = parameterizedHangerTree?.GetPayload(_assetLookupSystem.Blackboard);
				if (parameterizedHangerEntry2 != null)
				{
					yield return GenerateHanger(model, hiddenAbility, _injectors, parameterizedHangerEntry2, _locManager);
				}
			}
		}
	}

	public static HangerConfig GenerateHanger(ICardDataAdapter model, AbilityPrintingData ability, IReadOnlyDictionary<ParameterizedInjectors, IParameterizedInjector> injectors, ParameterizedHangerEntry hangerEntry, IClientLocProvider locManager)
	{
		string text = locManager.GetLocalizedText(hangerEntry.Data.Term);
		string text2 = GetTitleText(hangerEntry, ability, locManager);
		foreach (KeyValuePair<ParameterizedInjectors, IParameterizedInjector> injector in injectors)
		{
			if (injector.Value is IUseParameterizedData useParameterizedData)
			{
				useParameterizedData.SetData(hangerEntry.Data.InjectorData);
			}
			if (hangerEntry.Data.Injectors.HasFlag(injector.Key))
			{
				text = injector.Value.Inject(text, model, ability);
				text2 = injector.Value.Inject(text2, model, ability);
			}
		}
		return new HangerConfig(text2, text, null, hangerEntry.Data.SpriteRef.RelativePath);
	}

	private static string GetTitleText(ParameterizedHangerEntry hangerEntry, AbilityPrintingData ability, IClientLocProvider locManager)
	{
		if (string.IsNullOrEmpty(hangerEntry.Data.Title))
		{
			return string.Empty;
		}
		string localizedText = locManager.GetLocalizedText(hangerEntry.Data.Title);
		if (!hangerEntry.Data.Injectors.HasFlag(ParameterizedInjectors.INJECT_MANA_TITLE))
		{
			return localizedText + " ";
		}
		string text = ManaUtilities.ConvertToOldSchoolManaText(ability.ManaCost);
		if (string.IsNullOrEmpty(text) && ability is DynamicAbilityPrintingData dynamicAbilityPrintingData)
		{
			text = dynamicAbilityPrintingData.Cost;
		}
		return localizedText + " " + text;
	}
}
