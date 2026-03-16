using System;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Meta.MainNavigation.SocialV2;

public struct GatheringPlayer
{
	private string _playerId;

	private string _displayName;

	private GatheringPlayerPresence _presence;

	private IGatheringServiceWrapper _serviceWrapper;

	public string PlayerId => _playerId;

	public string DisplayName => _displayName;

	public GatheringPlayerPresence Presence => _presence;

	public event Action<GatheringPlayerPresence> PlayerPresenceUpdated;

	public GatheringPlayer(string playerId, string displayName, GatheringPlayerPresence playerPresenceStatus = GatheringPlayerPresence.Unknown)
	{
		_playerId = playerId;
		_displayName = displayName;
		_presence = playerPresenceStatus;
		_serviceWrapper = Pantry.Get<IGatheringServiceWrapper>();
		this.PlayerPresenceUpdated = null;
	}

	public void UpdatePlayerPresence(GatheringPlayerPresence presence, bool updateService = false)
	{
		if (_presence != presence)
		{
			_presence = presence;
			this.PlayerPresenceUpdated?.Invoke(_presence);
		}
	}

	public static explicit operator GatheringPlayer(Wizards.Arena.Models.Network.GatheringPlayer gatheringPlayerNetwork)
	{
		return new GatheringPlayer(gatheringPlayerNetwork.PlayerId, gatheringPlayerNetwork.DisplayName);
	}
}
