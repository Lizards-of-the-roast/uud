using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.Cosmetic;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class RewardDisplayEmote : MonoBehaviour
{
	public enum EquipState
	{
		CapReached,
		Equipped,
		Unequipped
	}

	private static readonly int DISABLED_ANIMATOR_FLAG = Animator.StringToHash("Disabled");

	private const string EMOTE_EQUIPPED_SUBTITLE_KEY = "MainNav/Rewards/EquipEmoteSuccessful_Subtitle";

	private const string EMOTE_EQUIP_CAP_REACHED_KEY = "MainNav/Rewards/EquipEmoteCapReached_Subtitle";

	[SerializeField]
	private Transform _emoteViewParent;

	[SerializeField]
	private Vector3 _phraseViewScale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	private Vector3 _stickerViewScale = new Vector3(2f, 2f, 2f);

	[Header("Equip Button Parameters")]
	[SerializeField]
	private Button _equipButton;

	[SerializeField]
	private Animator _equipButtonAnimator;

	[Header("Equip Button Subtitle Parameters")]
	[SerializeField]
	private TMP_Text _equipSubtitleText;

	private EmoteData _emoteData;

	public AudioEvent HoverSFX { get; private set; }

	public event Action<EmoteData, Action, Action> OnObjectClicked;

	public void Initialize(string emoteName, AssetLookupSystem assetLookupSystem, EquipState equipState, EmoteData emoteData)
	{
		_emoteData = emoteData;
		AssetLookupTree<EmoteViewPrefab> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<EmoteViewPrefab>(returnNewTree: false);
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.EmotePrefabData = new EmotePrefabData
		{
			Id = emoteData.Id,
			Page = emoteData.Entry.Page
		};
		EmoteViewPrefab payload = assetLookupTree.GetPayload(assetLookupSystem.Blackboard);
		EmoteView emoteViewInstance = AssetLoader.Instantiate<EmoteView>(payload.PrefabPath, _emoteViewParent);
		SfxData emoteSfxData = EmoteUtils.GetEmoteSfxData(emoteData.Id, assetLookupSystem);
		emoteViewInstance.Init(emoteName, EmoteUtils.GetPreviewLocKey(emoteData.Id, assetLookupSystem), emoteSfxData);
		emoteViewInstance.SetEquipped(isEquipped: true);
		emoteViewInstance.SetClickable(emoteSfxData != null);
		emoteViewInstance.transform.ZeroOut();
		emoteViewInstance.SetScale((emoteData.Entry.Page == EmotePage.Phrase) ? _phraseViewScale : _stickerViewScale);
		emoteViewInstance.OnClick += delegate
		{
			emoteViewInstance.PlaySfx();
		};
		switch (equipState)
		{
		case EquipState.Equipped:
			_setEmoteEquippedState();
			break;
		case EquipState.Unequipped:
			_equipButton.onClick.AddListener(_onEquipButtonClicked);
			_setEmoteEquipNowState();
			break;
		case EquipState.CapReached:
			_setEmoteCapReachedState();
			break;
		}
	}

	private void _onEquipButtonClicked()
	{
		_setEquipButtonInteractable(interactable: false);
		this.OnObjectClicked?.Invoke(_emoteData, delegate
		{
			_setEmoteCapReachedState();
		}, delegate
		{
			_equipSubtitleText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EquipEmoteSuccessful_Subtitle");
		});
	}

	private void _setEquipButtonInteractable(bool interactable)
	{
		_equipButton.interactable = interactable;
		_equipButtonAnimator.SetBool(DISABLED_ANIMATOR_FLAG, !interactable);
	}

	private void _setEmoteEquipNowState()
	{
		_setEquipButtonInteractable(interactable: true);
		_equipSubtitleText.text = Languages.ActiveLocProvider.GetLocalizedText("EMPTY");
	}

	private void _setEmoteCapReachedState()
	{
		_setEquipButtonInteractable(interactable: false);
		_equipSubtitleText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EquipEmoteCapReached_Subtitle");
	}

	private void _setEmoteEquippedState()
	{
		_setEquipButtonInteractable(interactable: false);
		_equipSubtitleText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EquipEmoteSuccessful_Subtitle");
	}
}
