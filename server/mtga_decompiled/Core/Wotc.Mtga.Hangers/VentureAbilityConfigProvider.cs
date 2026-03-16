using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class VentureAbilityConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locManager;

	private readonly IPathProvider<AbilityPrintingData> _iconPathProvider;

	public VentureAbilityConfigProvider(IClientLocProvider locManager, IPathProvider<AbilityPrintingData> iconPathProvider)
	{
		_locManager = locManager ?? NullLocProvider.Default;
		_iconPathProvider = iconPathProvider ?? new NullPathProvider<AbilityPrintingData>();
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (VenturesIntoTheDungeon(model))
		{
			string localizedText = _locManager.GetLocalizedText("AbilityHanger/Keyword/VentureIntoTheDungeon_Title");
			string localizedText2 = _locManager.GetLocalizedText("AbilityHanger/Keyword/VentureIntoTheDungeon");
			string path = _iconPathProvider.GetPath(model.Abilities.FirstOrDefault((AbilityPrintingData x) => x.ReferencedAbilityTypes.Contains(AbilityType.VentureIntoTheDungeon)));
			yield return new HangerConfig(localizedText, localizedText2, null, path);
		}
	}

	public static bool VenturesIntoTheDungeon(ICardDataAdapter model)
	{
		foreach (AbilityPrintingData ability in model.Abilities)
		{
			if (ability.ReferencedAbilityTypes.Contains(AbilityType.VentureIntoTheDungeon))
			{
				return true;
			}
		}
		return false;
	}
}
