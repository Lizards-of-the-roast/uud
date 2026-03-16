using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Emotes;

public interface IEmoteManager : IEmoteControllerProvider, IEntityDialogControllerProvider
{
	void MuteEmotes(bool isMuted);

	void CreateEmotesForPlayer(MtgPlayer player);
}
