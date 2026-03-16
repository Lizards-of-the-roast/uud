namespace Wotc.Mtga.DuelScene;

public class NullIdNameProvider : IEntityNameProvider<uint>
{
	public static readonly IEntityNameProvider<uint> Default = new NullIdNameProvider();

	public string GetName(uint entityId, bool formatted = true)
	{
		return string.Empty;
	}
}
