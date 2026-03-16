using Core.Code.Input;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;

namespace Wotc.Mtga.Login;

public class UpdatePoliciesPanel : Panel
{
	[SerializeField]
	private Toggle termsAndConditions_Toggle;

	[SerializeField]
	private Toggle codeOfConduct_Toggle;

	[SerializeField]
	private Toggle privacyPolicy_Toggle;

	public static bool NeedsToAcceptPolicy;

	public static bool PolicyAcceptedThisSession { get; private set; }

	public override void Initialize(LoginScene loginScene, IActionSystem actions, KeyboardManager keyboardManager, IBILogger biLogger)
	{
		termsAndConditions_Toggle.onValueChanged.AddListener(OnTogglesChanged);
		codeOfConduct_Toggle.onValueChanged.AddListener(OnTogglesChanged);
		privacyPolicy_Toggle.onValueChanged.AddListener(OnTogglesChanged);
		_mainButton.OnClick.AddListener(OnAccept);
		_panelType = PanelType.UpdatePolicies;
		base.Initialize(loginScene, actions, keyboardManager, biLogger);
		EnableButton(enabled: false);
	}

	private void OnTogglesChanged(bool _)
	{
		PolicyAcceptedThisSession = termsAndConditions_Toggle.isOn && codeOfConduct_Toggle.isOn && privacyPolicy_Toggle.isOn;
		EnableButton(PolicyAcceptedThisSession);
	}

	public override void OnAccept()
	{
		NeedsToAcceptPolicy = false;
		_mainButton.OnClick.RemoveListener(OnAccept);
		_loginScene.LoadNextPanelBasedOnLoginState();
		_loginScene.ReAttachToFrontDoor();
		base.OnAccept();
	}

	private void OnDestroy()
	{
		termsAndConditions_Toggle.onValueChanged.RemoveListener(OnTogglesChanged);
		codeOfConduct_Toggle.onValueChanged.RemoveListener(OnTogglesChanged);
		privacyPolicy_Toggle.onValueChanged.RemoveListener(OnTogglesChanged);
	}

	public static void ResetPolicyAccepted()
	{
		NeedsToAcceptPolicy = false;
		PolicyAcceptedThisSession = false;
	}
}
