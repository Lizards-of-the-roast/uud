namespace Wizards.Arena.Gathering.Friend_Invite;

public class UsernameValidator : IUsernameValidator
{
	public bool ValidUsername(string username)
	{
		if (string.IsNullOrEmpty(username))
		{
			return false;
		}
		if (!username.Contains("@") && !username.Contains("#"))
		{
			return false;
		}
		return true;
	}
}
