using System.Collections.Generic;
using System.Text;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class ColorModifiedConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _clientLocProvider;

	public ColorModifiedConfigProvider(IClientLocProvider clientLocProvider)
	{
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (!CardUtilities.HasChangedColor(model))
		{
			yield break;
		}
		MtgCardInstance instance = model.Instance;
		StringBuilder colorValueStringBuilder = new StringBuilder();
		if (SetToColorless(instance))
		{
			colorValueStringBuilder.Append(_clientLocProvider.GetLocalizedText("AbilityHanger/Color/Colorless"));
			yield break;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		using (List<CardColor>.Enumerator enumerator = instance.Colors.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case CardColor.White:
					flag = true;
					break;
				case CardColor.Blue:
					flag2 = true;
					break;
				case CardColor.Black:
					flag3 = true;
					break;
				case CardColor.Red:
					flag4 = true;
					break;
				case CardColor.Green:
					flag5 = true;
					break;
				}
			}
		}
		if (flag && flag2 && flag3 && flag4 && flag5)
		{
			colorValueStringBuilder.Append(_clientLocProvider.GetLocalizedText("AbilityHanger/Color/AllColors"));
		}
		else
		{
			if (flag)
			{
				AddWordToList(colorValueStringBuilder, _clientLocProvider.GetLocalizedText("AbilityHanger/Color/White"));
			}
			if (flag2)
			{
				AddWordToList(colorValueStringBuilder, _clientLocProvider.GetLocalizedText("AbilityHanger/Color/Blue"));
			}
			if (flag3)
			{
				AddWordToList(colorValueStringBuilder, _clientLocProvider.GetLocalizedText("AbilityHanger/Color/Black"));
			}
			if (flag4)
			{
				AddWordToList(colorValueStringBuilder, _clientLocProvider.GetLocalizedText("AbilityHanger/Color/Red"));
			}
			if (flag5)
			{
				AddWordToList(colorValueStringBuilder, _clientLocProvider.GetLocalizedText("AbilityHanger/Color/Green"));
			}
		}
		string localizedText = _clientLocProvider.GetLocalizedText("AbilityHanger/ColorChange/ColorModified_Header");
		string bodyKey = GetBodyKey(instance.Zone?.Type ?? ZoneType.None);
		string localizedText2 = _clientLocProvider.GetLocalizedText(bodyKey, ("color", colorValueStringBuilder.ToString()));
		yield return new HangerConfig(localizedText, localizedText2);
		colorValueStringBuilder.Clear();
		static void AddWordToList(StringBuilder builder, string word)
		{
			if (builder.Length == 0)
			{
				builder.Append(word);
			}
			else
			{
				builder.AppendFormat(", {0}", word);
			}
		}
	}

	private static bool SetToColorless(MtgCardInstance instance)
	{
		if (instance.ColorModifications.Count > 0)
		{
			List<ColorModification> colorModifications = instance.ColorModifications;
			return colorModifications[colorModifications.Count - 1].SetToColorless;
		}
		return false;
	}

	private static string GetBodyKey(ZoneType zoneType)
	{
		return zoneType switch
		{
			ZoneType.Stack => "AbilityHanger/ColorChange/ColorModified_Body_Singular_Spell", 
			ZoneType.Battlefield => "AbilityHanger/ColorChange/ColorModified_Body_Singular", 
			_ => "AbilityHanger/ColorChange/ColorModified_Body_Singular_Card", 
		};
	}
}
