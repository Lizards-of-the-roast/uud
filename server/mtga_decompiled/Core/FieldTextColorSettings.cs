using UnityEngine;

[CreateAssetMenu(fileName = "FieldTextColorSettings", menuName = "ScriptableObject/Field Text Color Setting", order = 0)]
public class FieldTextColorSettings : ScriptableObject
{
	public CardTextColorSettings Settings = new CardTextColorSettings();
}
