using UnityEngine;

public class AutoRotate : MonoBehaviour
{
	[SerializeField]
	private Vector3 _rotationVector;

	[SerializeField]
	private float _speed;

	private Transform _cachedTransform;

	private void Awake()
	{
		_cachedTransform = GetComponent<Transform>();
	}

	private void Update()
	{
		_cachedTransform.Rotate(_rotationVector * Time.deltaTime * _speed);
	}
}
