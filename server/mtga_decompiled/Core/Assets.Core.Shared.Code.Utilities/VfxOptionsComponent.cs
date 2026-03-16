using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Assets.Core.Shared.Code.Utilities;

internal class VfxOptionsComponent : MonoBehaviour
{
	private bool _isTopOfStack;

	private ParticleSystem _particleSystem;

	public bool HideIfNotTopOfStack { get; set; }

	public bool IsTopOfStack
	{
		get
		{
			return _isTopOfStack;
		}
		set
		{
			_isTopOfStack = value;
			HandleStackOptions();
		}
	}

	private void Awake()
	{
		HideIfNotTopOfStack = false;
		_particleSystem = base.gameObject.GetComponent<ParticleSystem>();
	}

	public void HandleStackOptions()
	{
		if ((bool)_particleSystem)
		{
			if (_isTopOfStack && !_particleSystem.isPlaying)
			{
				_particleSystem.Play(withChildren: true);
			}
			else if (!_isTopOfStack && _particleSystem.isPlaying)
			{
				_particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
			}
		}
		else
		{
			base.gameObject.UpdateActive(_isTopOfStack);
		}
	}
}
