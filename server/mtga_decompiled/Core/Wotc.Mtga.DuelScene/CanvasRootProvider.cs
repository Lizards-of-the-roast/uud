using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class CanvasRootProvider : ICanvasRootProvider
{
	private readonly CanvasManager _canvasManager;

	public CanvasRootProvider(CanvasManager canvasManager)
	{
		_canvasManager = canvasManager;
	}

	public Transform GetCanvasRoot(CanvasLayer canvasLayer)
	{
		return _canvasManager.GetCanvasRoot(canvasLayer);
	}
}
