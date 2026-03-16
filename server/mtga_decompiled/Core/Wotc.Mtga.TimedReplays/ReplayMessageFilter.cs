using System.Collections.Generic;
using System.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.TimedReplays;

public class ReplayMessageFilter
{
	public static readonly IReadOnlyList<GREMessageType> CommonGREMessagesToFilter = new List<GREMessageType>
	{
		GREMessageType.ConnectResp,
		GREMessageType.DieRollResultsResp,
		GREMessageType.GameStateMessage,
		GREMessageType.Uimessage,
		GREMessageType.SetSettingsResp,
		GREMessageType.GetSettingsResp,
		GREMessageType.QueuedGameStateMessage,
		GREMessageType.ChooseStartingPlayerReq
	};

	public static readonly IReadOnlyList<ClientMessageType> CommonClientMessagesToFilter = new List<ClientMessageType>
	{
		ClientMessageType.Uimessage,
		ClientMessageType.SetSettingsReq,
		ClientMessageType.GetSettingsReq
	};

	private readonly IReadOnlyCollection<GREMessageType> _greMessageFilters;

	private readonly IReadOnlyCollection<ClientMessageType> _clientMessageFilters;

	public ReplayMessageFilter(IReadOnlyCollection<GREMessageType> greMessageFilters, IReadOnlyCollection<ClientMessageType> clientMessageFilters)
	{
		_greMessageFilters = greMessageFilters;
		_clientMessageFilters = clientMessageFilters;
	}

	public bool ShouldIgnore(GREToClientMessage msg)
	{
		return _greMessageFilters.Contains(msg.Type);
	}

	public bool ShouldIgnore(ClientToGREMessage msg)
	{
		return _clientMessageFilters.Contains(msg.Type);
	}

	public bool ShouldIgnore(ReplayEntry message)
	{
		if (message.GREToClient == null || !ShouldIgnore(message.GREToClient))
		{
			if (message.ClientToGRE != null)
			{
				return ShouldIgnore(message.ClientToGRE);
			}
			return false;
		}
		return true;
	}
}
