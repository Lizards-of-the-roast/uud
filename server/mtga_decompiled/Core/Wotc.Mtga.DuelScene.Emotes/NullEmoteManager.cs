using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Emotes;

public class NullEmoteManager : IEmoteManager, IEmoteControllerProvider, IEntityDialogControllerProvider
{
	public static readonly IEmoteManager Default = new NullEmoteManager();

	private static readonly IEmoteControllerProvider _controllerProvider = NullEmoteControllerProvider.Default;

	private static readonly IEntityDialogControllerProvider _dialogueProvider = NullEntityDialogControllerProvider.Default;

	public IEmoteController GetEmoteControllerByPlayerType(GREPlayerNum playerType)
	{
		return _controllerProvider.GetEmoteControllerByPlayerType(playerType);
	}

	public IEmoteController GetEmoteControllerById(uint id)
	{
		return _controllerProvider.GetEmoteControllerById(id);
	}

	public IEnumerable<IEmoteController> GetAllEmoteControllers()
	{
		return _controllerProvider.GetAllEmoteControllers();
	}

	public EntityDialogController GetDialogControllerByPlayerType(GREPlayerNum playerType)
	{
		return _dialogueProvider.GetDialogControllerByPlayerType(playerType);
	}

	public EntityDialogController GetDialogControllerById(uint id)
	{
		return _dialogueProvider.GetDialogControllerById(id);
	}

	public IEnumerable<EntityDialogController> GetAllDialogControllers()
	{
		return _dialogueProvider.GetAllDialogControllers();
	}

	public void MuteEmotes(bool isMuted)
	{
	}

	public void CreateEmotesForPlayer(MtgPlayer player)
	{
	}
}
