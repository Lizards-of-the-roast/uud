namespace Wotc.Mtga.DuelScene;

public class FamiliarControlModule : DebugModule
{
	private readonly string _name;

	private readonly string _description;

	private readonly MatchRenderer _matchRenderer;

	public override string Name => _name;

	public override string Description => _description;

	public FamiliarControlModule(string name, string description, MatchRenderer matchRenderer)
	{
		_name = name ?? "Headless Client Controls";
		_description = description ?? string.Empty;
		_matchRenderer = matchRenderer;
	}

	public override void Render()
	{
		_matchRenderer.Render();
	}
}
