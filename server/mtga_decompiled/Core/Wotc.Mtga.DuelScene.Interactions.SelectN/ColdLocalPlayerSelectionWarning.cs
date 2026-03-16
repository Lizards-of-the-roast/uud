using GreClient.Rules;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class ColdLocalPlayerSelectionWarning : ISelectionConfirmation
{
	private readonly IClientLocProvider _clientLocProvider;

	public ColdLocalPlayerSelectionWarning(IClientLocProvider clientLocProvider)
	{
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public string GetConfirmationText(HighlightType highlightType, IEntityView entityView, SelectNRequest request)
	{
		if (highlightType != HighlightType.Cold)
		{
			return null;
		}
		if (!(entityView is DuelScene_AvatarView { IsLocalPlayer: not false }))
		{
			return null;
		}
		return _clientLocProvider.GetLocalizedText("DuelScene/Selection_LocalPlayer");
	}
}
