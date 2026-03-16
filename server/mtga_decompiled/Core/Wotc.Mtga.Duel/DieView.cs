using System;
using System.Collections;
using AssetLookupTree;
using AssetLookupTree.Payloads.GeneralEffect;
using TMPro;
using UnityEngine;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Duel;

[RequireComponent(typeof(Transform))]
public class DieView : MonoBehaviour, IDieView, IDisposable
{
	[SerializeField]
	private GameObject _dieRootGo;

	[SerializeField]
	private Animation _rollAnimation;

	[SerializeField]
	private TMP_Text _naturalResultLabel;

	[SerializeField]
	private ParticleSystem _disappearVfx;

	private Coroutine _rollRoutine;

	private Coroutine _keepRoutine;

	private Coroutine _ignoreRoutine;

	private uint _naturalResult;

	private int _modifiedResult;

	private AssetLookupSystem _assetLookupSystem;

	private IVfxProvider _vfxProvider;

	private uint _dieFaces;

	public bool Disposed { get; private set; }

	public event Action<IDieView> RollCommencedHandlers;

	public event Action<IDieView> RollCompletedHandlers;

	public event Action<IDieView> KeepCommencedHandlers;

	public event Action<IDieView> KeepCompletedHandlers;

	public event Action<IDieView> IgnoreCommencedHandlers;

	public event Action<IDieView> IgnoreCompletedHandlers;

	public void Initialize(uint dieFaces, IVfxProvider vfxProvider, AssetLookupSystem assetLookupSystem)
	{
		_vfxProvider = vfxProvider ?? NullVfxProvider.Default;
		_assetLookupSystem = assetLookupSystem;
		_dieFaces = dieFaces;
	}

	public void Roll(uint naturalResult, int modifiedResult)
	{
		_naturalResult = naturalResult;
		_modifiedResult = modifiedResult;
		if (_rollRoutine != null)
		{
			StopCoroutine(_rollRoutine);
		}
		_rollRoutine = StartCoroutine(RollRoutine());
	}

	public void Keep()
	{
		if (_keepRoutine != null)
		{
			StopCoroutine(_keepRoutine);
		}
		_keepRoutine = StartCoroutine(KeepRoutine());
	}

	public void Ignore()
	{
		if (_ignoreRoutine != null)
		{
			StopCoroutine(_ignoreRoutine);
		}
		_ignoreRoutine = StartCoroutine(IgnoreRoutine());
	}

	public void Dispose()
	{
		if (!Disposed && (bool)this && (bool)base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator RollRoutine()
	{
		this.RollCommencedHandlers?.Invoke(this);
		_naturalResultLabel.gameObject.SetActive(value: false);
		_rollAnimation.Play();
		while (_rollAnimation.isPlaying)
		{
			yield return null;
		}
		_naturalResultLabel.text = _naturalResult.ToString();
		_naturalResultLabel.gameObject.SetActive(value: true);
		this.RollCompletedHandlers?.Invoke(this);
	}

	private IEnumerator KeepRoutine()
	{
		this.KeepCommencedHandlers?.Invoke(this);
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.DieFaces = _dieFaces;
		_assetLookupSystem.Blackboard.DieRollNaturalResult = _naturalResult;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DieRollVfx> loadedTree))
		{
			DieRollVfx payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				float num = 0f;
				foreach (VfxData vfxData in payload.VfxDatas)
				{
					if (num < vfxData.PrefabData.CleanupAfterTime)
					{
						num = vfxData.PrefabData.CleanupAfterTime;
					}
					GameObject gameObject = _vfxProvider.PlayVFX(vfxData, null, null, base.gameObject.transform);
					if ((bool)gameObject)
					{
						TMP_Text componentInChildrenEvenIfInactive = gameObject.GetComponentInChildrenEvenIfInactive<TMP_Text>();
						if ((object)componentInChildrenEvenIfInactive != null)
						{
							componentInChildrenEvenIfInactive.text = _modifiedResult.ToString();
							componentInChildrenEvenIfInactive.gameObject.SetActive(value: true);
						}
					}
				}
				yield return new WaitForSeconds(num);
			}
		}
		yield return Disappear();
		this.KeepCompletedHandlers?.Invoke(this);
	}

	private IEnumerator IgnoreRoutine()
	{
		this.IgnoreCommencedHandlers?.Invoke(this);
		yield return Disappear();
		this.IgnoreCompletedHandlers?.Invoke(this);
	}

	private IEnumerator Disappear()
	{
		yield return null;
		_dieRootGo.SetActive(value: false);
		_naturalResultLabel.gameObject.SetActive(value: false);
		_disappearVfx.gameObject.SetActive(value: true);
		_disappearVfx.Play(withChildren: true);
		while (_disappearVfx.IsAlive(withChildren: true))
		{
			yield return null;
		}
	}

	private void OnEnable()
	{
		_disappearVfx.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (_ignoreRoutine != null)
		{
			StopCoroutine(_ignoreRoutine);
			_ignoreRoutine = null;
		}
		if (_keepRoutine != null)
		{
			StopCoroutine(_keepRoutine);
			_keepRoutine = null;
		}
		if (_rollRoutine != null)
		{
			StopCoroutine(_rollRoutine);
			_rollRoutine = null;
		}
		this.IgnoreCompletedHandlers = null;
		this.IgnoreCommencedHandlers = null;
		this.KeepCompletedHandlers = null;
		this.KeepCommencedHandlers = null;
		this.RollCompletedHandlers = null;
		this.RollCommencedHandlers = null;
		Disposed = true;
	}
}
