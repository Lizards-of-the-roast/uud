using System;
using System.Collections.Generic;
using AssetLookupTree.Extractors.UI;
using AssetLookupTree.Payloads.Ability.Metadata;
using Core.Meta.MainNavigation.SocialV2;
using GreClient.CardData;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;
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
using Wotc.Mtga.Wrapper;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Blackboard;

public interface IBlackboard
{
	Dictionary<SupplementalKey, ICardDataAdapter> SupplementalCardData { get; }

	ulong ContentVersion { get; }

	ICardDatabaseAdapter CardDatabase { get; }

	ICardDataProvider CardDataProvider { get; }

	IAbilityDataProvider AbilityDataProvider { get; }

	IEntityNameProvider<uint> IdNameProvider { get; set; }

	bool InWrapper { get; set; }

	bool InDuelScene { get; set; }

	Material Material { get; set; }

	string MaterialName { get; set; }

	Texture2D Texture { get; set; }

	string TextureName { get; set; }

	TMP_FontAsset Font { get; set; }

	string FontName { get; set; }

	string CarouselName { get; set; }

	string CampaignGraphNodeName { get; set; }

	MouseOverType MouseOverType { get; set; }

	bool CardIsHovered { get; set; }

	bool IsHoverCopy { get; set; }

	bool IsExaminedCard { get; set; }

	bool IsDimmedCard { get; set; }

	ICardDataAdapter CardData { get; }

	CardHolderType CardHolderType { get; set; }

	CounterType CounterType { get; set; }

	CDCFieldFillerFieldType FieldFillerType { get; set; }

	CDCSpriteFiller.FieldType SpriteFillerType { get; set; }

	CDCSpriteFillerSDF.FieldType SpriteSdfFillerType { get; set; }

	CDCManaCostFiller.FieldType ManaFillerType { get; set; }

	ICardHolder CardHolder { get; set; }

	AbilityPrintingData Ability { get; set; }

	AbilityType AbilityType { get; set; }

	ReplacementEffectData ReplacementEffectData { get; set; }

	IBadgeEntryData BadgeData { get; set; }

	MatchManager.PlayerInfo PlayerInfoMatch { get; set; }

	PlayerInfo PlayerInfoGame { get; set; }

	MtgPlayer Player { get; set; }

	MtgGameState GameState { get; set; }

	BaseUserRequest Request { get; set; }

	WorkflowBase Interaction { get; set; }

	SelectionParams SelectionParams { get; set; }

	TargetSelectionParams TargetSelectionParams { get; set; }

	ResolutionEffectModel ActiveResolution { get; set; }

	string Language { get; set; }

	DateTime DateTimeUtc { get; set; }

	HighlightType HighlightType { get; set; }

	NavContentType NavContentType { get; set; }

	DeviceType DeviceType { get; set; }

	float AspectRatio { get; set; }

	string BattlefieldId { get; set; }

	string PetId { get; set; }

	string PetVariantId { get; set; }

	int PetLevel { get; set; }

	RewardType RewardType { get; set; }

	int? DamageAmount { get; set; }

	DamageType DamageType { get; set; }

	CardReactionEnum CardReactionType { get; set; }

	HashSet<DecoratorType> DecoratorTypes { get; set; }

	DuelSceneBrowserType CardBrowserType { get; set; }

	string CardBrowserElementID { get; set; }

	string CardBrowserLayoutID { get; set; }

	uint? CardBrowserCardCount { get; set; }

	(int min, uint max)? SelectCardBrowserMinMax { get; set; }

	int SelectCardBrowserCurrentSelectionCount { get; set; }

	ActionType GreActionType { get; set; }

	Wotc.Mtgo.Gre.External.Messaging.Action GreAction { get; set; }

	ButtonStyle.StyleType ButtonStyle { get; set; }

	MtgZone ZoneSelection { get; set; }

	MtgZone FromZone { get; set; }

	MtgZone ToZone { get; set; }

	ZoneTransferReason ZoneTransferReason { get; set; }

	LoyaltyValence LoyaltyValence { get; set; }

	EmotePrefabData EmotePrefabData { get; set; }

	GREPlayerNum GREPlayerNum { get; set; }

	TooltipContext TooltipContext { get; set; }

	QualityMode_CardVFX CardVfxQuality { get; set; }

	QualityMode_Pet PetQuality { get; set; }

	int LifeChange { get; set; }

	string LocalNotificationUID { get; set; }

	ManaColor ManaColor { get; set; }

	int ManaSelectionCount { get; set; }

	string CosmeticAvatarId { get; set; }

	string CosmeticStoreSKU { get; set; }

	string CosmeticSleeveId { get; set; }

	string CosmeticAccessoryId { get; set; }

	string CosmeticAccessoryMod { get; set; }

	string EmoteId { get; set; }

	string IncomingEmoteId { get; set; }

	ZonePair ZonePair { get; set; }

	RegionPair RegionPair { get; set; }

	CollationMapping BoosterCollationMapping { get; set; }

	EventContext Event { get; set; }

	(RankingClassType rank, int tier) ConstructedRank { get; set; }

	(RankingClassType rank, int tier) LimitedRank { get; set; }

	HashSet<PropertyType> UpdatedProperties { get; set; }

	ConditionType Condition { get; set; }

	QualificationData? Qualification { get; set; }

	string PrefabName { get; set; }

	string SetCode { get; set; }

	int? TargetIndex { get; set; }

	string LookupString { get; set; }

	ManaMovementData ManaMovement { get; set; }

	Prompt Prompt { get; set; }

	uint PromptParameterId { get; set; }

	string RewardTrack { get; set; }

	CardHolderBase.CardPosition CardInsertionPosition { get; set; }

	SyntheticEventType SyntheticEvent { get; set; }

	string LayeredEffectType { get; set; }

	IEnumerable<LayeredEffectData> LayeredEffects { get; set; }

	Designation? Designation { get; set; }

	LinkInfoData LinkInfo { get; set; }

	LinkedInfoText LinkedInfoText { get; set; }

	uint? DieRollNaturalResult { get; set; }

	uint HoverFaceHangerCount { get; set; }

	uint ExamineFaceHangerCount { get; set; }

	bool InHorizontalDeckBuilder { get; set; }

	bool CanCraft { get; set; }

	bool IsHangerText { get; set; }

	int UnitCount { get; set; }

	string Flavor { get; set; }

	ICardTextEntry CardTextEntry { get; set; }

	MtgEntity DamageRecipientEntity { get; set; }

	string LetterBannerArtId { get; set; }

	string SceneToLoad { get; set; }

	string LearnMoreSectionContentName { get; set; }

	AvatarFramePartType AvatarFramePart { get; set; }

	PhaseIconType PhaseIconType { get; set; }

	ManaPaymentCondition ManaPaymentCondition { get; set; }

	bool FixedRulesTextSize { get; set; }

	LUTVFXType LutVfxType { get; set; }

	bool IsNPEComplete { get; set; }

	bool IsExpanded { get; set; }

	Vector3 IdealWorldPosition { get; set; }

	uint DieFaces { get; set; }

	PurchaseFlow PurchaseFlow { get; set; }

	GatheringPrivilegeLevel GatheringUserPrivilege { get; set; }

	void Clear();

	void AddFillerDelegate(Action<IBlackboard> bbFiller);

	void RemoveFillerDelegate(Action<IBlackboard> bbFiller);

	void SetCardDataExtensive(MtgCardInstance cardInstance);

	void SetCardDataExtensive(ICardDataAdapter cardData);

	void SetCardDataRaw(ICardDataAdapter cardData);

	void SetAbilityDataFromChild(ICardDataAdapter cardData);

	void SetCdcViewMetadata(CDCViewMetadata metadata);

	void Cleanup();
}
