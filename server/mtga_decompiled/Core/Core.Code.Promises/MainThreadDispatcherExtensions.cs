using System;
using Wizards.Arena.Promises;

namespace Core.Code.Promises;

public static class MainThreadDispatcherExtensions
{
	public static Promise<T> ThenOnMainThread<T>(this Promise<T> promise, Action<T> action)
	{
		return promise.Then(delegate(Promise<T> p)
		{
			MainThreadDispatcher.Dispatch(delegate
			{
				action(p.Result);
			});
		});
	}

	public static Promise<T> ThenOnMainThread<T>(this Promise<T> promise, Action<Promise<T>> action)
	{
		return promise.Then(delegate(Promise<T> p)
		{
			MainThreadDispatcher.Dispatch(delegate
			{
				action(p);
			});
		});
	}

	public static Promise<T> ThenOnMainThread<T>(this Promise<T> promise, Action action)
	{
		return promise.Then(delegate
		{
			MainThreadDispatcher.Dispatch(action);
		});
	}

	public static Promise<T> ThenOnMainThreadIfSuccess<T>(this Promise<T> promise, Action<T> action)
	{
		return promise.IfSuccess(delegate(Promise<T> p)
		{
			MainThreadDispatcher.Dispatch(delegate
			{
				action(p.Result);
			});
		});
	}

	public static Promise<T> ThenOnMainThreadIfSuccess<T>(this Promise<T> promise, Action action)
	{
		return promise.IfSuccess(delegate
		{
			MainThreadDispatcher.Dispatch(action);
		});
	}

	public static Promise<T> ThenOnMainThreadIfError<T>(this Promise<T> promise, Action<Error> action)
	{
		return promise.IfError(delegate(Promise<T> p)
		{
			MainThreadDispatcher.Dispatch(delegate
			{
				action(p.Error);
			});
		});
	}

	public static Promise<T> ThenOnMainThreadIfError<T>(this Promise<T> promise, Action action)
	{
		return promise.IfError((Action)delegate
		{
			MainThreadDispatcher.Dispatch(action);
		});
	}
}
