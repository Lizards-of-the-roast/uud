namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public readonly struct EmblemData
{
	public readonly uint Id;

	public readonly string Title;

	public readonly string Description;

	public EmblemData(uint id, string title, string description)
	{
		Id = id;
		Title = title;
		Description = description;
	}
}
