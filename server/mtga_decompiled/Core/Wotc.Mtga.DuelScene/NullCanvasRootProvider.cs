using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class NullCanvasRootProvider : ICanvasRootProvider
{
	public static readonly ICanvasRootProvider Default = new NullCanvasRootProvider();

	public Transform GetCanvasRoot(CanvasLayer canvasLayer)
	{
		return null;
	}
}
