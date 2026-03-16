using System;
using System.Collections.Generic;
using System.Text;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.Credits;

public class CreditsTextProvider : ICreditsTextProvider
{
	private const int HEADER_SIZE = 54;

	private readonly IClientLocProvider _locProvider;

	private readonly ICreditsDataProvider _creditsDataProvider;

	public static ICreditsTextProvider PantryCreate()
	{
		return new CreditsTextProvider(Pantry.Get<IClientLocProvider>(), Pantry.Get<ICreditsDataProvider>());
	}

	public CreditsTextProvider(IClientLocProvider locProvider, ICreditsDataProvider creditsDataProvider)
	{
		_locProvider = locProvider ?? NullLocProvider.Default;
		_creditsDataProvider = creditsDataProvider ?? NullCreditsDataProvider.Default;
	}

	public string GetCreditsText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (CreditSectionData item in (IEnumerable<CreditSectionData>)(((object)_creditsDataProvider.GetCredits()) ?? ((object)Array.Empty<CreditSectionData>())))
		{
			string text = item.HeadingLocKey ?? string.Empty;
			if (!string.IsNullOrWhiteSpace(text))
			{
				string arg = (_locProvider.DoesContainTranslation(text) ? _locProvider.GetLocalizedText(text) : text);
				int num = ((item.HeaderTextSize != 0) ? item.HeaderTextSize : 54);
				arg = $"<font=\"Font_Title\"><size={num}>{arg}</size></font>";
				stringBuilder.AppendLine(arg);
				stringBuilder.AppendLine(string.Empty);
			}
			CreditRoleData[] array = item.Roles ?? Array.Empty<CreditRoleData>();
			string[] array2;
			foreach (CreditRoleData obj in array)
			{
				string text2 = obj.TitleLocKey ?? string.Empty;
				if (!string.IsNullOrWhiteSpace(text2))
				{
					if (_locProvider.DoesContainTranslation(text2))
					{
						string localizedText = _locProvider.GetLocalizedText(text2);
						stringBuilder.AppendLine("    " + localizedText);
					}
					else
					{
						stringBuilder.AppendLine("    " + text2);
					}
				}
				array2 = obj.Text ?? Array.Empty<string>();
				foreach (string text3 in array2)
				{
					stringBuilder.AppendLine("        " + text3);
				}
				stringBuilder.AppendLine(string.Empty);
			}
			array2 = item.Text ?? Array.Empty<string>();
			foreach (string value in array2)
			{
				stringBuilder.AppendLine(value);
			}
			stringBuilder.AppendLine(string.Empty);
			array2 = item.BulletedText ?? Array.Empty<string>();
			foreach (string text4 in array2)
			{
				stringBuilder.AppendLine("        •<indent=4%>" + text4 + "</indent>");
			}
			stringBuilder.AppendLine(string.Empty);
			stringBuilder.AppendLine(string.Empty);
		}
		return stringBuilder.ToString();
	}

	public string GetUniversesBeyondHeaderText()
	{
		return _locProvider.GetLocalizedText("Credits/UniversesBeyondPartners");
	}
}
