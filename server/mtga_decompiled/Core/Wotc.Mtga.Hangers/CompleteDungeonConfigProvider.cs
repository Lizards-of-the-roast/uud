using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class CompleteDungeonConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locManager;

	private readonly ICardTitleProvider _cardTitleProvider;

	private readonly IPathProvider<AbilityPrintingData> _iconPathProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private HashSet<uint> _completedDungeons = new HashSet<uint>();

	private List<string> _bodyCache = new List<string>();

	public CompleteDungeonConfigProvider(IClientLocProvider locManager, ICardTitleProvider cardTitleProvider, IPathProvider<AbilityPrintingData> iconPathProvider, IGameStateProvider gameStateProvider)
	{
		_locManager = locManager ?? NullLocProvider.Default;
		_cardTitleProvider = cardTitleProvider ?? NullCardTitleProvider.Default;
		_iconPathProvider = iconPathProvider ?? new NullPathProvider<AbilityPrintingData>();
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		MtgPlayer mtgPlayer = GameStateOwner(model, _gameStateProvider.CurrentGameState);
		if (CaresAboutCompletedDungeons(model, mtgPlayer))
		{
			string localizedText = _locManager.GetLocalizedText("AbilityHanger/Keyword/VentureIntoTheDungeon_CompletedDungeons");
			string details = BodyText(mtgPlayer.DungeonState.CompletedDungeons);
			string path = _iconPathProvider.GetPath(model.Abilities.FirstOrDefault((AbilityPrintingData x) => x.SubCategory == AbilitySubCategory.CompleteDungeon));
			yield return new HangerConfig(localizedText, details, null, path);
		}
	}

	private MtgPlayer GameStateOwner(ICardDataAdapter model, MtgGameState gameState)
	{
		if (model == null || gameState == null)
		{
			return null;
		}
		if (model.Owner == null)
		{
			return null;
		}
		return gameState.GetPlayerById(model.Owner.InstanceId);
	}

	public static bool CaresAboutCompletedDungeons(ICardDataAdapter model, MtgPlayer owner)
	{
		if (owner == null)
		{
			return false;
		}
		if (owner.DungeonState.Equals(default(DungeonData)))
		{
			return false;
		}
		if (owner.DungeonState.CompletedDungeons.Length != 0)
		{
			return model.Abilities.Any((AbilityPrintingData x) => x.SubCategory == AbilitySubCategory.CompleteDungeon);
		}
		return false;
	}

	private string BodyText(uint[] completedDungeons)
	{
		_completedDungeons.Clear();
		foreach (uint item in completedDungeons)
		{
			_completedDungeons.Add(item);
		}
		return BodyText(_completedDungeons);
	}

	private string BodyText(HashSet<uint> completedDungeons)
	{
		_bodyCache.Clear();
		foreach (uint completedDungeon in completedDungeons)
		{
			_bodyCache.Add(_cardTitleProvider.GetCardTitle(completedDungeon));
		}
		return string.Join("\n", _bodyCache);
	}
}
