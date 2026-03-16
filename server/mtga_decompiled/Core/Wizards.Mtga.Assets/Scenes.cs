using System.IO;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Scenes;
using Core.Code.AssetLookupTree.AssetLookup;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wizards.Mtga.Assets;

public static class Scenes
{
	public static readonly string[] ClientScenePaths = new string[14]
	{
		"Assets/Core/Bootstrap/Bootstrap.unity",
		"Assets/Core/AssetPrep/AssetPrep.unity",
		"Assets/Core/Meta/EmptyScene.unity",
		BattlefieldUtil.FallbackBattlefieldPath,
		"Assets/Core/Meta/MainNavigation/Store/Store.unity",
		"Assets/Core/Meta/MainNavigation/RewardTrack/RewardTrack.unity",
		"Assets/Core/Meta/MainNavigation/Profile/Profile.unity",
		"Assets/Core/Meta/MainNavigation/NavBar.unity",
		"Assets/Core/Meta/MainNavigation/LearntoPlayv2/LearnToPlay.unity",
		"Assets/Core/Meta/MainNavigation/Home/HomePage.unity",
		"Assets/Core/Meta/MainNavigation/DeckBuilder/DeckBuilder.unity",
		"Assets/Core/Meta/MainNavigation/DeckManager/Decks.unity",
		"Assets/Core/Meta/MainNavigation/BoosterChamber/BoosterChamber.unity",
		"Assets/Core/Meta/MainNavigation/Achievements/Achievements.unity"
	};

	public static readonly string[] DevOnlyClientScenePaths = new string[2] { "Assets/Core/DuelScene/BotBattle/BotBattleScene.unity", "Assets/Core/DuelScene/BotBattle/BotBattleLauncher.unity" };

	public static bool LoadScene(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
	{
		if (IsSceneInBuild(sceneName) || IsBattlefieldScene(sceneName))
		{
			SceneManager.LoadScene(sceneName, loadMode);
			return true;
		}
		if (!LoadSceneBundle(sceneName))
		{
			return false;
		}
		SceneManager.LoadScene(sceneName, loadMode);
		return true;
	}

	public static async UniTask<T> LoadSceneAsync<T>(this string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single) where T : MonoBehaviour
	{
		await LoadSceneAsync(sceneName, loadMode);
		return SceneManager.GetSceneByName(sceneName).GetSceneComponent<T>();
	}

	public static AsyncOperation? LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
	{
		if (IsSceneInBuild(sceneName) || IsBattlefieldScene(sceneName))
		{
			return SceneManager.LoadSceneAsync(sceneName, loadMode);
		}
		if (!LoadSceneBundle(sceneName))
		{
			return null;
		}
		return SceneManager.LoadSceneAsync(sceneName, loadMode);
	}

	public static void SceneLoaded(Scene scene, LoadSceneMode __)
	{
		if (TryGetPayload(scene.name, out ScenePayloads scenePayload, out string _))
		{
			AssetLoader.ReleaseAsset(scenePayload.ScenePath);
		}
	}

	public static bool IsSceneAvailable(string sceneName)
	{
		if (!IsSceneInBuild(sceneName) && !IsBattlefieldScene(sceneName))
		{
			return LoadSceneBundle(sceneName);
		}
		return true;
	}

	private static bool IsSceneInBuild(string sceneName)
	{
		if (!Enumerable.Select(ClientScenePaths, Path.GetFileNameWithoutExtension).Contains<string>(sceneName))
		{
			return Enumerable.Select(DevOnlyClientScenePaths, Path.GetFileNameWithoutExtension).Contains<string>(sceneName);
		}
		return true;
	}

	private static bool LoadSceneBundle(string sceneName)
	{
		if (!TryGetPayload(sceneName, out ScenePayloads scenePayload, out string error))
		{
			SimpleLog.LogError(error);
			return false;
		}
		if (!AssetLoader.AddReferenceCount(scenePayload.ScenePath))
		{
			SimpleLog.LogError("Could not load scene async with path from bundles " + scenePayload.ScenePath);
			return false;
		}
		return true;
	}

	private static bool TryGetPayload(string sceneName, out ScenePayloads? scenePayload, out string? error)
	{
		error = null;
		if (AssetBundleManager.AssetBundlesActive && !AssetBundleManager.Instance.Initialized)
		{
			scenePayload = null;
			error = "Asset bundle manager is not initialized";
			return false;
		}
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SceneToLoad = sceneName;
		scenePayload = assetLookupSystem.TreeLoader.LoadTree<ScenePayloads>().GetPayload(assetLookupSystem.Blackboard);
		if (scenePayload == null)
		{
			error = "Scene " + sceneName + " is not in ScenePayloads ALT";
			return false;
		}
		return true;
	}

	private static bool IsBattlefieldScene(string name)
	{
		return name.Contains("Battlefield");
	}
}
