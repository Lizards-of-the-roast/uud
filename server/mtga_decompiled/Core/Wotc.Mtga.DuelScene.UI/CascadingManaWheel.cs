using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.UI.DuelScene.ManaWheel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI;

public class CascadingManaWheel : ManaColorSelector
{
	[SerializeField]
	private bool _useVerticalPreviewOnStack;

	[Header("Containers & Objects")]
	[SerializeField]
	private CascadingManaPreview preview;

	[SerializeField]
	private Transform buttonParent;

	[SerializeField]
	private TextMeshProUGUI assignCountField;

	[SerializeField]
	private Image tapIcon;

	[Header("Configuration")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private float cascadeDelay = 0.1f;

	[Header("Animation Triggers")]
	[SerializeField]
	private string IntroAnimation = "Intro";

	[SerializeField]
	private string PulseAnimation = "Pulse";

	[SerializeField]
	private string OutroAnimation = "Outro";

	private CascadingManaWedge[] _currentWedges;

	private string[] _previewVFXPaths;

	private Dictionary<ManaColor, ManaConfig> _manaConfigPool = new Dictionary<ManaColor, ManaConfig>();

	private Dictionary<int, WedgeConfig> _wedgeConfigPool = new Dictionary<int, WedgeConfig>();

	private bool _hasInit;

	private bool _previewEnabled;

	private IEnumerator _cascadeInstantiation;

	public bool UseVerticalPreview
	{
		get
		{
			return preview.UseVerticalPreview;
		}
		set
		{
			preview.UseVerticalPreview = value;
		}
	}

	protected override void Setup()
	{
		base.Setup();
		if (base.IsOpen)
		{
			SetupWheel();
		}
	}

	private void SetupWheel(bool skipIntro = false)
	{
		DefaultMultiplierState();
		if (!_hasInit)
		{
			_previewEnabled = _selectionProvider.MaxSelections > 1 || !_selectionProvider.CurrentConstantCount.HasValue || (_selectionProvider.CurrentConstantCount.HasValue && _selectionProvider.CurrentConstantCount.Value > 1) || _selectionProvider.SelectedColors.Count() > 0;
			if (_previewEnabled)
			{
				preview.UseVerticalPreview = _useVerticalPreviewOnStack && _config.OnTheStack;
				preview.SetEnabled(_selectionProvider, _objectPool, _genericPool, GetPreviewResources());
				preview.PreviewOff();
			}
			else
			{
				preview.SetDisabled();
			}
			_hasInit = true;
		}
		else
		{
			skipIntro = true;
		}
		CleanupOldWedges();
		_currentWedges = new CascadingManaWedge[_selectionProvider.ValidSelectionCount];
		_previewVFXPaths = new string[_selectionProvider.ValidSelectionCount];
		if (!skipIntro)
		{
			animator.SetTrigger(IntroAnimation);
		}
		if (_cascadeInstantiation != null)
		{
			StopCoroutine(_cascadeInstantiation);
		}
		_cascadeInstantiation = CascadeInstantiation(skipIntro);
		StartCoroutine(_cascadeInstantiation);
	}

	private IEnumerator CascadeInstantiation(bool skipIntro)
	{
		int i = 0;
		int selectionCount = _selectionProvider.ValidSelectionCount;
		foreach (ManaProducedData selection in _selectionProvider.ValidSelections)
		{
			if (!skipIntro || cascadeDelay == 0f)
			{
				yield return new WaitForSeconds(cascadeDelay);
			}
			ManaConfig manaConfig = GetManaConfig(selection.PrimaryColor);
			if (manaConfig != null)
			{
				_previewVFXPaths[i] = manaConfig.VFXPath;
				WedgeConfig wedgeConfig = GetWedgeConfig(selectionCount);
				CascadingManaWedge cascadingManaWedge = MakeWedge(wedgeConfig);
				if (cascadingManaWedge != null)
				{
					cascadingManaWedge.SetVisuals(manaConfig.SpritePath, manaConfig.Tint, new Vector3(0f, 0f, wedgeConfig.Orientation + wedgeConfig.Increment * (float)i));
					cascadingManaWedge.SetIndexAndEvents(i, WedgeClicked, WedgeHover, WedgeNotHover);
					_currentWedges[i] = cascadingManaWedge;
				}
				if (!_selectionProvider.CurrentConstantCount.HasValue && selection.CountOfColor > 1)
				{
					_currentWedges[i].SetQuantity(selection.CountOfColor);
				}
				else
				{
					_currentWedges[i].HideQuantity();
				}
			}
			i++;
		}
		if (skipIntro || cascadeDelay == 0f)
		{
			yield return null;
		}
		_cascadeInstantiation = null;
	}

	private void WedgeHover(int index)
	{
		ManaProducedData elementAt = _selectionProvider.GetElementAt(index);
		if (_previewEnabled)
		{
			preview.PreviewHover(elementAt);
		}
		if (!_selectionProvider.CurrentConstantCount.HasValue && elementAt.CountOfColor > 1)
		{
			SetMultiplier(elementAt.CountOfColor);
		}
	}

	private void WedgeNotHover()
	{
		if (_previewEnabled)
		{
			preview.PreviewOff();
		}
		if (!_selectionProvider.CurrentConstantCount.HasValue)
		{
			DefaultMultiplierState();
		}
	}

	private void WedgeClicked(int index)
	{
		animator.SetTrigger(PulseAnimation);
		ManaProducedData elementAt = _selectionProvider.GetElementAt(index);
		if (_previewEnabled)
		{
			string vfxPath = ((_previewVFXPaths.Length > index) ? _previewVFXPaths[index] : null);
			preview.SetPreview(elementAt, vfxPath);
		}
		OnClicked();
		SelectColor(elementAt.PrimaryColor);
	}

	private void DefaultMultiplierState()
	{
		if (_selectionProvider.CurrentConstantCount.HasValue && _selectionProvider.CurrentConstantCount.Value > 1)
		{
			SetMultiplier(_selectionProvider.CurrentConstantCount.Value);
			return;
		}
		assignCountField.gameObject.UpdateActive(active: false);
		tapIcon.gameObject.UpdateActive(_selectionProvider.WillTap);
	}

	private void SetMultiplier(uint multiplier)
	{
		tapIcon.gameObject.UpdateActive(active: false);
		assignCountField.text = "x" + multiplier;
		assignCountField.gameObject.UpdateActive(active: true);
	}

	private CascadingManaWedge MakeWedge(WedgeConfig wedgeConfig)
	{
		if (wedgeConfig != null)
		{
			GameObject gameObject = _objectPool.PopObject(wedgeConfig.PrefabPath, buttonParent);
			gameObject.transform.ZeroOut();
			if (gameObject != null)
			{
				CascadingManaWedge component = gameObject.GetComponent<CascadingManaWedge>();
				if (component != null)
				{
					return component;
				}
			}
		}
		return null;
	}

	private void CleanupOldWedges()
	{
		if (_currentWedges == null || _currentWedges.Length == 0)
		{
			return;
		}
		CascadingManaWedge[] currentWedges = _currentWedges;
		foreach (CascadingManaWedge cascadingManaWedge in currentWedges)
		{
			if (cascadingManaWedge != null)
			{
				_objectPool.PushObject(cascadingManaWedge.gameObject);
			}
		}
	}

	public override void CloseSelector()
	{
		Cleanup();
		animator.SetTrigger(OutroAnimation);
		base.CloseSelector();
	}

	protected override void Cleanup()
	{
		base.Cleanup();
		_hasInit = false;
		_manaConfigPool.Clear();
		_wedgeConfigPool.Clear();
		CleanupOldWedges();
		if (_previewEnabled)
		{
			preview.Cleanup();
			_previewEnabled = false;
		}
		if ((bool)animator)
		{
			animator.ResetTrigger(IntroAnimation);
			animator.ResetTrigger(OutroAnimation);
			animator.ResetTrigger(PulseAnimation);
		}
		if (_cascadeInstantiation != null)
		{
			StopCoroutine(_cascadeInstantiation);
		}
	}

	private ManaConfig GetManaConfig(ManaColor color)
	{
		if (_manaConfigPool.ContainsKey(color))
		{
			return _manaConfigPool[color];
		}
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ManaConfig> loadedTree))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.ManaColor = color;
			ManaConfig payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_manaConfigPool.Add(color, payload);
				return payload;
			}
			_assetLookupSystem.Blackboard.Clear();
		}
		return null;
	}

	private WedgeConfig GetWedgeConfig(int colorCount)
	{
		if (_wedgeConfigPool.ContainsKey(colorCount))
		{
			return _wedgeConfigPool[colorCount];
		}
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<WedgeConfig> loadedTree))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.ManaSelectionCount = colorCount;
			WedgeConfig payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_wedgeConfigPool.Add(colorCount, payload);
				return payload;
			}
			_assetLookupSystem.Blackboard.Clear();
		}
		return null;
	}

	private PoolPreviewResources GetPreviewResources()
	{
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PoolPreviewResources> loadedTree))
		{
			_assetLookupSystem.Blackboard.Clear();
			PoolPreviewResources payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				return payload;
			}
		}
		return null;
	}

	protected override void OpenSelector(IManaSelectorProvider provider, Vector2 screenPosition, ManaColorSelectorConfig config, Action<IReadOnlyCollection<ManaColor>> callback)
	{
		base.OpenSelector(provider, screenPosition, config, callback);
		if (provider.ValidSelectionCount <= 1)
		{
			WedgeClicked(0);
		}
	}
}
