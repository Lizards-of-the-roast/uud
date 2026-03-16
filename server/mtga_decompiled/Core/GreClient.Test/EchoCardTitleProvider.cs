using System.Collections.Generic;
using System.Text;
using Wotc.Mtga.Cards.Database;

namespace GreClient.Test;

public class EchoCardTitleProvider : ICardTitleProvider
{
	public string GetCardTitle(uint grpId, bool formatted = true, string overrideLanguageCode = null)
	{
		return grpId.ToString();
	}

	public string GetCardInterchangeableTitle(uint grpId, bool formatted = true, string overrideLanguageCode = null)
	{
		return grpId.ToString();
	}

	public string GetCardTitle(ICollection<uint> grpIds, bool formatted = true, string overrideLanguageCode = null)
	{
		if (grpIds.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (uint grpId in grpIds)
		{
			stringBuilder.AppendLine($"{grpId},");
		}
		stringBuilder.Length--;
		return stringBuilder.ToString();
	}
}
