using System;

namespace Wizards.Mtga.Diagnostics;

public interface IErrorReporter
{
	void ReportError(string message);

	void ReportError(Exception error);
}
