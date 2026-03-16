using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

public class HiddenInformationManaInfoConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locProvider;

	public HiddenInformationManaInfoConfigProvider(IClientLocProvider clientLocProvider)
	{
		_locProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (ShouldShowHanger(model))
		{
			yield return new HangerConfig(GetHangerText(model.Instance.FaceDownState.ManaCost), string.Empty);
		}
	}

	public bool ShouldShowHanger(ICardDataAdapter model)
	{
		if (model != null && model.Instance?.FaceDownState.ManaCost >= 0)
		{
			return true;
		}
		return false;
	}

	private string GetHangerText(int manaCost)
	{
		return _locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/ManaValueIdentifier", ("manaCost", manaCost.ToString()));
	}
}
