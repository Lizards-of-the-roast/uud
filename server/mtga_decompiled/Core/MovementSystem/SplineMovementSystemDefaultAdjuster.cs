using UnityEngine;

namespace MovementSystem;

public class SplineMovementSystemDefaultAdjuster : MonoBehaviour
{
	public float Speed;

	public AnimationCurve Curve;

	public float TangentWeight;

	public bool GenerateCurve;

	public bool ApplySettings;

	private void Update()
	{
		if (GenerateCurve)
		{
			Curve = new AnimationCurve(new Keyframe(0f, 0f, TangentWeight, TangentWeight), new Keyframe(1f, 1f, 0f, 0f));
			GenerateCurve = false;
		}
		if (ApplySettings)
		{
			SplineMovementSystem.AdjustGlobalAnimation(Speed, Curve);
		}
	}

	private void Start()
	{
		Speed = SplineMovementSystem.GetDefaultSpeed();
		Curve = SplineMovementSystem.GetDefaultEasingCurve();
	}
}
