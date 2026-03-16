using System.Linq;
using UnityEngine;

namespace Wotc.Mtga;

public static class UnityUtilities
{
	public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
	{
		intersection = Vector3.zero;
		float num = Vector3.Dot(planePoint - linePoint, planeNormal);
		float num2 = Vector3.Dot(lineVec, planeNormal);
		if (!Mathf.Approximately(num2, 0f))
		{
			float num3 = num / num2;
			Vector3 vector = lineVec.normalized * num3;
			intersection = linePoint + vector;
			return true;
		}
		return false;
	}

	public static T FindObjectOfType<T>(bool includeInactive) where T : Component
	{
		if (includeInactive)
		{
			return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault((T o) => o.gameObject.scene.rootCount != 0);
		}
		return Object.FindObjectOfType<T>();
	}

	public static bool CanOpenDirectory()
	{
		if (!Application.isEditor && Application.platform != RuntimePlatform.WindowsPlayer)
		{
			return Application.platform == RuntimePlatform.OSXPlayer;
		}
		return true;
	}

	public static void OpenDirectory(string directoryPath)
	{
		if (Application.platform == RuntimePlatform.OSXPlayer)
		{
			directoryPath = directoryPath.Replace(" ", "%20");
		}
		Application.OpenURL("file://" + directoryPath);
	}
}
