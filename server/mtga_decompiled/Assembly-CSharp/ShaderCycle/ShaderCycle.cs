using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ShaderCycle;

public class ShaderCycle : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer cube;

	[SerializeField]
	private Text txtDebug;

	private List<string> shaderPaths;

	[SerializeField]
	private Shader defaultShader;

	private int s = -1;

	private void Start()
	{
		ButtonDefaultAndClear();
		shaderPaths = (from p in AssetBundleManager.Instance.GetAllBundledFilePaths()
			where p.EndsWith(".shader")
			select p).ToList();
	}

	public void ButtonChangeShader()
	{
		s++;
		if (s >= shaderPaths.Count)
		{
			s = 0;
		}
		Shader objectData = AssetLoader.GetObjectData<Shader>(shaderPaths[s]);
		if (objectData == null)
		{
			txtDebug.text = shaderPaths[s] + " not found!";
			return;
		}
		cube.material.shader = objectData;
		txtDebug.text = objectData.name;
		Debug.Log("[ShaderCycle] loaded: (" + s + "/" + shaderPaths.Count + ") - " + shaderPaths[s]);
	}

	public void ButtonDefaultAndClear()
	{
		cube.material.shader = defaultShader;
		txtDebug.text = "none";
		PAPA.DebugClearCache();
	}
}
