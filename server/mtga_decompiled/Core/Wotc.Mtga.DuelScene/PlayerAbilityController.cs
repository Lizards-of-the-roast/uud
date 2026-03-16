using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card.RulesText;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class PlayerAbilityController : IPlayerAbilityController, IDisposable
{
	private const string MINI_CDC_KEY_FORMAT = "MiniCDC: Player {0} | Ability {1} | SourceAbility {2} | SourceGrpId {3}";

	private readonly IGameEffectBuilder _gameEffectBuilder;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly Dictionary<uint, List<AddedAbilityData>> _addedAbilityMap = new Dictionary<uint, List<AddedAbilityData>>();

	private MtgGameState GameState => _gameStateProvider.CurrentGameState;

	public PlayerAbilityController(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IGameEffectBuilder gameEffectBuilder, AssetLookupSystem assetLookupSystem)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_gameEffectBuilder = gameEffectBuilder ?? NullGameEffectBuilder.Default;
		_assetLookupSystem = assetLookupSystem;
		_gameStateProvider.CurrentGameState.ValueUpdated += UpdateCurrentGameState;
	}

	public void AddAbility(uint playerId, AddedAbilityData addedAbilityData)
	{
		if (_addedAbilityMap.TryGetValue(playerId, out var value))
		{
			bool num = !ContainsDuplicateAddedAbility(value, addedAbilityData);
			value.Add(addedAbilityData);
			if (num && ShouldCreateMiniCDC(addedAbilityData, out var result))
			{
				_gameEffectBuilder.Create(GameEffectType.PlayerAbility, MiniCDCKey(playerId, addedAbilityData), result);
			}
		}
	}

	public void RemoveAbility(uint playerId, AddedAbilityData addedAbilityData)
	{
		if (_addedAbilityMap.TryGetValue(playerId, out var value) && value.Remove(addedAbilityData) && !ContainsDuplicateAddedAbility(value, addedAbilityData))
		{
			_gameEffectBuilder.Destroy(MiniCDCKey(playerId, addedAbilityData));
		}
	}

	private static bool ContainsDuplicateAddedAbility(IEnumerable<AddedAbilityData> addedAbilities, AddedAbilityData other)
	{
		foreach (AddedAbilityData addedAbility in addedAbilities)
		{
			if (addedAbility.AbilityId == other.AbilityId && addedAbility.SourceAbilityId == other.SourceAbilityId && addedAbility.SourceGrpId == other.SourceGrpId)
			{
				return true;
			}
		}
		return false;
	}

	private static string MiniCDCKey(uint playerId, AddedAbilityData addedAbility)
	{
		return $"MiniCDC: Player {playerId} | Ability {addedAbility.AbilityId} | SourceAbility {addedAbility.SourceAbilityId} | SourceGrpId {addedAbility.SourceGrpId}";
	}

	private void UpdateCurrentGameState(MtgGameState gameState)
	{
		if (gameState == null)
		{
			return;
		}
		foreach (MtgPlayer player in gameState.Players)
		{
			uint instanceId = player.InstanceId;
			if (!_addedAbilityMap.ContainsKey(instanceId))
			{
				_addedAbilityMap[instanceId] = new List<AddedAbilityData>();
			}
		}
	}

	private bool ShouldCreateMiniCDC(AddedAbilityData addedAbility, out ICardDataAdapter result)
	{
		if (ShouldCreateMiniCDC(_cardDatabase.AbilityDataProvider.GetAbilityPrintingById(AddedAbilityId(addedAbility)), GameState.GetCardById(addedAbility.AddedById), _cardDatabase.CardDataProvider, out result))
		{
			IRulesTextOverride textOverride = GetTextOverride(result);
			if (textOverride != null)
			{
				result.RulesTextOverride = textOverride;
			}
			return true;
		}
		result = null;
		return false;
	}

	public static bool ShouldCreateMiniCDC(AbilityPrintingData ability, MtgCardInstance affector, ICardDataProvider cdb, out ICardDataAdapter result)
	{
		result = null;
		if (ability == null || affector == null)
		{
			return false;
		}
		if (ability.Category == AbilityCategory.Static && affector.Zone.Type == ZoneType.Battlefield)
		{
			return false;
		}
		CardPrintingData cardPrintingById = cdb.GetCardPrintingById(affector.Parent?.GrpId ?? affector.GrpId, (affector.Parent != null) ? affector.Parent.SkinCode : affector.SkinCode);
		CardPrintingRecord record = cardPrintingById.Record;
		string empty = string.Empty;
		IReadOnlyList<(uint, uint)> abilityIds = new(uint, uint)[1] { (ability.Id, 0u) };
		IReadOnlyList<SuperType> supertypes = Array.Empty<SuperType>();
		IReadOnlyList<CardType> types = Array.Empty<CardType>();
		IReadOnlyList<SubType> subtypes = Array.Empty<SubType>();
		CardPrintingData cardPrintingData = new CardPrintingData(cardPrintingById, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, empty, null, null, null, null, null, null, null, null, null, null, types, subtypes, supertypes, abilityIds));
		MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance(GameObjectType.Ability);
		mtgCardInstance.Owner = affector.Owner;
		mtgCardInstance.Abilities = new List<AbilityPrintingData>(cardPrintingData.Abilities);
		mtgCardInstance.Zone = new MtgZone
		{
			Type = ZoneType.Command,
			Visibility = Visibility.Public,
			Owner = affector.Owner
		};
		mtgCardInstance.Visibility = Visibility.Public;
		mtgCardInstance.Viewers.Add(GREPlayerNum.LocalPlayer);
		mtgCardInstance.Viewers.Add(GREPlayerNum.Opponent);
		mtgCardInstance.ParentId = affector.InstanceId;
		mtgCardInstance.Parent = affector;
		mtgCardInstance.GrpId = ability.Id;
		mtgCardInstance.ObjectSourceGrpId = cardPrintingData.GrpId;
		mtgCardInstance.TitleId = cardPrintingData.TitleId;
		mtgCardInstance.Controller = affector.Controller;
		mtgCardInstance.LinkedInfoTitleLocIds.UnionWith(affector.LinkedInfoTitleLocIds);
		mtgCardInstance.SkinCode = affector.SkinCode;
		result = new CardData(mtgCardInstance, cardPrintingData);
		return true;
	}

	private IRulesTextOverride GetTextOverride(ICardDataAdapter cardData)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		AvatarMiniCdcOverride avatarMiniCdcOverride = _assetLookupSystem.TreeLoader.LoadTree<AvatarMiniCdcOverride>(returnNewTree: false)?.GetPayload(_assetLookupSystem.Blackboard);
		_assetLookupSystem.Blackboard.Clear();
		if (avatarMiniCdcOverride != null)
		{
			AbilityTextOverride abilityTextOverride = new AbilityTextOverride(_cardDatabase, cardData.TitleId);
			{
				foreach (uint abilityId in avatarMiniCdcOverride.AbilityIds)
				{
					abilityTextOverride.AddAbility(abilityId);
				}
				return abilityTextOverride;
			}
		}
		return null;
	}

	private static uint AddedAbilityId(AddedAbilityData abilityData)
	{
		if (abilityData.SourceAbilityId == 0)
		{
			return abilityData.AbilityId;
		}
		return abilityData.SourceAbilityId;
	}

	public void Dispose()
	{
		_gameStateProvider.CurrentGameState.ValueUpdated -= UpdateCurrentGameState;
		foreach (KeyValuePair<uint, List<AddedAbilityData>> item in _addedAbilityMap)
		{
			uint key = item.Key;
			List<AddedAbilityData> value = item.Value;
			while (value.Count > 0)
			{
				_gameEffectBuilder.Destroy(MiniCDCKey(key, value[0]));
				value.RemoveAt(0);
			}
		}
		_addedAbilityMap.Clear();
	}
}
