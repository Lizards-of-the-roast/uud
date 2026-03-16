using System.Linq;
using System.Text;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.BrowserCardHeaders;

public class BaseIdAbilityHeaders : ModalBrowserCardHeaderProvider.ISubProvider
{
	private readonly IAbilityTextProvider _abilityTextProvider;

	private readonly StringBuilder _sb;

	public BaseIdAbilityHeaders(IAbilityTextProvider abilityTextProvider)
	{
		_abilityTextProvider = abilityTextProvider;
		_sb = new StringBuilder();
	}

	public bool TryGetHeaderData(ICardDataAdapter cardModel, AbilityPrintingData abilityData, Action action, out ModalBrowserCardHeaderProvider.HeaderData headerData)
	{
		if (abilityData != null && abilityData.BaseId != 0)
		{
			string abilityTextByCardAbilityGrpId = _abilityTextProvider.GetAbilityTextByCardAbilityGrpId(cardModel.GrpId, abilityData.BaseId, cardModel.AbilityIds, cardModel.TitleId);
			if (!string.IsNullOrEmpty(abilityTextByCardAbilityGrpId))
			{
				if (abilityData.BaseAbility != null && abilityData.BaseAbility.Tags.Contains(MetaDataTag.IncludeManaInModalAction))
				{
					string text = ManaUtilities.ManaRequirementsToTextString(action.ManaCost);
					if (text != null && !string.IsNullOrEmpty(text))
					{
						_sb.Clear();
						_sb.AppendJoin(' ', abilityTextByCardAbilityGrpId, text);
						headerData = new ModalBrowserCardHeaderProvider.HeaderData(useActionTypeHeader: true, _sb.ToString());
						return true;
					}
				}
				headerData = new ModalBrowserCardHeaderProvider.HeaderData(useActionTypeHeader: true, abilityTextByCardAbilityGrpId);
				return true;
			}
		}
		headerData = ModalBrowserCardHeaderProvider.HeaderData.Null;
		return false;
	}
}
