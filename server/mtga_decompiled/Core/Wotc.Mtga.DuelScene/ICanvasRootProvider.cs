using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public interface ICanvasRootProvider
{
	Transform GetCanvasRoot(CanvasLayer canvasLayer);
}
