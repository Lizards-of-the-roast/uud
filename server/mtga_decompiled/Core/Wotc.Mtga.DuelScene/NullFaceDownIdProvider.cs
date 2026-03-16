namespace Wotc.Mtga.DuelScene;

public class NullFaceDownIdProvider : IFaceDownIdProvider
{
	public static readonly IFaceDownIdProvider Default = new NullFaceDownIdProvider();

	public bool TryGetFaceDownId(uint instanceId, out uint faceDownId)
	{
		faceDownId = 0u;
		return false;
	}
}
