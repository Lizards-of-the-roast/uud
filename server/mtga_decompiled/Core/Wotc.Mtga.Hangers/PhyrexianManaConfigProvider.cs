using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class PhyrexianManaConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _clientLocManager;

	private readonly IPathProvider<ICardDataAdapter> _pathProvider;

	public PhyrexianManaConfigProvider(IClientLocProvider clientLocManager, IPathProvider<ICardDataAdapter> pathProvider)
	{
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_pathProvider = pathProvider ?? NullPathProvider<ICardDataAdapter>.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		foreach (ManaQuantity item in PhyrexianCostsForCard(model))
		{
			string path = _pathProvider.GetPath(model);
			if (model.Abilities.ContainsId(238u))
			{
				char color = ManaUtilities.ColorToChar(item.Color);
				if (item.AltColor == ManaColor.Phyrexian)
				{
					if (model.PrintedCastingCost.Count((ManaQuantity x) => x.IsPhyrexian) == 1)
					{
						yield return MonoColorCompleatedHanger(item, path);
					}
					else
					{
						yield return PluralMonoColorCompleatedHanger(item, path);
					}
				}
				else
				{
					yield return MultiColorCompleatedHanger(item, color, path);
				}
			}
			else
			{
				yield return KeywordPhyrexianHanger(path, item.Color);
			}
		}
	}

	private HangerConfig MonoColorCompleatedHanger(ManaQuantity phyMana, string iconSpritePath)
	{
		(string, string)[] locParams = new(string, string)[1] { ("color", ManaUtilities.ColorToChar(phyMana.Color).ToString()) };
		string localizedText = _clientLocManager.GetLocalizedText("AbilityHanger/Keyword/Compleated_Title");
		string localizedText2 = _clientLocManager.GetLocalizedText("AbilityHanger/Keyword/Compleated_Mono_Color_Body", locParams);
		return new HangerConfig(localizedText, localizedText2, null, iconSpritePath);
	}

	private HangerConfig PluralMonoColorCompleatedHanger(ManaQuantity phyMana, string iconSpritePath)
	{
		(string, string)[] locParams = new(string, string)[1] { ("color", ManaUtilities.ColorToChar(phyMana.Color).ToString()) };
		string localizedText = _clientLocManager.GetLocalizedText("AbilityHanger/Keyword/Compleated_Title");
		string localizedText2 = _clientLocManager.GetLocalizedText("AbilityHanger/Keyword/Compleated_Plural_Mono_Color_Body", locParams);
		return new HangerConfig(localizedText, localizedText2, null, iconSpritePath);
	}

	private HangerConfig MultiColorCompleatedHanger(ManaQuantity phyMana, char color, string iconSpritePath)
	{
		char c = ManaUtilities.ColorToChar(phyMana.AltColor);
		(string, string)[] locParams = new(string, string)[3]
		{
			("manaCombined", ManaUtilities.PhyrexianMultipleColorBuilder(color, c)),
			("manaType0", $"x{color}"),
			("manaType1", $"x{c}")
		};
		string localizedText = _clientLocManager.GetLocalizedText("AbilityHanger/Keyword/Compleated_Title");
		string localizedText2 = _clientLocManager.GetLocalizedText("AbilityHanger/Keyword/Compleated_Body", locParams);
		return new HangerConfig(localizedText, localizedText2, null, iconSpritePath);
	}

	private HangerConfig KeywordPhyrexianHanger(string manaSprite, ManaColor manaColor)
	{
		string localizedText = _clientLocManager.GetLocalizedText("AbilityHanger/PhyrexianMana_Title");
		(string, string)[] locParams = new(string, string)[1] { ("color", ManaUtilities.ColorToChar(manaColor).ToString()) };
		string localizedText2 = _clientLocManager.GetLocalizedText("AbilityHanger/PhyrexianMana_Body", locParams);
		return new HangerConfig(localizedText, localizedText2, null, manaSprite);
	}

	private IEnumerable<ManaQuantity> PhyrexianCostsForCard(ICardDataAdapter model)
	{
		foreach (ManaQuantity item in model.PrintedCastingCost)
		{
			if (item.IsPhyrexian)
			{
				yield return item;
			}
		}
		foreach (AbilityPrintingData ability in model.Abilities)
		{
			foreach (ManaQuantity item2 in ability.ManaCost)
			{
				if (item2.IsPhyrexian)
				{
					yield return item2;
				}
			}
		}
	}
}
