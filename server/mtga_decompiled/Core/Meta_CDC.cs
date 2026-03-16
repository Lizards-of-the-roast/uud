using AssetLookupTree;
using Core.Shared.Code.Utilities;
using GreClient.CardData;
using Pooling;
using TMPro;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class Meta_CDC : BASE_CDC
{
	[SerializeField]
	private MeshRenderer _fadeOverlay;

	[Header("Collection Info")]
	[SerializeField]
	private GameObject _collectionAnchor;

	[SerializeField]
	private GameObject _collectionCheckMark;

	[SerializeField]
	private TMP_Text _collectionText;

	private MaterialPropertyBlock _matBlock;

	public void Init(ICardDataAdapter model, bool isVisible, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, CardDatabase cardDatabase, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, AssetLookupSystem assetLookupSystem, IClientLocProvider localizationManager, IBILogger biLogger, ResourceErrorMessageManager resourceErrorMessageManager)
	{
		Init(model, isVisible, cardViewBuilder, cardMaterialBuilder, (ICardDatabaseAdapter)cardDatabase, unityObjectPool, genericObjectPool, assetLookupSystem, localizationManager, biLogger, resourceErrorMessageManager);
		_matBlock = new MaterialPropertyBlock();
		_fadeOverlay.gameObject.UpdateActive(active: false);
		_collectionAnchor.gameObject.UpdateActive(active: false);
	}

	public override void SetModel(ICardDataAdapter data, bool updateVisuals = true, CardHolderType cardHolderType = CardHolderType.None)
	{
		base.SetModel(data, updateVisuals, cardHolderType);
		if (base.Root != null && base.Root.parent != null)
		{
			base.Root.gameObject.SetLayer(base.Root.parent.gameObject.layer);
		}
	}

	public void SetDimmed(Color? color)
	{
		if (color.HasValue)
		{
			_matBlock.SetColor(ShaderPropertyIds.ColorPropId, color.Value);
			_fadeOverlay.SetPropertyBlock(_matBlock);
			_fadeOverlay.gameObject.UpdateActive(active: true);
		}
		else
		{
			_fadeOverlay.SetPropertyBlock(null);
			_fadeOverlay.gameObject.UpdateActive(active: false);
		}
	}

	public virtual void ShowCollectionInfo(bool active, int collected = 0, int max = 0)
	{
		_collectionAnchor.UpdateActive(active);
		if (active)
		{
			if (collected == max)
			{
				_collectionCheckMark.UpdateActive(active: true);
				_collectionText.transform.parent.gameObject.UpdateActive(active: false);
			}
			else
			{
				_collectionCheckMark.UpdateActive(active: false);
				_collectionText.transform.parent.gameObject.UpdateActive(active: true);
				_collectionText.SetText(collected + "/" + max);
			}
		}
	}

	internal override void OnDestroy()
	{
		if (_fadeOverlay != null)
		{
			_fadeOverlay.SetPropertyBlock(null);
		}
		_matBlock = null;
		base.OnDestroy();
	}
}
