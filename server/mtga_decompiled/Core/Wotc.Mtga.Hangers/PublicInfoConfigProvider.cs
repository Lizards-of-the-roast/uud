using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class PublicInfoConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locProvider;

	public PublicInfoConfigProvider(IClientLocProvider clientLocProvider)
	{
		_locProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (ShouldShowPublicInfoHanger(model))
		{
			yield return new HangerConfig(_locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/InformationVisibility/Public_Header"), _locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/InformationVisibility/Public_Body"));
		}
	}

	public bool ShouldShowPublicInfoHanger(ICardDataAdapter model)
	{
		if (model.Instance.Viewers.Contains(GREPlayerNum.Opponent) && model.Instance.Viewers.Contains(GREPlayerNum.LocalPlayer) && model.Visibility == Visibility.Private)
		{
			return true;
		}
		if (model.Zone != null && model.Visibility == Visibility.Public && model.Zone.Visibility != Visibility.Public)
		{
			return model.Zone.Type != ZoneType.Hand;
		}
		return false;
	}
}
