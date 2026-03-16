using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using Core.Code.Collations;
using Core.Meta.MainNavigation.BoosterChamber;
using Core.Shared.Code.Utilities;
using DG.Tweening;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Wrapper;

public class SealedBoosterView : MonoBehaviour, IBoosterChamberSetLogoInfo
{
	private const int _goldenBoosterPackCollationId = 900980;

	[SerializeField]
	private Animator _boosterAnimator;

	[SerializeField]
	private Renderer _boosterMeshRenderer;

	[SerializeField]
	private CanvasGroup _quantityCanvasGroup;

	[SerializeField]
	private TMP_Text _quantityText;

	private RendererReferenceLoader _boosterRendererRefLoader;

	public bool clicking;

	public bool _presenting;

	public BoosterChamberController _chamberController;

	public ClientBoosterInfo _info;

	private int _collationId;

	[FormerlySerializedAs("setCode")]
	public string SetCode = string.Empty;

	public bool _isUniversesBeyond;

	private string _boosterBackgroundTexturePath;

	private string _boosterSetLogoTexturePath;

	[NonSerialized]
	public string HeaderSetLogoTexturePath;

	private bool _hovering;

	[NonSerialized]
	public bool NeedsSynced = true;

	private static readonly int HoverOff = Animator.StringToHash("HoverOff");

	private static readonly int HoverOffPresentOff = Animator.StringToHash("HoverOff_PresentOFF");

	private static readonly int Hover = Animator.StringToHash("Hover");

	private static readonly int HoverPresentOff = Animator.StringToHash("Hover_PresentOFF");

	private static readonly int BoosterType = Animator.StringToHash("BoosterType");

	private const int _boosterType_Default = 0;

	private const int _boosterType_Alchemy = 1;

	private const int _boosterType_Mythic = 2;

	private const int _boosterType_BonusPack = 3;

	private InventoryManager _inventoryManager;

	private AssetLookupSystem _assetLookupSystem;

	public Animator BoosterAnimator => _boosterAnimator;

	public int CollationId
	{
		get
		{
			return _collationId;
		}
		set
		{
			_collationId = value;
		}
	}

	public string BoosterBackgroundTexturePath
	{
		get
		{
			return _boosterBackgroundTexturePath;
		}
		set
		{
			_boosterBackgroundTexturePath = value;
			if (_boosterRendererRefLoader == null)
			{
				_boosterRendererRefLoader = new RendererReferenceLoader(_boosterMeshRenderer);
			}
			_boosterRendererRefLoader.SetAndApplyPropertyBlockTexture(0, "_MainTex", _boosterBackgroundTexturePath);
		}
	}

	public string BoosterSetLogoTexturePath
	{
		get
		{
			return _boosterSetLogoTexturePath;
		}
		set
		{
			_boosterSetLogoTexturePath = value;
			UpdateSetLogoTexturePath();
		}
	}

	private void UpdateSetLogoTexturePath()
	{
		if (_boosterRendererRefLoader == null)
		{
			_boosterRendererRefLoader = new RendererReferenceLoader(_boosterMeshRenderer);
		}
		_boosterRendererRefLoader.SetAndApplyPropertyBlockTexture(0, "_Decal1", _boosterSetLogoTexturePath);
	}

	public string GetHeaderSetLogoTexturePath()
	{
		return HeaderSetLogoTexturePath;
	}

	public bool IsUniversesBeyond()
	{
		return _isUniversesBeyond;
	}

	public void Instantiate(InventoryManager inventoryManager, AssetLookupSystem assetLookupSystem)
	{
		_inventoryManager = inventoryManager;
		_assetLookupSystem = assetLookupSystem;
	}

	public void RefreshInfo(float fadeDelay = 1f)
	{
		ClientBoosterInfo info = _inventoryManager.Inventory.boosters.Find((ClientBoosterInfo x) => x.collationId == _info.collationId);
		_info = info;
		SetQuantity(fadeDelay);
	}

	private void SetQuantity(float fadeDelay)
	{
		_quantityText.text = ((_info != null) ? _info.count.ToString() : "0");
		if (_info == null || _info.count <= 1)
		{
			_quantityCanvasGroup.alpha = 0f;
		}
		else if (fadeDelay >= 0f)
		{
			Invoke("_fadeInText", fadeDelay);
		}
	}

	private void _fadeInText()
	{
		_quantityCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
	}

	private void Start()
	{
		_quantityCanvasGroup.alpha = 0f;
	}

	public void PresentOrOpen()
	{
		_chamberController.ClickedBooster(this);
	}

	public void UpdateHoverAnimation()
	{
		if (_hovering)
		{
			ConditionalHover();
		}
		else
		{
			ConditionalHoverOff();
		}
	}

	public void ConditionalHover()
	{
		_hovering = true;
		_boosterAnimator.ResetTrigger(HoverOff);
		_boosterAnimator.ResetTrigger(HoverOffPresentOff);
		if (_boosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON") || _boosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON_Transition"))
		{
			if (_boosterAnimator.GetCurrentAnimatorStateInfo(2).IsName("CarouselBooster_HoverIdle"))
			{
				_boosterAnimator.SetTrigger(HoverOffPresentOff);
			}
			_boosterAnimator.SetTrigger(Hover);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_pack_rollover, base.gameObject);
		}
		else
		{
			if (_boosterAnimator.GetCurrentAnimatorStateInfo(1).IsName("CarouselBooster_HoverIdle"))
			{
				_boosterAnimator.SetTrigger(HoverOff);
			}
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_pack_rollover, base.gameObject);
			_boosterAnimator.SetTrigger(HoverPresentOff);
		}
		AudioManager.SetRTPCValue("booster_packrollover", 100f);
		AudioManager.SetRTPCValue("boosterpack_" + SetCodeForAudio(SetCode), 100f);
	}

	public void ConditionalHoverOff()
	{
		_boosterAnimator.ResetTrigger(Hover);
		_boosterAnimator.ResetTrigger(HoverPresentOff);
		_hovering = false;
		if (_boosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON") || _boosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON_Transition"))
		{
			if (_boosterAnimator.GetCurrentAnimatorStateInfo(2).IsName("CarouselBooster_HoverIdle"))
			{
				_boosterAnimator.SetTrigger(HoverOffPresentOff);
			}
			_boosterAnimator.SetTrigger(HoverOff);
		}
		else
		{
			if (_boosterAnimator.GetCurrentAnimatorStateInfo(1).IsName("CarouselBooster_HoverIdle"))
			{
				_boosterAnimator.SetTrigger(HoverOff);
			}
			_boosterAnimator.SetTrigger(HoverOffPresentOff);
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_pack_rolloff, base.gameObject);
		AudioManager.SetRTPCValue("booster_packrollover", 0f);
		AudioManager.SetRTPCValue("boosterpack_" + SetCodeForAudio(SetCode), 0f);
	}

	private static string SetCodeForAudio(string setCode)
	{
		return setCode.Replace("-", "_");
	}

	public void ClickDown()
	{
		if (_chamberController.OkToSelectBooster)
		{
			AudioManager.PlayAudio(_boosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON") ? WwiseEvents.sfx_ui_boost_pack_depress : WwiseEvents.sfx_ui_main_rewards_pack_tap, base.gameObject);
			clicking = true;
		}
	}

	public void ReleaseClick()
	{
		if (_presenting)
		{
			if (clicking)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_pack_release, base.gameObject);
			}
			clicking = false;
		}
	}

	public void SetData(BoosterChamberController controller, ClientBoosterInfo iBoosterInfo, ISetMetadataProvider setMetadataProvider)
	{
		_chamberController = controller;
		GetComponent<AnimationForwarder_BoosterChamber>()._boosterChamberController = controller;
		_info = iBoosterInfo;
		_collationId = iBoosterInfo.collationId;
		if (_collationId == 900980)
		{
			_boosterAnimator.SetInteger(BoosterType, 3);
		}
		SetCode = SetCodeForCollationID(_collationId, setMetadataProvider);
		_isUniversesBeyond = setMetadataProvider.IsUniversesBeyond((CollationMapping)_collationId);
		_presenting = false;
		Refresh();
	}

	private static string SetCodeForCollationID(int collationId, ISetMetadataProvider setMetadataProvider)
	{
		ClientSetCollation clientSetCollation = setMetadataProvider.CollationForMapping((CollationMapping)collationId);
		SimpleLogUtils.LogErrorIfNull(clientSetCollation, $"[BoosterChamberController] Collation not found for {collationId}  {(CollationMapping)collationId}");
		string setCode = ((CollationMapping)collationId).GetName();
		if (clientSetCollation != null)
		{
			setCode = clientSetCollation.Set.SetCode;
		}
		return setCode;
	}

	public void Refresh()
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.BoosterCollationMapping = (CollationMapping)CollationId;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Background> loadedTree))
		{
			Background payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				BoosterBackgroundTexturePath = payload.TextureRef.RelativePath;
			}
		}
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree2))
		{
			Logo payload2 = loadedTree2.GetPayload(_assetLookupSystem.Blackboard);
			_boosterSetLogoTexturePath = payload2?.TextureRef.RelativePath;
			HeaderSetLogoTexturePath = payload2?.GetHeaderFilePath();
			UpdateSetLogoTexturePath();
		}
	}

	public void OnDestroy()
	{
		_boosterRendererRefLoader?.Cleanup();
	}
}
