using UnityEngine;

[ExecuteInEditMode]
public class ColliderScaler : MonoBehaviour
{
	private Transform _transform;

	private RectTransform _parent;

	private void Awake()
	{
		_transform = GetComponent<Transform>();
		_parent = _transform.parent.GetComponent<RectTransform>();
	}

	private void Update()
	{
		_transform.localScale = new Vector3(_parent.rect.size.x, _parent.rect.size.y, 1f);
	}
}
