using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/UXEvent/ScryResult Data", fileName = "ScryResultUXEvent_Data")]
public class ScryResultUXEvent_Data : ScriptableObject
{
	[Header("Per-Scry SFX")]
	public string PerScryAudioEvent;

	[Header("Per-Card SFX")]
	public string PerCardAudioEvent;

	[Space(10f)]
	[Header("Card Movement Splines")]
	public SplineMovementData ToTopSpline_Local;

	public SplineMovementData ToBottomSpline_Local;

	public SplineMovementData ToTopSpline_Opponent;

	public SplineMovementData ToBottomSpline_Opponent;

	public float Stagger = 0.5f;

	[Space(10f)]
	[Header("VFX on Library")]
	public GameObject ParticleEffectPrefab;

	public Vector3 ParticleEffectOffset = Vector3.zero;

	public Vector3 ParticleEffectRotation = Vector3.zero;

	public Vector3 ParticleEffectScale = Vector3.one;
}
