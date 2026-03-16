namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public readonly struct BattlefieldData
{
	public readonly string Name;

	public readonly bool InRandomPool;

	public readonly string ScenePath;

	public BattlefieldData(string name, bool inRandomPool, string scenePath)
	{
		Name = name;
		InRandomPool = inRandomPool;
		ScenePath = scenePath;
	}
}
