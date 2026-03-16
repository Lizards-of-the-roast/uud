using UnityEngine;

namespace StatsMonitor.Core;

public class Anchor
{
	public Vector2 position;

	public Vector2 min;

	public Vector2 max;

	public Vector2 pivot;

	public Anchor(float x, float y, float minX, float minY, float maxX, float maxY, float pivotX, float pivotY)
	{
		position = new Vector2(x, y);
		min = new Vector2(minX, minY);
		max = new Vector2(maxX, maxY);
		pivot = new Vector2(pivotX, pivotY);
	}
}
