using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/UXEvent/CoinFlip Data", fileName = "ChooseRandomUXEvent_CoinFlip_Data")]
public class ChooseRandomUXEvent_CoinFlip_Data : ScriptableObject
{
	public GameObject CoinPrefab;

	public AnimationClip HeadsAnimation;

	public AnimationClip TailsAnimation;

	public string WwiseEvent;

	[Space(10f)]
	public float CoinSize = 1f;

	public float CoinSpacing = 0.2f;

	public float BattlefieldOffset;

	[Space(10f)]
	public float FlipDuration = 2f;

	public float MaxDuration = 3f;

	public float FlipStagger = 0.5f;
}
