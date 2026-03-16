using System;
using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.DuelScene.UI;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.PlayerNameViews;

public class PlayerNameViewManager : IPlayerNameViewManager, IPlayerNameViewProvider, IPlayerNameViewController, IDisposable
{
	public readonly Dictionary<uint, PlayerName> PlayerIdToPlayerName = new Dictionary<uint, PlayerName>();

	private List<PlayerNameViewData> AllPlayerNameViewData = new List<PlayerNameViewData>();

	private AssetLookupSystem _assetLookupSystem;

	private CosmeticsProvider _cosmeticProvider;

	private IClientLocProvider _clientLocProvider;

	private IPlayerInfoProvider _playerInfoProvider;

	private MatchManager _matchManager;

	private IPlayerNameViewBuilder _playerNameViewBuilder;

	private UnityEngine.Color _mythicOrangeColor = new UnityEngine.Color(255f, 146f, 47f);

	public PlayerNameViewManager(CosmeticsProvider cosmeticProvider, AssetLookupSystem assetLookupSystem, IClientLocProvider clientLocProvider, IPlayerInfoProvider playerInfoProvider, MatchManager matchManager, IPlayerNameViewBuilder playerNameBuilder)
	{
		_assetLookupSystem = assetLookupSystem;
		_cosmeticProvider = cosmeticProvider;
		_clientLocProvider = clientLocProvider;
		_playerInfoProvider = playerInfoProvider;
		_matchManager = matchManager;
		_playerNameViewBuilder = playerNameBuilder;
	}

	public PlayerName CreatePlayerNameNPE(uint id, Transform localRoot, Transform oppRoot)
	{
		if (PlayerIdToPlayerName.TryGetValue(id, out var value))
		{
			return value;
		}
		Transform root = ((_matchManager.LocalPlayerSeatId == id) ? localRoot : oppRoot);
		GREPlayerNum playerNum = ((_matchManager.LocalPlayerSeatId == id) ? GREPlayerNum.LocalPlayer : GREPlayerNum.Opponent);
		PlayerName playerName = _playerNameViewBuilder.Create(playerNum, root);
		if (_playerInfoProvider.TryGetPlayerInfo(id, out var result))
		{
			playerName.SetName(result.ScreenName);
			playerName.SetRank(null, null, _assetLookupSystem);
			PlayerIdToPlayerName.Add(id, playerName);
			AllPlayerNameViewData.Add(new PlayerNameViewData
			{
				PlayerId = id,
				PlayerNameView = playerName,
				PlayerNum = playerNum
			});
			return playerName;
		}
		return null;
	}

	public PlayerName CreatePlayerName(uint id, Transform localRoot, Transform oppRoot)
	{
		if (PlayerIdToPlayerName.TryGetValue(id, out var value))
		{
			return value;
		}
		Transform root = ((_matchManager.LocalPlayerSeatId == id) ? localRoot : oppRoot);
		GREPlayerNum playerNum = ((_matchManager.LocalPlayerSeatId == id) ? GREPlayerNum.LocalPlayer : GREPlayerNum.Opponent);
		PlayerName playerName = _playerNameViewBuilder.Create(playerNum, root);
		EventContext eventContext = _matchManager.Event;
		if (eventContext != null)
		{
			MDNEFormatType? mDNEFormatType = eventContext.PlayerEvent?.EventInfo?.FormatType;
			MDNEFormatType mDNEFormatType2 = MDNEFormatType.Constructed;
			mDNEFormatType.GetValueOrDefault();
			_ = mDNEFormatType.HasValue;
		}
		if (_playerInfoProvider.TryGetPlayerInfo(id, out var result))
		{
			playerName.SetName(result.ScreenName);
			playerName.SetRank(new RankInfo
			{
				rankClass = result.RankingClass,
				level = result.RankingTier,
				steps = 0,
				mythicLeaderboardPlace = result.MythicPlacement,
				mythicPercentile = result.MythicPercentile
			}, _matchManager.Event?.PlayerEvent?.EventInfo, _assetLookupSystem);
			if (result.IsWotc)
			{
				playerName.SetNameColor(_mythicOrangeColor);
			}
			if (!string.IsNullOrEmpty(result.TitleSelection) && _cosmeticProvider.TitlesCatalog.TryGetValue(result.TitleSelection, out var value2))
			{
				playerName.SetTitle(_clientLocProvider.GetLocalizedText(value2.LocKey));
			}
			uint gameWinsRequired = GetGameWinsRequired();
			playerName.SetWins(gameWinsRequired, GetGameWins(playerNum));
			PlayerIdToPlayerName.Add(id, playerName);
			AllPlayerNameViewData.Add(new PlayerNameViewData
			{
				PlayerId = id,
				PlayerNameView = playerName,
				PlayerNum = playerNum
			});
			return playerName;
		}
		return null;
	}

	private uint GetGameWinsRequired()
	{
		uint result = 1u;
		if (_matchManager != null)
		{
			switch (_matchManager.WinCondition)
			{
			case MatchWinCondition.SingleElimination:
				result = 1u;
				break;
			case MatchWinCondition.Best2Of3:
				result = 2u;
				break;
			case MatchWinCondition.Best3Of5:
				result = 3u;
				break;
			}
		}
		return result;
	}

	private uint GetGameWins(GREPlayerNum playerNum)
	{
		uint num = 0u;
		foreach (MatchManager.GameResult gameResult in _matchManager.GameResults)
		{
			if (gameResult.Result == ResultType.WinLoss && gameResult.Winner == playerNum)
			{
				num++;
			}
		}
		return num;
	}

	public PlayerName GetPlayerNameById(uint id)
	{
		if (PlayerIdToPlayerName.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public PlayerName GetPlayerNameByGrePlayerNum(GREPlayerNum playerNum)
	{
		return AllPlayerNameViewData.Find((PlayerNameViewData x) => x.PlayerNum == playerNum).PlayerNameView;
	}

	public IReadOnlyList<PlayerNameViewData> GetAllPlayerNameDataList()
	{
		return AllPlayerNameViewData;
	}

	public void Dispose()
	{
		AllPlayerNameViewData.Clear();
		PlayerIdToPlayerName.Clear();
	}
}
