using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public interface ICameraAdapter
{
	Transform CameraRoot { get; }

	Camera CameraReference { get; }

	Vector3 ViewportToWorldPoint(Vector2 viewport, float depth);
}
