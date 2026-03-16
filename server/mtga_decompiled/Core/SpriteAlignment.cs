using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAlignment : MonoBehaviour
{
	public Vector2 Alignment;

	private SpriteRenderer _spriteRenderer;

	private Vector3 _originalPosition;

	private bool _inited;

	private void OnEnable()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void OnDisable()
	{
		base.transform.localPosition = _originalPosition;
		_inited = false;
	}

	private void Update()
	{
		if (!(_spriteRenderer.sprite == null))
		{
			if (!_inited)
			{
				_inited = true;
				_originalPosition = base.transform.localPosition;
			}
			Sprite sprite = _spriteRenderer.sprite;
			Vector3 vector = sprite.bounds.size / 2f * Alignment - (Vector2)sprite.bounds.center;
			vector.Scale(base.transform.localScale);
			base.transform.localPosition = _originalPosition + vector;
		}
	}
}
