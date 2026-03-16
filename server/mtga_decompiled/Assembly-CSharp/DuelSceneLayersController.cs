using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class DuelSceneLayersController : MonoBehaviour
{
	[Serializable]
	private class ChooseTargetsData
	{
		public string LabelText = "choosing targets";

		public Image Fader;

		public float FadeTime = 0.5f;

		public float FadeTo = 0.93f;

		public Transform[] TargetableCards;

		public string NewCardsLayer = "Featured";

		public Transform[] TargetableAvatars;

		public Transform NewAvatarCanvas;
	}

	[Serializable]
	private class ZoomData
	{
		public string LabelText = "zooming card";

		public Image Fader;

		public float FadeTime = 0.5f;

		public float FadeTo = 0.93f;

		public Transform ZoomCard;

		public string ZoomCardLayer = "Zoom";

		public float ZoomTime = 0.5f;

		public Transform Target;
	}

	[Serializable]
	private class TargetChosenData
	{
		public ChooseTargetsData ChooseTargets;

		public Transform TargetCard;

		public float TargetTime = 0.5f;

		public Transform TargetTarget;
	}

	private enum DuelSceneLayersStage
	{
		Default,
		ChooseTargets,
		Zoom,
		Unzoom,
		TargetChosen
	}

	[SerializeField]
	private TextMeshProUGUI stageLabel;

	[SerializeField]
	private ChooseTargetsData chooseTargetsData;

	[SerializeField]
	private ZoomData zoomData;

	[SerializeField]
	private ZoomData unzoomData;

	[SerializeField]
	private TargetChosenData targetChosenData;

	private DuelSceneLayersStage stage;

	private void Start()
	{
		chooseTargetsData.Fader.DOFade(0f, 0f);
		zoomData.Fader.DOFade(0f, 0f);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			GotoNextStage();
		}
	}

	private void GotoNextStage()
	{
		switch (stage)
		{
		case DuelSceneLayersStage.Default:
			HandleChooseTargetsData(chooseTargetsData);
			stage = DuelSceneLayersStage.ChooseTargets;
			break;
		case DuelSceneLayersStage.ChooseTargets:
			HandleZoomData(zoomData);
			stage = DuelSceneLayersStage.Zoom;
			break;
		case DuelSceneLayersStage.Zoom:
			HandleZoomData(unzoomData);
			stage = DuelSceneLayersStage.Unzoom;
			break;
		case DuelSceneLayersStage.Unzoom:
			HandleTargetChosenData(targetChosenData);
			stage = DuelSceneLayersStage.TargetChosen;
			break;
		}
	}

	private void HandleChooseTargetsData(ChooseTargetsData chooseTargetsData)
	{
		Transform[] targetableCards = chooseTargetsData.TargetableCards;
		for (int i = 0; i < targetableCards.Length; i++)
		{
			targetableCards[i].SetLayerRecursive(chooseTargetsData.NewCardsLayer);
		}
		targetableCards = chooseTargetsData.TargetableAvatars;
		for (int i = 0; i < targetableCards.Length; i++)
		{
			targetableCards[i].SetParent(chooseTargetsData.NewAvatarCanvas, worldPositionStays: true);
		}
		chooseTargetsData.Fader.DOFade(chooseTargetsData.FadeTo, chooseTargetsData.FadeTime);
		stageLabel.text = chooseTargetsData.LabelText;
	}

	private void HandleZoomData(ZoomData zoomData)
	{
		zoomData.ZoomCard.SetLayerRecursive(zoomData.ZoomCardLayer);
		zoomData.ZoomCard.DOMove(zoomData.Target.position, zoomData.ZoomTime);
		zoomData.ZoomCard.DORotate(zoomData.Target.rotation.eulerAngles, zoomData.ZoomTime);
		zoomData.Fader.DOFade(zoomData.FadeTo, zoomData.FadeTime);
		stageLabel.text = zoomData.LabelText;
	}

	private void HandleTargetChosenData(TargetChosenData targetChosenData)
	{
		HandleChooseTargetsData(targetChosenData.ChooseTargets);
		targetChosenData.TargetCard.DOMove(targetChosenData.TargetTarget.position, targetChosenData.TargetTime);
		targetChosenData.TargetCard.DORotate(targetChosenData.TargetTarget.rotation.eulerAngles, targetChosenData.TargetTime);
	}
}
