namespace Wotc.Mtga.Cards.Text;

public static class ICardTextEntryExtensions
{
	public static EntryType GetEntryType(this ICardTextEntry entry)
	{
		if (!(entry is BasicTextEntry))
		{
			if (!(entry is LoyaltyTextEntry))
			{
				if (!(entry is ChapterTextEntry))
				{
					if (!(entry is LevelTextEntry))
					{
						if (!(entry is TableTextEntry))
						{
							if (!(entry is DividerTextEntry))
							{
								if (!(entry is StationTextEntry))
								{
									if (!(entry is LevelUpTextEntry))
									{
										if (entry is LoyaltyTableTextEntry)
										{
											return EntryType.LoyaltyTable;
										}
										return EntryType.Invalid;
									}
									return EntryType.LevelUp;
								}
								return EntryType.Station;
							}
							return EntryType.Divider;
						}
						return EntryType.Table;
					}
					return EntryType.Level;
				}
				return EntryType.Chapter;
			}
			return EntryType.Loyalty;
		}
		return EntryType.Basic;
	}
}
