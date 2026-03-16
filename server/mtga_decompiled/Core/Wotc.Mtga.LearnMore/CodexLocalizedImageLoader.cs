using AssetLookupTree.Payloads;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.LearnMore;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(LayoutElement))]
public class CodexLocalizedImageLoader : UIBehaviour
{
	[SerializeField]
	private string _altImageKey;

	[SerializeField]
	[Tooltip("Enable if you'd like to control all settings of the image. (i.e. Layout Element preferred height and Image Preserve Aspect Ratio settings)")]
	private bool _manualControl;

	private RectTransform _rectTransform;

	private Image _image;

	private LayoutElement _layoutElement;

	private RectTransform _parentRectTransform;

	protected override void Awake()
	{
		_image = GetComponent<Image>();
		_layoutElement = GetComponent<LayoutElement>();
		_rectTransform = GetComponent<RectTransform>();
	}

	protected override void Start()
	{
		_parentRectTransform = _rectTransform.parent.GetComponent<RectTransform>();
		_image.preserveAspect = !_manualControl;
		SetLocalizedSprite();
		Languages.LanguageChangedSignal.Listeners += SetLocalizedSprite;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Languages.LanguageChangedSignal.Listeners -= SetLocalizedSprite;
	}

	private void SetLocalizedSprite()
	{
		AssetLookupManager assetLookupManager = Pantry.Get<AssetLookupManager>();
		assetLookupManager.AssetLookupSystem.Blackboard.Clear();
		assetLookupManager.AssetLookupSystem.Blackboard.TextureName = _altImageKey;
		Sprite objectData = AssetLoader.GetObjectData<Sprite>(assetLookupManager.AssetLookupSystem.TreeLoader.LoadTree<CodexSpritePayload>().GetPayload(assetLookupManager.AssetLookupSystem.Blackboard).Reference.RelativePath);
		_image.sprite = objectData;
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		_parentRectTransform = _rectTransform.parent.GetComponent<RectTransform>();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		if (!(_image == null) && !(_layoutElement == null) && !_manualControl)
		{
			float num = _image.sprite.rect.width / _image.sprite.rect.height;
			_layoutElement.preferredHeight = _parentRectTransform.rect.width / num;
		}
	}
}
