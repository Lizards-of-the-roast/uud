using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using ReferenceMap;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class ExiledUnderHangerConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _clientLocProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IBrowserProvider _browserProvider;

	private readonly IEntityNameProvider<uint> _entityNameProvider;

	public ExiledUnderHangerConfigProvider(IClientLocProvider clientLocProvider, IGameStateProvider gameStateProvider, IBrowserProvider browserProvider, IEntityNameProvider<uint> entityNameProvider)
	{
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_browserProvider = browserProvider ?? NullBrowserProvider.Default;
		_entityNameProvider = entityNameProvider ?? NullIdNameProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (TryGetDisplayedUnderId(model, _gameStateProvider.CurrentGameState, out var displayedUnderId))
		{
			yield return new HangerConfig(_clientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/ExiledUnder_Header"), _entityNameProvider.GetName(displayedUnderId));
		}
	}

	private bool TryGetDisplayedUnderId(ICardDataAdapter model, MtgGameState gameState, out uint displayedUnderId)
	{
		displayedUnderId = 0u;
		if (_browserProvider.IsBrowserVisible && _browserProvider.CurrentBrowser is ViewDismissBrowser)
		{
			return false;
		}
		MtgCardInstance instance = model.Instance;
		if (instance == null)
		{
			return false;
		}
		MtgZone zone = instance.Zone;
		if (zone == null)
		{
			return false;
		}
		displayedUnderId = GetDisplayedUnderId(gameState.ReferenceMap, instance.InstanceId);
		if (zone.Type == ZoneType.Exile)
		{
			return displayedUnderId != 0;
		}
		return false;
	}

	private static uint GetDisplayedUnderId(Map map, uint cardInstanceId)
	{
		return map?.GetDisplayedUnderId(cardInstanceId) ?? 0;
	}
}
