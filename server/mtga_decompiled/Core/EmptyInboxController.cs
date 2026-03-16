using Core.Meta.NewPlayerExperience.Graph;
using Core.Shared.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;

public class EmptyInboxController : MonoBehaviour
{
	[SerializeField]
	private CyclingTipsView _cyclingTipsView;

	[SerializeField]
	private Button _leftButton;

	[SerializeField]
	private Button _rightButton;

	[SerializeField]
	private TMP_Text _infoLabel;

	[SerializeField]
	private TMP_InputField _tipNumberInput;

	private void Awake()
	{
		_leftButton.onClick.AddListener(OnLeftClicked);
		_rightButton.onClick.AddListener(OnRightClicked);
		_tipNumberInput.onSubmit.AddListener(OnSubmitTipNumber);
	}

	private void OnSubmitTipNumber(string inputTest)
	{
		if (int.TryParse(inputTest, out var result))
		{
			result--;
			if (result != _cyclingTipsView.CurrentTipIndex && result > 0 && result < _cyclingTipsView.TipsCount)
			{
				_cyclingTipsView.SetTipNumber(result);
			}
		}
	}

	private void OnDestroy()
	{
		_leftButton.onClick.RemoveListener(OnLeftClicked);
		_rightButton.onClick.RemoveListener(OnRightClicked);
	}

	private void OnEnable()
	{
		IQueueTipProvider queueTipProvider = Pantry.Get<IQueueTipProvider>();
		NewPlayerExperienceStrategy npeGraphStrategy = Pantry.Get<NewPlayerExperienceStrategy>();
		_cyclingTipsView.StartTips(queueTipProvider, npeGraphStrategy);
		_cyclingTipsView.MarkDirty();
		RefreshLabel();
		_infoLabel.text = " / " + _cyclingTipsView.TipsCount;
	}

	private void OnLeftClicked()
	{
		_cyclingTipsView.SetPreviousTipNoAnimation();
		RefreshLabel();
	}

	private void OnRightClicked()
	{
		_cyclingTipsView.SetNextTipNoAnimation();
		RefreshLabel();
	}

	private void RefreshLabel()
	{
		_tipNumberInput.text = (_cyclingTipsView.CurrentTipIndex + 1).ToString();
	}
}
