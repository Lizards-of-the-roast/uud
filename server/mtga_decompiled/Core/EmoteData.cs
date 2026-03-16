public class EmoteData
{
	public readonly string Id;

	public ClientEmoteEntry Entry;

	public EmoteData(string id, ClientEmoteEntry clientEmoteEntry)
	{
		Id = id;
		Entry = clientEmoteEntry;
	}
}
