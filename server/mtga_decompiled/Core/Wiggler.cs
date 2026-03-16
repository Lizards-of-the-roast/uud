using System;
using DG.Tweening;
using UnityEngine;

public class Wiggler : MonoBehaviour
{
	[Serializable]
	public class ShakeData
	{
		public float Time = 0.5f;

		public Vector3 Strength = Vector3.one;

		public int Vibrato = 10;
	}

	[SerializeField]
	private float _loopTime = 3f;

	[SerializeField]
	private ShakeData _shakePosition;

	[SerializeField]
	private ShakeData _shakeRotation;

	private float _currentTime;

	private void Update()
	{
		_currentTime += Time.deltaTime;
		if (_currentTime > _loopTime)
		{
			_currentTime -= _loopTime;
			base.transform.DOShakePosition(_shakePosition.Time, _shakePosition.Strength, _shakePosition.Vibrato);
			base.transform.DOShakeRotation(_shakeRotation.Time, _shakeRotation.Strength, _shakeRotation.Vibrato);
		}
	}
}
