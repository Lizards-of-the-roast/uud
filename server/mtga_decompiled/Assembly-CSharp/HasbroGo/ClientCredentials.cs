using UnityEngine;

namespace HasbroGo;

[CreateAssetMenu(fileName = "NewClientData", menuName = "ScriptableObjects/HasbroGo/ClientCredentials")]
public class ClientCredentials : ScriptableObject
{
	public string ClientId;

	public string ClientSecret;
}
