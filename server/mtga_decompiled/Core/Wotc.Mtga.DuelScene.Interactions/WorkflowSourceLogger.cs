using System;
using System.Net.Http;
using System.Text;
using DuelSceneInernalInsights.Contracts;
using GreClient.Rules;
using Newtonsoft.Json;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.Interactions;

public class WorkflowSourceLogger : IWorkflowTranslator, IDisposable
{
	private const string INTERNAL_LOGGING_URL = "http://localhost:5001/SourceInteraction";

	private readonly HttpClient _httpClient = new HttpClient();

	private readonly IWorkflowTranslator _nestedTranslator;

	private readonly IEntityNameProvider<uint> _nameProvider;

	public WorkflowSourceLogger(IWorkflowTranslator nestedTranslator, IEntityNameProvider<uint> nameProvider)
	{
		_nestedTranslator = nestedTranslator ?? NullWorkflowTranslator.Default;
		_nameProvider = nameProvider ?? NullIdNameProvider.Default;
	}

	public WorkflowBase Translate(BaseUserRequest req)
	{
		WorkflowBase workflowBase = _nestedTranslator.Translate(req);
		LogWorkflowSource(req, workflowBase);
		return workflowBase;
	}

	private async void LogWorkflowSource(BaseUserRequest request, WorkflowBase workflow)
	{
		if (request == null || workflow == null)
		{
			return;
		}
		string name = _nameProvider.GetName(request.SourceId, formatted: false);
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		StringContent content = new StringContent(JsonConvert.SerializeObject(new SourceInteraction
		{
			Source = name,
			Request = request.ToString(),
			Workflow = workflow.ToString()
		}), Encoding.UTF8, "application/json");
		try
		{
			HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync("http://localhost:5001/SourceInteraction", content);
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				Debug.Log("Error sending data: " + httpResponseMessage.ReasonPhrase);
			}
		}
		catch (Exception arg)
		{
			Debug.Log($"Workflow Source Log Exception: {arg}");
		}
	}

	public void Dispose()
	{
		_httpClient.Dispose();
	}
}
