using UnityEngine;

public class LoadSceneSMB : SMBehaviour
{
	[SerializeField]
	private SceneChangeInitiator _initiator = SceneChangeInitiator.System;

	[SerializeField]
	private string _context = "LoadScene State Machine Behaviour";

	[SerializeField]
	private bool _specialNPEProfileLoad;

	protected override void OnEnter()
	{
		if (_specialNPEProfileLoad)
		{
			SceneLoader.GetSceneLoader().GoToProfileScreen(_initiator, _context);
		}
	}
}
