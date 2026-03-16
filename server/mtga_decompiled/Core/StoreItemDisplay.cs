using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Wrapper;

public class StoreItemDisplay : MonoBehaviour
{
	[FormerlySerializedAs("WideBase")]
	[SerializeField]
	[Header("Set to use wider base prefab for this Item")]
	private bool _wideBase;

	[SerializeField]
	[Range(0f, 2000f)]
	[Header("Value is added to base prefab width, ONLY when WideBase is set")]
	private int _wideBaseExtraWidth;

	[Space(10f)]
	public List<StoreCardView> CardViews;

	private Animator _animator;

	private List<Renderer> _renderers;

	private Material _originalBoosterMaterial;

	private List<int> _collationIds;

	private Image _image;

	private AssetLoader.AssetTracker<Sprite> _imageSpriteTracker;

	private readonly List<Material> _trackedMaterials = new List<Material>();

	public Animator Animator
	{
		get
		{
			if (_animator == null)
			{
				_animator = GetComponent<Animator>();
			}
			return _animator;
		}
	}

	public bool WideBase => _wideBase;

	public int WideBaseExtraWidth => _wideBaseExtraWidth;

	private void OnEnable()
	{
		SetCollationIds(_collationIds);
	}

	public virtual void SetZoomHandler(ICardRolloverZoom zoomHandler, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		SetZoomHandlers(CardViews, zoomHandler, cardDatabase, cardViewBuilder);
	}

	protected void SetZoomHandlers(IEnumerable<StoreCardView> storeCardViews, ICardRolloverZoom zoomHandler, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		if (storeCardViews == null)
		{
			return;
		}
		foreach (StoreCardView storeCardView in storeCardViews)
		{
			storeCardView.SetZoomHandler(zoomHandler, cardDatabase, cardViewBuilder);
		}
	}

	public virtual void Hover(bool on)
	{
		if (Animator.isActiveAndEnabled)
		{
			Animator.SetTrigger("Active", on);
		}
	}

	public virtual void OnClick()
	{
	}

	public virtual void SetBackgroundSprite(Sprite sprite)
	{
		if (_image == null)
		{
			_image = GetComponentInChildren<Image>(includeInactive: true);
		}
		if (!(_image == null))
		{
			_imageSpriteTracker?.Cleanup();
			_image.gameObject.UpdateActive(sprite != null);
			_image.sprite = sprite;
		}
	}

	public virtual void SetBackgroundSprite(string spritePath)
	{
		if (_image == null)
		{
			_image = GetComponentInChildren<Image>(includeInactive: true);
		}
		if (!(_image == null))
		{
			if (_imageSpriteTracker == null)
			{
				_imageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("StoreItemImageSprite");
			}
			_image.gameObject.UpdateActive(!string.IsNullOrEmpty(spritePath));
			AssetLoaderUtils.TrySetSprite(_image, _imageSpriteTracker, spritePath);
		}
	}

	public void SetCollationId(int collationId)
	{
		SetCollationIds(new List<int> { collationId });
	}

	public void SetCollationIds(List<CollationMapping> collationIds)
	{
		if (collationIds != null)
		{
			SetCollationIds(collationIds.Select((CollationMapping x) => (int)x).ToList());
		}
	}

	public void SetCollationIds(List<int> collationIds)
	{
		if (ShouldUpdateCollation(collationIds))
		{
			_collationIds = collationIds;
		}
	}

	public void RefreshBoosterMaterials()
	{
		if (base.isActiveAndEnabled)
		{
			GetBoosterPackRenderers(base.gameObject, ref _renderers, ref _originalBoosterMaterial);
			FreeMaterials(_trackedMaterials);
			ApplyBoosterMaterialsToPacks(_renderers, _collationIds, _originalBoosterMaterial, _trackedMaterials);
		}
	}

	public static bool ShouldUpdateCollation(IEnumerable<int> collationIds)
	{
		if (collationIds == null)
		{
			return false;
		}
		if (collationIds.Count() == 0)
		{
			return false;
		}
		if (collationIds.Count() == 1 && collationIds.First() == 0)
		{
			return false;
		}
		return true;
	}

	public static void GetBoosterPackRenderers(GameObject gameObject, ref List<Renderer> renderers, ref Material originalBoosterMaterial)
	{
		if (renderers != null)
		{
			return;
		}
		renderers = new List<Renderer>();
		MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (meshRenderer.sharedMaterial != null && meshRenderer.sharedMaterial.name.StartsWith("BoosterPack"))
			{
				renderers.Add(meshRenderer);
				if (originalBoosterMaterial == null)
				{
					originalBoosterMaterial = meshRenderer.sharedMaterial;
				}
			}
		}
	}

	public static void ApplyBoosterMaterialsToPacks(List<Renderer> renderers, List<int> collationIds, Material baseMaterial, List<Material> trackedMaterials)
	{
		Dictionary<int, Material> dictionary = new Dictionary<int, Material>();
		if (renderers.Count <= 0)
		{
			return;
		}
		Renderer[] array = (from r in renderers
			orderby 0f - r.transform.parent.position.z, r.transform.position.z descending
			select r).ToArray();
		int num = 0;
		int num2 = ((collationIds.Count > array.Length) ? 1 : (array.Length / collationIds.Count));
		for (int num3 = 0; num3 < array.Length; num3++)
		{
			int num4 = collationIds[num];
			if (!dictionary.TryGetValue(num4, out var value))
			{
				value = BoosterPayloadUtilities.GetBoosterMaterial(baseMaterial, num4, showLimitedDecal: false, WrapperController.Instance.AssetLookupSystem);
				trackedMaterials.Add(value);
				dictionary[num4] = value;
			}
			array[num3].material = value;
			if ((num3 + 1) % num2 == 0)
			{
				num = ++num % collationIds.Count;
			}
		}
	}

	public static void FreeMaterials(List<Material> trackedMaterials)
	{
		foreach (Material trackedMaterial in trackedMaterials)
		{
			BoosterPayloadUtilities.FreeBoosterMaterial(trackedMaterial);
		}
		trackedMaterials.Clear();
	}

	protected virtual void OnDestroy()
	{
		FreeMaterials(_trackedMaterials);
		AssetLoaderUtils.CleanupImage(_image, _imageSpriteTracker);
	}
}
