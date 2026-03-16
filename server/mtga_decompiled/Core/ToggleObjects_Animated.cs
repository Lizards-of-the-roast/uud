using UnityEngine;

public class ToggleObjects_Animated : MonoBehaviour
{
	public string propertyName = "CustomProperty";

	public GameObject[] objects;

	private Animator animator;

	private int lastActiveIndex = -1;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (!(animator == null) && objects.Length != 0 && !string.IsNullOrEmpty(propertyName))
		{
			int num = Mathf.Clamp(Mathf.RoundToInt(animator.GetFloat(propertyName)), 0, objects.Length - 1);
			if (num != lastActiveIndex)
			{
				SetActiveObject(num);
				lastActiveIndex = num;
			}
		}
	}

	private void SetActiveObject(int index)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(i == index);
		}
	}
}
