using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace EsportsCrowd;

[CreateAssetMenu(fileName = "CrowdPersonSettings", menuName = "ScriptableObject/Esports/Crowd Person Settings", order = 2)]
public class CrowdPersonSettings : ScriptableObject
{
	[Header("Animation Settings")]
	[SerializeField]
	private int _idleAnimIndex;

	[SerializeField]
	private int _cheerAnimIndex = 1;

	[SerializeField]
	private AnimationCurve _hypeRecoverySpeed = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Header("Starting Size (Range)")]
	[SerializeField]
	private AnimationCurve _heightCurve = new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(0.5f, 1f, 0f, 0f), new Keyframe(1f, 1.2f));

	[SerializeField]
	private AnimationCurve _weightCurve = new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(0.5f, 1f, 0f, 0f), new Keyframe(1f, 1.2f));

	[Header("Starting Color Values")]
	[SerializeField]
	private Gradient _skinColorVariation;

	[SerializeField]
	private Gradient _shirtColorVariation;

	[SerializeField]
	private Gradient _pantsColorVariation;

	[SerializeField]
	private Gradient _sleevesColorVariation;

	[SerializeField]
	private Gradient _flagColorVariation;

	[Header("Affinity Color Values")]
	[SerializeField]
	private Gradient _colorlessAffinityGradient;

	[SerializeField]
	private Gradient _whiteAffinityGradient;

	[SerializeField]
	private Gradient _blueAffinityGradient;

	[SerializeField]
	private Gradient _blackAffinityGradient;

	[SerializeField]
	private Gradient _redAffinityGradient;

	[SerializeField]
	private Gradient _greenAffinityGradient;

	[Header("Starting Hype (Range)")]
	[SerializeField]
	private AnimationCurve _genericStartingHypeRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _colorlessStartingHypeRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _whiteStartingHypeRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _blueStartingHypeRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _blackStartingHypeRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _redStartingHypeRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _greenStartingHypeRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[Header("Hype Threshold (Range)")]
	[SerializeField]
	private AnimationCurve _genericHypeLimitRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _colorlessHypeLimitRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _whiteHypeLimitRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _blueHypeLimitRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _blackHypeLimitRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _redHypeLimitRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[SerializeField]
	private AnimationCurve _greenHypeLimitRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);

	[Header("Hype Contagion (Range)")]
	[SerializeField]
	private AnimationCurve _hypeDistanceRange = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 0.2f);

	[SerializeField]
	private AnimationCurve _absorbedHypeMultiplierRange = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.2f);

	[SerializeField]
	private AnimationCurve _overflowHypeMultiplierRange = AnimationCurve.EaseInOut(0f, 0.8f, 1f, 1.2f);

	[SerializeField]
	private AnimationCurve _adjacentHypeAwarenessRange = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 0.5f);

	[SerializeField]
	private float _minimumHypeFallover = 0.01f;

	public int IDLE_ANIMATION_INDEX => _idleAnimIndex;

	public int CHEER_ANIMATION_INDEX => _cheerAnimIndex;

	public float MINIMUM_HYPE_FALLOVER => _minimumHypeFallover;

	public void InitPerson(CrowdPersonBehaviour crowdPerson)
	{
		Vector3 localScale = default(Vector3);
		localScale.y = _heightCurve.Evaluate(Random.Range(0f, 1f));
		localScale.x = (localScale.z = _weightCurve.Evaluate(Random.Range(0f, 1f)));
		crowdPerson.transform.localScale = localScale;
		crowdPerson.SetSkinColor(_skinColorVariation.Evaluate(Random.Range(0f, 1f)));
		crowdPerson.SetShirtColor(_shirtColorVariation.Evaluate(Random.Range(0f, 1f)));
		crowdPerson.SetSleeveColor(_sleevesColorVariation.Evaluate(Random.Range(0f, 1f)));
		crowdPerson.SetPantsColor(_pantsColorVariation.Evaluate(Random.Range(0f, 1f)));
		crowdPerson.SetFlagColor(_flagColorVariation.Evaluate(Random.Range(0f, 1f)));
		crowdPerson.Status.GenericHypeThreshold = _genericHypeLimitRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeThreshold[CardColor.Colorless] = _colorlessHypeLimitRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeThreshold[CardColor.White] = _whiteHypeLimitRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeThreshold[CardColor.Blue] = _blueHypeLimitRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeThreshold[CardColor.Black] = _blackHypeLimitRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeThreshold[CardColor.Red] = _redHypeLimitRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeThreshold[CardColor.Green] = _greenHypeLimitRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.GenericHype = _genericStartingHypeRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeValues[CardColor.Colorless] = _colorlessStartingHypeRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeValues[CardColor.White] = _whiteStartingHypeRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeValues[CardColor.Blue] = _blueStartingHypeRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeValues[CardColor.Black] = _blackStartingHypeRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeValues[CardColor.Red] = _redStartingHypeRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeValues[CardColor.Green] = _greenStartingHypeRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeRange = _hypeDistanceRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.AbsorbedHypeMultiplier = _absorbedHypeMultiplierRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.OverflowHypeMultiplier = _overflowHypeMultiplierRange.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeRecoverySpeed = _hypeRecoverySpeed.Evaluate(Random.Range(0f, 1f));
		crowdPerson.Status.HypeAwarenessSpeed = _adjacentHypeAwarenessRange.Evaluate(Random.Range(0f, 1f));
	}

	public UnityEngine.Color GetAffinityColor(CardColor affinity)
	{
		return affinity switch
		{
			CardColor.White => _whiteAffinityGradient.Evaluate(Random.Range(0f, 1f)), 
			CardColor.Blue => _blueAffinityGradient.Evaluate(Random.Range(0f, 1f)), 
			CardColor.Black => _blackAffinityGradient.Evaluate(Random.Range(0f, 1f)), 
			CardColor.Red => _redAffinityGradient.Evaluate(Random.Range(0f, 1f)), 
			CardColor.Green => _greenAffinityGradient.Evaluate(Random.Range(0f, 1f)), 
			_ => _colorlessAffinityGradient.Evaluate(Random.Range(0f, 1f)), 
		};
	}
}
