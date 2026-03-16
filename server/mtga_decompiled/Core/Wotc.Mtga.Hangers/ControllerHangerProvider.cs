using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class ControllerHangerProvider : IHangerConfigProvider
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly IClientLocProvider _locManager;

	private readonly IEntityNameProvider<uint> _entityNameProvider;

	public ControllerHangerProvider(IGameStateProvider gameStateProvider, IClientLocProvider locManager, IEntityNameProvider<uint> entityNameProvider)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_locManager = locManager ?? NullLocProvider.Default;
		_entityNameProvider = entityNameProvider ?? NullIdNameProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (model != null && ShouldCreate(model.Instance, _gameStateProvider.LatestGameState))
		{
			string localizedText = _locManager.GetLocalizedText("AbilityHanger/SpecialHangers/Controller/Header");
			string name = _entityNameProvider.GetName(model.Controller?.InstanceId ?? 0);
			yield return new HangerConfig(localizedText, name, null, null, convertSymbols: false);
		}
	}

	public static bool ShouldCreate(MtgCardInstance instance, MtgGameState gameState)
	{
		if (gameState == null || instance == null || instance.Controller == null || instance.Owner == null || instance.Zone == null)
		{
			return false;
		}
		if (instance.Zone.Type != ZoneType.Battlefield)
		{
			return false;
		}
		if (gameState.IsMultiplayer && !instance.Controller.IsLocalPlayer)
		{
			return true;
		}
		if (instance.CardTypes.Contains(CardType.Battle))
		{
			return true;
		}
		if (instance.AttachedToId == 0)
		{
			return false;
		}
		GREPlayerNum clientPlayerEnum = instance.Controller.ClientPlayerEnum;
		GREPlayerNum gREPlayerNum = clientPlayerEnum;
		uint attachedToId = instance.AttachedToId;
		MtgCardInstance card;
		while (gameState.TryGetCard(attachedToId, out card) && card.Controller != null)
		{
			if (card.CardTypes.Contains(CardType.Battle))
			{
				return true;
			}
			gREPlayerNum = card.Controller.ClientPlayerEnum;
			attachedToId = card.AttachedToId;
		}
		return clientPlayerEnum != gREPlayerNum;
	}
}
