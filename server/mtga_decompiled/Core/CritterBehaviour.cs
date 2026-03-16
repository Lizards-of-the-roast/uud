using System.Collections;
using DG.Tweening;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga;

public class CritterBehaviour : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public enum CritterType
	{
		splineCritter,
		animatedMotionCritter,
		stationaryCritter
	}

	private enum ReactSize
	{
		small,
		medium
	}

	public CritterType _critterType;

	public Animator _animator;

	public float _skitterTimeout = 4f;

	public float _animationSpeedModifier;

	public float _skitterSpeedMultiplier;

	public float _skitterDuration = 1f;

	public float _squishMultiplier;

	public int _vibrato = 1;

	public float _elasticity = 0.5f;

	public float _punchTime = 1f;

	public bool mediumClick;

	public ParticleSystem _smallClickParticles;

	public ParticleSystem _mediumClickParticles;

	public string _smallReactAudio = "";

	public float _smallReactAudiodelay;

	public string _mediumReactAudio = "";

	public float _mediumReactAudiodelay;

	public string _skitterAudio = "";

	public float _skitterReactAudiodelay;

	public int minMediumOccurance = 3;

	public int maxMediumOccurance = 8;

	public float smallClickParticleDelay;

	public float mediumClickParticleDelay;

	private CritterManager _manager;

	private bool _skittering;

	private bool smallClickParticleDelayOn;

	private bool mediumClickParticleDelayOn;

	private Animation anim;

	private float instanceSpeed;

	private float _skitterTimer;

	private int _mediumReactionValue = 3;

	private int _clickCount;

	private SnowAccumulation[] snowAccumulator;

	private ReactSize reactSize;

	private IUnityObjectPool _objectPool;

	[HideInInspector]
	public CritterManager Manager
	{
		get
		{
			if (_manager == null)
			{
				_manager = Object.FindObjectOfType<CritterManager>();
			}
			return _manager;
		}
		set
		{
			_manager = value;
		}
	}

	private void Start()
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
		snowAccumulator = base.gameObject.GetComponentsInChildren<SnowAccumulation>();
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		if (mediumClick)
		{
			reactSize = ((_clickCount == _mediumReactionValue) ? ReactSize.medium : ReactSize.small);
		}
		bool flag = false;
		flag = SharedClickBehaviour() || flag;
		if (_critterType == CritterType.splineCritter)
		{
			flag = SplineCritterClickBehaviour() || flag;
		}
		if (_critterType == CritterType.animatedMotionCritter)
		{
			flag = AnimatedMotionCritterClick() || flag;
		}
		if (_critterType == CritterType.stationaryCritter)
		{
			flag = StationaryCritterClick() || flag;
		}
		if (flag)
		{
			_clickCount++;
			if (reactSize == ReactSize.medium && mediumClick)
			{
				_mediumReactionValue = _clickCount + Random.Range(minMediumOccurance, maxMediumOccurance);
			}
		}
	}

	private bool SharedClickBehaviour()
	{
		bool result = false;
		if (_squishMultiplier > 0f && base.transform.localScale.x < 1.5f && base.transform.localScale.x > 0.8f)
		{
			DOTween.Punch(() => base.transform.localScale, delegate(Vector3 x)
			{
				base.transform.localScale = x;
			}, Vector3.one * _squishMultiplier, _punchTime, _vibrato, _elasticity);
			result = true;
		}
		if (snowAccumulator != null)
		{
			SnowAccumulation[] array = snowAccumulator;
			for (int num = 0; num < array.Length; num++)
			{
				array[num].RemoveSnow();
			}
		}
		return result;
	}

	private bool SplineCritterClickBehaviour()
	{
		bool flag = false;
		if (reactSize == ReactSize.medium)
		{
			flag = TryPlayMediumAnimator() || flag;
			flag = TryPlayMediumParticles() || flag;
			flag = TryPlayMediumAudio() || flag;
		}
		else
		{
			flag = TryPlaySmallAnimator() || flag;
			flag = TryPlaySmallParticles() || flag;
			flag = TryPlaySmallAudio() || flag;
		}
		Skitter();
		return flag;
	}

	private bool AnimatedMotionCritterClick()
	{
		bool flag = false;
		if (reactSize == ReactSize.medium)
		{
			flag = TryPlayMediumParticles() || flag;
			flag = TryPlayMediumAudio() || flag;
		}
		else
		{
			flag = TryPlaySmallParticles() || flag;
			flag = TryPlaySmallAudio() || flag;
		}
		Skitter();
		return flag;
	}

	private bool StationaryCritterClick()
	{
		if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			return false;
		}
		bool flag = false;
		_animator.SetInteger("ReactionIndex", Random.Range(0, 100));
		if (reactSize == ReactSize.medium)
		{
			flag = TryPlayMediumAnimator() || flag;
			flag = TryPlayMediumParticles() || flag;
			flag = TryPlayMediumAudio() || flag;
		}
		else
		{
			flag = TryPlaySmallAnimator() || flag;
			flag = TryPlaySmallParticles() || flag;
			flag = TryPlaySmallAudio() || flag;
		}
		if (flag)
		{
			Manager.AllSkitter();
		}
		return flag;
	}

	private bool TryPlaySmallParticles()
	{
		if (null != _smallClickParticles && !smallClickParticleDelayOn)
		{
			_smallClickParticles.Play();
			if (smallClickParticleDelay != 0f)
			{
				smallClickParticleDelayOn = true;
				StartCoroutine(SmallParticleDelay(smallClickParticleDelay));
			}
			return true;
		}
		return false;
	}

	private bool TryPlayMediumParticles()
	{
		if (null != _mediumClickParticles && !mediumClickParticleDelayOn)
		{
			_mediumClickParticles.Play();
			if (mediumClickParticleDelay != 0f)
			{
				mediumClickParticleDelayOn = true;
				StartCoroutine(MediumParticleDelay(mediumClickParticleDelay));
			}
			return true;
		}
		return false;
	}

	private bool TryPlaySmallAudio()
	{
		if (!string.IsNullOrEmpty(_smallReactAudio))
		{
			AudioManager.PlayAudio(_smallReactAudio, base.gameObject, _smallReactAudiodelay);
			return true;
		}
		return false;
	}

	private bool TryPlayMediumAudio()
	{
		if (string.Empty != _mediumReactAudio && WwiseEvents.GetEventByName(_mediumReactAudio) != null)
		{
			AudioManager.PlayAudio(_mediumReactAudio, base.gameObject, _mediumReactAudiodelay);
			return true;
		}
		return false;
	}

	private bool TryPlaySmallAnimator()
	{
		if (_animator != null && !smallClickParticleDelayOn)
		{
			_animator.SetTrigger("ReactSmall");
			return true;
		}
		return false;
	}

	private bool TryPlayMediumAnimator()
	{
		if (_animator != null && !mediumClickParticleDelayOn)
		{
			_animator.SetTrigger("ReactMedium");
			return true;
		}
		return false;
	}

	public void Skitter()
	{
		if (_critterType == CritterType.splineCritter)
		{
			_skittering = true;
			_skitterTimer = _skitterDuration;
			if (null != anim)
			{
				anim[anim.clip.name].speed = _animationSpeedModifier * instanceSpeed * _skitterSpeedMultiplier;
			}
		}
		else if (_critterType == CritterType.animatedMotionCritter && !(_animator == null))
		{
			_animator.SetTrigger("Skitter");
			StartCoroutine(CleanupAfterTime(4f));
		}
	}

	public IEnumerator AnimBehave()
	{
		yield return new WaitForSeconds(_skitterTimeout);
		Skitter();
	}

	public IEnumerator SplineBehave(SplineMovementData splineData, float speed)
	{
		anim = base.gameObject.GetComponent<Animation>();
		if (null != anim)
		{
			instanceSpeed = speed;
			anim[anim.clip.name].speed = speed * _animationSpeedModifier;
		}
		float lerpTimer = 0f;
		while (lerpTimer < 1f)
		{
			if (_skittering)
			{
				_skitterTimer -= Time.deltaTime;
				float num = speed * Mathf.Lerp(1f, _skitterSpeedMultiplier, _skitterTimer / _skitterDuration);
				lerpTimer += Time.deltaTime * num;
				if (null != anim)
				{
					anim[anim.clip.name].speed = _animationSpeedModifier * num;
				}
				if (_skitterTimer <= 0f)
				{
					_skittering = false;
					if (null != anim)
					{
						anim[anim.clip.name].speed = speed * _animationSpeedModifier;
					}
				}
			}
			else
			{
				lerpTimer += Time.deltaTime * speed;
			}
			if (_squishMultiplier > 0f && base.transform.localScale.x != 1f)
			{
				base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.one, Time.deltaTime);
			}
			splineData.PlaceOnCurveEased(base.transform, lerpTimer, splineData.Spline.Start.Position, splineData.Spline.End.Position);
			yield return null;
		}
		Manager.activeCritters.Remove(this);
		ReturnToPool();
	}

	private IEnumerator CleanupAfterTime(float secs)
	{
		yield return new WaitForSeconds(secs);
		if ((bool)Manager)
		{
			Manager.activeCritters.Remove(this);
		}
		ReturnToPool();
	}

	private void ReturnToPool()
	{
		if (_objectPool != null)
		{
			_objectPool.PushObject(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator SmallParticleDelay(float secs)
	{
		yield return new WaitForSeconds(secs);
		smallClickParticleDelayOn = false;
	}

	private IEnumerator MediumParticleDelay(float secs)
	{
		yield return new WaitForSeconds(secs);
		mediumClickParticleDelayOn = false;
	}
}
