using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SplitPointController : MonoBehaviour
{
	public class TextChangedEvent : UnityEvent<string>
	{
	}

	private TextMeshPro p1_life_element;

	private TextMeshPro p2_life_element;

	private string p1_max_life;

	private string p2_max_life;

	private string p1_life_total;

	private string p2_life_total;

	private string p1_previous_text;

	private string p2_previous_text;

	private Scene subScene;

	[SerializeField]
	public Material battlefield_mat;

	public TextChangedEvent onTextChanged;

	public float currentValue = 0.5f;

	private float targetValue = 10f;

	private float duration = 2f;

	private float elapsedTime;

	private bool isAnimating;

	private float startValue;

	private void Start()
	{
		subScene = SceneManager.GetSceneByName("DuelScene");
		_ = subScene;
		StartCoroutine(WaitForDuelScene());
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChangedEvent);
		battlefield_mat.SetFloat("_Split01", 0.5f);
	}

	private IEnumerator WaitForDuelScene()
	{
		while (!subScene.isLoaded)
		{
			yield return null;
		}
		FindLifeInSubScene(subScene, "Players");
		if (p1_life_element != null && p1_life_element != null)
		{
			p1_previous_text = p1_life_element.text;
			p2_previous_text = p2_life_element.text;
			p1_max_life = p1_life_element.text;
			p2_max_life = p2_life_element.text;
			MonoBehaviour.print("SETTING PREVIOUS TEXTS and MAX LIFE");
		}
		else
		{
			MonoBehaviour.print("UNABLE TO SET PREVIOUS TEXT");
		}
	}

	public void StartSmoothStep(float target, float animaDuration)
	{
		startValue = currentValue;
		targetValue = target;
		duration = Mathf.Max(0.0001f, animaDuration);
		elapsedTime = 0f;
		isAnimating = true;
	}

	private void OnTextChangedEvent(Object obj)
	{
		if (obj == p1_life_element)
		{
			p1_life_total = p1_life_element.text;
			if (p1_life_total != p1_previous_text)
			{
				MonoBehaviour.print("*************************** PLAYER LIFE TOTAL CHANGED!!! from " + p1_previous_text + " to " + p1_life_total);
				MonoBehaviour.print($"CALCULATED OFFSET IS {CalculateOffset(float.Parse(p1_life_total), float.Parse(p2_life_total))}");
				p1_previous_text = p1_life_total;
			}
		}
		if (obj == p2_life_element)
		{
			p2_life_total = p2_life_element.text;
			if (p2_life_total != p2_previous_text)
			{
				MonoBehaviour.print("*************************** ENEMY LIFE TOTAL CHANGED!!! from " + p2_previous_text + " to " + p2_life_total);
				MonoBehaviour.print($"CALCULATED OFFSET IS {CalculateOffset(float.Parse(p1_life_total), float.Parse(p2_life_total))}");
				p2_previous_text = p2_life_total;
			}
		}
	}

	private float CalculateOffset(float num_1, float num_2, int sensitivity = 2)
	{
		if (num_1 <= 0f)
		{
			StartSmoothStep(0f, 1f);
			return num_1;
		}
		if (num_2 <= 0f)
		{
			StartSmoothStep(1f, 1f);
			return num_2;
		}
		float num = num_1 / float.Parse(p1_max_life);
		float num2 = num_2 / float.Parse(p2_max_life);
		MonoBehaviour.print($"Num_1 percent is {num} & Num_1 percent is {num2}");
		float num3 = 0.5f + (num - num2) / 2f;
		StartSmoothStep(num3, 1f);
		return num3;
	}

	private GameObject FindLifeInSubScene(Scene scene, string object_name)
	{
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			MonoBehaviour.print(gameObject.name);
			if (!(gameObject.name == object_name))
			{
				continue;
			}
			GameObject gameObject2 = gameObject.transform.Find("Player #1/SortingGroup/LifeBacker (Highlights)/Life").gameObject;
			GameObject gameObject3 = gameObject.transform.Find("Player #2/SortingGroup/LifeBacker (Highlights)/Life").gameObject;
			if (gameObject2 != null)
			{
				p1_life_element = gameObject2.GetComponent<TextMeshPro>();
				if (p1_life_element != null)
				{
					p1_life_total = p1_life_element.text;
					MonoBehaviour.print(":::::::::::::::::::::::: - PLAYER Life Total is " + p1_life_total);
				}
			}
			if (gameObject3 != null)
			{
				p2_life_element = gameObject3.GetComponent<TextMeshPro>();
				if (p2_life_element != null)
				{
					p2_life_total = p2_life_element.text;
					MonoBehaviour.print(":::::::::::::::::::::::: - ENEMY Life Total is " + p2_life_total);
				}
			}
			return gameObject;
		}
		return null;
	}

	private void OnDestroy()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChangedEvent);
	}

	private void Update()
	{
		if (isAnimating)
		{
			elapsedTime += Time.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / duration);
			currentValue = Mathf.SmoothStep(startValue, targetValue, num);
			battlefield_mat.SetFloat("_Split01", currentValue);
			if (num >= 1f)
			{
				isAnimating = false;
				currentValue = targetValue;
			}
		}
	}
}
