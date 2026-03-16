using System;

namespace Wizards.Mtga.Notifications;

public class PushNotificationBinder
{
	private readonly IAccountClient _accountClient;

	private readonly IPushNotificationAccountBinding _binding;

	public PushNotificationBinder(IAccountClient accountClient, IPushNotificationAccountBinding binding)
	{
		_accountClient = accountClient ?? throw new ArgumentNullException("accountClient");
		_binding = binding ?? throw new ArgumentNullException("binding");
		accountClient.LoginStateChanged += OnLoginStateChanged;
	}

	private void OnLoginStateChanged(LoginState loginState)
	{
		if (_accountClient.AccountInformation != null)
		{
			if (loginState == LoginState.FullyRegisteredLogin)
			{
				_binding.BindAccount(_accountClient.AccountInformation);
			}
			else
			{
				_binding.UnbindAccount(_accountClient.AccountInformation);
			}
		}
	}
}
