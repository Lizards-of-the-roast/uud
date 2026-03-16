using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateSpriteSheet : MonoBehaviour
{
	public delegate void VoidEvent();

	public int _columns = 2;

	public int _rows = 2;

	public Vector2 _scale = new Vector3(1f, 1f);

	public Vector2 _offset = Vector2.zero;

	public Vector2 _buffer = Vector2.zero;

	public float _framesPerSecond = 10f;

	public bool _playOnce;

	public bool _disableUponCompletion;

	public bool _enableEvents;

	public bool _playOnEnable = true;

	private int _index;

	private Vector2 _textureSize = Vector2.zero;

	private Renderer _renderer;

	[SerializeField]
	private List<string> _textureNames = new List<string> { "_MainTex" };

	private bool _isPlaying;

	private List<VoidEvent> _voidEventCallbackList;

	private Coroutine _updateTilingCoroutine;

	public void RegisterCallback(VoidEvent cbFunction)
	{
		if (_enableEvents)
		{
			_voidEventCallbackList.Add(cbFunction);
		}
		else
		{
			Debug.LogWarning("AnimateTiledTexture: You are attempting to register a callback but the events of this object are not enabled!");
		}
	}

	public void UnRegisterCallback(VoidEvent cbFunction)
	{
		if (_enableEvents)
		{
			_voidEventCallbackList.Remove(cbFunction);
		}
		else
		{
			Debug.LogWarning("AnimateTiledTexture: You are attempting to un-register a callback but the events of this object are not enabled!");
		}
	}

	[ContextMenu("Play")]
	public void Play()
	{
		if (_isPlaying)
		{
			if (_updateTilingCoroutine != null)
			{
				StopCoroutine(_updateTilingCoroutine);
				_updateTilingCoroutine = null;
			}
			_isPlaying = false;
		}
		_renderer.enabled = true;
		_index = _columns;
		_updateTilingCoroutine = StartCoroutine(updateTiling());
	}

	public void ChangeMaterial(Material material)
	{
		_renderer.material = Object.Instantiate(material);
		CalcTextureSize();
		_textureNames.ForEach(delegate(string x)
		{
			_renderer.material.SetTextureScale(x, _textureSize);
		});
	}

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
		if (_enableEvents)
		{
			_voidEventCallbackList = new List<VoidEvent>();
		}
		CalcTextureSize();
		_textureNames.ForEach(delegate(string x)
		{
			_renderer.material.SetTextureScale(x, _textureSize);
		});
	}

	private void HandleCallbacks(List<VoidEvent> cbList)
	{
		for (int i = 0; i < cbList.Count; i++)
		{
			cbList[i]();
		}
	}

	private void OnEnable()
	{
		CalcTextureSize();
		if (_playOnEnable)
		{
			Play();
		}
	}

	private void CalcTextureSize()
	{
		_textureSize = new Vector2(1f / (float)_columns, 1f / (float)_rows);
		_textureSize.x /= _scale.x;
		_textureSize.y /= _scale.y;
		_textureSize -= _buffer;
	}

	private IEnumerator updateTiling()
	{
		_isPlaying = true;
		int checkAgainst = _rows * _columns;
		while (true)
		{
			if (_index >= checkAgainst)
			{
				_index = 0;
				if (_playOnce)
				{
					if (checkAgainst == _columns)
					{
						break;
					}
					checkAgainst = _columns;
				}
			}
			ApplyOffset();
			_index++;
			yield return new WaitForSeconds(1f / _framesPerSecond);
		}
		if (_enableEvents)
		{
			HandleCallbacks(_voidEventCallbackList);
		}
		if (_disableUponCompletion)
		{
			_renderer.enabled = false;
		}
		_isPlaying = false;
	}

	private void ApplyOffset()
	{
		float num = _index % _columns;
		float num2 = _index / _columns;
		Vector2 offset = new Vector2(num * (1f / (float)_columns), 1f - num2 * (1f / (float)_rows));
		if (offset.y == 1f)
		{
			offset.y = 0f;
		}
		offset.x += (1f / (float)_columns - _textureSize.x) / 2f;
		offset.y += (1f / (float)_rows - _textureSize.y) / 2f;
		offset.x += _offset.x;
		offset.y += _offset.y;
		_textureNames.ForEach(delegate(string x)
		{
			_renderer.material.SetTextureOffset(x, offset);
		});
		Debug.Log(_index + " -> x: " + offset.x + ", y: " + offset.y);
	}
}
