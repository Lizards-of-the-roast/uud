using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Cards.Text;

public class AddedBackupAbilitiesTextParser : ITextEntryParser
{
	private const string ABILTY_WORD_BACKUP = "Backup";

	private readonly IAbilityDataProvider _abilityProvider;

	private readonly IGreLocProvider _locProvider;

	public AddedBackupAbilitiesTextParser(IGreLocProvider locProvider, IAbilityDataProvider abilityProvider)
	{
		_locProvider = locProvider ?? NullGreLocManager.Default;
		_abilityProvider = abilityProvider ?? NullAbilityDataProvider.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		if (CardOnlyTargetsSelf(card))
		{
			yield break;
		}
		foreach (AbilityWordData item in BackupAbilityWords(card))
		{
			if (_abilityProvider.TryGetAbilityRecordById(item.AbilityGrpId, out var record))
			{
				string localizedText = _locProvider.GetLocalizedText(record.TextId, overrideLang);
				localizedText = ManaUtilities.ConvertManaSymbols(localizedText);
				localizedText = string.Format(colorSettings.AddedFormat, localizedText);
				yield return new BasicTextEntry(localizedText);
			}
		}
	}

	private IEnumerable<AbilityWordData> BackupAbilityWords(ICardDataAdapter card)
	{
		foreach (AbilityWordData activeAbilityWord in card.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord == "Backup")
			{
				yield return activeAbilityWord;
			}
		}
	}

	private static bool CardOnlyTargetsSelf(ICardDataAdapter card)
	{
		uint instanceId = card.InstanceId;
		foreach (MtgEntity target in card.Targets)
		{
			if (target.InstanceId != instanceId)
			{
				return false;
			}
		}
		return card.Targets.Count > 0;
	}
}
