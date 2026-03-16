using Wizards.Arena.Enums.Cosmetic;

public struct ClientEmoteEntry
{
	public string Id;

	public EmotePage Page;

	public bool IsDefault;

	public string Category;

	public ClientEmoteEntry(string id, EmotePage page, bool isDefault, string category = null)
	{
		Id = id;
		Page = page;
		IsDefault = isDefault;
		Category = category ?? string.Empty;
	}

	public static ClientEmoteEntry DefaultPhrase(string id)
	{
		return new ClientEmoteEntry(id, EmotePage.Phrase, isDefault: true);
	}

	public EmoteData ToEmoteData()
	{
		return new EmoteData(Id, this);
	}
}
