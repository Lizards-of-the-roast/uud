using System;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.Cards;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using MTGA.KeyboardManager;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga;
using Wizards.Mtga.Format;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;

public abstract class CardRolloverZoomBase : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, ICardRolloverZoom
{
	protected Coroutine _rolloverCoroutine;

	protected ICardDataAdapter _lastRolloverModel;

	[SerializeField]
	protected Transform _cardParent;

	protected HangerSituation _lastHangerSituation;

	protected Meta_CDC _zoomCard;

	protected CardViewBuilder _cardViewBuilder;

	protected CardDatabase _cardDatabase;

	protected IUnityObjectPool _unityObjectPool;

	protected KeyboardManager _keyboardManager;

	public ICardDataAdapter LastRolloverModel => _lastRolloverModel;

	public bool IsActive { get; set; }

	public Action<Meta_CDC> OnRollover { get; set; }

	public Action<Meta_CDC> OnRolloff { get; set; }

	public Action<ICardDataAdapter> OnRolloverStart { get; set; }

	public PriorityLevelEnum Priority => PriorityLevelEnum.Moz;

	private void Start()
	{
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if (sceneLoader != null)
		{
			sceneLoader.GetCardZoomView();
		}
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public virtual void Initialize(CardViewBuilder cardViewBuilder, CardDatabase cardDatabase, IClientLocProvider locManager, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, KeyboardManager keyboardManager, DeckFormat currentEventFormat)
	{
		_cardViewBuilder = cardViewBuilder;
		_cardDatabase = cardDatabase;
		_unityObjectPool = unityObjectPool;
		_keyboardManager = keyboardManager;
		_keyboardManager?.Subscribe(this);
		_zoomCard = CreateCardView();
	}

	public abstract bool CardRolledOver(ICardDataAdapter model, Bounds cardColliderBounds, HangerSituation hangerSituation = default(HangerSituation), Vector2 offset = default(Vector2));

	public abstract void CardRolledOff(ICardDataAdapter model, bool alwaysRollOff = false);

	public abstract void CardPointerDown(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null, HangerSituation hangerSituation = default(HangerSituation));

	public abstract void CardPointerUp(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null);

	public abstract bool CardScrolled(Vector2 scrollDelta);

	public virtual void Close()
	{
		_lastRolloverModel = null;
		if (_rolloverCoroutine != null)
		{
			StopCoroutine(_rolloverCoroutine);
		}
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape && PlatformUtils.IsHandheld() && _lastRolloverModel != null)
		{
			Close();
			return true;
		}
		return false;
	}

	public virtual void Cleanup()
	{
		_keyboardManager?.Unsubscribe(this);
		_cardViewBuilder?.DestroyCDC(_zoomCard);
		_zoomCard = null;
		_cardDatabase = null;
		_cardViewBuilder = null;
	}

	protected Meta_CDC CreateCardView(Transform cardParent = null, string zoomLayerName = null)
	{
		Meta_CDC meta_CDC = _cardViewBuilder.CreateMetaCdc(CardDataExtensions.CreateBlank(), cardParent);
		if (!string.IsNullOrEmpty(zoomLayerName))
		{
			meta_CDC.transform.SetLayerRecursive(zoomLayerName);
		}
		meta_CDC.GetComponentInChildren<Collider>().enabled = false;
		meta_CDC.gameObject.SetActive(value: false);
		return meta_CDC;
	}

	protected void DecorateLastHangerSituation()
	{
		_lastHangerSituation.ShowFlavorText = true;
		if (WrapperController.Instance != null)
		{
			_lastHangerSituation.BannedFormats = FormatUtilitiesClient.GetBannedFormatsName(_zoomCard.Model.TitleId, FormatUtilitiesClient.GetActiveFormats(WrapperController.Instance.FormatManager, WrapperController.Instance.EventManager));
			_lastHangerSituation.RestrictedFormats = FormatUtilitiesClient.GetRestrictedFormatsNames(_zoomCard.Model.TitleId, FormatUtilitiesClient.GetActiveFormats(WrapperController.Instance.FormatManager, WrapperController.Instance.EventManager));
		}
		_lastHangerSituation.EmergencyTempBanHanger = (Pantry.Get<IEmergencyCardBansProvider>().IsTitleIdEmergencyBanned(_zoomCard.Model.TitleId) ? new HangerConfig?(EmergencyCardBanUtils.HangerData(Pantry.Get<AssetLookupManager>().AssetLookupSystem, Pantry.Get<IClientLocProvider>(), _zoomCard.Model.Printing)) : ((HangerConfig?)null));
	}

	private void OnDestroy()
	{
		Cleanup();
	}
}
