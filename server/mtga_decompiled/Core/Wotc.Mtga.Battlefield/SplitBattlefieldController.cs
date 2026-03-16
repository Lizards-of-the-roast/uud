using UnityEngine;
using Wotc.Mtga.Quality;

namespace Wotc.Mtga.Battlefield;

public class SplitBattlefieldController : MonoBehaviour
{
	[SerializeField]
	private LifeRatioUpdatedEvent _lifeRatioUpdatedEvent;

	[SerializeField]
	private Material _battlefieldMaterial;

	private Material _battlefieldMaterialInstance;

	private const float Duration = 1f;

	private float _currentValue = 0.5f;

	private float _targetValue = 0.5f;

	private float _elapsedTime;

	private bool _isAnimating;

	private float _startValue;

	private const string PropertyName = "_Split01";

	private void Awake()
	{
		_battlefieldMaterialInstance = new Material(_battlefieldMaterial);
		QualitySettingsUtil.Instance.SetHybridBattlefieldMatInstance(_battlefieldMaterialInstance);
		SetMaterialValue(_currentValue);
		_lifeRatioUpdatedEvent.LifeRatioUpdated += OnLifeRatioUpdated;
	}

	private void OnDestroy()
	{
		_lifeRatioUpdatedEvent.LifeRatioUpdated -= OnLifeRatioUpdated;
	}

	private void OnLifeRatioUpdated(float lifeRatio)
	{
		_startValue = _currentValue;
		_targetValue = lifeRatio;
		_elapsedTime = 0f;
		_isAnimating = true;
	}

	private void Update()
	{
		if (_isAnimating)
		{
			_elapsedTime += Time.deltaTime;
			float num = Mathf.Clamp01(_elapsedTime / 1f);
			float t = 1f - (1f - num) * (1f - num);
			_currentValue = Mathf.SmoothStep(_startValue, _targetValue, t);
			SetMaterialValue(_currentValue);
			if (num >= 1f)
			{
				_isAnimating = false;
				_currentValue = _targetValue;
			}
		}
	}

	private void SetMaterialValue(float value)
	{
		_battlefieldMaterialInstance.SetFloat("_Split01", value);
	}
}
