using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class PrivateInfoConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locProvider;

	public PrivateInfoConfigProvider(IClientLocProvider clientLocProvider)
	{
		_locProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (ShouldShowPrivateInfoHanger(model))
		{
			yield return new HangerConfig(_locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/InformationVisibility/Private_Header"), _locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/InformationVisibility/Private_Body"));
		}
	}

	public bool ShouldShowPrivateInfoHanger(ICardDataAdapter model)
	{
		if (model.Zone != null && model.Visibility == Visibility.Private && model.Zone.Visibility != Visibility.Private)
		{
			MtgCardInstance instance = model.Instance;
			if (instance != null && instance.Viewers?.Count == 1)
			{
				return model.Instance.Viewers.Contains(GREPlayerNum.LocalPlayer);
			}
		}
		return false;
	}
}
