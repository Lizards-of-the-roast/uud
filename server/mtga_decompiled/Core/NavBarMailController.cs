using System.Collections.Generic;
using System.Linq;
using Core.Code.ClientFeatureToggle;
using Core.Code.PlayerInbox;
using Core.Code.Promises;
using TMPro;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;

public class NavBarMailController : MonoBehaviour
{
	public CustomButton MailButton;

	private Animator MailAnimator;

	private PlayerInboxDataProvider _playerInboxDataProvider;

	private ClientFeatureToggleDataProvider _featureToggleDataProvider;

	private InventoryManager _inventoryManager;

	private static readonly int Unread = Animator.StringToHash("Unread");

	public TMP_Text UnreadMailCount;

	public GameObject iconContainer;

	private void Awake()
	{
		MailAnimator = MailButton.GetComponent<Animator>();
		_playerInboxDataProvider = Pantry.Get<PlayerInboxDataProvider>();
		_featureToggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
		_featureToggleDataProvider.RegisterForToggleUpdates(AttemptFeatureToggleLookup);
		AttemptFeatureToggleLookup();
	}

	private void OnEnable()
	{
		_playerInboxDataProvider.RegisterForLetterChanges(LetterRefreshListener);
		WrapperController.Instance?.InventoryManager.SubscribeToAll(HandleNewLetters);
		RefreshLetterCount();
	}

	private void LetterRefreshListener(PlayerInboxDataProvider.LetterDataChange _, List<Client_Letter> letters)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			SetMailButtonAnimatorState(_playerInboxDataProvider.Letters);
		});
	}

	private void AttemptFeatureToggleLookup()
	{
		iconContainer.SetActive(_featureToggleDataProvider.IsInitialized() && _featureToggleDataProvider.GetToggleValueById("ClientPlayerInbox"));
	}

	private void SetMailButtonAnimatorState(List<Client_Letter> clientLetters)
	{
		int num = clientLetters?.Count(delegate(Client_Letter l)
		{
			Client_LetterState state = l.State;
			return state == null || !state.ReadDate.HasValue;
		}) ?? 0;
		if (MailAnimator != null && MailAnimator.gameObject.activeInHierarchy)
		{
			MailAnimator.SetBool(Unread, num > 0);
		}
		if (num > 0)
		{
			UnreadMailCount.text = num.ToString();
		}
	}

	private void HandleNewLetters(ClientInventoryUpdateReportItem updateReportItem)
	{
		InventoryDelta delta = updateReportItem.delta;
		if (delta != null && delta.newLetters?.Count > 0)
		{
			_playerInboxDataProvider.AddLetters(updateReportItem.delta.newLetters);
		}
	}

	public void RefreshLetterCount()
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			SetMailButtonAnimatorState(_playerInboxDataProvider.Letters);
		});
	}

	private void OnDestroy()
	{
		_featureToggleDataProvider?.UnRegisterForToggleUpdates(AttemptFeatureToggleLookup);
		_playerInboxDataProvider?.UnRegisterForLetterChanges(LetterRefreshListener);
		WrapperController.Instance?.InventoryManager?.UnsubscribeFromAll(HandleNewLetters);
	}
}
