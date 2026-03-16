using UnityEngine;

namespace EsportsCrowd;

[CreateAssetMenu(fileName = "CrowdSettings", menuName = "ScriptableObject/Esports/Crowd Settings", order = 2)]
public class CrowdSettings : ScriptableObject
{
	[SerializeField]
	private AnimationCurve _crowdParticipationPercentage = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 0.3f);

	public float CrowdParticipation => _crowdParticipationPercentage.Evaluate(Random.Range(0f, 1f));
}
