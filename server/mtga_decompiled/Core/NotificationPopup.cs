using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.Meta.MainNavigation.Rewards.Code.Views;
using Core.Meta.Shared;
using Core.Shared.Code;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class NotificationPopup : MonoBehaviour
{
	public class PopupData
	{
		public MTGALocalizedString HeaderString1;

		public MTGALocalizedString HeaderString2;

		public MTGALocalizedString DescriptionString;

		public MTGALocalizedString FooterString;

		public MTGALocalizedString ProgressString;

		public MTGALocalizedString RefreshButtonString;

		public Vector3 WorldPos = Vector3.zero;

		public string UnlocProgressString;

		public UnityAction RefreshButtonOnClickListener;

		public bool IsLocked;

		public bool RefreshButtonActive;

		private RectTransform _safeArea;

		private Camera _camera;

		private RewardDisplayData _displayData;

		private string _rewardPrefabPath;

		private NotificationPopupReward _rewardPrefab;

		private string _bgPath;

		private string _fgPath;

		public void SetSafeArea(RectTransform safeArea, Camera camera)
		{
			_safeArea = safeArea;
			_camera = camera;
		}

		public void SetPopup3dObject(NotificationPopupReward rewardPrefab, string bgPath, string fgPath)
		{
			_rewardPrefab = rewardPrefab;
			_bgPath = bgPath;
			_fgPath = fgPath;
			_displayData = null;
			_rewardPrefabPath = null;
		}

		public void SetPopup3dObject(string rewardPrefabPath, string bgPath, string fgPath)
		{
			_rewardPrefabPath = rewardPrefabPath;
			_bgPath = bgPath;
			_fgPath = fgPath;
			_displayData = null;
			_rewardPrefab = null;
		}

		public void SetPopupDisplayData(RewardDisplayData displayData)
		{
			_displayData = displayData;
			_rewardPrefabPath = null;
			_rewardPrefab = null;
			_bgPath = null;
			_fgPath = null;
		}

		public void ApplyData(NotificationPopup popup)
		{
			if (HeaderString1 != null)
			{
				popup.SetHeaderText(HeaderString1, HeaderString2);
			}
			if (DescriptionString != null)
			{
				popup.SetDescriptionText(DescriptionString);
			}
			if (FooterString != null)
			{
				popup.SetFooterText(FooterString);
			}
			if (ProgressString != null)
			{
				popup.SetProgressText(ProgressString);
			}
			else if (UnlocProgressString != null)
			{
				popup.SetUnlocalizedProgressText(UnlocProgressString);
			}
			popup.SetRefreshButtonText(RefreshButtonString);
			popup.SetRefreshButtonOnClickListener(RefreshButtonOnClickListener);
			popup.SetRefreshButtonActive(RefreshButtonActive);
			popup.SetLocked(IsLocked);
			if (WorldPos != Vector3.zero && _camera != null)
			{
				Vector3 vector = _camera.WorldToScreenPoint(WorldPos);
				popup.SetPosition(_safeArea, vector, _camera);
				WorldPos = Vector3.zero;
			}
			if (_displayData != null)
			{
				popup.SetPopup3DObject(_displayData);
			}
			else if (_rewardPrefab != null)
			{
				popup.SetPopup3DObject(_rewardPrefab, _bgPath, _fgPath);
			}
			else if (_rewardPrefabPath != null)
			{
				popup.SetPopup3DObjectByPath(_rewardPrefabPath, _bgPath, _fgPath);
			}
		}
	}

	[SerializeField]
	private bool _chimeSounds;

	[SerializeField]
	private Localize _headerText1;

	[SerializeField]
	private Localize _headerText2;

	[SerializeField]
	private Localize _descriptionText;

	[SerializeField]
	private Localize _footerText;

	[SerializeField]
	private Localize _progressText;

	[SerializeField]
	private Localize _refreshButtonText;

	[SerializeField]
	private Transform _rewardParent;

	[SerializeField]
	private GameObject _lockIcon;

	[SerializeField]
	private CustomButton _refreshButton;

	[SerializeField]
	private GameObject _tail;

	[SerializeField]
	private GameObject _base;

	[SerializeField]
	private MetaCardHolder _cardHolder;

	[Header("Rects")]
	[SerializeField]
	private RectTransform _popupRoot;

	[SerializeField]
	private RectTransform _tailRect;

	[SerializeField]
	private RectTransform _widthRect;

	[Header("Placement Settings")]
	[SerializeField]
	private float _maxTailOffset = 72f;

	[SerializeField]
	private float _screenEdgePadding = 15f;

	[SerializeField]
	private float _yOffset;

	[SerializeField]
	private float _zOffset = -500f;

	private NotificationPopupReward _rewardInstance;

	private static readonly Dictionary<string, NotificationPopupReward> _storedPrefabsByPath = new Dictionary<string, NotificationPopupReward>();

	private static readonly Dictionary<NotificationPopupReward, NotificationPopupReward> _storedPrefabs = new Dictionary<NotificationPopupReward, NotificationPopupReward>();

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	public void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		if (_cardHolder != null)
		{
			_cardHolder.RolloverZoomView = SceneLoader.GetSceneLoader().GetCardZoomView();
			_cardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		}
	}

	private void OnEnable()
	{
		if (!(_base == null) && !_base.Equals(null))
		{
			_base.SetActive(value: false);
		}
	}

	protected void SetPosition(RectTransform safeArea, Vector2 screenPos, Camera camera)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(safeArea, screenPos, camera, out var localPoint);
		float num = _widthRect.rect.width * 0.5f;
		float num2 = safeArea.rect.width * 0.5f - _screenEdgePadding;
		Vector2 vector = _tailRect.localPosition;
		if (localPoint.x - num < 0f - num2)
		{
			float num3 = 0f - num2 + num;
			vector.x = Mathf.Max(0f - num + _maxTailOffset, localPoint.x - num3);
			localPoint.x = num3;
		}
		else if (localPoint.x + num > num2)
		{
			float num4 = num2 - num;
			vector.x = Mathf.Min(num - _maxTailOffset, localPoint.x - num4);
			localPoint.x = num4;
		}
		else
		{
			vector.x = 0f;
		}
		_tailRect.localPosition = vector;
		Vector3 position = new Vector3(localPoint.x, localPoint.y, 0f);
		_popupRoot.position = safeArea.TransformPoint(position);
		_popupRoot.localPosition = new Vector3(_popupRoot.localPosition.x, _popupRoot.localPosition.y + _yOffset, _zOffset);
	}

	private void SetPopup3DObject(RewardDisplayData displayData)
	{
		SetPopup3DObjectStatic(ref _rewardInstance, _rewardParent, Pantry.Get<GlobalCoroutineExecutor>(), _cardDatabase, _cardViewBuilder, displayData, _cardHolder);
	}

	public static void SetPopup3DObjectStatic(ref NotificationPopupReward rewardInstance, Transform rewardParent, GlobalCoroutineExecutor coroutineExecutor, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, RewardDisplayData displayData, MetaCardHolder cardHolder = null)
	{
		if (displayData.OverridePopup3dObject != null)
		{
			SetPopup3DObjectByObject(ref rewardInstance, rewardParent, _storedPrefabs, displayData.OverridePopup3dObject, displayData.PopupObjectBackgroundTexturePath, displayData.PopupObjectForegroundTexturePath);
		}
		else
		{
			SetPopup3DObjectByPath(ref rewardInstance, rewardParent, _storedPrefabsByPath, coroutineExecutor, displayData.Popup3dObjectPath, displayData.PopupObjectBackgroundTexturePath, displayData.PopupObjectForegroundTexturePath, displayData.ReferenceID);
		}
		if (rewardInstance == null)
		{
			return;
		}
		EmoteView componentInChildren = rewardInstance.GetComponentInChildren<EmoteView>();
		if (componentInChildren != null)
		{
			componentInChildren.SetEquipped(isEquipped: true);
			string previewLocKey = EmoteUtils.GetPreviewLocKey(displayData.ReferenceID, WrapperController.Instance.AssetLookupSystem);
			if (previewLocKey != null && !string.IsNullOrEmpty(previewLocKey))
			{
				componentInChildren.SetLocalizationKey(previewLocKey);
			}
		}
		RewardDisplayTitle componentInChildren2 = rewardInstance.GetComponentInChildren<RewardDisplayTitle>();
		if (componentInChildren2 != null)
		{
			string referenceID = displayData.ReferenceID;
			string titleLocKey = CosmeticsUtils.TitleLocKey(Pantry.Get<CosmeticsProvider>(), referenceID);
			componentInChildren2.Init(referenceID, titleLocKey, null);
		}
		rewardInstance.GetComponentInChildren<AchievementRewardDisplayAvatar>()?.SetAvatar(WrapperController.Instance.AssetLookupSystem, displayData.ReferenceID);
		CDCMetaCardView componentInChildren3 = rewardInstance.GetComponentInChildren<CDCMetaCardView>();
		if (!(componentInChildren3 != null))
		{
			return;
		}
		if (displayData.ReferenceID != null)
		{
			uint grpId;
			string styleName;
			if (Regex.Match(displayData.ReferenceID, "cardbacks\\.", RegexOptions.IgnoreCase).Success || Regex.Match(displayData.ReferenceID, "cardback_", RegexOptions.IgnoreCase).Success)
			{
				string sleeveCode = (displayData.ReferenceID.Contains(".") ? displayData.ReferenceID.Split('.')[1] : displayData.ReferenceID);
				CardData data = CardDataExtensions.CreateSkinCard(0u, cardDatabase, "", sleeveCode, faceDown: true);
				componentInChildren3.Init(cardDatabase, cardViewBuilder);
				componentInChildren3.SetData(data);
			}
			else if (RewardDisplayData.TryParseCard(displayData.ReferenceID, out grpId, out styleName))
			{
				CardData cardData = CardDataExtensions.CreateSkinCard(grpId, cardDatabase, styleName);
				componentInChildren3.Init(cardDatabase, cardViewBuilder);
				cardData.IsFakeStyleCard = !string.IsNullOrEmpty(styleName);
				componentInChildren3.SetData(cardData);
			}
		}
		if (cardHolder != null)
		{
			componentInChildren3.Holder = cardHolder;
			return;
		}
		BoxCollider componentInChildren4 = componentInChildren3.GetComponentInChildren<BoxCollider>();
		if (componentInChildren4 != null)
		{
			componentInChildren4.enabled = false;
		}
	}

	private void SetPopup3DObjectByPath(string rewardPrefabPath, string bgPath, string fgPath)
	{
		SetPopup3DObjectByPath(ref _rewardInstance, _rewardParent, _storedPrefabsByPath, Pantry.Get<GlobalCoroutineExecutor>(), rewardPrefabPath, bgPath, fgPath);
	}

	private static void SetPopup3DObjectByPath(ref NotificationPopupReward rewardInstance, Transform rewardParent, Dictionary<string, NotificationPopupReward> storedPrefabsByPath, GlobalCoroutineExecutor coroutineExecutor, string rewardPrefabPath, string bgPath, string fgPath, string referenceId = "")
	{
		if (rewardInstance != null)
		{
			rewardInstance.gameObject.UpdateActive(active: false);
		}
		if (string.IsNullOrEmpty(rewardPrefabPath))
		{
			return;
		}
		if (storedPrefabsByPath.TryGetValue(rewardPrefabPath + "." + referenceId, out rewardInstance) && rewardInstance != null)
		{
			rewardInstance.transform.SetParent(rewardParent, worldPositionStays: false);
			rewardInstance.gameObject.SetActive(value: true);
			CheckForAndPlayAnimatedCardBack(rewardInstance);
		}
		else
		{
			rewardInstance = AssetLoader.Instantiate<NotificationPopupReward>(rewardPrefabPath, rewardParent);
			if (rewardInstance != null)
			{
				storedPrefabsByPath[rewardPrefabPath + "." + referenceId] = rewardInstance;
				coroutineExecutor.StartGlobalCoroutine(DelayCheckForAndPlayAnimatedCardback(rewardInstance));
			}
			else
			{
				Debug.LogWarning("Reward Prefab not Instantiated, prefab path was not found: " + rewardPrefabPath);
			}
		}
		if (rewardInstance != null)
		{
			rewardInstance.SetBackgroundTexture(bgPath);
			rewardInstance.SetForegroundTexture(fgPath);
		}
	}

	private static void CheckForAndPlayAnimatedCardBack(NotificationPopupReward rewardInstance)
	{
		if (!(rewardInstance == null))
		{
			CDCPart_ControllerAnimatedCardback componentInChildren = rewardInstance.gameObject.GetComponentInChildren<CDCPart_ControllerAnimatedCardback>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.PlayAnimations();
			}
		}
	}

	private static IEnumerator DelayCheckForAndPlayAnimatedCardback(NotificationPopupReward rewardInstance)
	{
		yield return new WaitForSeconds(1f);
		CheckForAndPlayAnimatedCardBack(rewardInstance);
	}

	private void SetPopup3DObject(NotificationPopupReward rewardPrefab, string bgPath, string fgPath)
	{
		SetPopup3DObjectByObject(ref _rewardInstance, _rewardParent, _storedPrefabs, rewardPrefab, bgPath, fgPath);
	}

	private static void SetPopup3DObjectByObject(ref NotificationPopupReward rewardInstance, Transform rewardParent, Dictionary<NotificationPopupReward, NotificationPopupReward> storedPrefabs, NotificationPopupReward rewardPrefab, string bgPath, string fgPath)
	{
		if (rewardInstance != null)
		{
			rewardInstance.gameObject.UpdateActive(active: false);
		}
		if (!(rewardPrefab == null))
		{
			if (storedPrefabs.TryGetValue(rewardPrefab, out rewardInstance) && rewardInstance != null)
			{
				rewardInstance.gameObject.SetActive(value: true);
			}
			else
			{
				rewardInstance = Object.Instantiate(rewardPrefab, rewardParent);
				storedPrefabs[rewardPrefab] = rewardInstance;
			}
			if (!string.IsNullOrEmpty(bgPath))
			{
				rewardInstance.SetBackgroundTexture(bgPath);
			}
			if (!string.IsNullOrEmpty(fgPath))
			{
				rewardInstance.SetForegroundTexture(fgPath);
			}
		}
	}

	protected void SetProgressText(MTGALocalizedString newProgressText)
	{
		_progressText.transform.parent.gameObject.UpdateActive(newProgressText.Key != "MainNav/General/Empty_String");
		_progressText.SetText(newProgressText);
	}

	public void SetUnlocalizedProgressText(string text)
	{
		if (_progressText != null)
		{
			_progressText.GetComponent<TextMeshProUGUI>().text = text;
		}
	}

	protected void SetHeaderText(MTGALocalizedString line1, MTGALocalizedString line2 = null)
	{
		_headerText1.SetText(line1 ?? ((MTGALocalizedString)"MainNav/General/Empty_String"));
		_headerText2.SetText(line2 ?? ((MTGALocalizedString)"MainNav/General/Empty_String"));
	}

	protected void SetDescriptionText(MTGALocalizedString desc)
	{
		if (_descriptionText != null)
		{
			_descriptionText.SetText(desc ?? ((MTGALocalizedString)"MainNav/General/Empty_String"));
		}
	}

	protected void SetFooterText(MTGALocalizedString footerText)
	{
		if (_footerText != null)
		{
			_footerText.SetText(footerText ?? ((MTGALocalizedString)"MainNav/General/Empty_String"));
		}
	}

	protected void SetRefreshButtonText(MTGALocalizedString refreshButtonText)
	{
		if (_refreshButtonText != null)
		{
			_refreshButton.SetText(refreshButtonText ?? ((MTGALocalizedString)"MainNav/General/Empty_String"));
		}
	}

	protected void SetLocked(bool value)
	{
		if (_lockIcon != null)
		{
			_lockIcon.UpdateActive(value);
		}
	}

	public void OnVisuallyActive()
	{
		if (_chimeSounds)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_rollover_initial, base.gameObject);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_rollover_b, base.gameObject);
		}
		if (!(_rewardInstance == null) && _rewardInstance.gameObject.activeSelf)
		{
			string text = _rewardInstance.gameObject.name.Replace("RewardPopup3DIcon_", "").ToLower();
			bool flag = true;
			if (text.Contains("coin"))
			{
				flag = false;
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_rollover_reveal_coin, base.gameObject);
			}
			if (text.Contains("gem"))
			{
				flag = false;
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_rollover_reveal_gem, base.gameObject);
			}
			if (text.Contains("pack"))
			{
				flag = false;
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_rollover_reveal_pack, base.gameObject);
			}
			if (text.Contains("card"))
			{
				flag = false;
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_rollover_reveal_card, base.gameObject);
			}
			if (text.Contains("orb"))
			{
				flag = false;
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_rollover_reveal_orb, base.gameObject);
			}
			if (text.Contains("cat"))
			{
				flag = false;
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_rollover_reveal_pet_cat, base.gameObject);
			}
			if (flag)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_rollover_reveal_generic, base.gameObject);
			}
		}
	}

	public void CallObjectiveRefresh()
	{
		ObjectiveBubble component = _rewardParent.GetComponent<ObjectiveBubble>();
		component.DeactivatePopup();
		component.OnClick();
	}

	protected void SetRefreshButtonActive(bool isActive)
	{
		if (_refreshButton != null)
		{
			_refreshButton.gameObject.SetActive(isActive);
			_footerText.gameObject.SetActive(!isActive);
		}
	}

	protected void SetRefreshButtonOnClickListener(UnityAction call)
	{
		if (_refreshButton != null)
		{
			_refreshButton.OnClick.RemoveAllListeners();
			_refreshButton.OnClick.AddListener(call);
		}
	}

	public void SetTailActive(bool active)
	{
		_tail.UpdateActive(active);
	}
}
