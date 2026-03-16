using System;
using System.Collections.Generic;
using System.Text;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ReplacementEffectController : IReplacementEffectController, IDisposable
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IGameEffectBuilder _gameEffectBuilder;

	private readonly IEntityNameProvider<MtgEntity> _entityNameProvider;

	private readonly HashSet<ReplacementEffectData> _activeReplacementEffects = new HashSet<ReplacementEffectData>();

	private readonly Dictionary<string, DuelScene_CDC> _miniCDCs = new Dictionary<string, DuelScene_CDC>();

	private MtgGameState _prvGameState;

	private MtgGameState _gameState;

	public ReplacementEffectController(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IGameEffectBuilder gameEffectBuilder, IEntityNameProvider<MtgEntity> entityNameProvider)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_gameEffectBuilder = gameEffectBuilder ?? NullGameEffectBuilder.Default;
		_entityNameProvider = entityNameProvider ?? NullEntityNameProvider.Default;
		_gameStateProvider.CurrentGameState.ValueUpdated += UpdateCurrentGameState;
	}

	public bool CanAddReplacementEffect(ReplacementEffectData data, MtgGameState gameState)
	{
		return CanAddReplacementEffect(data, gameState, _cardDatabase?.AbilityDataProvider?.GetAbilityPrintingById(data.AbilityId));
	}

	public bool CanAddReplacementEffect(ReplacementEffectData data, MtgGameState gameState, AbilityPrintingData ability)
	{
		if (_activeReplacementEffects.Contains(data))
		{
			return false;
		}
		if (data.HasInvalidAbilityId())
		{
			return false;
		}
		if (gameState == null)
		{
			return false;
		}
		if (data.AffectedIdIsPlayer(gameState))
		{
			return true;
		}
		if (data.RecipientIsPlayer(gameState))
		{
			return data.AbilityIsNotStaticDamagePrevention(ability);
		}
		if (data.EffectIsGeneralDamageRedirectionToCard(gameState))
		{
			return true;
		}
		if (data.SpawnerType.Count > 0)
		{
			return false;
		}
		if (data.AffectorAndAffectedAreSame(gameState))
		{
			return true;
		}
		if (data.AffectorIsBattlefieldButAffectedDoesntExist(gameState))
		{
			return true;
		}
		return false;
	}

	public void TryAddReplacementEffect(ReplacementEffectData data)
	{
		if (CanAddReplacementEffect(data, _gameState, _cardDatabase?.AbilityDataProvider?.GetAbilityPrintingById(data.AbilityId)))
		{
			AbilityPrintingRecord abilityRecordById = _cardDatabase.AbilityDataProvider.GetAbilityRecordById(data.AbilityId);
			MtgCardInstance cardById = _gameState.GetCardById(data.AffectorId);
			MtgPlayer affectedPlayerForMiniCDC = GetAffectedPlayerForMiniCDC(data, GameStates());
			string key = data.Key;
			if (ShouldCreateMiniCDC(abilityRecordById, cardById, affectedPlayerForMiniCDC) && !_miniCDCs.ContainsKey(key))
			{
				_miniCDCs[key] = _gameEffectBuilder.Create(GameEffectType.ReplacementEffect, data.Key, GenerateMiniCDCModel(data, abilityRecordById, cardById, affectedPlayerForMiniCDC));
			}
			_activeReplacementEffects.Add(data);
		}
	}

	public void UpdateReplacementEffect(ReplacementEffectData data)
	{
		if (_activeReplacementEffects.Contains(data))
		{
			UpdateMiniCDC(data.Key);
			return;
		}
		foreach (ReplacementEffectData activeReplacementEffect in _activeReplacementEffects)
		{
			if (ReplacementEffectData.IgnoreSourcesAndRecipientsEqComparer.Equals(activeReplacementEffect, data))
			{
				UpdateMiniCDC(data.Key);
				break;
			}
		}
	}

	public void RemoveReplacementEffect(ReplacementEffectData data)
	{
		string key = data.Key;
		_activeReplacementEffects.RemoveWhere((ReplacementEffectData x) => x.Key == key);
		if (_miniCDCs.ContainsKey(key))
		{
			_miniCDCs.Remove(key);
			_gameEffectBuilder.Destroy(key);
		}
	}

	public static bool ShouldCreateMiniCDC(AbilityPrintingRecord ability, MtgCardInstance affectorCard, MtgPlayer affectedPlayer)
	{
		if (!ability.Equals(AbilityPrintingRecord.Blank) && ability.Category != AbilityCategory.Static && affectorCard != null)
		{
			return affectedPlayer != null;
		}
		return false;
	}

	private static MtgPlayer GetAffectedPlayerForMiniCDC(ReplacementEffectData re, IEnumerable<MtgGameState> gameStates)
	{
		foreach (MtgGameState gameState in gameStates)
		{
			if (gameState.TryGetEntity(re.AffectedId, out var mtgEntity))
			{
				if (mtgEntity is MtgPlayer result)
				{
					return result;
				}
				if (mtgEntity is MtgCardInstance { Controller: var controller })
				{
					return controller;
				}
			}
		}
		return null;
	}

	private ICardDataAdapter GenerateMiniCDCModel(ReplacementEffectData replacementEffect, AbilityPrintingRecord ability, MtgCardInstance affectorCard, MtgPlayer affectedPlayer)
	{
		MtgCardInstance mtgCardInstance = ((affectorCard.Parent != null) ? affectorCard.Parent : affectorCard);
		CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(GrpIdInternal(mtgCardInstance), mtgCardInstance.SkinCode);
		CardPrintingData cardPrintingData = cardPrintingById.CreateMiniCDCPrintingData(ability.Id);
		MtgCardInstance mtgCardInstance2 = cardPrintingData.CreateInstance(GameObjectType.Ability);
		mtgCardInstance2.Abilities = new List<AbilityPrintingData>(cardPrintingData.Abilities);
		mtgCardInstance2.Zone = new MtgZone
		{
			Type = ZoneType.Command,
			Visibility = Visibility.Public,
			Owner = affectedPlayer
		};
		mtgCardInstance2.Visibility = Visibility.Public;
		mtgCardInstance2.Viewers.Add(GREPlayerNum.LocalPlayer);
		mtgCardInstance2.Viewers.Add(GREPlayerNum.Opponent);
		mtgCardInstance2.ParentId = affectorCard.InstanceId;
		mtgCardInstance2.Parent = affectorCard;
		mtgCardInstance2.GrpId = ability.Id;
		mtgCardInstance2.ObjectSourceGrpId = cardPrintingData.GrpId;
		mtgCardInstance2.TitleId = cardPrintingData.TitleId;
		mtgCardInstance2.Controller = affectedPlayer;
		mtgCardInstance2.LinkedInfoTitleLocIds.UnionWith(affectorCard.LinkedInfoTitleLocIds);
		mtgCardInstance2.ReplacementEffects.Add(replacementEffect);
		mtgCardInstance2.AffectorOfLinkInfos.AddRange(affectorCard.AffectorOfLinkInfos);
		IRulesTextOverride rulesTextOverride = new AbilityTextOverride(_cardDatabase, cardPrintingById.TitleId, removeLoyaltyPrefix: true).AddAbility(ability.Id).AddSource(affectorCard).AddSource(cardPrintingData);
		IRulesTextOverride rulesTextOverride2 = DamagePreventionText(replacementEffect);
		IRulesTextOverride rulesTextOverride3;
		if (rulesTextOverride2 == null)
		{
			rulesTextOverride3 = rulesTextOverride;
		}
		else
		{
			IRulesTextOverride rulesTextOverride4 = new RulesTextOverrideAggregate(rulesTextOverride, rulesTextOverride2);
			rulesTextOverride3 = rulesTextOverride4;
		}
		IRulesTextOverride rulesTextOverride5 = rulesTextOverride3;
		return new CardData(mtgCardInstance2, cardPrintingData)
		{
			RulesTextOverride = rulesTextOverride5
		};
	}

	private IRulesTextOverride DamagePreventionText(ReplacementEffectData data)
	{
		if (data.SpawnerType.Contains(ReplacementEffectSpawnerType.PreventDamage))
		{
			MtgEntity entity = getAffected(data.AffectedId, GameStates());
			StringBuilder stringBuilder = new StringBuilder();
			if (data.SourceIds != null && data.SourceIds.Contains(data.AffectedId))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("<#0000FF>{0}</color>", string.Format(Languages.ActiveLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/Source", ("replacementSource", _entityNameProvider.GetName(entity)))));
			}
			if (data.RecipientIds != null && data.RecipientIds.Contains(data.AffectedId))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("<#0000FF>{0}</color>", string.Format(Languages.ActiveLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/Recipient", ("replacementRecipient", _entityNameProvider.GetName(entity)))));
			}
			if (data.Durability.HasValue)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("<#0000FF>{0}</color>", string.Format(Languages.ActiveLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/ReplacementEffect/AmountValue", ("durability", data.Durability.Value.ToString()))));
			}
			return new RawTextOverride(stringBuilder.ToString());
		}
		return null;
		static MtgEntity getAffected(uint affectedId, IEnumerable<MtgGameState> gameStates)
		{
			foreach (MtgGameState gameState in gameStates)
			{
				if (gameState.TryGetEntity(affectedId, out var mtgEntity))
				{
					return mtgEntity;
				}
			}
			return null;
		}
	}

	private IEnumerable<MtgGameState> GameStates()
	{
		if (_gameState != null)
		{
			yield return _gameState;
		}
		if (_prvGameState != null)
		{
			yield return _prvGameState;
		}
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

	public void UpdateCurrentGameState(MtgGameState gameState)
	{
		_prvGameState = _gameState;
		_gameState = gameState;
	}

	private void UpdateMiniCDC(string key)
	{
		if (_miniCDCs.TryGetValue(key, out var value))
		{
			value.UpdateVisuals();
		}
	}

	public void Dispose()
	{
		_gameStateProvider.CurrentGameState.ValueUpdated -= UpdateCurrentGameState;
		_gameState = null;
		_prvGameState = null;
		_activeReplacementEffects.Clear();
		_miniCDCs.Clear();
	}
}
