using System.Collections;
using UnityEngine;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Loc;

public class ButtonForwarder_BoosterChamber : MonoBehaviour
{
	[SerializeField]
	private BoosterChamberController _boosterChamberController;

	public void TransitionToStore()
	{
		StartCoroutine(Coroutine_TransitionToStore());
	}

	private IEnumerator Coroutine_TransitionToStore()
	{
		if (WrapperController.Instance.Store.StoreStatus.DisabledTags.Contains(EProductTag.Booster))
		{
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Status_Error_Text"));
			yield break;
		}
		yield return new WaitForSeconds(0.1f);
		SceneLoader.GetSceneLoader().GoToStore(StoreTabType.Packs, "Booster Chamber No Packs");
	}

	public void OpenTenBoosters()
	{
		_boosterChamberController.OpenTen();
	}
}
