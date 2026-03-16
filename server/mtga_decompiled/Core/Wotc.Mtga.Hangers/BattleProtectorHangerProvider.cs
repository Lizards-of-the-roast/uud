using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class BattleProtectorHangerProvider : IHangerConfigProvider
{
	private readonly IEntityNameProvider<uint> _nameProvider;

	private readonly IClientLocProvider _locProvider;

	private readonly IGameStateProvider _gameStateProvider;

	public BattleProtectorHangerProvider(IEntityNameProvider<uint> nameProvider, IClientLocProvider locProvider, IGameStateProvider gameStateProvider)
	{
		_nameProvider = nameProvider ?? NullIdNameProvider.Default;
		_locProvider = locProvider ?? NullLocProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (BattleIsInPlay(model) && TryGetProtectorId(model.InstanceId, _gameStateProvider.CurrentGameState, out var protectorId))
		{
			string localizedText = _locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/BattleProtector/Header");
			string name = _nameProvider.GetName(protectorId);
			yield return new HangerConfig(localizedText, name);
		}
	}

	private static bool BattleIsInPlay(ICardDataAdapter cardData)
	{
		if (cardData.CardTypes.Contains(CardType.Battle))
		{
			return cardData.Zone.Type == ZoneType.Battlefield;
		}
		return false;
	}

	private static bool TryGetProtectorId(uint battleInstanceId, MtgGameState gameState, out uint protectorId)
	{
		protectorId = 0u;
		foreach (DesignationData designation in gameState.Designations)
		{
			if (designation.Type == Designation.Protector && designation.AffectorId == battleInstanceId)
			{
				protectorId = designation.AffectedId;
				return true;
			}
		}
		return false;
	}
}
