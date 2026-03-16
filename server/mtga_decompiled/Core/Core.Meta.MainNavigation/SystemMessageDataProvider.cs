using System.Collections;
using System.Collections.Generic;
using Core.Shared.Code;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Meta.MainNavigation;

public class SystemMessageDataProvider
{
	private ISystemMessageServiceWrapper _wrapper;

	private MOTDSession _motdSession = new MOTDSession
	{
		LastMessage = null,
		Requested = false
	};

	private bool _oncePerSession;

	private MessageOfTheDay _messageOfTheDay;

	private bool _isLoadingMotD;

	public bool Initialized { get; private set; }

	public MessageOfTheDay MessageOfTheDay
	{
		get
		{
			return _messageOfTheDay;
		}
		set
		{
			_messageOfTheDay = value;
		}
	}

	public static SystemMessageDataProvider Create()
	{
		return new SystemMessageDataProvider();
	}

	private SystemMessageDataProvider()
	{
		_wrapper = Pantry.Get<ISystemMessageServiceWrapper>();
		Pantry.Get<GlobalCoroutineExecutor>().StartGlobalCoroutine(UpdateSystemMessageData());
	}

	private IEnumerator UpdateSystemMessageData(MOTDSession session = null, bool oncePerSession = false)
	{
		_motdSession = ((session != null) ? session : _motdSession);
		_oncePerSession = oncePerSession;
		if (!_oncePerSession || !_motdSession.Requested)
		{
			_isLoadingMotD = true;
			_motdSession.Requested = true;
			yield return _wrapper.GetSystemMessages(delegate(List<SystemMessageShared> messages)
			{
				if (messages.Count > 0)
				{
					_messageOfTheDay = messages[0].ToMotD();
				}
				_isLoadingMotD = false;
			});
			yield return new WaitUntil(() => !_isLoadingMotD);
		}
		if (_messageOfTheDay != null)
		{
			_motdSession.LastMessage = _messageOfTheDay.Message;
		}
		Initialized = true;
	}
}
