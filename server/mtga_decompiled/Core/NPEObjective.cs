using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPEObjective : MonoBehaviour
{
	[SerializeField]
	private Image _mainImage;

	[SerializeField]
	private TextMeshProUGUI _mainText;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private GameObject _locationIndicator;

	[SerializeField]
	private TextMeshProUGUI _circleText;

	public void UnLock()
	{
		_animator.SetTrigger("Unlock");
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_progress_complete, _animator.gameObject);
	}

	public void CompleteThisGame()
	{
		_animator.SetTrigger("Complete");
	}

	public void SetToLock()
	{
		_animator.SetTrigger("toLock");
	}

	public void SetToNormal()
	{
		_animator.SetTrigger("toNormal");
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_progress_poof, _animator.gameObject);
	}

	public void SetToCompleted()
	{
		_animator.SetTrigger("toComplete");
	}

	private void Reset()
	{
		_animator.ResetTrigger("Unlock");
		_animator.ResetTrigger("Complete");
		_animator.ResetTrigger("toLock");
		_animator.ResetTrigger("toNormal");
		_animator.ResetTrigger("toComplete");
	}

	public void SetImageSprite(Sprite newSprite)
	{
		_mainImage.sprite = newSprite;
	}

	public void SetText(string text)
	{
		_mainText.text = "";
		_circleText.text = text;
	}

	public Vector3 GetIndicatorPosition()
	{
		return _locationIndicator.transform.position;
	}
}
