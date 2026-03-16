using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Helpers;
using AssetLookupTree.Payloads.MiniCDC;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class QualificationController : IQualificationController, IDisposable
{
	private class ClientOrGreTextOverride : IRulesTextOverride
	{
		private readonly ClientOrGreLocKey _locKey;

		private readonly IClientLocProvider _clientLocProvider;

		private readonly IGreLocProvider _greLocProvider;

		private readonly Dictionary<string, string> _locParams;

		public ClientOrGreTextOverride(ClientOrGreLocKey locKey, IClientLocProvider clientLocProvider, IGreLocProvider greLocProvider, Dictionary<string, string> locParams = null)
		{
			_locKey = locKey;
			_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
			_greLocProvider = greLocProvider ?? NullGreLocManager.Default;
			_locParams = locParams;
		}

		public string GetOverride(CardTextColorSettings textColorSettings)
		{
			return _locKey.GetText(_clientLocProvider, _greLocProvider, _locParams);
		}
	}

	private const string FAKE_CARD_FORMAT = "Qualification #{0}_Affected{1}";

	private const string HACK_LIVING_BREAKTHROUGH_LOC_PARAM = "prohibitedValues";

	public const string HACK_LIVING_BREAKTHROUGH_CONSOLODATION_KEY = "Living Breakthrough";

	public const uint HACK_LIVING_BREAKTHROUGH_ABILITYID = 147831u;

	private const string DETAILS_KEY_DISQUALIFIED_MANAVALS = "disallowed_mana_value";

	public const string DETAILS_KEY_AFFECTED_TYPE = "affectedType";

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IFakeCardViewManager _fakeCardViewManager;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IClientLocProvider _clientLocManager;

	private readonly IGameEffectBuilder _gameEffectBuilder;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly HashSet<QualificationData> _activeQualifications = new HashSet<QualificationData>();

	private readonly Dictionary<(uint qualificationId, uint affectedId), DuelScene_CDC> _qualificationIdToMiniCDC = new Dictionary<(uint, uint), DuelScene_CDC>();

	private MtgGameState _prvGameState;

	private MtgGameState _currentGameState;

	public static readonly HashSet<QualificationType> MINI_CDC_QUAL_TYPES = new HashSet<QualificationType>
	{
		QualificationType.CostToCast,
		QualificationType.CantBeCountered,
		QualificationType.CantBePrevented,
		QualificationType.CantPlay,
		QualificationType.CantCast,
		QualificationType.CantAttack,
		QualificationType.CantBlock,
		QualificationType.CantBeBlocked,
		QualificationType.MustAttack,
		QualificationType.AttackCost,
		QualificationType.CantLoseTheGame,
		QualificationType.MayPlayAtInstantSpeed,
		QualificationType.LethalDeterminedByPower,
		QualificationType.IgnoreBoastActivationLimit,
		QualificationType.CantGainLife,
		QualificationType.MayActivateLoyaltyAbilitiesNtimes,
		QualificationType.MayLookAtFacedownCards,
		QualificationType.NextSpellCastGainsAbility,
		QualificationType.CantLoseMana
	};

	public static readonly HashSet<(QualificationType, QualificationSubtype, uint)> ACCEPTABLE_QUAL_COMBOS = new HashSet<(QualificationType, QualificationSubtype, uint)>
	{
		(QualificationType.MayPlay, QualificationSubtype.MayPlayTopItem, 192985u),
		(QualificationType.MayPlay, QualificationSubtype.MayPlayTopItem, 169710u)
	};

	public static readonly HashSet<QualificationType> SUPERFLUOUS_QUAL_TYPES = new HashSet<QualificationType> { QualificationType.CantLoseMana };

	public QualificationController(ICardDatabaseAdapter cardDatabase, IClientLocProvider clientLocManager, IFakeCardViewManager fakeCardViewManager, IGameEffectBuilder gameEffectBuilder, IGameStateProvider gameStateProvider, AssetLookupSystem assetLookupSystem)
	{
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_fakeCardViewManager = fakeCardViewManager ?? NullFakeCardViewManager.Default;
		_gameEffectBuilder = gameEffectBuilder ?? NullGameEffectBuilder.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_assetLookupSystem = assetLookupSystem;
		_currentGameState = _gameStateProvider.CurrentGameState;
		_gameStateProvider.CurrentGameState.ValueUpdated += UpdateCurrentGameState;
	}

	public void AddQualification(QualificationData qualification)
	{
		_activeQualifications.Add(qualification);
		MtgEntity affectorEntity = GetAffectorEntity(qualification.AffectorId);
		MtgEntity affectedEntity = GetAffectedEntity(qualification);
		AbilityPrintingData abilityPrintingById = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(qualification.AbilityId);
		QualificationTextOverride qualificationOverride = GetQualificationOverride(qualification, abilityPrintingById);
		if (qualification.AbilityId == 147831)
		{
			if (!_fakeCardViewManager.TryGetFakeCard("Living Breakthrough", out var fakeCdc))
			{
				fakeCdc = CreateMiniCDC("Living Breakthrough", GenerateMiniCDCModel((MtgCardInstance)affectorEntity, (MtgPlayer)affectedEntity, abilityPrintingById));
			}
			fakeCdc.Model.RulesTextOverride = new ClientLocTextOverride(_clientLocManager, "DuelScene/RuleText/CantCastWithManaValues", ("prohibitedValues", LocParamsForDetails(QualificationsWithMatchingAbilityId(_activeQualifications, 147831u), "disallowed_mana_value")));
			_qualificationIdToMiniCDC[(qualification.Id, qualification.AffectedId)] = fakeCdc;
		}
		else if (DisplayQualification(qualificationOverride, ShouldCreateMiniCDC(abilityPrintingById, affectorEntity, affectedEntity, qualification, MiniCdcDatas())))
		{
			_qualificationIdToMiniCDC[(qualification.Id, qualification.AffectedId)] = CreateMiniCDC($"Qualification #{qualification.Id}_Affected{qualification.AffectedId}", GenerateMiniCDCModel((MtgCardInstance)affectorEntity, (MtgPlayer)affectedEntity, abilityPrintingById, ToTextOverride(qualificationOverride)));
		}
	}

	private IEnumerable<QualificationData> MiniCdcDatas()
	{
		foreach (KeyValuePair<(uint, uint), DuelScene_CDC> item in _qualificationIdToMiniCDC)
		{
			yield return _activeQualifications.Find(item.Key, (QualificationData data, (uint qualificationId, uint affectedId) key) => data.Id == key.qualificationId && data.AffectedId == key.affectedId);
		}
	}

	private QualificationTextOverride GetQualificationOverride(QualificationData qualification, AbilityPrintingData ability)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.Ability = ability;
		_assetLookupSystem.Blackboard.Qualification = qualification;
		return _assetLookupSystem.TreeLoader.LoadTree<QualificationTextOverride>()?.GetPayload(_assetLookupSystem.Blackboard);
	}

	public static bool DisplayQualification(QualificationTextOverride qualificationOverride, bool shouldCreate)
	{
		if (qualificationOverride == null)
		{
			return shouldCreate;
		}
		if (qualificationOverride.IgnoreQualification)
		{
			return false;
		}
		if (!qualificationOverride.IgnoreQualification)
		{
			return true;
		}
		return shouldCreate;
	}

	private ClientOrGreTextOverride ToTextOverride(QualificationTextOverride qualificationOverride)
	{
		if (qualificationOverride == null)
		{
			return null;
		}
		return new ClientOrGreTextOverride(qualificationOverride.LocKey, _cardDatabase.ClientLocProvider, _cardDatabase.GreLocProvider);
	}

	private MtgEntity GetAffectorEntity(uint affectorId)
	{
		return GetEntity(_currentGameState, _prvGameState, affectorId);
	}

	private MtgEntity GetAffectedEntity(QualificationData qualification)
	{
		return GetAffectedEntity(qualification, _currentGameState, _prvGameState);
	}

	private static MtgEntity GetEntity(MtgGameState currentState, MtgGameState previousState, uint entityId)
	{
		if (currentState != null && currentState.TryGetEntity(entityId, out var mtgEntity))
		{
			return mtgEntity;
		}
		if (previousState != null && previousState.TryGetEntity(entityId, out mtgEntity))
		{
			return mtgEntity;
		}
		return null;
	}

	public static MtgEntity GetAffectedEntity(QualificationData qualification, MtgGameState currentState, MtgGameState previousState)
	{
		if (qualification.Details != null && qualification.Details.TryGetValue("affectedType", out var value) && value == "mana")
		{
			return null;
		}
		return GetEntity(currentState, previousState, qualification.AffectedId);
	}

	public void RemoveQualification(QualificationData qualification)
	{
		string key = qualification.Key;
		_activeQualifications.RemoveWhere((QualificationData x) => x.Key == key);
		if (qualification.AbilityId == 147831)
		{
			_qualificationIdToMiniCDC.Remove((qualification.Id, qualification.AffectedId));
			DuelScene_CDC fakeCdc;
			if (!ContainsQualificationWithMatchingAbilityId(_activeQualifications, 147831u))
			{
				_fakeCardViewManager.DeleteFakeCard("Living Breakthrough");
			}
			else if (_fakeCardViewManager.TryGetFakeCard("Living Breakthrough", out fakeCdc))
			{
				fakeCdc.Model.RulesTextOverride = new ClientLocTextOverride(_clientLocManager, "DuelScene/RuleText/CantCastWithManaValues", ("prohibitedValues", LocParamsForDetails(QualificationsWithMatchingAbilityId(_activeQualifications, 147831u), "disallowed_mana_value")));
			}
		}
		else if (_fakeCardViewManager.DeleteFakeCard($"Qualification #{qualification.Id}_Affected{qualification.AffectedId}"))
		{
			_qualificationIdToMiniCDC.Remove((qualification.Id, qualification.AffectedId));
		}
	}

	public void Dispose()
	{
		_gameStateProvider.CurrentGameState.ValueUpdated -= UpdateCurrentGameState;
		_prvGameState = null;
		_currentGameState = null;
		_activeQualifications.Clear();
	}

	public bool TryGetRelatedMiniCDC(QualificationData qualification, out DuelScene_CDC relatedMiniCDC)
	{
		if (_qualificationIdToMiniCDC.TryGetValue((qualification.Id, qualification.AffectedId), out relatedMiniCDC))
		{
			return relatedMiniCDC != null;
		}
		return false;
	}

	private DuelScene_CDC CreateMiniCDC(string key, ICardDataAdapter model)
	{
		return _gameEffectBuilder.Create(GameEffectType.Qualification, key, model);
	}

	private CardData GenerateMiniCDCModel(MtgCardInstance affectorCard, MtgPlayer affectedPlayer, AbilityPrintingData abilityPrinting, IRulesTextOverride rulesTextOverride = null)
	{
		CardPrintingData cardPrintingData = _cardDatabase.CardDataProvider.GetCardPrintingById(affectorCard.Parent?.GrpId ?? affectorCard.GrpId, (affectorCard.Parent != null) ? affectorCard.Parent.SkinCode : affectorCard.SkinCode).CreateMiniCDCPrintingData(abilityPrinting.Id);
		MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance(GameObjectType.Ability);
		mtgCardInstance.Owner = affectedPlayer;
		mtgCardInstance.Abilities = new List<AbilityPrintingData>(cardPrintingData.Abilities);
		mtgCardInstance.Zone = new MtgZone
		{
			Type = ZoneType.Command,
			Visibility = Visibility.Public,
			Owner = affectedPlayer
		};
		mtgCardInstance.Visibility = Visibility.Public;
		mtgCardInstance.Viewers.Add(GREPlayerNum.LocalPlayer);
		mtgCardInstance.Viewers.Add(GREPlayerNum.Opponent);
		mtgCardInstance.ParentId = affectorCard.InstanceId;
		mtgCardInstance.Parent = affectorCard;
		mtgCardInstance.GrpId = abilityPrinting.Id;
		mtgCardInstance.ObjectSourceGrpId = cardPrintingData.GrpId;
		mtgCardInstance.TitleId = cardPrintingData.TitleId;
		mtgCardInstance.Controller = affectedPlayer;
		mtgCardInstance.LinkedInfoTitleLocIds.UnionWith(affectorCard.LinkedInfoTitleLocIds);
		return new CardData(mtgCardInstance, cardPrintingData)
		{
			RulesTextOverride = rulesTextOverride
		};
	}

	private static IEnumerable<QualificationData> QualificationsWithMatchingAbilityId(IEnumerable<QualificationData> qualifications, uint abilityId)
	{
		if (qualifications == null)
		{
			yield break;
		}
		foreach (QualificationData qualification in qualifications)
		{
			if (qualification.AbilityId == abilityId)
			{
				yield return qualification;
			}
		}
	}

	private static bool ContainsQualificationWithMatchingAbilityId(IEnumerable<QualificationData> qualifications, uint abilityId)
	{
		if (qualifications == null)
		{
			return false;
		}
		foreach (QualificationData qualification in qualifications)
		{
			if (qualification.AbilityId == abilityId)
			{
				return true;
			}
		}
		return false;
	}

	private static string LocParamsForDetails(IEnumerable<QualificationData> qualifications, string detailKey, string separator = ", ")
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (QualificationData qualification in qualifications)
		{
			if (qualification.Details.TryGetValue(detailKey, out var value))
			{
				hashSet.Add(value);
			}
		}
		if (hashSet.Count > 0)
		{
			return string.Join(separator, hashSet);
		}
		return string.Empty;
	}

	private void UpdateCurrentGameState(MtgGameState gameState)
	{
		_prvGameState = _currentGameState;
		_currentGameState = gameState;
	}

	public static bool ShouldCreateMiniCDC(AbilityPrintingData abilityPrinting, MtgEntity affector, MtgEntity affected, QualificationData qualificationData, IEnumerable<QualificationData> existingMiniCdcs)
	{
		if (abilityPrinting == null)
		{
			return false;
		}
		if (abilityPrinting.Category == AbilityCategory.Static)
		{
			return false;
		}
		if (!(affector is MtgCardInstance))
		{
			return false;
		}
		if (!(affected is MtgPlayer))
		{
			return false;
		}
		if (ContainsSuperflousMiniCdc(qualificationData, existingMiniCdcs))
		{
			return false;
		}
		if (!MINI_CDC_QUAL_TYPES.Contains(qualificationData.Type))
		{
			return ACCEPTABLE_QUAL_COMBOS.Contains((qualificationData.Type, qualificationData.SubType, abilityPrinting.Id));
		}
		return true;
	}

	private static bool ContainsSuperflousMiniCdc(QualificationData qualification, IEnumerable<QualificationData> miniCdcDatas)
	{
		if (!SUPERFLUOUS_QUAL_TYPES.Contains(qualification.Type))
		{
			return false;
		}
		foreach (QualificationData miniCdcData in miniCdcDatas)
		{
			if (miniCdcData.IsSuperfluous(qualification))
			{
				return true;
			}
		}
		return false;
	}
}
