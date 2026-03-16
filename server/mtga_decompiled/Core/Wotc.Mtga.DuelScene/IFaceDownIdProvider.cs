namespace Wotc.Mtga.DuelScene;

public interface IFaceDownIdProvider
{
	bool TryGetFaceDownId(uint instanceId, out uint faceDownId);
}
