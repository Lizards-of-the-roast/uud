using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Card;
using AssetLookupTree.Payloads.Prefab;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using ReferenceMap;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Duel;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Hangers.AbilityHangers;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class AbilityHanger : AbilityHangerBase
{
	private IconPathProvider_IconId _iconPathProvider;

	private readonly IConfigProviderTranslator _configTranslator = new ConfigProviderTranslator();

	private IHangerConfigProvider _counterHangers = NullConfigProvider.Default;

	private IHangerConfigProvider _dealtDamageLastTurnHanger = NullConfigProvider.Default;

	private IHangerConfigProvider _faceDownIdProvider = NullConfigProvider.Default;

	private IHangerConfigProvider _hiddenInformationManaInfoProvider = NullConfigProvider.Default;

	private IHangerConfigProvider _modifiedActionCostProvider = NullConfigProvider.Default;

	private IHangerConfigProvider _specialHangerConfigProvider = NullConfigProvider.Default;

	private IHangerConfigProvider _damagedCardProvider = NullConfigProvider.Default;

	private IHangerConfigProvider _colorModifiedProvider = NullConfigProvider.Default;

	private IHangerConfigProvider _parameterizedCardInfoProvider = NullConfigProvider.Default;

	private Transform _hangerParent;

	private IGameStateProvider _gameStateProvider = NullGameStateProvider.Default;

	private IWorkflowProvider _workflowProvider = NullWorkflowProvider.Default;

	private IEntityViewProvider _entityViewProvider = NullEntityViewProvider.Default;

	private IPromptEngine _promptEngine = NullPromptEngine.Default;

	private IObjectPool _genericPool = NullObjectPool.Default;

	private IClientLocProvider _locManager = NullLocProvider.Default;

	private IEntityNameProvider<uint> _entityNameProvider = NullIdNameProvider.Default;

	private IReferenceMap _referenceMap;

	private NPEDirector _npeDirector;

	public void Init(Transform hangerParent, IContext context, AssetLookupSystem assetLookupSystem, IFaceInfoGenerator faceInfoGenerator, DeckFormat deckFormat, NPEDirector npeDirector)
	{
		_hangerParent = hangerParent;
		_gameStateProvider = context.Get<IGameStateProvider>() ?? NullGameStateProvider.Default;
		_workflowProvider = context.Get<IWorkflowProvider>() ?? NullWorkflowProvider.Default;
		_entityViewProvider = context.Get<IEntityViewProvider>() ?? NullEntityViewProvider.Default;
		_genericPool = context.Get<IObjectPool>() ?? new ObjectPool();
		_locManager = context.Get<IClientLocProvider>() ?? NullLocProvider.Default;
		_promptEngine = context.Get<IPromptEngine>() ?? NullPromptEngine.Default;
		_entityNameProvider = context.Get<IEntityNameProvider<uint>>() ?? NullIdNameProvider.Default;
		_referenceMap = context.Get<IReferenceMap>();
		_npeDirector = npeDirector;
		ICardDatabaseAdapter cardDatabaseAdapter = context.Get<ICardDatabaseAdapter>() ?? NullCardDatabaseAdapter.Default;
		IClientLocProvider clientLocProvider = cardDatabaseAdapter.ClientLocProvider;
		Init(cardDatabaseAdapter, assetLookupSystem, context.Get<IUnityObjectPool>(), _genericPool, faceInfoGenerator, _locManager, deckFormat);
		_abilityHangerProvider = new AbilityHangerConfigProvider(assetLookupSystem, cardDatabaseAdapter, clientLocProvider, _genericPool, _entityNameProvider);
		_parameterizedHangers = new ParameterizedHangerConfigProvider(cardDatabaseAdapter.ClientLocProvider, assetLookupSystem, _genericPool, cardDatabaseAdapter.CardDataProvider, new Dictionary<ParameterizedInjectors, IParameterizedInjector>
		{
			[ParameterizedInjectors.INJECT_MANA] = new InjectMana(),
			[ParameterizedInjectors.INJECT_LINKED_INFO] = new InjectLinkInfo(cardDatabaseAdapter.GreLocProvider),
			[ParameterizedInjectors.INJECT_CONTROLLER_GY_PERMANENTS] = new InjectControllersGraveyardPermanentTypes(cardDatabaseAdapter.GreLocProvider, _genericPool, _gameStateProvider),
			[ParameterizedInjectors.INJECT_NUMERAL] = new InjectNumeral(),
			[ParameterizedInjectors.INJECT_FAKE_NUMERAL] = new InjectFakeNumeral(),
			[ParameterizedInjectors.INJECT_MANA_COLOR] = new InjectManaColor(clientLocProvider),
			[ParameterizedInjectors.INJECT_X] = new InjectX(clientLocProvider),
			[ParameterizedInjectors.INJECT_STATION_CHARGE_REQUIREMENT] = new InjectStationChargeRequirement(),
			[ParameterizedInjectors.INJECT_NUMERAL_MANA_SYMBOLS] = new InjectNumeralManaSymbols(),
			[ParameterizedInjectors.INJECT_NUMERAL_CARD_NAME] = new InjectNumeralCardName(cardDatabaseAdapter.GreLocProvider)
		});
		_parameterizedCardInfoProvider = new ParameterizedCardHangerProvider(_assetLookupSystem, _locManager, _genericPool);
		_iconPathProvider = new IconPathProvider_IconId(assetLookupSystem.TreeLoader.LoadTree<SpecialIcon>(), assetLookupSystem.Blackboard);
		IPathProvider<AbilityPrintingData> iconPathProvider = new IconPathProvider_ALT(assetLookupSystem.TreeLoader.LoadTree<AssetLookupTree.Payloads.Ability.BadgeEntry>(), assetLookupSystem.Blackboard);
		IHangerConfigProvider hangerConfigProvider = new VentureAbilityConfigProvider(clientLocProvider, iconPathProvider);
		IHangerConfigProvider hangerConfigProvider2 = new CompleteDungeonConfigProvider(clientLocProvider, cardDatabaseAdapter.CardTitleProvider, iconPathProvider, _gameStateProvider);
		_dungeonHangerProvider = new NullModelDecorator(new NullInstanceDecorator(new DungeonConfigProvider(hangerConfigProvider, hangerConfigProvider2)));
		_counterHangers = new CounterConfigProvider(context.Get<IClientLocProvider>(), context.Get<IGreLocProvider>(), assetLookupSystem);
		_dealtDamageLastTurnHanger = new DealtDamageLastTurnConfigProvider(cardDatabaseAdapter.ClientLocProvider, cardDatabaseAdapter.GreLocProvider);
		_faceDownIdProvider = new FaceDownIdConfigProvider(context.Get<IFaceDownIdProvider>(), clientLocProvider);
		_hiddenInformationManaInfoProvider = new HiddenInformationManaInfoConfigProvider(clientLocProvider);
		_modifiedActionCostProvider = new ModifiedCastingCostProvider(cardDatabaseAdapter, _locManager, context.Get<IActionProvider>(), new ActionHangerConfigProvider(_locManager, assetLookupSystem));
		_specialHangerConfigProvider = SpecialHangerConfigProviders.Create_DS(context, _iconPathProvider, _assetLookupSystem);
		_damagedCardProvider = new DamagedCardConfigProvider(_locManager, _assetLookupSystem);
		_colorModifiedProvider = new ColorModifiedConfigProvider(context.Get<IClientLocProvider>());
	}

	protected override void AddHangersInternal(BASE_CDC cardView, ICardDataAdapter sourceModel, HangerSituation situation)
	{
		IHangerConfigProvider hangerConfigProvider = _configTranslator.Translate(sourceModel);
		if (hangerConfigProvider != null)
		{
			foreach (HangerConfig hangerConfig in hangerConfigProvider.GetHangerConfigs(sourceModel))
			{
				CreateHangerItem(hangerConfig);
			}
			return;
		}
		CardData cardData = sourceModel as CardData;
		MtgGameState gameState = _gameStateProvider.CurrentGameState;
		WorkflowBase currentWorkflow = _workflowProvider.GetCurrentWorkflow();
		foreach (HangerConfig hangerConfig2 in _parameterizedCardInfoProvider.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig2);
		}
		foreach (HangerConfig hangerConfig3 in _faceDownIdProvider.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig3);
		}
		foreach (HangerConfig hangerConfig4 in _hiddenInformationManaInfoProvider.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig4);
		}
		foreach (HangerConfig hangerConfig5 in _modifiedActionCostProvider.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig5);
		}
		foreach (HangerConfig hangerConfig6 in _counterHangers.GetHangerConfigs(cardData))
		{
			CreateHangerItem(hangerConfig6.Header, hangerConfig6.Details, null, hangerConfig6.SpritePath, convertSymbols: false, -1);
		}
		base.AddHangersInternal(cardView, sourceModel, situation);
		foreach (HangerConfig hangerConfig7 in _damagedCardProvider.GetHangerConfigs(cardData))
		{
			CreateHangerItem(hangerConfig7);
		}
		CreateNPEHangers(sourceModel, situation);
		if (cardData.Instance != null)
		{
			AddLayeredEffects(cardData);
		}
		if (cardData.AffectedByQualifications.Count > 0)
		{
			AddQualifications(cardView, cardData, gameState);
		}
		if (cardData.ReplacementEffects.Count > 0)
		{
			AddReplacementEffects(cardData, gameState);
		}
		if (ActionsAvailableWorkflow.GetInteractionsForId(cardData.InstanceId, currentWorkflow).Count > 0)
		{
			IReadOnlyDictionary<uint, List<DisqualificationType>> disqualifiersForId = ActionsAvailableWorkflow.GetDisqualifiersForId(cardData.InstanceId, currentWorkflow);
			if (disqualifiersForId.Count > 0)
			{
				AddDisqualifiers(cardData, gameState, disqualifiersForId);
			}
			if (cardData.Instance != null && cardData.Instance.PlayWarnings.Count > 0)
			{
				AddPlayWarnings(cardData);
			}
		}
		AddTargetedHangers(cardData);
		AddTargetedByHangers(cardData, gameState);
		foreach (HangerConfig hangerConfig8 in _colorModifiedProvider.GetHangerConfigs(cardData))
		{
			CreateHangerItem(hangerConfig8);
		}
		if (cardData.Instance != null && cardData.Instance.Designations.Count > 0)
		{
			AddCommanderHangers(cardData);
		}
		foreach (HangerConfig hangerConfig9 in _dealtDamageLastTurnHanger.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig9);
		}
	}

	protected override void AddRemovedAbilities(ICardDataAdapter sourceCardData)
	{
		List<ICardDataAdapter> list = _genericPool.PopObject<List<ICardDataAdapter>>();
		list.Add(sourceCardData);
		for (int i = 0; i < sourceCardData.LinkedFaceGrpIds.Count; i++)
		{
			list.Add(sourceCardData.GetLinkedFaceAtIndex(i, ignoreInstance: false, _cardDatabase.CardDataProvider));
		}
		Dictionary<uint, uint> dictionary = _genericPool.PopObject<Dictionary<uint, uint>>();
		Dictionary<uint, uint> dictionary2 = _genericPool.PopObject<Dictionary<uint, uint>>();
		foreach (ICardDataAdapter item in list)
		{
			if (BasicLandWithAbilitiesRemovedConfigProvider.IsBasicLandMissingIntrinsicAbility(item))
			{
				continue;
			}
			foreach (KeyValuePair<AbilityPrintingData, AbilityState> allAbility in item.AllAbilities)
			{
				AbilityPrintingData ability = allAbility.Key;
				if (allAbility.Value != AbilityState.Removed)
				{
					continue;
				}
				if (!item.Printing.Abilities.Exists((AbilityPrintingData a) => a.Id == ability.Id) && item.Instance != null && !item.Instance.AbilityAdders.Exists((AddedAbilityData b) => b.AbilityId == ability.Id))
				{
					continue;
				}
				RemovedAbilityData removedData = default(RemovedAbilityData);
				if (item.Instance != null)
				{
					removedData = item.Instance.AbilityRemovers.Find((RemovedAbilityData x) => x.AbilityId == ability.Id);
					if (removedData.Equals(default(RemovedAbilityData)))
					{
						removedData = item.Instance.AbilityRemovers.Find((RemovedAbilityData x) => x.AbilityId == ability.BaseId);
					}
				}
				if (!removedData.Equals(default(RemovedAbilityData)))
				{
					if (item.Instance.LayeredEffects.Find((LayeredEffectData x) => x.LayeredEffectId == removedData.LayeredEffectId).IsPerpetual())
					{
						IncrementSourceCount(dictionary2, removedData.RemovedById);
					}
					else
					{
						IncrementSourceCount(dictionary, removedData.RemovedById);
					}
				}
			}
		}
		list.Clear();
		_genericPool.PushObject(list, tryClear: false);
		string value = GenerateSourceCountString(dictionary, isPerpetual: false);
		string value2 = GenerateSourceCountString(dictionary2, isPerpetual: true);
		if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(value2))
		{
			return;
		}
		uint num = 0u;
		foreach (KeyValuePair<uint, uint> item2 in dictionary)
		{
			num += item2.Value;
		}
		foreach (KeyValuePair<uint, uint> item3 in dictionary2)
		{
			num += item3.Value;
		}
		string empty = string.Empty;
		empty = _locManager.GetLocalizedText((num == 1) ? "AbilityHanger/PlayWarning/Ability_Removed_Title" : "AbilityHanger/PlayWarning/Abilities_Removed_Title");
		StringBuilder stringBuilder = _genericPool.PopObject<StringBuilder>();
		if (!string.IsNullOrEmpty(value))
		{
			stringBuilder.AppendLine(value);
		}
		if (!string.IsNullOrEmpty(value2))
		{
			stringBuilder.AppendLine(value2);
		}
		CreateHangerItem(empty, stringBuilder.ToString(), string.Empty);
		stringBuilder.Clear();
		_genericPool.PushObject(stringBuilder, tryClear: false);
		dictionary.Clear();
		_genericPool.PushObject(dictionary, tryClear: false);
		dictionary2.Clear();
		_genericPool.PushObject(dictionary2, tryClear: false);
		string GenerateSourceCountString(Dictionary<uint, uint> sourceCountDict, bool isPerpetual)
		{
			uint sourceId = 0u;
			uint num2 = 0u;
			if (sourceCountDict.Count == 1)
			{
				using Dictionary<uint, uint>.Enumerator enumerator4 = sourceCountDict.GetEnumerator();
				if (enumerator4.MoveNext())
				{
					KeyValuePair<uint, uint> current3 = enumerator4.Current;
					sourceId = current3.Key;
					num2 = current3.Value;
				}
			}
			else
			{
				foreach (KeyValuePair<uint, uint> item4 in sourceCountDict)
				{
					num2 += item4.Value;
				}
			}
			if (num2 == 0)
			{
				return string.Empty;
			}
			string text = GetSourceName(sourceId);
			if (isPerpetual)
			{
				if (num2 == 1)
				{
					if (!string.IsNullOrEmpty(text))
					{
						return _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Ability_Perpetually_RemovedBy_Text", ("count", num2.ToString()), ("remover", text));
					}
					return _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Ability_Perpetually_Removed_Text", ("count", num2.ToString()));
				}
				if (!string.IsNullOrEmpty(text))
				{
					return _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Abilities_Perpetually_RemovedBy_Text", ("count", num2.ToString()), ("remover", text));
				}
				return _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Abilities_Perpetually_Removed_Text", ("count", num2.ToString()));
			}
			if (num2 == 1)
			{
				if (!string.IsNullOrEmpty(text))
				{
					return _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Ability_RemovedBy_Text", ("count", num2.ToString()), ("remover", text));
				}
				return _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Ability_Removed_Text", ("count", num2.ToString()));
			}
			if (!string.IsNullOrEmpty(text))
			{
				return _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Abilities_RemovedBy_Text", ("count", num2.ToString()), ("remover", text));
			}
			return _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Abilities_Removed_Text", ("count", num2.ToString()));
		}
		string GetSourceName(uint sourceId)
		{
			if (sourceId == 0)
			{
				return string.Empty;
			}
			MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
			string result = string.Empty;
			MtgCardInstance cardById = mtgGameState.GetCardById(sourceId);
			if (cardById == null)
			{
				uint parentId = _referenceMap.GetParentId(sourceId);
				cardById = mtgGameState.GetCardById(parentId);
			}
			if (cardById != null)
			{
				result = _cardDatabase.GreLocProvider.GetLocalizedText(cardById.TitleId);
			}
			return result;
		}
		static void IncrementSourceCount(Dictionary<uint, uint> countDict, uint sourceId)
		{
			if (!countDict.ContainsKey(sourceId))
			{
				countDict.Add(sourceId, 0u);
			}
			countDict[sourceId]++;
		}
	}

	private void CreateNPEHangers(ICardDataAdapter sourceModel, HangerSituation situation)
	{
		if (_npeDirector == null)
		{
			return;
		}
		if (!situation.ShowOnlyTapped && sourceModel.CardTypes.Contains(CardType.Land))
		{
			AbilityBadgeData badgeDataForCondition = AbilityBadgeUtil.GetBadgeDataForCondition(_assetLookupSystem, ConditionType.NPE_Land, sourceModel);
			if (badgeDataForCondition != null)
			{
				string localizedText = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/NPE/Land_Title");
				string localizedText2 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/NPE/Land_Body");
				CreateHangerItem(localizedText, localizedText2, "", badgeDataForCondition.IconSpritePath, convertSymbols: true, 0, addedItem: false, situation.UseNPEHanger);
			}
		}
		if (!situation.HideTapped && sourceModel.IsTapped)
		{
			AbilityBadgeData badgeDataForCondition2 = AbilityBadgeUtil.GetBadgeDataForCondition(_assetLookupSystem, ConditionType.NPE_Tapped, sourceModel);
			if (badgeDataForCondition2 != null)
			{
				string localizedText3 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/NPE/Tapped_Title");
				string body = string.Empty;
				if (sourceModel.CardTypes.Contains(CardType.Creature))
				{
					body = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/NPE/Tapped_Body_Creature");
				}
				if (sourceModel.CardTypes.Contains(CardType.Land))
				{
					body = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/NPE/Tapped_Body_Lands");
				}
				CreateHangerItem(localizedText3, body, "", badgeDataForCondition2.IconSpritePath, convertSymbols: true, 0, addedItem: false, situation.UseNPEHanger);
			}
		}
		if (!situation.HideSummoningSickness && !situation.ShowOnlyTapped && sourceModel.HasSummoningSickness)
		{
			AbilityBadgeData badgeDataForCondition3 = AbilityBadgeUtil.GetBadgeDataForCondition(_assetLookupSystem, ConditionType.NPE_SummoningSick, sourceModel);
			if (badgeDataForCondition3 != null)
			{
				string localizedText4 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/NPE/SumSick_Title");
				string localizedText5 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/NPE/SumSick_Body");
				CreateHangerItem(localizedText4, localizedText5, "", badgeDataForCondition3.IconSpritePath, convertSymbols: true, 0, addedItem: false, situation.UseNPEHanger);
			}
		}
	}

	private void AddLayeredEffects(CardData cardData)
	{
		List<LayeredEffectData> list = _genericPool.PopObject<List<LayeredEffectData>>();
		foreach (LayeredEffectData layeredEffect in cardData.Instance.LayeredEffects)
		{
			if (layeredEffect.IsPerpetualPowerToughnessChange())
			{
				list.Add(layeredEffect);
			}
			else if (!string.IsNullOrEmpty(layeredEffect.Type))
			{
				switch (layeredEffect.Type)
				{
				case "TemporaryType":
					AddTemporaryTypeSourceHanger(layeredEffect.SourceAbilityId, layeredEffect.AffectorId, _gameStateProvider.CurrentGameState);
					break;
				default:
					AddLayeredEffectHanger(layeredEffect.Type, (int)layeredEffect.PromptId, cardData);
					break;
				case "Ring-Bearer":
				case "MutateOver":
				case "MutateUnder":
					break;
				}
			}
		}
		TryAddPerpetualPowerToughnessChangeLayeredEffectHanger(list);
		list.Clear();
		_genericPool.PushObject(list, tryClear: false);
	}

	private void AddTemporaryTypeSourceHanger(uint abilityId, uint affectorId, MtgGameState gameState)
	{
		string abilityTextByCardAbilityGrpId = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(gameState.GetCardById(affectorId).GrpId, abilityId, new List<uint> { abilityId });
		CreateHangerItem("", abilityTextByCardAbilityGrpId, "");
	}

	private void AddLayeredEffectHanger(string layeredEffectType, int promptId, ICardDataAdapter cardData)
	{
		AbilityBadgeData layeredEffectLocKeys = AbilityBadgeUtil.GetLayeredEffectLocKeys(_assetLookupSystem, layeredEffectType, cardData);
		string format = layeredEffectLocKeys?.LocTitle ?? ("AbilityHanger/LayeredEffect/Type_{0}" + ((promptId != 0) ? string.Empty : "_NOPROMPT"));
		string format2 = layeredEffectLocKeys?.LocTerm ?? ("AbilityHanger/LayeredEffect/Body_{0}" + ((promptId != 0) ? string.Empty : "_NOPROMPT"));
		string text = layeredEffectLocKeys?.LocAddendum ?? string.Empty;
		string localizedText = _locManager.GetLocalizedText(string.Format(format, layeredEffectType));
		string text2 = _locManager.GetLocalizedText(string.Format(format2, layeredEffectType));
		string addendum = (string.IsNullOrEmpty(text) ? string.Empty : _locManager.GetLocalizedText(text));
		if (text2.Contains("[PROMPTID]"))
		{
			text2 = text2.Replace("[PROMPTID]", _promptEngine.GetPromptText(promptId));
		}
		if (text2.Contains("[OVERLAYCARDNAME]"))
		{
			string newValue = ((cardData.Instance.FaceDownState.IsCopiedFaceDown && layeredEffectType.Equals("CopyObject")) ? _cardDatabase.CardTitleProvider.GetCardTitle(3u) : _cardDatabase.CardTitleProvider.GetCardTitle((cardData.Instance.CopyObjectGrpId != 0) ? cardData.Instance.CopyObjectGrpId : cardData.Instance.GrpId));
			text2 = text2.Replace("[OVERLAYCARDNAME]", newValue);
		}
		CreateHangerItem(localizedText, text2, addendum);
	}

	private bool TryAddPerpetualPowerToughnessChangeLayeredEffectHanger(ICollection<LayeredEffectData> layeredEffects)
	{
		bool result = false;
		if (layeredEffects.Count != 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (LayeredEffectData layeredEffect in layeredEffects)
			{
				int value;
				bool flag = layeredEffect.Details.TryGetValue<int>("perpetualPowerSet", out value);
				int value2;
				bool flag2 = layeredEffect.Details.TryGetValue<int>("perpetualToughnessSet", out value2);
				int value3;
				bool flag3 = layeredEffect.Details.TryGetValue<int>("perpetualPowerMod", out value3);
				int value4;
				bool flag4 = layeredEffect.Details.TryGetValue<int>("perpetualToughnessMod", out value4);
				if (flag || flag2 || flag3 || flag4)
				{
					if (flag && flag2)
					{
						stringBuilder.AppendLine(_locManager.GetLocalizedText("AbilityHanger/LayeredEffect/PerpetualPowerToughness_Body_SetPowerToughness", ("power", value.ToString()), ("toughness", value2.ToString())));
					}
					else if (flag)
					{
						stringBuilder.AppendLine(_locManager.GetLocalizedText("AbilityHanger/LayeredEffect/PerpetualPowerToughness_Body_SetPower", ("power", value.ToString())));
					}
					else if (flag2)
					{
						stringBuilder.AppendLine(_locManager.GetLocalizedText("AbilityHanger/LayeredEffect/PerpetualPowerToughness_Body_SetToughness", ("toughness", value2.ToString())));
					}
					if (flag3 || flag4)
					{
						stringBuilder.AppendLine(AddPowerToughnessMod(value3, value4, "power", "toughness"));
					}
				}
			}
			string localizedText = _locManager.GetLocalizedText("AbilityHanger/LayeredEffect/PerpetualPowerToughness_Title");
			string body = stringBuilder.ToString();
			string empty = string.Empty;
			CreateHangerItem(localizedText, body, empty, null, convertSymbols: true, 0, addedItem: false, useNPEBattlefieldItem: false, perpetualItem: true);
			result = true;
		}
		return result;
	}

	private string AddPowerToughnessMod(int perpetualPowerMod, int perpetualToughnessMod, string FORMAT_ITEM_POWER, string FORMAT_ITEM_TOUGHNESS)
	{
		int num = perpetualPowerMod;
		int num2 = perpetualToughnessMod;
		if (num == 0)
		{
			num = num2;
		}
		if (num2 == 0)
		{
			num2 = num;
		}
		if (num < 0 && num2 < 0)
		{
			return _locManager.GetLocalizedText("AbilityHanger/LayeredEffect/PerpetualPowerToughness_Body_ModPowerToughnessNegative", (FORMAT_ITEM_POWER, Mathf.Abs(perpetualPowerMod).ToString()), (FORMAT_ITEM_TOUGHNESS, Mathf.Abs(perpetualToughnessMod).ToString()));
		}
		if (num > 0 && num2 < 0)
		{
			return _locManager.GetLocalizedText("AbilityHanger/LayeredEffect/PerpetualPowerToughness_Body_ModPowerPositiveToughnessNegative", (FORMAT_ITEM_POWER, perpetualPowerMod.ToString()), (FORMAT_ITEM_TOUGHNESS, Mathf.Abs(perpetualToughnessMod).ToString()));
		}
		if (num < 0 && num2 > 0)
		{
			return _locManager.GetLocalizedText("AbilityHanger/LayeredEffect/PerpetualPowerToughness_Body_ModPowerNegativeToughnessPositive", (FORMAT_ITEM_POWER, Mathf.Abs(perpetualPowerMod).ToString()), (FORMAT_ITEM_TOUGHNESS, perpetualToughnessMod.ToString()));
		}
		return _locManager.GetLocalizedText("AbilityHanger/LayeredEffect/PerpetualPowerToughness_Body_ModPowerToughnessPositive", (FORMAT_ITEM_POWER, perpetualPowerMod.ToString()), (FORMAT_ITEM_TOUGHNESS, perpetualToughnessMod.ToString()));
	}

	private void AddDisqualifiers(CardData cardData, MtgGameState gameState, IReadOnlyDictionary<uint, List<DisqualificationType>> disqualifierDictionary)
	{
		foreach (KeyValuePair<uint, List<DisqualificationType>> item in disqualifierDictionary)
		{
			MtgCardInstance cardById = gameState.GetCardById(item.Key);
			foreach (DisqualificationType item2 in item.Value)
			{
				string text = string.Empty;
				string text2 = string.Empty;
				string addendum = ((cardById != null) ? CardUtilities.FormatComplexTitle(_cardDatabase.CardTitleProvider.GetCardTitle(cardById.GrpId)) : string.Empty);
				switch (item2)
				{
				case DisqualificationType.Cast:
					text = _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Cast_Prevented_Title");
					text2 = getAbilityText(cardById, QualificationType.CantCast);
					break;
				case DisqualificationType.Play:
					text = _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Play_Prevented_Title");
					text2 = getAbilityText(cardById, QualificationType.CantPlay);
					break;
				}
				if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(text2))
				{
					CreateHangerItem(text, text2, addendum);
				}
			}
		}
		string getAbilityText(MtgCardInstance affector, QualificationType qualificationType)
		{
			string result = string.Empty;
			if (affector != null && cardData.Controller != null)
			{
				foreach (QualificationData affectedByQualification in cardData.Controller.AffectedByQualifications)
				{
					if (affectedByQualification.AffectorId == affector.InstanceId && affectedByQualification.Type == qualificationType)
					{
						result = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(affector.GrpId, affectedByQualification.AbilityId, affector.Abilities.Select((AbilityPrintingData o) => o.Id), affector.TitleId);
						break;
					}
				}
			}
			return result;
		}
	}

	private void AddQualifications(BASE_CDC cardView, CardData cardData, MtgGameState gameState)
	{
		List<QualificationData> list = _genericPool.PopObject<List<QualificationData>>();
		list.AddRange(cardData.AffectedByQualifications);
		if (list.Exists((QualificationData x) => x.Type == QualificationType.CantAttack) && list.Exists((QualificationData x) => x.Type == QualificationType.CantBlock))
		{
			List<QualificationData> list2 = list.FindAll((QualificationData x) => x.Type == QualificationType.CantAttack);
			List<QualificationData> list3 = list.FindAll((QualificationData x) => x.Type == QualificationType.CantBlock);
			foreach (QualificationData cantAttack in list2)
			{
				if (list3.Exists((QualificationData x) => x.AbilityId == cantAttack.AbilityId && x.AffectorId == cantAttack.AffectorId))
				{
					QualificationData item = list3.Find((QualificationData x) => x.AbilityId == cantAttack.AbilityId && x.AffectorId == cantAttack.AffectorId);
					list.Remove(cantAttack);
					list.Remove(item);
					List<uint> list4 = _genericPool.PopObject<List<uint>>();
					PopulateListOfGrpIdsToCheckForQualification(cardData, gameState, cantAttack, list4);
					string text = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(list4, cantAttack.AbilityId, cardData.AbilityIds, cardData.TitleId);
					AbilityPrintingData abilityPrintingById = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(cantAttack.AbilityId);
					if (abilityPrintingById != null && abilityPrintingById.Category == AbilityCategory.Activated && text.Contains(":"))
					{
						text = text.Substring(text.IndexOf(':') + 1);
					}
					string addendum = CardUtilities.FormatComplexTitle(_cardDatabase.CardTitleProvider.GetCardTitle(list4));
					string path = _iconPathProvider.GetPath("CantAttackOrBlock");
					CreateHangerItem(_locManager.GetLocalizedText("AbilityHanger/PlayWarning/Attack_Block_Prevented_Title"), text, addendum, path);
					list4.Clear();
					_genericPool.PushObject(list4, tryClear: false);
				}
			}
		}
		foreach (QualificationData item2 in list)
		{
			if (_cardDatabase.AbilityDataProvider.TryGetAbilityPrintingById(item2.AbilityId, out var ability) && !AbilityBadgeUtil.ShouldShowAbilityHanger(ability))
			{
				continue;
			}
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.Qualification = item2;
			_assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
			if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<QualificationHanger> loadedTree))
			{
				continue;
			}
			QualificationHanger payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload == null)
			{
				continue;
			}
			string text2 = string.Empty;
			string text3 = string.Empty;
			string addendum2 = string.Empty;
			string badgePath = payload.IconRef?.RelativePath ?? string.Empty;
			if (!string.IsNullOrEmpty(payload.LocTitleKey))
			{
				text2 = _locManager.GetLocalizedText(payload.LocTitleKey);
			}
			if (item2.AbilityId != 0)
			{
				CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(item2.AbilityParentGrpId);
				List<uint> list5 = _genericPool.PopObject<List<uint>>();
				PopulateListOfGrpIdsToCheckForQualification(cardData, gameState, item2, list5);
				text3 = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(list5, item2.AbilityId, cardData.AbilityIds, cardPrintingById?.TitleId ?? cardData.TitleId);
				if (text3.Contains(":") && text3.Substring(text3.IndexOf(':') + 1).Split('"').Length % 2 == 1)
				{
					text3 = text3.Substring(text3.IndexOf(':') + 1);
				}
				addendum2 = CardUtilities.FormatComplexTitle(_cardDatabase.CardTitleProvider.GetCardTitle(list5));
				list5.Clear();
				_genericPool.PushObject(list5, tryClear: false);
			}
			if (!string.IsNullOrEmpty(text2) || !string.IsNullOrEmpty(text3))
			{
				if ((item2.Type == QualificationType.MustBlockAttacker || item2.Type == QualificationType.BlockIfAble) && item2.Details.ContainsKey("must_block") && item2.Details.ContainsKey("must_be_blocked") && uint.TryParse(item2.Details["must_block"], out var result) && uint.TryParse(item2.Details["must_be_blocked"], out var result2) && _entityViewProvider.TryGetEntity(result, out var entityView) && _entityViewProvider.TryGetEntity(result2, out var entityView2) && cardView is DuelScene_CDC duelScene_CDC)
				{
					IEntityView arrowThisEntityView = duelScene_CDC;
					IEntityView arrowThatEntityView = ((item2.AffectedId != result2) ? entityView2 : entityView);
					bool arrowPointsToThat = item2.AffectedId == result2;
					CreateHangerItem(text2, text3, addendum2, badgePath, arrowThisEntityView, arrowThatEntityView, _hangerParent, arrowPointsToThat);
					return;
				}
				CreateHangerItem(text2, text3, addendum2, badgePath);
			}
		}
		list.Clear();
		_genericPool.PushObject(list, tryClear: false);
	}

	private void PopulateListOfGrpIdsToCheckForQualification(CardData cardData, MtgGameState gameState, QualificationData qualification, List<uint> grpIdListToPopulate)
	{
		if (qualification.AbilityParentGrpId != 0)
		{
			grpIdListToPopulate.Add(qualification.AbilityParentGrpId);
		}
		if (gameState.TryGetCard(qualification.AffectorId, out var card) || gameState.TrackedHistoricCards.TryGetValue(qualification.AffectorId, out card))
		{
			if (card.ObjectType == GameObjectType.Ability)
			{
				if (card.Parent != null)
				{
					grpIdListToPopulate.Add(card.Parent.GrpId);
				}
			}
			else
			{
				grpIdListToPopulate.Add(card.GrpId);
			}
		}
		grpIdListToPopulate.Add(cardData.GrpId);
		if (cardData.Parent != null)
		{
			grpIdListToPopulate.Add(cardData.Parent.GrpId);
		}
	}

	private void AddReplacementEffects(CardData cardData, MtgGameState gameState)
	{
		foreach (ReplacementEffectData replacementEffect in cardData.ReplacementEffects)
		{
			MtgCardInstance cardById = gameState.GetCardById(replacementEffect.AffectorId);
			uint num = cardById?.GrpId ?? 0;
			uint num2 = cardById?.BaseGrpId ?? 0;
			uint num3 = cardById?.ObjectSourceGrpId ?? 0;
			using IEnumerator<ReplacementEffectSpawnerType> enumerator2 = replacementEffect.SpawnerType.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				switch (enumerator2.Current)
				{
				case ReplacementEffectSpawnerType.PreventDamage:
				{
					StringBuilder stringBuilder = new StringBuilder();
					CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(num);
					CardPrintingData cardPrintingData = null;
					if (cardPrintingById != null && cardPrintingById.Abilities.Exists((AbilityPrintingData x) => x.Id == replacementEffect.AbilityId))
					{
						cardPrintingData = cardPrintingById;
					}
					if (num != num2 && cardPrintingData == null)
					{
						CardPrintingData cardPrintingById2 = _cardDatabase.CardDataProvider.GetCardPrintingById(num2);
						if (cardPrintingById2 != null && cardPrintingById2.Abilities.Exists((AbilityPrintingData x) => x.Id == replacementEffect.AbilityId))
						{
							cardPrintingData = cardPrintingById2;
						}
					}
					if (num != num3 && cardPrintingData == null)
					{
						CardPrintingData cardPrintingById3 = _cardDatabase.CardDataProvider.GetCardPrintingById(num3);
						if (cardPrintingById3 != null && cardPrintingById3.Abilities.Exists((AbilityPrintingData x) => x.Id == replacementEffect.AbilityId))
						{
							cardPrintingData = cardPrintingById3;
						}
					}
					if (cardPrintingData == null)
					{
						cardPrintingData = cardPrintingById;
					}
					stringBuilder.Append(ManaUtilities.RemovePrefixedLoyaltyCostFromAbilityText(_cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(cardPrintingData.GrpId, replacementEffect.AbilityId, cardData.AbilityIds, cardPrintingData.TitleId)));
					string addendum = CardUtilities.FormatComplexTitle(_cardDatabase.CardTitleProvider.GetCardTitle(num));
					if (replacementEffect.Durability.HasValue && replacementEffect.Durability.Value != 0)
					{
						if (replacementEffect.SourceIds != null && replacementEffect.SourceIds.Contains(cardData.InstanceId))
						{
							string localizedText2 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/PreventDamageHeader");
							StringBuilder stringBuilder2 = new StringBuilder(stringBuilder.ToString());
							AppendRecipient(replacementEffect.RecipientIds[0], gameState, stringBuilder2);
							AppendDurability(replacementEffect, stringBuilder2);
							string body = stringBuilder2.ToString();
							IEntityView entity = _entityViewProvider.GetEntity(replacementEffect.SourceIds[0]);
							IEntityView entity2 = _entityViewProvider.GetEntity(replacementEffect.RecipientIds[0]);
							CreateHangerItem(localizedText2, body, addendum, string.Empty, entity, entity2, _hangerParent);
						}
						if (replacementEffect.RecipientIds != null && replacementEffect.RecipientIds.Contains(cardData.InstanceId) && replacementEffect.SourceIds.Count > 0)
						{
							string localizedText3 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/ProtectedHeader");
							StringBuilder stringBuilder3 = new StringBuilder(stringBuilder.ToString());
							AppendSource(replacementEffect.SourceIds[0], gameState, stringBuilder3);
							AppendDurability(replacementEffect, stringBuilder3);
							string body2 = stringBuilder3.ToString();
							IEntityView entity3 = _entityViewProvider.GetEntity(replacementEffect.SourceIds[0]);
							IEntityView entity4 = _entityViewProvider.GetEntity(replacementEffect.RecipientIds[0]);
							CreateHangerItem(localizedText3, body2, addendum, string.Empty, entity3, entity4, _hangerParent);
						}
						break;
					}
					if (replacementEffect.SourceIds != null && replacementEffect.SourceIds.Contains(cardData.InstanceId))
					{
						MtgCardInstance cardById2 = gameState.GetCardById(cardData.InstanceId);
						if (cardById2 != null)
						{
							string localizedText4 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/PreventDamageHeader", ("cardName", _cardDatabase.GreLocProvider.GetLocalizedText(cardById2.TitleId)));
							string body3 = stringBuilder.ToString();
							CreateHangerItem(localizedText4, body3, addendum);
						}
					}
					if (replacementEffect.RecipientIds != null && replacementEffect.RecipientIds.Contains(cardData.InstanceId))
					{
						MtgCardInstance cardById3 = gameState.GetCardById(cardData.InstanceId);
						if (cardById3 != null)
						{
							string localizedText5 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/ProtectedHeader", ("cardName", _cardDatabase.GreLocProvider.GetLocalizedText(cardById3.TitleId)));
							string body4 = stringBuilder.ToString();
							CreateHangerItem(localizedText5, body4, addendum);
						}
					}
					break;
				}
				case ReplacementEffectSpawnerType.ExileOnDeath:
					if (cardData.ZoneType == ZoneType.Battlefield)
					{
						string empty = string.Empty;
						string localizedText = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/ExileOnDeath_Text");
						CreateHangerItem(empty, localizedText, "");
					}
					break;
				}
			}
		}
	}

	private void AddPlayWarnings(CardData cardData)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PlayWarningHangerEntry> loadedTree))
		{
			return;
		}
		HashSet<PlayWarningHangerEntry> hashSet = _genericPool.PopObject<HashSet<PlayWarningHangerEntry>>();
		loadedTree.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet);
		foreach (PlayWarningHangerEntry item in hashSet)
		{
			CreateHangerItem(_locManager.GetLocalizedText(item.Data.Title), _locManager.GetLocalizedText(item.Data.Term), "");
		}
	}

	protected override void AddSpecialHangers(BASE_CDC cardView, ICardDataAdapter sourceModel, HangerSituation situation)
	{
		if (sourceModel.ZoneType == ZoneType.Battlefield && sourceModel.ObjectType == GameObjectType.Token && sourceModel.Instance != null && sourceModel.Instance.IsCopy)
		{
			AddLayeredEffectHanger("CopyObject", 0, sourceModel);
		}
		foreach (HangerConfig hangerConfig in _specialHangerConfigProvider.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig);
		}
		if (cardView is DuelScene_CDC card)
		{
			foreach (LinkInfoData affectorOfLinkInfo in sourceModel.AffectorOfLinkInfos)
			{
				switch (affectorOfLinkInfo.LinkType)
				{
				case LinkType.Choose:
				{
					int value3;
					if (affectorOfLinkInfo.Details.TryGetValue<string>("ChooseLinkType", out var value2) && value2 == "AttributedInstance")
					{
						CreateChooseHanger(card, affectorOfLinkInfo.AffectorId, affectorOfLinkInfo.AffectedId);
					}
					else if (affectorOfLinkInfo.Details.TryGetValue<int>("damageRedirectionDestination", out value3))
					{
						IEntityView entity5 = _entityViewProvider.GetEntity(affectorOfLinkInfo.AffectorId);
						IEntityView entity6 = _entityViewProvider.GetEntity((uint)value3);
						string item3 = _entityNameProvider.GetName((uint)value3);
						_view.CreateHangerItem(_locManager.GetLocalizedText("DuelScene/FaceHanger/Annotation_AttributedInstance_Chosen"), _locManager.GetLocalizedText("DuelScene/FaceHanger/CardName", ("cardName", item3)), string.Empty, string.Empty, entity5, entity6, _hangerParent);
					}
					break;
				}
				case LinkType.Target:
				{
					if (affectorOfLinkInfo.Details.TryGetValue<string>("TargetLinkType", out var value) && value == "AttributedInstance")
					{
						IEntityView entity3 = _entityViewProvider.GetEntity(affectorOfLinkInfo.AffectorId);
						IEntityView entity4 = _entityViewProvider.GetEntity(affectorOfLinkInfo.AffectedId);
						string item2 = _entityNameProvider.GetName(affectorOfLinkInfo.AffectedId);
						_view.CreateHangerItem(_locManager.GetLocalizedText("DuelScene/FaceHanger/Annotation_AttributedInstance_Attached"), _locManager.GetLocalizedText("DuelScene/FaceHanger/CardName", ("cardName", item2)), string.Empty, string.Empty, entity3, entity4, _hangerParent);
					}
					break;
				}
				case LinkType.LoseAbility:
				{
					IEntityView entity = _entityViewProvider.GetEntity(affectorOfLinkInfo.AffectorId);
					IEntityView entity2 = _entityViewProvider.GetEntity(affectorOfLinkInfo.AffectedId);
					string item = _entityNameProvider.GetName(affectorOfLinkInfo.AffectedId);
					_view.CreateHangerItem(_locManager.GetLocalizedText("DuelScene/FaceHanger/Annotation_AttributedInstance_Targeted"), _locManager.GetLocalizedText("DuelScene/FaceHanger/CardName", ("cardName", item)), string.Empty, string.Empty, entity, entity2, _hangerParent);
					break;
				}
				}
			}
			foreach (LinkInfoData affectedByLinkInfo in sourceModel.AffectedByLinkInfos)
			{
				switch (affectedByLinkInfo.LinkType)
				{
				case LinkType.Choose:
				{
					int value5;
					if (affectedByLinkInfo.Details.TryGetValue<string>("ChooseLinkType", out var value4) && value4 == "AttributedInstance")
					{
						CreateChosenByHanger(affectedByLinkInfo.AffectorId, affectedByLinkInfo.AffectedId);
					}
					else if (affectedByLinkInfo.Details.TryGetValue<int>("damageRedirectionDestination", out value5))
					{
						IEntityView entity9 = _entityViewProvider.GetEntity(affectedByLinkInfo.AffectorId);
						IEntityView entity10 = _entityViewProvider.GetEntity((uint)value5);
						string item5 = _entityNameProvider.GetName(affectedByLinkInfo.AffectorId);
						_view.CreateHangerItem(_locManager.GetLocalizedText("DuelScene/FaceHanger/Annotation_AttributedInstance_ChosenBy"), _locManager.GetLocalizedText("DuelScene/FaceHanger/CardName", ("cardName", item5)), string.Empty, string.Empty, entity9, entity10, _hangerParent);
					}
					break;
				}
				case LinkType.LoseAbility:
				{
					IEntityView entity7 = _entityViewProvider.GetEntity(affectedByLinkInfo.AffectorId);
					IEntityView entity8 = _entityViewProvider.GetEntity(affectedByLinkInfo.AffectedId);
					string item4 = _entityNameProvider.GetName(affectedByLinkInfo.AffectorId);
					_view.CreateHangerItem(_locManager.GetLocalizedText("DuelScene/FaceHanger/Annotation_AttributedInstance_TargetedBy"), _locManager.GetLocalizedText("DuelScene/FaceHanger/CardName", ("cardName", item4)), string.Empty, string.Empty, entity7, entity8, _hangerParent);
					break;
				}
				}
			}
			foreach (QualificationData affectorOfQualification in sourceModel.AffectorOfQualifications)
			{
				if (affectorOfQualification.Type == QualificationType.CantGetCountersOnObject)
				{
					uint affector = ((affectorOfQualification.SourceParent != 0) ? affectorOfQualification.SourceParent : affectorOfQualification.AffectorId);
					CreateChooseHanger(card, affector, affectorOfQualification.AffectedId);
				}
			}
			foreach (QualificationData affectedByQualification in sourceModel.AffectedByQualifications)
			{
				if (affectedByQualification.Type == QualificationType.CantGetCountersOnObject)
				{
					uint affector2 = ((affectedByQualification.SourceParent != 0) ? affectedByQualification.SourceParent : affectedByQualification.AffectorId);
					CreateChosenByHanger(affector2, affectedByQualification.AffectedId);
				}
			}
		}
		base.AddSpecialHangers(cardView, sourceModel, situation);
	}

	private void CreateChooseHanger(DuelScene_CDC card, uint affector, uint affected)
	{
		IEntityView entityView = _entityViewProvider.GetEntity(affector);
		if (entityView == null)
		{
			entityView = card;
		}
		IEntityView entity = _entityViewProvider.GetEntity(affected);
		string item = _entityNameProvider.GetName(affected);
		string localizedText = _locManager.GetLocalizedText("DuelScene/FaceHanger/Annotation_AttributedInstance_Chosen");
		string localizedText2 = _locManager.GetLocalizedText("DuelScene/FaceHanger/CardName", ("cardName", item));
		_view.CreateHangerItem(localizedText, localizedText2, string.Empty, string.Empty, entityView, entity, _hangerParent);
	}

	private void CreateChosenByHanger(uint affector, uint affected)
	{
		IEntityView entity = _entityViewProvider.GetEntity(affector);
		IEntityView entity2 = _entityViewProvider.GetEntity(affected);
		string item = _entityNameProvider.GetName(affector);
		string localizedText = _locManager.GetLocalizedText("DuelScene/FaceHanger/Annotation_AttributedInstance_ChosenBy");
		string localizedText2 = _locManager.GetLocalizedText("DuelScene/FaceHanger/CardName", ("cardName", item));
		_view.CreateHangerItem(localizedText, localizedText2, string.Empty, string.Empty, entity, entity2, _hangerParent);
	}

	protected override void AddTypeHangers(BASE_CDC sourceCard)
	{
		if (sourceCard.Model.Instance == null)
		{
			base.AddTypeHangers(sourceCard);
			return;
		}
		bool flag = sourceCard.Model.Subtypes.Contains(SubType.AllCreatureTypes);
		if (!flag)
		{
			GameObjectType objectType = sourceCard.Model.ObjectType;
			if ((uint)(objectType - 2) <= 2u)
			{
				base.AddTypeHangers(sourceCard);
				return;
			}
		}
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		if (!sourceCard.Model.Supertypes.ContainSame(sourceCard.Model.Printing.Supertypes))
		{
			list.AddRange(GetAddedLocalizedTypes(sourceCard.Model.Supertypes, sourceCard.Model.Printing.Supertypes));
			list2.AddRange(GetRemovedLocalizedTypes(sourceCard.Model.Supertypes, sourceCard.Model.Printing.Supertypes));
		}
		if (!sourceCard.Model.CardTypes.ContainSame(sourceCard.Model.Printing.Types))
		{
			list.AddRange(GetAddedLocalizedTypes(sourceCard.Model.CardTypes, sourceCard.Model.Printing.Types));
			list2.AddRange(GetRemovedLocalizedTypes(sourceCard.Model.CardTypes, sourceCard.Model.Printing.Types));
		}
		if (!sourceCard.Model.Subtypes.ContainSame(sourceCard.Model.Printing.Subtypes))
		{
			list.AddRange(GetAddedLocalizedTypes(sourceCard.Model.Subtypes, sourceCard.Model.Printing.Subtypes));
			list2.AddRange(GetRemovedLocalizedTypes(sourceCard.Model.Subtypes, sourceCard.Model.Printing.Subtypes));
		}
		if (sourceCard.Model.Subtypes.Contains(SubType.AllCreatureTypes))
		{
			list2.Clear();
			foreach (SubType removedSubtype in sourceCard.Model.RemovedSubtypes)
			{
				string localizedTextForEnumValue = _cardDatabase.GreLocProvider.GetLocalizedTextForEnumValue("SubType", Convert.ToInt32(removedSubtype));
				list2.Add(localizedTextForEnumValue);
			}
		}
		if (ShouldAddTypelineHanger(sourceCard) || flag)
		{
			if (list.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(_cardDatabase.CardTypeProvider.GetCardTypeText(sourceCard.Model, CardTextColorSettings.INVERTED));
				stringBuilder.Append(string.Format(CardTextColorSettings.INVERTED.DefaultFormat, " - "));
				if (sourceCard.Model.Subtypes.Contains(SubType.AllCreatureTypes))
				{
					if (list2.Count > 0)
					{
						string localizedText = _locManager.GetLocalizedText("Card/Subtype/AllCreatureTypesExcept", ("exceptions", string.Join(", ", list2)));
						stringBuilder.Append(localizedText);
					}
					else
					{
						stringBuilder.Append(_locManager.GetLocalizedText("Card/Subtype/AllCreatureTypes"));
					}
				}
				else
				{
					stringBuilder.Append(_cardDatabase.CardTypeProvider.GetSubTypeText(sourceCard.Model, CardTextColorSettings.INVERTED));
				}
				CreateHangerItem(string.Empty, stringBuilder.ToString(), string.Empty, null, convertSymbols: true, -1, addedItem: true);
			}
			else
			{
				base.AddTypeHangers(sourceCard);
			}
		}
		if (list2.Count > 0)
		{
			CreateHangerItem(string.Empty, string.Format(CardTextColorSettings.INVERTED.RemovedFormat, string.Format("{0}: {1}", _locManager.GetLocalizedText("AbilityHanger/Type/Hanger_RemovedTypes"), string.Join(", ", list2))), string.Empty, null, convertSymbols: true, -2);
		}
	}

	private void AddTargetedHangers(CardData cardData)
	{
		foreach (MtgEntity target in cardData.Targets)
		{
			string localizedText = _locManager.GetLocalizedText("DuelScene/FaceHanger/Annotation_AttributedInstance_Targeted");
			string body = string.Empty;
			if (target is MtgPlayer mtgPlayer)
			{
				string item = _entityNameProvider.GetName(mtgPlayer.InstanceId);
				body = _locManager.GetLocalizedText("DuelScene/FaceHanger/PlayerName", ("playerName", item));
			}
			else if (target is MtgCardInstance instance)
			{
				string originalCardTitle = _cardDatabase.GetOriginalCardTitle(instance);
				body = _locManager.GetLocalizedText("DuelScene/FaceHanger/CardName", ("cardName", originalCardTitle));
			}
			CreateHangerItem(localizedText, convertHeaderSymbols: false, body, convertBodySymbols: false, string.Empty, convertAddendumSymbols: false);
		}
	}

	private void AddTargetedByHangers(CardData cardData, MtgGameState gameState)
	{
		foreach (MtgEntity entity in cardData.TargetedBy)
		{
			if (!(entity is MtgCardInstance instance))
			{
				continue;
			}
			string localizedText = _locManager.GetLocalizedText("DuelScene/FaceHanger/Annotation_AttributedInstance_TargetedBy");
			string localizedText2 = _locManager.GetLocalizedText("DuelScene/FaceHanger/CardName", ("cardName", _cardDatabase.GetCurrentCardTitle(instance)));
			string addendum = string.Empty;
			List<TargetSpec> list = gameState.TargetInfo.FindAll((TargetSpec x) => x.Affector == entity.InstanceId);
			if (list.Count > 0)
			{
				int num = -1;
				TargetSpec targetSpec = default(TargetSpec);
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					targetSpec = list[num2];
					uint item = ((cardData.Instance.ExamineSourceId != 0) ? cardData.Instance.ExamineSourceId : cardData.InstanceId);
					if (targetSpec.Affected.Contains(item))
					{
						num = num2;
						break;
					}
				}
				bool flag = list.Count == 1 && targetSpec.Affected.Count == 1;
				if (num > -1 && !flag)
				{
					addendum = _promptEngine.GetPromptText(targetSpec.Prompt, gameState, _cardDatabase);
					addendum = string.Format(TargetingColorer.GetHangerTextTargetingFormat(num, _assetLookupSystem, cardData), addendum);
				}
			}
			CreateHangerItem(localizedText, localizedText2, addendum);
		}
	}

	private void AddCommanderHangers(CardData cardData)
	{
		foreach (DesignationData designation in cardData.Instance.Designations)
		{
			if (designation.Type == Designation.Commander)
			{
				string item = _entityNameProvider.GetName(cardData.Owner.InstanceId);
				string localizedText = _locManager.GetLocalizedText("AbilityHanger/Designation/Commander_Title", ("PlayerName", item));
				string item2 = $"{{o{designation.CostIncrease}}}";
				string text = "";
				string iconSpritePath = AbilityBadgeUtil.GetBadgeDataForDesignation(_assetLookupSystem, Designation.Commander, cardData).IconSpritePath;
				if (cardData.AbilityIds.ContainsId(346u))
				{
					text = text + _locManager.GetLocalizedText("AbilityHanger/Keyword/Partner_Body") + "\n";
				}
				else if (cardData.AbilityIds.ContainsId(393u))
				{
					text = text + _locManager.GetLocalizedText("AbilityHanger/Keyword/PartnerCharacterSelect_Body") + "\n";
				}
				text += _locManager.GetLocalizedText("AbilityHanger/Designation/Commander_Body", ("cost", item2));
				CreateHangerItem(localizedText, convertHeaderSymbols: false, text, convertBodySymbols: true, "", convertAddendumSymbols: true, iconSpritePath);
			}
		}
	}

	protected void CreateHangerItem(string header, string body, string addendum, string badgePath, IEntityView arrowThisEntityView, IEntityView arrowThatEntityView, Transform parent, bool arrowPointsToThat = true)
	{
		_view.CreateHangerItem(header, body, addendum, badgePath, arrowThisEntityView, arrowThatEntityView, parent, arrowPointsToThat);
	}

	private void AppendSource(uint sourceId, MtgGameState gameState, StringBuilder sb)
	{
		MtgCardInstance cardById = gameState.GetCardById(sourceId);
		if (cardById != null)
		{
			sb.AppendLine();
			sb.AppendFormat(_locManager.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/Source", ("replacementSource", CardUtilities.FormatComplexTitle(_cardDatabase.CardTitleProvider.GetCardTitle(cardById.GrpId)))));
		}
	}

	private void AppendRecipient(uint recipientId, MtgGameState gameState, StringBuilder sb)
	{
		MtgEntity entityById = gameState.GetEntityById(recipientId);
		if (entityById != null)
		{
			string text = string.Empty;
			if (entityById is MtgCardInstance)
			{
				text = _cardDatabase.CardTitleProvider.GetCardTitle(((MtgCardInstance)entityById).GrpId);
			}
			else if (entityById is MtgPlayer)
			{
				text = _entityNameProvider.GetName(recipientId);
			}
			sb.AppendLine();
			sb.AppendFormat(_locManager.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/Recipient", ("replacementRecipient", CardUtilities.FormatComplexTitle(text))));
		}
	}

	private void AppendDurability(ReplacementEffectData replacementEffectData, StringBuilder sb)
	{
		if (replacementEffectData.Durability.HasValue)
		{
			sb.AppendLine();
			sb.AppendFormat(_locManager.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/AmountValue", ("durability", replacementEffectData.Durability.Value.ToString())));
		}
	}

	public void Shutdown()
	{
		DeactivateHanger();
		_gameStateProvider = NullGameStateProvider.Default;
		_workflowProvider = NullWorkflowProvider.Default;
		_entityViewProvider = NullEntityViewProvider.Default;
		_promptEngine = NullPromptEngine.Default;
		_genericPool = NullObjectPool.Default;
		_locManager = NullLocProvider.Default;
		_entityNameProvider = NullIdNameProvider.Default;
		_npeDirector = null;
		_hangerParent = null;
	}

	protected override void OnDestroy()
	{
		Shutdown();
		base.OnDestroy();
	}

	public static AbilityHanger Create(AssetLookupSystem assetLookupSystem, Transform parent, int layer = 0)
	{
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AbilityHangerPrefab> loadedTree))
		{
			AbilityHangerPrefab payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				AbilityHanger abilityHanger = AssetLoader.Instantiate<AbilityHanger>(payload.PrefabPath, parent);
				abilityHanger.gameObject.SetLayer(layer);
				return abilityHanger;
			}
		}
		return null;
	}
}
