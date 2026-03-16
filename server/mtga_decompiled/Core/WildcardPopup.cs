using System.Globalization;
using MTGA.KeyboardManager;
using TMPro;
using UnityEngine;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Loc;

public class WildcardPopup : PopupBase, IKeyDownSubscriber, IKeySubscriber
{
	[SerializeField]
	private TMP_Text _commonWildcardsLabel;

	[SerializeField]
	private TMP_Text _uncommonWildcardsLabel;

	[SerializeField]
	private TMP_Text _rareWildcardsLabel;

	[SerializeField]
	private TMP_Text _mythicWildcardsLabel;

	[SerializeField]
	private TMP_Text _vaultProgressLabel;

	[SerializeField]
	private TMP_Text _vaultProgressDescriptionLabel;

	private KeyboardManager _keyboardManager;

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper;

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
		if (base.IsShowing)
		{
			Activate(activate: false);
		}
	}

	public void Inject(KeyboardManager keyboardManager)
	{
		_keyboardManager = keyboardManager;
	}

	protected override void Show()
	{
		base.Show();
		ClientPlayerInventory clientPlayerInventory = WrapperController.Instance?.InventoryManager?.Inventory;
		if (clientPlayerInventory != null)
		{
			UpdateWildcardLabels(clientPlayerInventory.wcCommon, clientPlayerInventory.wcUncommon, clientPlayerInventory.wcRare, clientPlayerInventory.wcMythic, clientPlayerInventory.vaultProgress);
			UpdateVaultAnimator(clientPlayerInventory);
			_keyboardManager?.Subscribe(this);
		}
	}

	protected override void Hide()
	{
		base.Hide();
		_keyboardManager?.Unsubscribe(this);
	}

	private void UpdateVaultAnimator(ClientPlayerInventory inv)
	{
		Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].name.Contains("Nav_Vault"))
			{
				componentsInChildren[i].SetBool("Active", inv.vaultProgress >= 100.0);
			}
		}
	}

	private void UpdateWildcardLabels(int commonWildcards, int uncommonWildcards, int rareWildcards, int mythicWildcards, double vaultProgress)
	{
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildcardsTooltip_Common", ("quantity", commonWildcards.ToString("N0")));
		string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildcardsTooltip_Uncommon", ("quantity", uncommonWildcards.ToString("N0")));
		string localizedText3 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildcardsTooltip_Rare", ("quantity", rareWildcards.ToString("N0")));
		string localizedText4 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildcardsTooltip_MythicRare", ("quantity", mythicWildcards.ToString("N0")));
		NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
		numberFormatInfo.PercentPositivePattern = 1;
		numberFormatInfo.PercentDecimalDigits = 1;
		string localizedText5 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildCardsTooltip_Vault", ("percent", (vaultProgress / 100.0).ToString("P", numberFormatInfo)));
		localizedText5 = "<style=\"VaultText\">" + localizedText5 + "</style>";
		string localizedText6 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/NavBar/WildCardsTooltip_Vault_Description");
		_commonWildcardsLabel.text = localizedText;
		_uncommonWildcardsLabel.text = localizedText2;
		_rareWildcardsLabel.text = localizedText3;
		_mythicWildcardsLabel.text = localizedText4;
		_vaultProgressLabel.text = localizedText5;
		_vaultProgressDescriptionLabel.text = localizedText6;
	}

	public void VaultButton_OnClick()
	{
		SceneLoader.GetSceneLoader().CurrentNavContent?.OnNavBarScreenChange(delegate
		{
			if (WrapperController.Instance.InventoryManager.Inventory.vaultProgress >= 100.0)
			{
				Activate(activate: false);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_home_open, base.gameObject);
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext
				{
					OpenVault = true
				}, forceReload: true);
			}
		});
	}

	public void DecksButton_OnClick()
	{
		SceneLoader.GetSceneLoader().CurrentNavContent?.OnNavBarScreenChange(delegate
		{
			Activate(activate: false);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_packmanager_open, base.gameObject);
			SceneLoader.GetSceneLoader().GoToDeckManager();
		});
	}

	public void StoreButton_OnClick()
	{
		SceneLoader.GetSceneLoader().CurrentNavContent?.OnNavBarScreenChange(delegate
		{
			Activate(activate: false);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_store_open, base.gameObject);
			SceneLoader.GetSceneLoader().GoToStore(StoreTabType.Featured, "Wildcard Modal");
		});
	}

	public void PacksButton_OnClick()
	{
		SceneLoader.GetSceneLoader().CurrentNavContent?.OnNavBarScreenChange(delegate
		{
			Activate(activate: false);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_packmanager_open, base.gameObject);
			SceneLoader.GetSceneLoader().GoToBoosterChamber("Wildcard Modal");
		});
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape && base.IsShowing)
		{
			Activate(activate: false);
			return true;
		}
		return false;
	}
}
