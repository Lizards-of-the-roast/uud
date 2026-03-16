using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Pooling;
using TMPro;
using UnityEngine;

namespace Assets.Core.Meta.LearnMore;

public class LearnToPlayContents : MonoBehaviour
{
	[SerializeField]
	private CustomButton backButton;

	[SerializeField]
	private TMP_Text indexText;

	[SerializeField]
	private string indexTextSeparator;

	[SerializeField]
	private GameObject contentsContainer;

	private IUnityObjectPool _objectPool = NullUnityObjectPool.Default;

	private RectTransformCopyInfo? _originalTransform;

	private RectTransformCopyInfo? _originalContentTransform;

	private GameObject _contentsObj;

	public event Action BackButtonClicked;

	public void OnEnable()
	{
		backButton.OnClick.AddListener(OnBackButtonClicked);
	}

	public void OnDisable()
	{
		backButton.OnClick.RemoveListener(OnBackButtonClicked);
	}

	public void LocalizeIndexBreadcrumbs(string[] pathTitles)
	{
		string sourceText = string.Join(indexTextSeparator, pathTitles);
		indexText.SetText(sourceText);
	}

	public void Init(string contentName, AssetLookupSystem assetLookupSystem, IUnityObjectPool objectPool)
	{
		_objectPool = objectPool ?? NullUnityObjectPool.Default;
		_contentsObj = InstantiateContentObject(contentName, assetLookupSystem);
		RectTransform component = GetComponent<RectTransform>();
		if (!_originalTransform.HasValue)
		{
			_originalTransform = RectTransformCopyInfo.FromTransform(component);
		}
		else
		{
			_originalTransform.Value.ApplyToTransform(component);
		}
		RectTransform component2 = _contentsObj.gameObject.GetComponent<RectTransform>();
		if (!_originalContentTransform.HasValue)
		{
			RectTransformCopyInfo valueOrDefault = _originalContentTransform.GetValueOrDefault();
			if (!_originalContentTransform.HasValue)
			{
				valueOrDefault = RectTransformCopyInfo.FromTransform(component2);
				_originalContentTransform = valueOrDefault;
			}
		}
		else
		{
			_originalContentTransform.Value.ApplyToTransform(component2);
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnBackButtonClicked()
	{
		this.BackButtonClicked?.Invoke();
	}

	private GameObject InstantiateContentObject(string contentName, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LearnMoreSectionContentName = contentName;
		string assetPath = assetLookupSystem.TreeLoader.LoadTree<LearnToPlaySectionContent>(returnNewTree: false).GetPayload(assetLookupSystem.Blackboard)?.PrefabPath;
		return _objectPool.PopObject(assetPath, contentsContainer.transform);
	}

	public void DeInit()
	{
		if (_objectPool != null && _contentsObj != null)
		{
			_objectPool.PushObject(_contentsObj.gameObject, worldPositionStays: true);
			_contentsObj = null;
		}
		this.BackButtonClicked = null;
	}

	public void OnDestroy()
	{
		DeInit();
		_objectPool = NullUnityObjectPool.Default;
		_originalTransform = null;
		_originalContentTransform = null;
	}
}
