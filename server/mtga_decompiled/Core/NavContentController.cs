using System;
using UnityEngine;
using Wotc.Mtga.Extensions;

public abstract class NavContentController : MonoBehaviour
{
	public abstract NavContentType NavContentType { get; }

	public bool IsOpen { get; private set; }

	public virtual bool IsReadyToShow { get; } = true;

	public virtual bool SkipScreen { get; }

	protected virtual void Start()
	{
	}

	public void BeginClose()
	{
		IsOpen = false;
		OnBeginClose();
		SetAlphaOfAllChildCards(visible: false);
	}

	public void BeginOpen()
	{
		SetAlphaOfAllChildCards(visible: true);
		OnBeginOpen();
	}

	public void FinishClose()
	{
		IsOpen = false;
		OnFinishClose();
		SetAlphaOfAllChildCards(visible: false);
	}

	public void FinishOpen()
	{
		IsOpen = true;
		SetAlphaOfAllChildCards(visible: true);
		OnFinishOpen();
	}

	private void SetAlphaOfAllChildCards(bool visible)
	{
		Meta_CDC[] componentsInChildren = base.gameObject.GetComponentsInChildren<Meta_CDC>(includeInactive: true);
		foreach (Meta_CDC meta_CDC in componentsInChildren)
		{
			if (meta_CDC.PartsRoot != null)
			{
				meta_CDC.PartsRoot.gameObject.UpdateActive(visible);
			}
		}
	}

	public virtual void OnNavBarScreenChange(Action screenChangeAction)
	{
		screenChangeAction();
	}

	public virtual void OnNavBarExit(Action exitAction)
	{
		exitAction();
	}

	public virtual void OnHandheldBackButton()
	{
		OnNavBarScreenChange(delegate
		{
			SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
		});
	}

	public virtual void OnBeginClose()
	{
		Activate(active: false);
	}

	public virtual void OnBeginOpen()
	{
		Activate(active: true);
	}

	public virtual void OnFinishClose()
	{
	}

	public virtual void OnFinishOpen()
	{
	}

	public virtual void Activate(bool active)
	{
	}

	public virtual void Skipped()
	{
	}
}
