using System.Text;
using UnityEngine;

public static class GameObjectExtensions
{
	public static string GetFullPath(this GameObject go)
	{
		Transform transform = go.transform;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Insert(0, transform.name);
		transform = transform.parent;
		while ((bool)transform)
		{
			stringBuilder.Insert(0, "/");
			stringBuilder.Insert(0, transform.name);
			transform = transform.parent;
		}
		return stringBuilder.ToString();
	}
}
