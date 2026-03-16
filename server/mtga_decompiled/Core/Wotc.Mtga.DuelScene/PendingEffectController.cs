using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.MiniCDC;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class PendingEffectController : IPendingEffectController, IDisposable
{
	private const string DETAIL_MAXHANDSIZE = "MaxHandSize";

	private const string DETAIL_GRPID = "grpid";

	private const string DETAIL_XVAL = "qualification_value_of_x";

	private const string LOC_PARAM_COUNT = "count";

	private readonly Dictionary<uint, DuelScene_CDC> _activePendingEffectCards = new Dictionary<uint, DuelScene_CDC>();

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IClientLocProvider _clientLocManager;

	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	private static HashSet<AbilityCategory> _suppressedCategories = new HashSet<AbilityCategory>
	{
		AbilityCategory.None,
		AbilityCategory.Static
	};

	private static HashSet<ZoneType> _suppressedZoneTypes = new HashSet<ZoneType>
	{
		ZoneType.Battlefield,
		ZoneType.Command
	};

	public PendingEffectController(ICardDatabaseAdapter cardDatabase, IClientLocProvider clientLocManager, ICardBuilder<DuelScene_CDC> cardBuilder, ICardHolderProvider cardHolderProvider, AssetLookupSystem assetLookupSystem)
	{
		_cardDatabase = cardDatabase;
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_cardBuilder = cardBuilder ?? NullCardBuilder<DuelScene_CDC>.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public void AddPendingEffect(PendingEffectData pendingEffect, MtgCardInstance Affector, MtgPlayer player)
	{
		if (player != null && Affector != null && !SupressMiniCDC(pendingEffect, Affector) && _cardHolderProvider.TryGetCardHolder(player.ClientPlayerEnum, CardHolderType.Command, out var cardHolder))
		{
			DuelScene_CDC duelScene_CDC = _cardBuilder.CreateCDC(MiniCDCData(pendingEffect, Affector));
			_activePendingEffectCards.Add(pendingEffect.AnnotationId, duelScene_CDC);
			cardHolder.AddCard(duelScene_CDC);
		}
	}

	private bool SupressMiniCDC(PendingEffectData pendingEffect, MtgCardInstance affector)
	{
		if (pendingEffect.Details.TryGetValue<int>("grpid", out var value))
		{
			AbilityPrintingData abilityPrintingById = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById((uint)value);
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.Ability = abilityPrintingById;
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PendingEffectTextOverride> loadedTree))
			{
				PendingEffectTextOverride payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					return payload.IgnorePendingEffect;
				}
			}
		}
		AbilityCategory abilityCategory = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(pendingEffect.AbilityId)?.Category ?? AbilityCategory.None;
		return SupressMiniCDC(cardZoneType(affector), abilityCategory);
		static ZoneType cardZoneType(MtgCardInstance card)
		{
			if (card == null)
			{
				return ZoneType.None;
			}
			if (card.Zone == null)
			{
				return ZoneType.None;
			}
			return card.Zone.Type;
		}
	}

	public static bool SupressMiniCDC(ZoneType affectorZoneType, AbilityCategory abilityCategory)
	{
		if (_suppressedZoneTypes.Contains(affectorZoneType))
		{
			return _suppressedCategories.Contains(abilityCategory);
		}
		return false;
	}

	private ICardDataAdapter MiniCDCData(PendingEffectData effectData, MtgCardInstance Affector)
	{
		CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(GrpId(Affector));
		CardPrintingRecord record = cardPrintingById.Record;
		string empty = string.Empty;
		IReadOnlyList<(uint, uint)> abilityIds = Array.Empty<(uint, uint)>();
		IReadOnlyList<SuperType> supertypes = Array.Empty<SuperType>();
		IReadOnlyList<CardType> types = Array.Empty<CardType>();
		IReadOnlyList<SubType> subtypes = Array.Empty<SubType>();
		CardPrintingData cardPrintingData = new CardPrintingData(cardPrintingById, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, empty, null, null, null, null, null, null, null, null, null, null, types, subtypes, supertypes, abilityIds));
		MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance(GameObjectType.Ability);
		mtgCardInstance.Zone = new MtgZone
		{
			Type = ZoneType.Command,
			Visibility = Visibility.Public,
			Owner = mtgCardInstance.Owner
		};
		mtgCardInstance.Abilities.Clear();
		mtgCardInstance.ParentId = effectData.AffectorId;
		mtgCardInstance.SkinCode = Affector.SkinCode;
		if (effectData.Details.TryGetValue<int>("qualification_value_of_x", out var value))
		{
			mtgCardInstance.ChooseXResult = (uint)value;
		}
		CardData cardData = new CardData(mtgCardInstance, cardPrintingData);
		cardData.RulesTextOverride = EffectText(effectData, Affector, cardPrintingData, cardData);
		return cardData;
	}

	public static uint GrpId(MtgCardInstance affector)
	{
		if (affector == null)
		{
			return 0u;
		}
		return GrpIdInternal((affector.Parent != null) ? affector.Parent : affector);
	}

	private static uint GrpIdInternal(MtgCardInstance card)
	{
		if (card == null)
		{
			return 0u;
		}
		if (card.ObjectType == GameObjectType.Emblem || card.ObjectType == GameObjectType.Boon)
		{
			return card.ObjectSourceGrpId;
		}
		return card.GrpId;
	}

	private IRulesTextOverride EffectText(PendingEffectData effectData, MtgCardInstance Affector, CardPrintingData printingData, CardData miniCdcData)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(miniCdcData);
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PendingEffectTextOverride> loadedTree))
		{
			PendingEffectTextOverride payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				return new ClientLocTextOverride(_clientLocManager, payload.LocKey);
			}
		}
		if (effectData.Details.TryGetValue<int>("MaxHandSize", out var value))
		{
			if (value == int.MaxValue)
			{
				return new ClientLocTextOverride(_clientLocManager, "DuelScene/RuleText/NoMaximumHandSize");
			}
			return new ClientLocTextOverride(_clientLocManager, "DuelScene/RuleText/MaximumHandSizeIs", ("count", value.ToString()));
		}
		if (effectData.Details.TryGetValue<int>("grpid", out var value2))
		{
			return new AbilityTextOverride(_cardDatabase, printingData.TitleId).AddAbility((uint)value2).AddSource(Affector).AddSource(printingData);
		}
		return new AbilityTextOverride(_cardDatabase, printingData.TitleId).AddAbility(printingData.GrpId).AddSource(Affector).AddSource(printingData);
	}

	public void RemovePendingEffect(PendingEffectData pendingEffect)
	{
		uint annotationId = pendingEffect.AnnotationId;
		if (_activePendingEffectCards.TryGetValue(annotationId, out var value))
		{
			value.CurrentCardHolder.RemoveCard(value);
			_cardBuilder.DestroyCDC(value);
			_activePendingEffectCards.Remove(annotationId);
		}
	}

	public void Dispose()
	{
		if (_cardBuilder != null)
		{
			foreach (KeyValuePair<uint, DuelScene_CDC> activePendingEffectCard in _activePendingEffectCards)
			{
				if (activePendingEffectCard.Value != null)
				{
					_cardBuilder.DestroyCDC(activePendingEffectCard.Value);
				}
			}
		}
		_activePendingEffectCards.Clear();
	}
}
