using System;
using System.Collections;
using System.Collections.Generic;
using StatsMonitor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Mtga;
using Wizards.Mtga.Assets;

public class AnalysisPageGUI : IDebugGUIPage
{
	private DebugInfoIMGUIOnGui _GUI;

	private string DSSName;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Analysis;

	public string TabName => "Analysis";

	public bool HiddenInTab => false;

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		return true;
	}

	public void OnGUI()
	{
		_GUI.ShowLabelInfo("Bootup Time", (WrapperController.Instance?.SecondsSinceBoot.ToString() ?? "???") + "s");
		if (_GUI.ShowDebugButton("Dump Memory Usage To File", 500f))
		{
			MemoryUtil.DumpMemoryUsageToFile();
			Debug.Log("Dump Memory Usage To File");
		}
		if (_GUI.ShowDebugButton("Dump Object Counts", 500f))
		{
			MemoryUtil.DumpObjectsCountToFile();
			Debug.Log("Dump Object Count");
		}
		if (_GUI.ShowDebugButton("Unload All Scenes", 500f))
		{
			PAPA.DebugUnloadAllScenes();
			Debug.Log("Unload All Scenes");
		}
		if (_GUI.ShowDebugButton("Clear Cache", 500f))
		{
			PAPA.DebugClearCache();
			Debug.Log("Clear Cache");
		}
		if (_GUI.ShowDebugButton("Unload All Bundles", 500f))
		{
			AssetBundleManager.Instance.UnloadAllExtreme();
			Debug.Log("Unload All Bundle");
		}
		if (_GUI.ShowDebugButton("Unload Bundles And Scenes", 500f))
		{
			PAPA.DebugUnloadBundlesAndScenes();
			Debug.Log("DebugUnloadBundlesAndScenes");
		}
		if (_GUI.ShowDebugButton("Unbound frame rate", 500f))
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = -1;
		}
		if (_GUI.ShowDebugButton("Unload Unused Assets", 500f))
		{
			Resources.UnloadUnusedAssets();
		}
		if (_GUI.ShowDebugButton("GC Collect", 500f))
		{
			GC.Collect();
		}
		Bootstrap bootstrap;
		if (_GUI.ShowDebugButton("Unload everything", 500f))
		{
			bootstrap = UnityEngine.Object.FindObjectOfType<Bootstrap>();
			bootstrap.StartCoroutine(UnloadAll());
		}
		if (_GUI.ShowDebugButton($"Straight To Duel Scene {MDNPlayerPrefs.StraightToDuelScene}", 500f))
		{
			MDNPlayerPrefs.StraightToDuelScene = !MDNPlayerPrefs.StraightToDuelScene;
		}
		if (_GUI.ShowDebugButton("Time prefab loads", 500f))
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			Scenes.LoadScene("EmptyScene");
		}
		if ((bool)MatchSceneManager.Instance && _GUI.ShowDebugButton("Clear CDCs and caches", 500f))
		{
			List<BASE_CDC> list = new List<BASE_CDC>();
			List<BASE_CDC> list2 = new List<BASE_CDC>();
			GameObject[] rootGameObjects = SceneManager.GetSceneByName("DuelScene").GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				rootGameObjects[i].GetComponentsInChildren(list2);
				list.AddRange(list2);
			}
			foreach (BASE_CDC item in list)
			{
				Pantry.Get<CardViewBuilder>()?.DestroyCDC(item);
			}
			PAPA.ClearCaches();
		}
		if (!MDNPlayerPrefs.RunningDSS && _GUI.ShowDebugButton("Begin DSS Reporting (Restart Required)", 500f))
		{
			MDNPlayerPrefs.RunningDSS = true;
		}
		else if (MDNPlayerPrefs.RunningDSS && _GUI.ShowDebugButton("Stop DSS Reporting (Restart Required)", 500f))
		{
			MDNPlayerPrefs.RunningDSS = false;
		}
		GUILayout.BeginHorizontal(GUILayout.Width(Screen.width / 2));
		DSSName = _GUI.ShowTextField(DSSName);
		if (_GUI.ShowDebugButton("Set DSS Report Name (Restart Required)", 500f))
		{
			MDNPlayerPrefs.DSSReportName = DSSName;
		}
		GUILayout.EndHorizontal();
		if (_GUI.ShowDebugButton("Clear DSS Report Name", 500f))
		{
			MDNPlayerPrefs.DSSReportName = "";
		}
		bool flag = StatsMonitorWrapper.TargetInstance.Mode == Mode.Active;
		if (_GUI.ShowToggle(flag, "Display Stats Monitor") != flag)
		{
			StatsMonitorWrapper.TargetInstance.Toggle();
		}
		static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			new GameObject("LoadTimer").AddComponent<DebugLoadTimerComponent>();
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
		IEnumerator UnloadAll()
		{
			yield return bootstrap.Shutdown();
			GC.Collect();
			Resources.UnloadUnusedAssets();
			GameObject gameObject = null;
			try
			{
				gameObject = new GameObject();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				Scene scene = gameObject.scene;
				UnityEngine.Object.DestroyImmediate(gameObject);
				gameObject = null;
				GameObject[] rootGameObjects2 = scene.GetRootGameObjects();
				for (int j = 0; j < rootGameObjects2.Length; j++)
				{
					UnityEngine.Object.Destroy(rootGameObjects2[j]);
				}
			}
			finally
			{
				if (gameObject != null)
				{
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
			}
			AssetBundleManager.Instance.UnloadAll();
		}
	}
}
