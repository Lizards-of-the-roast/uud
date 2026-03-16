using UnityEngine;

public class ElfDeluxeTooltipController : MonoBehaviour
{
	private void PlayElfWelcomeDialog()
	{
		AudioManager.PlayAudio(WwiseEvents.vo_elf_018.EventName, AudioManager.Default);
	}
}
