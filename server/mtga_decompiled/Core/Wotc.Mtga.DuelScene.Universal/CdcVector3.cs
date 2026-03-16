using UnityEngine;

namespace Wotc.Mtga.DuelScene.Universal;

public static class CdcVector3
{
	public static Vector3 right => Vector3.left;

	public static Vector3 left => Vector3.right;

	public static Vector3 forward => Vector3.back;

	public static Vector3 back => Vector3.forward;

	public static Vector3 up => Vector3.up;

	public static Vector3 down => Vector3.down;
}
