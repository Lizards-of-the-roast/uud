using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Card;
using AssetLookupTree.Payloads.Prefab;
using GreClient.CardData;
using Pooling;
using UnityEngine;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Hangers.AbilityHangers;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class AbilityHangerBase : HangerBase
{
	protected AbilityHangerView _view;

	protected ICardDatabaseAdapter _cardDatabase;

	private readonly UnityEngine.Color _tooltipTextColor = new UnityEngine.Color(44f / 51f, 0.64705884f, 0.21960784f, 1f);

	private ICardDataAdapter _sourceModel;

	private HangerSituation _situation;

	private BASE_CDC _cardView;

	private bool _delayShow;

	protected AssetLookupSystem _assetLookupSystem;

	private IHangerConfigProvider _costModifiedConfigProvider = new NullConfigProvider();

	private IHangerConfigProvider _phyrexianManaConfigProvider = new NullConfigProvider();

	protected IHangerConfigProvider _dungeonHangerProvider = new NullConfigProvider();

	protected IHangerConfigProvider _parameterizedHangers = new NullConfigProvider();

	private MultiFaceHangerConfigProvider _multiFaceHangerConfigProvider;

	private IFlavorTextProvider _flavorTextProvider = new NullFlavorTextProvider();

	private IUnityObjectPool _unityObjectPool;

	private IFaceInfoGenerator _faceInfoGenerator;

	private IClientLocProvider _locManager;

	private IGreLocProvider _greLocManager;

	protected IAbilityHangerConfigProvider _abilityHangerProvider;

	public bool IsDirty { get; set; }

	public override bool IsDisplayedOnLeftSide
	{
		set
		{
			base.IsDisplayedOnLeftSide = value;
			_view.SetHangerSide(value ? HangerSide.Left : HangerSide.Right);
		}
	}

	public void Init(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, IFaceInfoGenerator faceInfoGenerator, IClientLocProvider locManager, DeckFormat currentEventFormat)
	{
		base.gameObject.SetActive(value: false);
		_cardDatabase = cardDatabase;
		_costModifiedConfigProvider = new CostModifiedConfigProvider(_cardDatabase.AbilityDataProvider);
		_phyrexianManaConfigProvider = new NullModelDecorator(new PhyrexianManaConfigProvider(_cardDatabase.ClientLocProvider, new PhyrexianManaIconPathProvider_ALT(assetLookupSystem.TreeLoader.LoadTree<PhyrexianManaIcon>(), assetLookupSystem.Blackboard)));
		_locManager = locManager;
		_greLocManager = cardDatabase.GreLocProvider;
		IPathProvider<AbilityPrintingData> iconPathProvider = new IconPathProvider_ALT(assetLookupSystem.TreeLoader.LoadTree<AssetLookupTree.Payloads.Ability.BadgeEntry>(), assetLookupSystem.Blackboard);
		IHangerConfigProvider configProvider = new VentureAbilityConfigProvider(_locManager, iconPathProvider);
		_dungeonHangerProvider = new NullModelDecorator(configProvider);
		_abilityHangerProvider = new AbilityHangerBaseConfigProvider(assetLookupSystem, cardDatabase, locManager, genericObjectPool);
		_parameterizedHangers = new ParameterizedHangerConfigProvider(cardDatabase.ClientLocProvider, assetLookupSystem, genericObjectPool, cardDatabase.CardDataProvider, new Dictionary<ParameterizedInjectors, IParameterizedInjector>
		{
			[ParameterizedInjectors.INJECT_MANA] = new InjectMana(),
			[ParameterizedInjectors.INJECT_LINKED_INFO] = new InjectLinkInfo(cardDatabase.GreLocProvider),
			[ParameterizedInjectors.INJECT_NUMERAL] = new InjectNumeral(),
			[ParameterizedInjectors.INJECT_FAKE_NUMERAL] = new InjectFakeNumeral(),
			[ParameterizedInjectors.INJECT_MANA_COLOR] = new InjectManaColor(cardDatabase.ClientLocProvider),
			[ParameterizedInjectors.INJECT_X] = new InjectX(cardDatabase.ClientLocProvider),
			[ParameterizedInjectors.INJECT_STATION_CHARGE_REQUIREMENT] = new InjectStationChargeRequirement(),
			[ParameterizedInjectors.INJECT_NUMERAL_MANA_SYMBOLS] = new InjectNumeralManaSymbols(),
			[ParameterizedInjectors.INJECT_NUMERAL_CARD_NAME] = new InjectNumeralCardName(cardDatabase.GreLocProvider)
		});
		_assetLookupSystem = assetLookupSystem;
		_unityObjectPool = unityObjectPool;
		_flavorTextProvider = new DefaultFlavorTextProvider(cardDatabase.GreLocProvider, cardDatabase.AltFlavorTextKeyProvider, cardDatabase.ClientLocProvider);
		_multiFaceHangerConfigProvider = new MultiFaceHangerConfigProvider(cardDatabase.ClientLocProvider, cardDatabase.GreLocProvider, _assetLookupSystem, _cardDatabase, currentEventFormat, genericObjectPool);
		_faceInfoGenerator = faceInfoGenerator;
		SpawnHangerView();
	}

	private void SpawnHangerView()
	{
		if (_view == null)
		{
			_assetLookupSystem.Blackboard.Clear();
			AbilityHangerViewPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<AbilityHangerViewPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
			_view = AssetLoader.Instantiate(payload.Prefab, base.transform);
			_view.Init(_unityObjectPool, _locManager, _assetLookupSystem);
		}
	}

	public void CopyHangarViewTransform(RectTransform sourceTransform)
	{
		RectTransform component = _view.GetComponent<RectTransform>();
		component.anchorMax = sourceTransform.anchorMax;
		component.anchorMin = sourceTransform.anchorMin;
		component.sizeDelta = sourceTransform.sizeDelta;
		component.anchoredPosition = sourceTransform.anchoredPosition;
		component.localScale = sourceTransform.localScale;
	}

	public void SetViewDragScroll(bool isDragScroll)
	{
		_view.SetDragScroll(isDragScroll);
	}

	protected virtual void OnDestroy()
	{
		if ((bool)_cardView)
		{
			_cardView.OnPostCardUpdated -= OnPostCardUpdated;
			_cardView = null;
		}
		_unityObjectPool = null;
	}

	public override void ActivateHanger(BASE_CDC cardView, ICardDataAdapter sourceModel, HangerSituation situation, bool delayShow = false)
	{
		_cardView = cardView;
		_sourceModel = sourceModel;
		_situation = situation;
		_delayShow = delayShow;
		cardView.OnPostCardUpdated += OnPostCardUpdated;
		cardView.IsDirty = true;
		IsDirty = true;
		cardView.ImmediateUpdate();
	}

	private void OnPostCardUpdated()
	{
		if ((bool)_cardView)
		{
			_cardView.OnPostCardUpdated -= OnPostCardUpdated;
		}
		if (IsDirty)
		{
			IsDirty = false;
			if ((bool)_cardView && _cardView.Model != null && _sourceModel != null)
			{
				AddHangersInternal(_cardView, _sourceModel, _situation);
			}
			if ((bool)_view && _view.HangerItems != null)
			{
				base.Active = _view.HangerItems.Count > 0;
			}
			if ((bool)base.gameObject)
			{
				base.gameObject.UpdateActive(base.Active && !_delayShow);
			}
		}
	}

	public override void DeactivateHanger()
	{
		if ((bool)_cardView)
		{
			_cardView.OnPostCardUpdated -= OnPostCardUpdated;
		}
		base.Active = false;
		_abilityHangerProvider?.Cleanup();
		if ((bool)_view)
		{
			_view.CleanupAllHangers();
		}
		base.gameObject.UpdateActive(active: false);
	}

	public override bool HandleScroll(Vector2 delta)
	{
		return false;
	}

	public override float GetHangerWidth()
	{
		Vector3[] array = new Vector3[4];
		GetComponent<RectTransform>().GetWorldCorners(array);
		return Mathf.Abs(array[0].x - array[2].x);
	}

	protected void CreateHangerItem(HangerConfig config, bool npe = false)
	{
		bool addedItem = config.Color == HangerColor.Added;
		bool perpetualItem = config.Color == HangerColor.Perpetual;
		CreateHangerItem(config.Header, config.Details, config.Addendum, config.SpritePath, config.ConvertSymbols, config.Section, addedItem, npe, perpetualItem);
	}

	protected void CreateHangerItem(string header, string body, string addendum, string badgePath, UnityEngine.Color color, bool convertSymbols = true, int section = 0, bool useNPEBattlefieldItem = false)
	{
		_view.CreateHangerItem(header, body, addendum, badgePath, color, convertSymbols, section, useNPEBattlefieldItem);
	}

	protected void CreateHangerItem(string header, string body, string addendum, string badgePath = null, bool convertSymbols = true, int section = 0, bool addedItem = false, bool useNPEBattlefieldItem = false, bool perpetualItem = false)
	{
		_view.CreateHangerItem(header, body, addendum, badgePath, convertSymbols, section, addedItem, useNPEBattlefieldItem, perpetualItem);
	}

	protected void CreateHangerItem(string header, bool convertHeaderSymbols, string body, bool convertBodySymbols, string addendum, bool convertAddendumSymbols, string badgePath = null, int section = 0, bool addedItem = false, bool useNPEBattlefieldItem = false, bool perpetualItem = false)
	{
		_view.CreateHangerItem(header, convertHeaderSymbols, body, convertBodySymbols, addendum, convertAddendumSymbols, badgePath, section, addedItem, useNPEBattlefieldItem, perpetualItem);
	}

	protected virtual void AddHangersInternal(BASE_CDC cardView, ICardDataAdapter sourceModel, HangerSituation situation)
	{
		ICardDataAdapter model = cardView.Model;
		if (model.IsFakeStyleCard)
		{
			_view.CreateHangerCardStyle();
		}
		foreach (HangerConfig hangerConfig in _parameterizedHangers.GetHangerConfigs(model))
		{
			CreateHangerItem(hangerConfig);
		}
		foreach (InfoHanger infoHangerPayload in cardView.GetInfoHangerPayloads())
		{
			CreateInfoHangerItem(infoHangerPayload);
		}
		if (situation.ShowFlavorText)
		{
			AddFlavorText(sourceModel);
		}
		AddTypeHangers(cardView);
		AddSpecialHangers(cardView, sourceModel, situation);
		if (!situation.ShowOnlyTapped)
		{
			if (model.ObjectType != GameObjectType.Ability || (model.Abilities.Count == 1 && model.Abilities[0].Category == AbilityCategory.Static))
			{
				AddAbilities(cardView, situation);
				AddRemovedAbilities(model);
			}
			else if (model.ObjectType == GameObjectType.Ability)
			{
				AbilityPrintingData abilityPrintingData = ((model.Abilities.Count > 0) ? model.Abilities[0] : _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(model.GrpId));
				if (abilityPrintingData != null)
				{
					foreach (HangerConfig item in _abilityHangerProvider.GetHangerConfigsForAbility(cardView.Model, cardView.HolderType, new CDCViewMetadata(cardView), abilityPrintingData))
					{
						CreateHangerItem(item, situation.UseNPEHanger);
					}
				}
			}
		}
		foreach (HangerConfig hangerConfig2 in _dungeonHangerProvider.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig2);
		}
		foreach (HangerConfig hangerConfig3 in _phyrexianManaConfigProvider.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig3);
		}
		foreach (HangerConfig hangerConfig4 in _costModifiedConfigProvider.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig4);
		}
		_multiFaceHangerConfigProvider.FaceInfoGenerator = _faceInfoGenerator;
		_multiFaceHangerConfigProvider.CardView = _cardView;
		foreach (HangerConfig hangerConfig5 in _multiFaceHangerConfigProvider.GetHangerConfigs(sourceModel))
		{
			CreateHangerItem(hangerConfig5.Header, hangerConfig5.Details, string.Empty, hangerConfig5.SpritePath, _tooltipTextColor);
		}
		foreach (FaceHanger.FaceCardInfo item2 in _faceInfoGenerator.GenerateFaceCardInfo(cardView.Model, sourceModel))
		{
			if (item2.HangerType != FaceHanger.HangerType.TokenReference && item2.HangerType != FaceHanger.HangerType.RoleReference)
			{
				continue;
			}
			foreach (HangerConfig item3 in _abilityHangerProvider.GetHangerConfigsForCard(item2.CardData, cardView.HolderType, new CDCViewMetadata(cardView)))
			{
				CreateHangerItem(item3, situation.UseNPEHanger);
			}
		}
	}

	private void CreateInfoHangerItem(InfoHanger payload)
	{
		string text = (string.IsNullOrEmpty(payload.AddendumLocKey.Key) ? string.Empty : ("\n" + payload.AddendumLocKey.GetText(_locManager, _greLocManager)));
		_view.CreateHangerItem(formatText(payload.Formatting, payload.HeaderLocKey.GetText(_locManager, _greLocManager)), formatText(payload.Formatting, string.Concat(payload.BodyLocKey.GetText(_locManager, _greLocManager), text)), "", payload.BadgeRef.RelativePath);
		static string formatText(InfoHanger.TextFormatting formatting, string rawText)
		{
			if (formatting != InfoHanger.TextFormatting.None && formatting == InfoHanger.TextFormatting.RemoveLoyaltyValue)
			{
				int num = rawText.IndexOf(':');
				if (num == -1)
				{
					num = rawText.IndexOf('：');
				}
				return rawText.Substring(num + 1).Trim();
			}
			return rawText;
		}
	}

	private void AddFlavorText(ICardDataAdapter cardData)
	{
		string promoLabel = cardData.Printing.PromoLabel;
		if (_flavorTextProvider.TryGetFlavorText(cardData, out var flavorText) && !isDisplayingFlavorText(_cardView))
		{
			flavorText += (string.IsNullOrWhiteSpace(promoLabel) ? null : ("<br></i>" + promoLabel + "<i>"));
			_view.CreateHangerQuote(flavorText, 5);
		}
		else if (!string.IsNullOrWhiteSpace(promoLabel))
		{
			_view.CreateHangerQuote(promoLabel, 5);
		}
		static bool isDisplayingFlavorText(BASE_CDC cardView)
		{
			if (cardView == null)
			{
				return false;
			}
			CDCPart_TextBox_Rules cDCPart_TextBox_Rules = cardView.FindPart<CDCPart_TextBox_Rules>(AnchorPointType.TextBox);
			if (cDCPart_TextBox_Rules != null)
			{
				return cDCPart_TextBox_Rules.DisplayingFlavorText;
			}
			return false;
		}
	}

	private void AddAbilities(BASE_CDC cardView, HangerSituation situation)
	{
		foreach (HangerConfig item in _abilityHangerProvider.GetHangerConfigsForCard(cardView.Model, cardView.HolderType, new CDCViewMetadata(cardView)))
		{
			CreateHangerItem(item, situation.UseNPEHanger);
		}
	}

	protected virtual void AddRemovedAbilities(ICardDataAdapter cardData)
	{
	}

	protected virtual void AddSpecialHangers(BASE_CDC sourceCard, ICardDataAdapter sourceModel, HangerSituation situation)
	{
		AbilityHangerData[] contextualHangers = situation.ContextualHangers;
		if (contextualHangers != null && contextualHangers.Length != 0)
		{
			AbilityHangerData[] contextualHangers2 = situation.ContextualHangers;
			for (int i = 0; i < contextualHangers2.Length; i++)
			{
				AbilityHangerData abilityHangerData = contextualHangers2[i];
				CreateHangerItem(abilityHangerData.Header, abilityHangerData.Body, abilityHangerData.Addendum, abilityHangerData.BadgePath, abilityHangerData.Color);
			}
		}
		if (sourceModel.Supertypes.Contains(SuperType.Legendary) && sourceModel.CardTypes.Contains(CardType.Sorcery))
		{
			string localizedText = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/LegendarySorcery/Header");
			string localizedText2 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/LegendarySorcery/Body");
			CreateHangerItem(localizedText, localizedText2, "");
		}
		if (sourceModel.Supertypes.Contains(SuperType.Legendary) && sourceModel.CardTypes.Contains(CardType.Instant))
		{
			string localizedText3 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/LegendaryInstant/Header");
			string localizedText4 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/LegendaryInstant/Body");
			CreateHangerItem(localizedText3, localizedText4, "");
		}
		if (sourceModel.PowerToughnessInverted)
		{
			CreateHangerItem(_locManager.GetLocalizedText("AbilityHanger/SpecialHangers/PowerToughnessInverted_Title"), _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/PowerToughnessInverted_Body"), "");
		}
		if (sourceModel.IsWildcard)
		{
			string text = string.Empty;
			string item = string.Empty;
			switch (sourceModel.Rarity)
			{
			case CardRarity.Common:
				text = _locManager.GetLocalizedText("MainNav/General/CommonWildcard");
				item = _locManager.GetLocalizedText("MainNav/General/Rarity/Common");
				break;
			case CardRarity.Uncommon:
				text = _locManager.GetLocalizedText("MainNav/General/UncommonWildcard");
				item = _locManager.GetLocalizedText("MainNav/General/Rarity/Uncommon");
				break;
			case CardRarity.Rare:
				text = _locManager.GetLocalizedText("MainNav/General/RareWildcard");
				item = _locManager.GetLocalizedText("MainNav/General/Rarity/Rare");
				break;
			case CardRarity.MythicRare:
				text = _locManager.GetLocalizedText("MainNav/General/MythicRareWildcard");
				item = _locManager.GetLocalizedText("MainNav/General/Rarity/MythicRare");
				break;
			}
			string header = text;
			string localizedText5 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/Wildcard/Wildcard_Body", ("rarity", item));
			CreateHangerItem(header, localizedText5, "", AbilityBadgeUtil.GetBadgeDataForCondition(_assetLookupSystem, ConditionType.WildCard, sourceModel).IconSpritePath);
		}
		string[] bannedFormats = situation.BannedFormats;
		if (bannedFormats != null && bannedFormats.Length != 0)
		{
			string localizedText6 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/Banned_Header");
			string item2 = string.Join(Environment.NewLine, situation.BannedFormats);
			string localizedText7 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/Banned_Body", ("formats", item2));
			CreateHangerItem(localizedText6, localizedText7, "", AbilityBadgeUtil.GetBadgeDataForCondition(_assetLookupSystem, ConditionType.BannedCard, sourceModel).IconSpritePath);
		}
		string[] restrictedFormats = situation.RestrictedFormats;
		if (restrictedFormats != null && restrictedFormats.Length != 0)
		{
			string localizedText8 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/Restricted_Header");
			for (int j = 0; j < situation.RestrictedFormats.Length; j++)
			{
				situation.RestrictedFormats[j] = situation.RestrictedFormats[j].Replace(",", " (" + _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/Restricted_Header") + " ") + ")";
			}
			string item3 = string.Join(Environment.NewLine, situation.RestrictedFormats);
			string localizedText9 = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/Restricted_Body", ("formats", item3));
			CreateHangerItem(localizedText8, localizedText9, "", AbilityBadgeUtil.GetBadgeDataForCondition(_assetLookupSystem, ConditionType.Restricted, sourceModel).IconSpritePath);
		}
		if (situation.EmergencyTempBanHanger.HasValue)
		{
			CreateHangerItem(situation.EmergencyTempBanHanger.Value);
		}
	}

	protected bool ShouldAddTypelineHanger(BASE_CDC sourceCard)
	{
		CDCFieldFiller filler;
		CDCPart cDCPart = sourceCard.FindPartWithFiller<CDCFieldFiller, CDCFieldFillerFieldType, CDCPart>(CDCFieldFillerFieldType.TypeLine, out filler);
		if ((bool)cDCPart)
		{
			cDCPart.UpdatePendingFields(force: true);
			return filler.IsTextTruncated();
		}
		return false;
	}

	protected virtual void AddTypeHangers(BASE_CDC sourceCard)
	{
		if (!sourceCard.Model.IsWildcard && ShouldAddTypelineHanger(sourceCard))
		{
			CreateHangerItem("", _cardDatabase.CardTypeProvider.GetTypelineText(sourceCard.Model, CardTextColorSettings.INVERTED), "", null, convertSymbols: true, -1);
		}
	}

	protected List<string> GetAddedLocalizedTypes<T>(IReadOnlyCollection<T> currentTypes, IReadOnlyCollection<T> printingTypes) where T : Enum
	{
		List<string> list = new List<string>(currentTypes.Count);
		foreach (T currentType in currentTypes)
		{
			if (!printingTypes.Contains(currentType))
			{
				string localizedTextForEnumValue = _cardDatabase.GreLocProvider.GetLocalizedTextForEnumValue(typeof(T).Name, Convert.ToInt32(currentType));
				list.Add(localizedTextForEnumValue);
			}
		}
		return list;
	}

	protected List<string> GetRemovedLocalizedTypes<T>(IReadOnlyCollection<T> currentTypes, IReadOnlyCollection<T> printingTypes) where T : Enum
	{
		List<string> list = new List<string>(currentTypes.Count);
		foreach (T printingType in printingTypes)
		{
			if (!currentTypes.Contains(printingType))
			{
				string localizedTextForEnumValue = _cardDatabase.GreLocProvider.GetLocalizedTextForEnumValue(typeof(T).Name, Convert.ToInt32(printingType));
				list.Add(localizedTextForEnumValue);
			}
		}
		return list;
	}
}
