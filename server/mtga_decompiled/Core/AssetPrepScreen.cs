using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssetPrepScreen : MonoBehaviour
{
	public TextMeshProUGUI BuildVersionText;

	public TextMeshProUGUI InfoText;

	public GameObject OverlayDialogInfoTextPrefab;

	public TextMeshProUGUI OverlayDialogInfoText;

	public Button RetryButton;

	public GameObject ProgressPips;

	public GameObject LoadingBar;

	public Button DownloadButton;

	public Button NpeWithoutDownloadButton;

	public GameObject DownloadUI;

	public GameObject WifiWarning;

	public bool ShowWifiWarning;

	public bool UseOverlayDialogInfoText;

	public bool UseLoadingBar;
}
