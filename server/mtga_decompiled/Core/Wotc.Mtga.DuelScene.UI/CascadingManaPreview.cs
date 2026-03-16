using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Payloads.UI.DuelScene.ManaWheel;
using Pooling;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI;

public class CascadingManaPreview : MonoBehaviour
{
	public bool UseVerticalPreview;

	[SerializeField]
	private float _vfxCleanupTimer = 1f;

	[Header("Layout")]
	[SerializeField]
	private RectTransform mainLayout;

	[SerializeField]
	private Transform backingLayout;

	[SerializeField]
	private RectTransform mainLayoutVertical;

	[SerializeField]
	private Transform backingLayoutVertical;

	[SerializeField]
	private int singleIconPreviewThreshold = 5;

	[SerializeField]
	private GameObject rowBacking;

	[SerializeField]
	private int iconsPerRow;

	[SerializeField]
	private float spacing;

	[Header("Progress behavior:")]
	[SerializeField]
	private Image progressFill;

	[SerializeField]
	private AnimationCurve progressFade;

	[SerializeField]
	private AnimationCurve progressMove;

	[SerializeField]
	private float progressDuration;

	private readonly List<GameObject> _rowBackings = new List<GameObject>();

	private readonly List<List<CascadingManaPreviewWidget>> _layoutList = new List<List<CascadingManaPreviewWidget>>();

	private readonly List<CascadingManaPreviewWidget> _extraPreviews = new List<CascadingManaPreviewWidget>();

	private List<CascadingManaPreviewWidget>[] _previews;

	private IManaSelectorProvider _provider;

	private IUnityObjectPool _objectPool;

	private IObjectPool _genericPool;

	private GameObject[] _previewVFX;

	private PoolPreviewResources _resources;

	private Sprite _nullIcon;

	private AssetLoader.AssetTracker<Sprite> _nullIconTracker = new AssetLoader.AssetTracker<Sprite>("CascadeManaPreviewNullIcon");

	private ManaPoolSpriteTable _spriteTable;

	private int _extraColorPreview;

	private int _selectionsToHide;

	private IEnumerator _decisionIntro;

	private IEnumerator _progress;

	public void SetEnabled(IManaSelectorProvider provider, IUnityObjectPool objectPool, IObjectPool genericPool, PoolPreviewResources resources)
	{
		_provider = provider;
		_objectPool = objectPool;
		_genericPool = genericPool;
		_resources = resources;
		_spriteTable = GetSpriteTable(_resources);
		_nullIcon = GetNullIcon(_resources);
		if (provider.SelectedColors.Count() > 0)
		{
			foreach (ManaColor selectedColor in provider.SelectedColors)
			{
				CascadingManaPreviewWidget cascadingManaPreviewWidget = MakeWidget();
				if (cascadingManaPreviewWidget != null)
				{
					_extraPreviews.Add(cascadingManaPreviewWidget);
					cascadingManaPreviewWidget.Set(GetSprite(selectedColor), null);
				}
			}
		}
		_previews = new List<CascadingManaPreviewWidget>[provider.MaxSelections];
		for (int i = 0; i < provider.MaxSelections; i++)
		{
			_previews[i] = _genericPool.PopObject<List<CascadingManaPreviewWidget>>();
			uint num = provider.GetConstantCountForSelection(i) ?? 1;
			if (num > singleIconPreviewThreshold)
			{
				CascadingManaPreviewWidget cascadingManaPreviewWidget2 = MakeWidget();
				cascadingManaPreviewWidget2.SetQuantity(num);
				_previews[i].Add(cascadingManaPreviewWidget2);
				continue;
			}
			for (uint num2 = 0u; num2 < num; num2++)
			{
				CascadingManaPreviewWidget cascadingManaPreviewWidget3 = MakeWidget();
				if (cascadingManaPreviewWidget3 != null)
				{
					_previews[i].Add(cascadingManaPreviewWidget3);
				}
			}
		}
		LayoutPreviews();
		if (_decisionIntro != null)
		{
			StopCoroutine(_decisionIntro);
		}
		_decisionIntro = DecisionCascadeAnimation();
		StartCoroutine(_decisionIntro);
		progressFill.fillAmount = 1f;
		ToggleFill(visible: true);
	}

	private void ToggleFill(bool visible)
	{
		progressFill.gameObject.UpdateActive(visible);
	}

	public void SetDisabled()
	{
		progressFill.gameObject.UpdateActive(active: false);
	}

	public void SetPreview(ManaColorSelector.ManaProducedData colorData, string vfxPath)
	{
		Sprite sprite = GetSprite(colorData.PrimaryColor);
		int count = _previews[_provider.CurrentSelection].Count;
		_previewVFX = new GameObject[count];
		for (int i = 0; i < count; i++)
		{
			_previewVFX[i] = GetPreviewVFX(vfxPath);
			if ((bool)_previewVFX[i])
			{
				Invoke("VFXCleanup", _vfxCleanupTimer);
			}
			_previews[_provider.CurrentSelection][i].Set(sprite, _previewVFX[i]);
		}
		if (_progress != null)
		{
			StopCoroutine(_progress);
		}
		_progress = ProgressUpdate(_provider.CurrentSelection, 1);
		StartCoroutine(_progress);
	}

	public void PreviewOff(int? decisionIndex = null)
	{
		int num = decisionIndex ?? _provider.CurrentSelection;
		bool flag = false;
		if (_previews[num].Count == 1)
		{
			_previews[num][0].SetQuantity(0u);
		}
		if (_extraColorPreview > 0)
		{
			for (int i = 0; i < _extraColorPreview; i++)
			{
				RemoveLastPreview(num);
			}
			flag = true;
			_extraColorPreview = 0;
		}
		if (_selectionsToHide != 0)
		{
			for (int j = 1; j <= _selectionsToHide; j++)
			{
				SetSelectionVisibility(_previews.Length - j, visibility: true);
			}
			flag = true;
			ToggleFill(visible: true);
			_selectionsToHide = 0;
		}
		if (flag)
		{
			LayoutPreviews();
		}
		foreach (CascadingManaPreviewWidget item in _previews[num])
		{
			item.Highlight();
		}
	}

	public void PreviewHover(ManaColorSelector.ManaProducedData manaData)
	{
		int currentSelection = _provider.CurrentSelection;
		Sprite sprite = GetSprite(manaData);
		bool flag = false;
		if (!_provider.GetConstantCountForSelection(currentSelection).HasValue)
		{
			if (manaData.CountOfColor > singleIconPreviewThreshold)
			{
				_previews[currentSelection][0].SetQuantity(manaData.CountOfColor);
			}
			else
			{
				_previews[currentSelection][0].SetQuantity(0u);
				for (int i = 0; i < manaData.CountOfColor - 1; i++)
				{
					_extraColorPreview++;
					AddPreview(currentSelection, sprite);
				}
				flag = true;
			}
		}
		else
		{
			_extraColorPreview = 0;
		}
		uint? branchingSelectionCount = _provider.GetBranchingSelectionCount(currentSelection, manaData.PrimaryColor);
		if (branchingSelectionCount.HasValue)
		{
			_selectionsToHide = (int)(_provider.MaxSelections - branchingSelectionCount.Value);
			for (int j = 1; j <= _selectionsToHide; j++)
			{
				SetSelectionVisibility(_previews.Length - j, visibility: false);
			}
			if (currentSelection == 0 && _provider.MaxSelections - _selectionsToHide == 1)
			{
				ToggleFill(visible: false);
			}
			flag = true;
		}
		foreach (CascadingManaPreviewWidget item in _previews[currentSelection])
		{
			item.Preview(sprite);
		}
		if (flag)
		{
			LayoutPreviews();
		}
	}

	private void SetSelectionVisibility(int index, bool visibility)
	{
		foreach (CascadingManaPreviewWidget item in _previews[index])
		{
			item.gameObject.UpdateActive(visibility);
		}
	}

	private void AddPreview(int index, Sprite icon = null)
	{
		CascadingManaPreviewWidget cascadingManaPreviewWidget = MakeWidget();
		_previews[index].Add(cascadingManaPreviewWidget);
		if (icon != null)
		{
			cascadingManaPreviewWidget.AutoPreview(icon);
		}
	}

	private void RemoveLastPreview(int index)
	{
		if (_previews[index] != null && _previews[index].Count != 0 && index >= 0 && index < _previews.Length)
		{
			int index2 = _previews[index].Count - 1;
			_objectPool.PushObject(_previews[index][index2].gameObject);
			_previews[index].RemoveAt(index2);
		}
	}

	private void LayoutPreviews()
	{
		_layoutList.Clear();
		int countInRow = 0;
		int currentRowIndex = 0;
		_layoutList.Add(_genericPool.PopObject<List<CascadingManaPreviewWidget>>());
		LayoutPreviewList(_extraPreviews, countInRow, ref currentRowIndex);
		List<CascadingManaPreviewWidget>[] previews = _previews;
		foreach (List<CascadingManaPreviewWidget> previews2 in previews)
		{
			LayoutPreviewList(previews2, countInRow, ref currentRowIndex);
		}
		while (_rowBackings.Count > _layoutList.Count)
		{
			_objectPool.PushObject(_rowBackings[_rowBackings.Count - 1]);
			int index = _layoutList.Count - 1;
			_genericPool.PushObject(_layoutList[index]);
			_layoutList.RemoveAt(index);
		}
		while (_rowBackings.Count < _layoutList.Count)
		{
			GameObject rowBacker = GetRowBacker();
			_rowBackings.Add(rowBacker);
		}
		if (!UseVerticalPreview)
		{
			for (int j = 0; j < _layoutList.Count; j++)
			{
				int count = _layoutList[j].Count;
				float size = spacing * (float)count;
				RectTransform obj = (RectTransform)_rowBackings[j].transform;
				obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
				obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, spacing);
				obj.anchoredPosition = new Vector2(0f, spacing * (float)j);
				float num = (0f - spacing * (float)(count - 1)) / 2f;
				for (int k = 0; k < count; k++)
				{
					(_layoutList[j][k].transform as RectTransform).anchoredPosition = new Vector2(num + spacing * (float)k, (float)j * spacing);
				}
			}
			return;
		}
		for (int l = 0; l < _layoutList.Count; l++)
		{
			int count2 = _layoutList[l].Count;
			float size2 = spacing * (float)count2;
			RectTransform obj2 = (RectTransform)_rowBackings[l].transform;
			obj2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, spacing);
			obj2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size2);
			obj2.anchoredPosition = new Vector2(spacing * (float)l, 0f);
			float num2 = spacing * (float)(count2 - 1) / 2f;
			for (int m = 0; m < count2; m++)
			{
				(_layoutList[l][m].transform as RectTransform).anchoredPosition = new Vector2((float)(-l) * spacing, num2 + spacing * (float)(-m));
			}
		}
	}

	private void LayoutPreviewList(List<CascadingManaPreviewWidget> previews, int countInRow, ref int currentRowIndex)
	{
		List<CascadingManaPreviewWidget> list = _layoutList[currentRowIndex];
		foreach (CascadingManaPreviewWidget preview in previews)
		{
			if (countInRow >= iconsPerRow)
			{
				currentRowIndex = _layoutList.Count;
				list = _genericPool.PopObject<List<CascadingManaPreviewWidget>>();
				_layoutList.Add(list);
				countInRow = 0;
			}
			if (preview.gameObject.activeSelf)
			{
				list.Add(preview);
				countInRow++;
			}
		}
	}

	private IEnumerator DecisionCascadeAnimation()
	{
		List<CascadingManaPreviewWidget> list = _genericPool.PopObject<List<CascadingManaPreviewWidget>>();
		list.AddRange(_extraPreviews);
		List<CascadingManaPreviewWidget>[] previews = _previews;
		foreach (List<CascadingManaPreviewWidget> collection in previews)
		{
			list.AddRange(collection);
		}
		foreach (CascadingManaPreviewWidget item in list)
		{
			item.Intro();
			yield return new WaitForEndOfFrame();
		}
		_decisionIntro = null;
	}

	private Sprite GetSprite(ManaColorSelector.ManaProducedData manaData)
	{
		return GetSprite(manaData.PrimaryColor);
	}

	private Sprite GetSprite(ManaColor color)
	{
		Sprite sprite = _spriteTable.GetSpritesForColor(color).Default(HighlightType.None);
		if (sprite != null)
		{
			return sprite;
		}
		return _nullIcon;
	}

	private CascadingManaPreviewWidget MakeWidget()
	{
		if (_resources != null && !string.IsNullOrEmpty(_resources.WidgetPath))
		{
			GameObject gameObject = _objectPool.PopObject(_resources.WidgetPath, UseVerticalPreview ? mainLayoutVertical : mainLayout);
			gameObject.transform.ZeroOut();
			if (gameObject != null)
			{
				CascadingManaPreviewWidget component = gameObject.GetComponent<CascadingManaPreviewWidget>();
				component.ResetWidget();
				if (component != null)
				{
					return component;
				}
			}
		}
		return null;
	}

	private ManaPoolSpriteTable GetSpriteTable(PoolPreviewResources resources)
	{
		ManaPoolSpriteTable objectData = AssetLoader.GetObjectData(resources.SpriteTableRef);
		if ((object)objectData != null)
		{
			return objectData;
		}
		return null;
	}

	private Sprite GetNullIcon(PoolPreviewResources resources)
	{
		string nullIconPath = resources.NullIconPath;
		return _nullIconTracker.Acquire(nullIconPath);
	}

	private GameObject GetPreviewVFX(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			GameObject gameObject = _objectPool.PopObject(path);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	private void VFXCleanup()
	{
		if (_previewVFX != null && _previewVFX.Length != 0)
		{
			GameObject[] previewVFX = _previewVFX;
			foreach (GameObject instance in previewVFX)
			{
				_objectPool.PushObject(instance);
			}
			_previewVFX = null;
		}
	}

	private GameObject GetRowBacker()
	{
		if (_resources != null && !string.IsNullOrEmpty(_resources.RowBackingPath))
		{
			GameObject gameObject = _objectPool.PopObject(_resources.RowBackingPath, UseVerticalPreview ? backingLayoutVertical : backingLayout);
			gameObject.transform.ZeroOut();
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	public void Cleanup()
	{
		VFXCleanup();
		_nullIconTracker.Cleanup();
		if (_previews != null && _previews.Length != 0)
		{
			List<CascadingManaPreviewWidget>[] previews = _previews;
			foreach (List<CascadingManaPreviewWidget> list in previews)
			{
				foreach (CascadingManaPreviewWidget item in list)
				{
					if (item != null)
					{
						item.ResetWidget();
						_objectPool.PushObject(item.gameObject);
					}
				}
				_genericPool.PushObject(list);
			}
			_previews = null;
		}
		if (_extraPreviews != null && _extraPreviews.Count > 0)
		{
			foreach (CascadingManaPreviewWidget extraPreview in _extraPreviews)
			{
				_objectPool.PushObject(extraPreview.gameObject);
			}
			_extraPreviews.Clear();
		}
		if (_layoutList != null && _layoutList.Count > 0)
		{
			foreach (List<CascadingManaPreviewWidget> layout in _layoutList)
			{
				_genericPool.PushObject(layout);
			}
			_layoutList.Clear();
		}
		if (_rowBackings != null && _rowBackings.Count > 0)
		{
			foreach (GameObject rowBacking in _rowBackings)
			{
				_objectPool.PushObject(rowBacking);
			}
			_rowBackings.Clear();
		}
		if (_progress != null)
		{
			StopCoroutine(_progress);
			_progress = null;
		}
		_extraColorPreview = 0;
		_provider = null;
		_spriteTable = null;
		_nullIcon = null;
		_resources = null;
	}

	private IEnumerator ProgressUpdate(int firstIndex, int change)
	{
		float firstValue = 1f - (float)firstIndex / (float)_previews.Length;
		float lastValue = 1f - (float)(firstIndex + change) / (float)_previews.Length;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime / progressDuration;
			progressFill.fillAmount = Mathf.Lerp(firstValue, lastValue, progressMove.Evaluate(t));
			UnityEngine.Color color = progressFill.color;
			color.a = progressFade.Evaluate(t);
			progressFill.color = color;
			yield return new WaitForEndOfFrame();
		}
	}
}
