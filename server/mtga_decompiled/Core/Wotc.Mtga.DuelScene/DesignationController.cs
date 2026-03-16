using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card.Designation;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class DesignationController : IDesignationController, IDisposable
{
	private const string CITYS_BLESSING_FORMAT = "CITYS_BLESSING_FOR_PLAYER_{0}";

	private const string DAYBOUND_NIGHTBOUND = "DAYBOUND_NIGHTBOUND";

	private const uint DAY_TOKEN_GRPID = 79412u;

	private const uint NIGHT_TOKEN_GRPID = 79413u;

	private readonly ICardDataProvider _cardDatabase;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IGameEffectBuilder _gameEffectBuilder;

	private readonly ICardViewManager _cardViewManager;

	private readonly IPromptEngine _promptEngine;

	private readonly PlayerNames _playerNames;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IVfxProvider _vfxProvider;

	private readonly Dictionary<uint, DuelScene_CDC> _miniCdcs = new Dictionary<uint, DuelScene_CDC>();

	private MtgGameState GameState => _gameStateProvider.CurrentGameState;

	public DesignationController(ICardDataProvider cardDataProvider, IGameStateProvider gameStateProvider, IGameEffectBuilder gameEffectBuilder, ICardViewManager cardViewManager, IPromptEngine promptEngine, PlayerNames playerNames, AssetLookupSystem assetLookupSystem, IVfxProvider vfxProvider)
	{
		_cardDatabase = cardDataProvider ?? NullCardDataProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_gameEffectBuilder = gameEffectBuilder ?? NullGameEffectBuilder.Default;
		_cardViewManager = cardViewManager ?? NullCardViewManager.Default;
		_promptEngine = promptEngine ?? NullPromptEngine.Default;
		_playerNames = playerNames;
		_assetLookupSystem = assetLookupSystem;
		_vfxProvider = vfxProvider;
	}

	public void AddDesignation(DesignationData designation)
	{
		switch (designation.Type)
		{
		case Designation.CitysBlessing:
			CitysBlessingAdded(designation);
			break;
		case Designation.Commander:
			CommanderAdded(designation);
			break;
		case Designation.Companion:
			CompanionAdded(designation);
			break;
		case Designation.Day:
		case Designation.Night:
			DayboundNightboundAdded(designation);
			break;
		}
		PlayCDCVFX<AddedVfx>(designation);
	}

	private void PlayCDCVFX<T>(DesignationData designation) where T : DesignationVfx
	{
		_assetLookupSystem.Blackboard.Clear();
		foreach (DuelScene_CDC item in GetAssociatedCards(designation))
		{
			_assetLookupSystem.Blackboard.Clear();
			if (item != null && item.Model != null)
			{
				_assetLookupSystem.Blackboard.SetCardDataExtensive(item.Model);
			}
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<T> loadedTree))
			{
				T payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					foreach (VfxData vfxData in payload.VfxDatas)
					{
						_vfxProvider.PlayVFX(vfxData, (item != null) ? item.Model : null, item?.Model?.Instance, item?.EffectsRoot);
					}
				}
			}
			_assetLookupSystem.Blackboard.Clear();
		}
		IEnumerable<DuelScene_CDC> GetAssociatedCards(DesignationData designationData)
		{
			if (_miniCdcs.TryGetValue(designationData.Id, out var value))
			{
				yield return value;
			}
			if (_cardViewManager.TryGetCardView(designationData.AffectedId, out var cardView))
			{
				yield return cardView;
			}
		}
	}

	private void CitysBlessingAdded(DesignationData designation)
	{
		if (GameState.TryGetPlayer(designation.AffectedId, out var player))
		{
			CardPrintingData cardPrintingData = _cardDatabase.GetCardPrintingById(67082u) ?? CardPrintingData.Blank;
			MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance(GameObjectType.Emblem);
			mtgCardInstance.GrpId = cardPrintingData.GrpId;
			mtgCardInstance.Zone = new MtgZone
			{
				Type = ZoneType.Command,
				Visibility = Visibility.Public,
				Owner = player
			};
			mtgCardInstance.Owner = player;
			mtgCardInstance.Controller = player;
			mtgCardInstance.Visibility = Visibility.Public;
			mtgCardInstance.Viewers.Add(GREPlayerNum.LocalPlayer);
			mtgCardInstance.Viewers.Add(GREPlayerNum.Opponent);
			mtgCardInstance.ParentId = designation.AffectorId;
			mtgCardInstance.Designations.Add(designation);
			CardData cardData = new CardData(mtgCardInstance, cardPrintingData);
			if (designation.DisplayText != PromptMessage.None)
			{
				cardData.RulesTextOverride = new GrePromptTextOverride(_promptEngine, (int)designation.DisplayText);
			}
			_miniCdcs[designation.Id] = _gameEffectBuilder.Create(GameEffectType.Designation, GetMiniCDCKeyForDesignation(designation), cardData);
		}
	}

	private void CommanderAdded(DesignationData designation)
	{
		if (GameState.TryGetPlayer(designation.AffectedId, out var player))
		{
			CardPrintingData cardPrintingById = _cardDatabase.GetCardPrintingById(designation.GrpId);
			CardColorFlags colorIdentityFlags = cardPrintingById.ColorIdentityFlags;
			uint titleId = ((cardPrintingById.AltTitleId == 0) ? cardPrintingById.TitleId : cardPrintingById.AltTitleId);
			_playerNames.SetPlayerCommanderInfo(player.InstanceId, titleId, colorIdentityFlags);
		}
	}

	private void CompanionAdded(DesignationData designation)
	{
		if (GameState.TryGetPlayer(designation.AffectedId, out var player))
		{
			CardPrintingData cardPrintingById = _cardDatabase.GetCardPrintingById(designation.GrpId);
			_playerNames.SetPlayerCompanionTitleId(player.InstanceId, cardPrintingById.TitleId);
		}
	}

	private void DayboundNightboundAdded(DesignationData designation)
	{
		_miniCdcs[designation.Id] = _gameEffectBuilder.Create(GameEffectType.Designation, GetMiniCDCKeyForDesignation(designation), DayboundNightboundCardData(designation));
	}

	public void RemoveDesignation(DesignationData designation)
	{
		if (_miniCdcs.TryGetValue(designation.Id, out var _))
		{
			_gameEffectBuilder.Destroy(GetMiniCDCKeyForDesignation(designation));
		}
	}

	public void UpdateDesignation(DesignationData designation)
	{
		Designation type = designation.Type;
		if ((uint)(type - 10) <= 1u)
		{
			UpdateDayboundNightbound(designation);
		}
		PlayCDCVFX<UpdatedVFX>(designation);
	}

	private void UpdateDayboundNightbound(DesignationData designationData)
	{
		if (_miniCdcs.TryGetValue(designationData.Id, out var value))
		{
			value.SetModel(DayboundNightboundCardData(designationData));
		}
	}

	private ICardDataAdapter DayboundNightboundCardData(DesignationData designation)
	{
		uint id = ((designation.Type == Designation.Day) ? 79412u : 79413u);
		CardPrintingData cardPrintingData = _cardDatabase.GetCardPrintingById(id) ?? CardPrintingData.Blank;
		MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance(GameObjectType.Token);
		mtgCardInstance.Zone = new MtgZone
		{
			Type = ZoneType.Command,
			Visibility = Visibility.Public,
			Owner = new MtgPlayer()
		};
		mtgCardInstance.Controller = new MtgPlayer(GREPlayerNum.LocalPlayer);
		mtgCardInstance.Viewers.Add(GREPlayerNum.LocalPlayer);
		mtgCardInstance.Viewers.Add(GREPlayerNum.Opponent);
		mtgCardInstance.Visibility = Visibility.Public;
		mtgCardInstance.Designations.Add(designation);
		return new CardData(mtgCardInstance, cardPrintingData);
	}

	private static string GetMiniCDCKeyForDesignation(DesignationData designation)
	{
		return designation.Type switch
		{
			Designation.Day => "DAYBOUND_NIGHTBOUND", 
			Designation.Night => "DAYBOUND_NIGHTBOUND", 
			Designation.CitysBlessing => $"CITYS_BLESSING_FOR_PLAYER_{designation.AffectedId}", 
			_ => string.Empty, 
		};
	}

	public void Dispose()
	{
		_miniCdcs.Clear();
	}
}
