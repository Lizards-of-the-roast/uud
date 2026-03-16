using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.BrowserCardHeaders;

public class LinkedFaceHeaders : ModalBrowserCardHeaderProvider.ISubProvider
{
	private readonly IClientLocProvider _clientLocProvider;

	private readonly IGreLocProvider _greLocProvider;

	private readonly IAbilityDataProvider _abilityDataProvider;

	public LinkedFaceHeaders(IClientLocProvider clientLocProvider, IGreLocProvider greLocProvider, IAbilityDataProvider abilityDataProvider)
	{
		_clientLocProvider = clientLocProvider;
		_greLocProvider = greLocProvider;
		_abilityDataProvider = abilityDataProvider;
	}

	public bool TryGetHeaderData(ICardDataAdapter cardModel, AbilityPrintingData abilityData, Action action, out ModalBrowserCardHeaderProvider.HeaderData headerData)
	{
		switch (cardModel.LinkedFaceType)
		{
		case LinkedFace.MdfcFront:
			headerData = new ModalBrowserCardHeaderProvider.HeaderData(_clientLocProvider.GetLocalizedText("DuelScene/FaceHanger/BackSide"), string.Empty);
			break;
		case LinkedFace.MdfcBack:
			headerData = new ModalBrowserCardHeaderProvider.HeaderData(_clientLocProvider.GetLocalizedText("DuelScene/FaceHanger/FrontSide"), string.Empty);
			break;
		case LinkedFace.PrototypeParent:
		{
			AbilityPrintingData abilityPrintingById = _abilityDataProvider.GetAbilityPrintingById(263u);
			string localizedText = _greLocProvider.GetLocalizedText(abilityPrintingById.TextId);
			headerData = new ModalBrowserCardHeaderProvider.HeaderData(useActionTypeHeader: true, localizedText);
			break;
		}
		default:
			headerData = ModalBrowserCardHeaderProvider.HeaderData.Null;
			break;
		}
		return !headerData.IsNull();
	}
}
