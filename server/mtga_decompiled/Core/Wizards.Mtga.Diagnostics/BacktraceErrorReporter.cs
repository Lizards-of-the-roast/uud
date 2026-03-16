using System;
using Backtrace.Unity;
using Backtrace.Unity.Model;

namespace Wizards.Mtga.Diagnostics;

public class BacktraceErrorReporter : IErrorReporter
{
	public void ReportError(string message)
	{
		BacktraceClient.Instance.Send(new BacktraceReport(message));
	}

	public void ReportError(Exception error)
	{
		BacktraceClient.Instance.Send(new BacktraceReport(error));
	}
}
