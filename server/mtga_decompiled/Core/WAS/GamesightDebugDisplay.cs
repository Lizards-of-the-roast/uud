using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Arena.Promises;
using Wizards.Mtga;

namespace WAS;

public static class GamesightDebugDisplay
{
	public static void RenderDebugUI(DebugInfoIMGUIOnGui gui)
	{
		if (SceneManager.GetActiveScene().name.Equals("MainNavigation") && gui.ShowDebugButton("Test Gamesight", 500f))
		{
			TestGamesight();
		}
	}

	private static void TestGamesight()
	{
		if (!(Pantry.Get<IAccountClient>() is WizardsAccountsClient wizardsAccountsClient))
		{
			Debug.LogError("No WizardsAccountsClient instance.");
			return;
		}
		wizardsAccountsClient.GetGamesightLoginRequest(wizardsAccountsClient.AccountInformation.PersonaID).Then(delegate(Promise<string> p)
		{
			PromiseExtensions.Logger.Info(JsonConvert.SerializeObject(p.Result) ?? "");
		}).IfError(HandleError);
	}

	private static void HandleError(Error e)
	{
		PromiseExtensions.Logger.Error($"{e}");
		WASUtils.ToAccountError(e);
	}
}
