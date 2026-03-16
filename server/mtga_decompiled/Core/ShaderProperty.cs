using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Graphic))]
public class ShaderProperty : MonoBehaviour
{
	public string FloatName;

	public float FloatValue;

	private Graphic _graphic;

	private void Awake()
	{
		_graphic = GetComponent<Graphic>();
	}

	private void Update()
	{
		_graphic.material.SetFloat(FloatName, FloatValue);
	}
}
