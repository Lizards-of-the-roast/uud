using UnityEngine;

public class TextureChangeAnimation : MonoBehaviour
{
	[SerializeField]
	private Texture[] _textures;

	[SerializeField]
	private float _changeInterval = 0.016f;

	[SerializeField]
	private bool _randomizedStartIndex = true;

	private Renderer _renderer;

	private float _timeOffset;

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
		if (_renderer != null && _textures != null && _textures.Length != 0)
		{
			base.enabled = true;
			if (_randomizedStartIndex)
			{
				_timeOffset = Random.Range(0f, _changeInterval * (float)_textures.Length);
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		int num = Mathf.FloorToInt((Time.time + _timeOffset) / _changeInterval) % _textures.Length;
		_renderer.material.mainTexture = _textures[num];
	}
}
