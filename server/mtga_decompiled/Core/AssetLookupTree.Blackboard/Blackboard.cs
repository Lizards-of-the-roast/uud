using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Extractors.UI;
using AssetLookupTree.Payloads.Ability.Metadata;
using Assets.Core.Shared.Code;
using Core.Meta.MainNavigation.SocialV2;
using GreClient.CardData;
using GreClient.Rules;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Store;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Duel;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.AvatarView;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Wrapper;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Blackboard;

public class Blackboard : IBlackboard
{
	private readonly HashSet<Action<IBlackboard>> _bbFillers = new HashSet<Action<IBlackboard>>();

	[JsonIgnore]
	private Material _material;

	[JsonIgnore]
	private Texture2D _texture;

	[JsonIgnore]
	private TMP_FontAsset _font;

	private ICardDataAdapter _cardData;

	private DateTime? _explicitDateTimeUtc;

	public Dictionary<SupplementalKey, ICardDataAdapter> SupplementalCardData { get; } = new Dictionary<SupplementalKey, ICardDataAdapter>();

	public ulong ContentVersion { get; private set; }

	[JsonIgnore]
	public ICardDatabaseAdapter CardDatabase { get; private set; }

	[JsonIgnore]
	public ICardDataProvider CardDataProvider => CardDatabase?.CardDataProvider;

	[JsonIgnore]
	public IAbilityDataProvider AbilityDataProvider => CardDatabase?.AbilityDataProvider;

	[JsonIgnore]
	public IEntityNameProvider<uint> IdNameProvider { get; set; }

	public bool InWrapper { get; set; }

	public bool InDuelScene { get; set; }

	[JsonIgnore]
	public Material Material
	{
		get
		{
			return _material;
		}
		set
		{
			_material = value;
			MaterialName = (value ? value.name : string.Empty);
		}
	}

	public string MaterialName { get; set; }

	[JsonIgnore]
	public Texture2D Texture
	{
		get
		{
			return _texture;
		}
		set
		{
			_texture = value;
			TextureName = (value ? value.name : string.Empty);
		}
	}

	public string TextureName { get; set; }

	[JsonIgnore]
	public TMP_FontAsset Font
	{
		get
		{
			return _font;
		}
		set
		{
			_font = value;
			FontName = (value ? value.name : string.Empty);
		}
	}

	public string FontName { get; set; }

	public string CampaignGraphNodeName { get; set; }

	public bool CardIsHovered { get; set; }

	public bool IsHoverCopy { get; set; }

	public bool IsExaminedCard { get; set; }

	public bool IsDimmedCard { get; set; }

	public ICardDataAdapter CardData => _cardData;

	[JsonIgnore]
	public ICardHolder CardHolder { get; set; }

	public CardHolderType CardHolderType { get; set; }

	public CounterType CounterType { get; set; }

	public CDCFieldFillerFieldType FieldFillerType { get; set; }

	public CDCSpriteFiller.FieldType SpriteFillerType { get; set; }

	public CDCSpriteFillerSDF.FieldType SpriteSdfFillerType { get; set; }

	public CDCManaCostFiller.FieldType ManaFillerType { get; set; }

	public AbilityPrintingData Ability { get; set; }

	public AbilityType AbilityType { get; set; }

	public ReplacementEffectData ReplacementEffectData { get; set; }

	public IBadgeEntryData BadgeData { get; set; }

	public MatchManager.PlayerInfo PlayerInfoMatch { get; set; }

	public PlayerInfo PlayerInfoGame { get; set; }

	public MtgPlayer Player { get; set; }

	public MtgGameState GameState { get; set; }

	[JsonIgnore]
	public BaseUserRequest Request { get; set; }

	[JsonIgnore]
	public WorkflowBase Interaction { get; set; }

	public SelectionParams SelectionParams { get; set; }

	public TargetSelectionParams TargetSelectionParams { get; set; }

	public MouseOverType MouseOverType { get; set; }

	public ResolutionEffectModel ActiveResolution { get; set; }

	public string Language { get; set; }

	public DateTime DateTimeUtc
	{
		get
		{
			return _explicitDateTimeUtc ?? ServerGameTime.GameTime;
		}
		set
		{
			_explicitDateTimeUtc = value;
		}
	}

	public HighlightType HighlightType { get; set; }

	public NavContentType NavContentType { get; set; }

	public DeviceType DeviceType { get; set; }

	public float AspectRatio { get; set; }

	public string BattlefieldId { get; set; }

	public string PetId { get; set; }

	public string PetVariantId { get; set; }

	public int PetLevel { get; set; }

	public RewardType RewardType { get; set; }

	public int? DamageAmount { get; set; }

	public DamageType DamageType { get; set; }

	public CardReactionEnum CardReactionType { get; set; }

	public HashSet<DecoratorType> DecoratorTypes { get; set; }

	public DuelSceneBrowserType CardBrowserType { get; set; }

	public string CardBrowserElementID { get; set; }

	public string CardBrowserLayoutID { get; set; }

	public uint? CardBrowserCardCount { get; set; }

	public (int min, uint max)? SelectCardBrowserMinMax { get; set; }

	public ActionType GreActionType { get; set; }

	public Wotc.Mtgo.Gre.External.Messaging.Action GreAction { get; set; }

	public ButtonStyle.StyleType ButtonStyle { get; set; }

	public MtgZone ZoneSelection { get; set; }

	public MtgZone FromZone { get; set; }

	public MtgZone ToZone { get; set; }

	public ZoneTransferReason ZoneTransferReason { get; set; }

	public LoyaltyValence LoyaltyValence { get; set; }

	public EmotePrefabData EmotePrefabData { get; set; }

	public GREPlayerNum GREPlayerNum { get; set; }

	public TooltipContext TooltipContext { get; set; }

	public string CarouselName { get; set; }

	public QualityMode_CardVFX CardVfxQuality { get; set; }

	public QualityMode_Pet PetQuality { get; set; }

	public int LifeChange { get; set; }

	public string LocalNotificationUID { get; set; }

	public ManaColor ManaColor { get; set; }

	public int ManaSelectionCount { get; set; }

	public string CosmeticAvatarId { get; set; }

	public string CosmeticStoreSKU { get; set; }

	public string CosmeticSleeveId { get; set; }

	public string CosmeticAccessoryId { get; set; }

	public string CosmeticAccessoryMod { get; set; }

	public string EmoteId { get; set; }

	public string IncomingEmoteId { get; set; }

	public ZonePair ZonePair { get; set; }

	public RegionPair RegionPair { get; set; }

	public CollationMapping BoosterCollationMapping { get; set; }

	public EventContext Event { get; set; }

	public (RankingClassType rank, int tier) ConstructedRank { get; set; }

	public (RankingClassType rank, int tier) LimitedRank { get; set; }

	public HashSet<PropertyType> UpdatedProperties { get; set; }

	public ConditionType Condition { get; set; }

	public QualificationData? Qualification { get; set; }

	public string PrefabName { get; set; }

	public string SetCode { get; set; }

	public int? TargetIndex { get; set; }

	public string LookupString { get; set; }

	public ManaMovementData ManaMovement { get; set; }

	public Prompt Prompt { get; set; }

	public uint PromptParameterId { get; set; }

	public string RewardTrack { get; set; }

	public CardHolderBase.CardPosition CardInsertionPosition { get; set; }

	public SyntheticEventType SyntheticEvent { get; set; }

	public string LayeredEffectType { get; set; }

	public IEnumerable<LayeredEffectData> LayeredEffects { get; set; }

	public Designation? Designation { get; set; }

	public LinkInfoData LinkInfo { get; set; }

	public LinkedInfoText LinkedInfoText { get; set; }

	public uint? DieRollNaturalResult { get; set; }

	public uint HoverFaceHangerCount { get; set; }

	public uint ExamineFaceHangerCount { get; set; }

	public bool InHorizontalDeckBuilder { get; set; }

	public bool CanCraft { get; set; }

	public bool IsHangerText { get; set; }

	public int UnitCount { get; set; }

	public string Flavor { get; set; }

	public ICardTextEntry CardTextEntry { get; set; }

	public MtgEntity DamageRecipientEntity { get; set; }

	public string LetterBannerArtId { get; set; }

	public string SceneToLoad { get; set; }

	public string LearnMoreSectionContentName { get; set; }

	public AvatarFramePartType AvatarFramePart { get; set; }

	public PhaseIconType PhaseIconType { get; set; }

	public ManaPaymentCondition ManaPaymentCondition { get; set; }

	public bool FixedRulesTextSize { get; set; }

	public LUTVFXType LutVfxType { get; set; }

	public int SelectCardBrowserCurrentSelectionCount { get; set; }

	public bool IsNPEComplete { get; set; }

	public bool IsExpanded { get; set; }

	public Vector3 IdealWorldPosition { get; set; }

	public uint DieFaces { get; set; }

	public PurchaseFlow PurchaseFlow { get; set; }

	public GatheringPrivilegeLevel GatheringUserPrivilege { get; set; }

	public Blackboard()
	{
		QualityModeProvider qualityModeProvider = PlatformContext.GetQualityModeProvider();
		CardVfxQuality = qualityModeProvider.GetQualityMode_CardVFX();
		PetQuality = qualityModeProvider.GetQualityMode_Pet();
		DeviceType = PlatformUtils.GetCurrentDeviceType();
		AspectRatio = PlatformUtils.GetCurrentAspectRatio();
		Clear();
		AddFillerDelegate(DefaultFillBlackboard);
	}

	private void DefaultFillBlackboard(IBlackboard bb)
	{
		bb.Language = Languages.CurrentLanguage;
	}

	public void Inject(ICardDatabaseAdapter cardDatabase)
	{
		CardDatabase = cardDatabase;
	}

	public void Clear()
	{
		ContentVersion++;
		Material = null;
		MaterialName = null;
		Texture = null;
		TextureName = null;
		Font = null;
		FontName = null;
		CampaignGraphNodeName = null;
		CarouselName = null;
		SetCardDataRaw(null);
		SupplementalCardData.Clear();
		CardHolder = null;
		CardHolderType = CardHolderType.Invalid;
		CounterType = CounterType.None;
		FieldFillerType = CDCFieldFillerFieldType.None;
		SpriteFillerType = CDCSpriteFiller.FieldType.None;
		SpriteSdfFillerType = CDCSpriteFillerSDF.FieldType.None;
		Ability = null;
		AbilityType = AbilityType.None;
		ReplacementEffectData = default(ReplacementEffectData);
		BadgeData = null;
		PlayerInfoMatch = null;
		PlayerInfoGame = null;
		Player = null;
		Request = null;
		Interaction = null;
		SelectionParams = default(SelectionParams);
		TargetSelectionParams = default(TargetSelectionParams);
		CardIsHovered = false;
		MouseOverType = MouseOverType.None;
		ActiveResolution = null;
		IsHoverCopy = false;
		IsExaminedCard = false;
		IsHangerText = false;
		IsDimmedCard = false;
		Language = null;
		GameState = null;
		IdNameProvider = null;
		HighlightType = HighlightType.None;
		NavContentType = NavContentType.None;
		PetId = null;
		PetLevel = 0;
		RewardType = RewardType.None;
		BattlefieldId = null;
		DecoratorTypes = null;
		DamageAmount = null;
		DamageType = DamageType.None;
		CardReactionType = CardReactionEnum.None;
		CardBrowserType = DuelSceneBrowserType.Invalid;
		CardBrowserLayoutID = null;
		CardBrowserCardCount = null;
		SelectCardBrowserMinMax = null;
		SelectCardBrowserCurrentSelectionCount = 0;
		GreActionType = ActionType.None;
		GreAction = null;
		_explicitDateTimeUtc = null;
		ButtonStyle = global::ButtonStyle.StyleType.None;
		FromZone = null;
		ToZone = null;
		ZoneTransferReason = ZoneTransferReason.Invalid;
		LoyaltyValence = LoyaltyValence.Invalid;
		EmotePrefabData = null;
		GREPlayerNum = GREPlayerNum.Invalid;
		TooltipContext = TooltipContext.Default;
		LifeChange = 0;
		LocalNotificationUID = null;
		CosmeticAvatarId = null;
		CosmeticStoreSKU = null;
		CosmeticSleeveId = null;
		CosmeticAccessoryId = null;
		CosmeticAccessoryMod = null;
		EmoteId = null;
		IncomingEmoteId = null;
		ZonePair = default(ZonePair);
		RegionPair = default(RegionPair);
		BoosterCollationMapping = CollationMapping.None;
		Event = null;
		ConstructedRank = default((RankingClassType, int));
		LimitedRank = default((RankingClassType, int));
		UpdatedProperties = null;
		Condition = ConditionType.None;
		Qualification = null;
		PrefabName = null;
		SetCode = null;
		LookupString = null;
		TargetIndex = null;
		ManaMovement = default(ManaMovementData);
		Prompt = null;
		PromptParameterId = 0u;
		RewardTrack = null;
		CardInsertionPosition = CardHolderBase.CardPosition.None;
		SyntheticEvent = SyntheticEventType.None;
		LayeredEffectType = null;
		LayeredEffects = null;
		Designation = null;
		LinkInfo = default(LinkInfoData);
		LinkedInfoText = default(LinkedInfoText);
		HoverFaceHangerCount = 0u;
		ExamineFaceHangerCount = 0u;
		UnitCount = 0;
		Flavor = null;
		CardTextEntry = null;
		DamageRecipientEntity = null;
		LetterBannerArtId = null;
		LearnMoreSectionContentName = null;
		AvatarFramePart = AvatarFramePartType.None;
		PhaseIconType = PhaseIconType.None;
		FixedRulesTextSize = false;
		LutVfxType = LUTVFXType.Sweeper;
		IsNPEComplete = false;
		IsExpanded = false;
		IdealWorldPosition = default(Vector3);
		DieFaces = 0u;
		GatheringUserPrivilege = GatheringPrivilegeLevel.None;
		PurchaseFlow = PurchaseFlow.XsollaEmbedded;
		foreach (Action<IBlackboard> bbFiller in _bbFillers)
		{
			bbFiller?.Invoke(this);
		}
	}

	public void AddFillerDelegate(Action<IBlackboard> bbFiller)
	{
		_bbFillers.Add(bbFiller);
	}

	public void RemoveFillerDelegate(Action<IBlackboard> bbFiller)
	{
		_bbFillers.Remove(bbFiller);
	}

	public void SetCardDataExtensive(MtgCardInstance cardInstance)
	{
		if (cardInstance != null && CardDatabase != null)
		{
			SetCardDataExtensive(CardDataExtensions.CreateWithDatabase(cardInstance, CardDatabase));
		}
	}

	public void SetCardDataExtensive(ICardDataAdapter cardData)
	{
		_cardData = CardUtilities.CreateAbilityModData(cardData);
		if (cardData == null)
		{
			return;
		}
		if (CardHolderType == CardHolderType.None)
		{
			CardHolderType = cardData.ZoneType.ToCardHolderType();
		}
		if (Ability == null && AbilityDataProvider != null)
		{
			switch (cardData.ObjectType)
			{
			case GameObjectType.Ability:
				Ability = AbilityDataProvider.GetAbilityPrintingById(cardData.GrpId);
				break;
			case GameObjectType.Card:
				Ability = cardData.Abilities.FirstOrDefault((AbilityPrintingData x) => x.Category == AbilityCategory.Spell);
				break;
			case GameObjectType.Emblem:
			case GameObjectType.Boon:
				Ability = ((cardData.Abilities.Count > 0) ? cardData.Abilities[0] : null);
				break;
			}
		}
		if (!string.IsNullOrEmpty(cardData.SleeveCode))
		{
			CosmeticSleeveId = cardData.SleeveCode;
		}
	}

	public void SetCardDataRaw(ICardDataAdapter cardData)
	{
		_cardData = cardData;
	}

	public void SetAbilityDataFromChild(ICardDataAdapter cardData)
	{
		if (Ability != null || AbilityDataProvider == null)
		{
			return;
		}
		foreach (MtgCardInstance child in cardData.Children)
		{
			if (child.ObjectType == GameObjectType.Ability)
			{
				Ability = AbilityDataProvider.GetAbilityPrintingById(child.GrpId);
				break;
			}
		}
	}

	public void SetCdcViewMetadata(CDCViewMetadata metadata)
	{
		InWrapper = metadata.IsMeta;
		InDuelScene = !metadata.IsMeta;
		IsDimmedCard = metadata.IsDimmed;
		CardIsHovered = metadata.IsMouseOver;
		MouseOverType = (metadata.IsMouseOver ? MouseOverType.MouseOver : MouseOverType.None);
		IsExaminedCard = metadata.IsExaminedCard;
		IsHoverCopy = metadata.IsHoverCopy;
	}

	public void Cleanup()
	{
		RemoveFillerDelegate(DefaultFillBlackboard);
	}
}
