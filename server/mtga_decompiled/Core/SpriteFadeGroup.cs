using UnityEngine;

public class SpriteFadeGroup : MonoBehaviour
{
	public enum FadeState
	{
		Inactive,
		FadingIn,
		FadingOut
	}

	public Color Tint = Color.white;

	private SpriteRenderer[] _spriteRenderers;

	private Color[] _originalTints;

	public FadeState CurrentFadeState { get; set; }

	private void Awake()
	{
		_spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		_originalTints = new Color[_spriteRenderers.Length];
		for (int i = 0; i < _spriteRenderers.Length; i++)
		{
			_originalTints[i] = _spriteRenderers[i].color;
		}
	}

	private void LateUpdate()
	{
		if (CurrentFadeState != FadeState.Inactive)
		{
			for (int i = 0; i < _spriteRenderers.Length; i++)
			{
				_spriteRenderers[i].color = _originalTints[i] * Tint;
			}
		}
	}
}
