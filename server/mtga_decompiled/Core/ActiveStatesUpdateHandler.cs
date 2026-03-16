using UnityEngine;

public class ActiveStatesUpdateHandler : MonoBehaviour, ICustomButtonAnimationHandler
{
	[SerializeField]
	private GameObject[] _objOnWhenActive;

	[SerializeField]
	private GameObject[] _objOnWhenDisabled;

	[SerializeField]
	private GameObject[] _objOnWhenMouseOver;

	[SerializeField]
	private GameObject[] _objOnWhenMousePress;

	public void BeginDisabled()
	{
		GameObject[] objOnWhenActive = _objOnWhenActive;
		foreach (GameObject gameObject in objOnWhenActive)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
		}
		objOnWhenActive = _objOnWhenMouseOver;
		foreach (GameObject gameObject2 in objOnWhenActive)
		{
			if (gameObject2.activeSelf)
			{
				gameObject2.SetActive(value: false);
			}
		}
		objOnWhenActive = _objOnWhenMousePress;
		foreach (GameObject gameObject3 in objOnWhenActive)
		{
			if (gameObject3.activeSelf)
			{
				gameObject3.SetActive(value: false);
			}
		}
		objOnWhenActive = _objOnWhenDisabled;
		foreach (GameObject gameObject4 in objOnWhenActive)
		{
			if (!gameObject4.activeSelf)
			{
				gameObject4.SetActive(value: true);
			}
		}
	}

	public void BeginDisabled(float duration)
	{
		BeginDisabled();
	}

	public void BeginMouseOff()
	{
		GameObject[] objOnWhenDisabled = _objOnWhenDisabled;
		foreach (GameObject gameObject in objOnWhenDisabled)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
		}
		objOnWhenDisabled = _objOnWhenMouseOver;
		foreach (GameObject gameObject2 in objOnWhenDisabled)
		{
			if (gameObject2.activeSelf)
			{
				gameObject2.SetActive(value: false);
			}
		}
		objOnWhenDisabled = _objOnWhenMousePress;
		foreach (GameObject gameObject3 in objOnWhenDisabled)
		{
			if (gameObject3.activeSelf)
			{
				gameObject3.SetActive(value: false);
			}
		}
		objOnWhenDisabled = _objOnWhenActive;
		foreach (GameObject gameObject4 in objOnWhenDisabled)
		{
			if (!gameObject4.activeSelf)
			{
				gameObject4.SetActive(value: true);
			}
		}
	}

	public void BeginMouseOff(float duration)
	{
		BeginMouseOff();
	}

	public void BeginMouseOver()
	{
		GameObject[] objOnWhenActive = _objOnWhenActive;
		foreach (GameObject gameObject in objOnWhenActive)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
		}
		objOnWhenActive = _objOnWhenDisabled;
		foreach (GameObject gameObject2 in objOnWhenActive)
		{
			if (gameObject2.activeSelf)
			{
				gameObject2.SetActive(value: false);
			}
		}
		objOnWhenActive = _objOnWhenMousePress;
		foreach (GameObject gameObject3 in objOnWhenActive)
		{
			if (gameObject3.activeSelf)
			{
				gameObject3.SetActive(value: false);
			}
		}
		objOnWhenActive = _objOnWhenMouseOver;
		foreach (GameObject gameObject4 in objOnWhenActive)
		{
			if (!gameObject4.activeSelf)
			{
				gameObject4.SetActive(value: true);
			}
		}
	}

	public void BeginMouseOver(float duration)
	{
	}

	public void BeginPressedOver()
	{
		GameObject[] objOnWhenActive = _objOnWhenActive;
		foreach (GameObject gameObject in objOnWhenActive)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
		}
		objOnWhenActive = _objOnWhenDisabled;
		foreach (GameObject gameObject2 in objOnWhenActive)
		{
			if (gameObject2.activeSelf)
			{
				gameObject2.SetActive(value: false);
			}
		}
		objOnWhenActive = _objOnWhenMouseOver;
		foreach (GameObject gameObject3 in objOnWhenActive)
		{
			if (gameObject3.activeSelf)
			{
				gameObject3.SetActive(value: false);
			}
		}
		objOnWhenActive = _objOnWhenMousePress;
		foreach (GameObject gameObject4 in objOnWhenActive)
		{
			if (!gameObject4.activeSelf)
			{
				gameObject4.SetActive(value: true);
			}
		}
	}

	public void BeginPressedOver(float duration)
	{
		BeginPressedOver();
	}

	public void BeginPressedOff()
	{
		BeginMouseOff();
	}

	public void BeginPressedOff(float duration)
	{
		BeginMouseOff();
	}
}
