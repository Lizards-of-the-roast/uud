using GreClient.CardData;
using UnityEngine;

public abstract class HangerBase : MonoBehaviour
{
	public bool Active { get; protected set; }

	public virtual bool IsDisplayedOnLeftSide { get; set; }

	public abstract void ActivateHanger(BASE_CDC cardView, ICardDataAdapter sourceModel, HangerSituation situation, bool delayShow = false);

	public abstract void DeactivateHanger();

	public abstract bool HandleScroll(Vector2 delta);

	public abstract float GetHangerWidth();
}
