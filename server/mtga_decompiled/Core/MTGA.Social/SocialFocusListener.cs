using Core.Code.Input;

namespace MTGA.Social;

public class SocialFocusListener : INextActionHandler, IPreviousActionHandler
{
	protected readonly IActionSystem _actions;

	private readonly SocialUI _socialUI;

	public SocialFocusListener(IActionSystem actions, SocialUI socialUI)
	{
		_actions = actions;
		_socialUI = socialUI;
		_actions.PushFocus(this);
	}

	public virtual void OnNext()
	{
		_actions.PushFocus(_socialUI);
		_socialUI.Show();
	}

	public virtual void OnPrevious()
	{
		_actions.PushFocus(_socialUI);
		_socialUI.Show();
	}

	public void CleanUp()
	{
		_actions.PopFocus(this);
	}
}
