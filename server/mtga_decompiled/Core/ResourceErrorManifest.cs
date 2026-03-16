using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;

public class ResourceErrorManifest
{
	private class ResourceError
	{
		public string ErrorType;

		public string Message;

		public Dictionary<string, string> Details;

		public uint Count;
	}

	private List<ResourceError> errors = new List<ResourceError>();

	public int ErrorCount => errors.Count;

	public void AddErrorMessage(string errorType, string message, Dictionary<string, string> details)
	{
		if (details == null)
		{
			details = new Dictionary<string, string>();
		}
		IClientVersionInfo versionInfo = Global.VersionInfo;
		details["clientVersion"] = versionInfo.GetFullVersionString() + "-" + versionInfo.Platform;
		ResourceError resourceError = errors.Find(delegate(ResourceError x)
		{
			if (x.ErrorType != errorType)
			{
				return false;
			}
			if (x.Message != message)
			{
				return false;
			}
			if (x.Details.Count != details.Count)
			{
				return false;
			}
			foreach (KeyValuePair<string, string> detail in x.Details)
			{
				if (!details.ContainsKey(detail.Key))
				{
					return false;
				}
				if (details[detail.Key] != x.Details[detail.Key])
				{
					return false;
				}
			}
			return true;
		});
		if (resourceError == null)
		{
			errors.Add(new ResourceError
			{
				ErrorType = errorType,
				Message = message,
				Details = details,
				Count = 1u
			});
		}
		else
		{
			resourceError.Count++;
		}
	}

	public void BILogErrors(IBILogger biLogger)
	{
		foreach (ResourceError error in errors)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(error.Message);
			foreach (KeyValuePair<string, string> detail in error.Details)
			{
				stringBuilder.AppendLine($"K:{detail.Key}, V:{detail.Value}");
			}
			biLogger.Send(ClientBusinessEventType.ResourceError, new Wizards.Models.ClientBusinessEvents.ResourceError
			{
				Message = stringBuilder.ToString(),
				Error = error.ErrorType,
				EventTime = DateTime.UtcNow
			});
		}
	}

	public void ClearErrors()
	{
		errors.Clear();
	}

	public void SerializeToFile(string path)
	{
		List<JObject> list = new List<JObject>();
		foreach (ResourceError error in errors)
		{
			JObject jObject = new JObject();
			jObject.Add(new JProperty("ErrorType", error.ErrorType));
			jObject.Add(new JProperty("Message", error.Message));
			List<JObject> list2 = new List<JObject>();
			foreach (KeyValuePair<string, string> detail in error.Details)
			{
				list2.Add(new JObject(new JProperty("Key", detail.Key), new JProperty("Value", detail.Value)));
			}
			jObject.Add(new JProperty("Details", list2));
			jObject.Add(new JProperty("Count", error.Count));
			list.Add(jObject);
		}
		JObject jObject2 = new JObject();
		jObject2.Add(new JProperty("Errors", list));
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		using StreamWriter streamWriter = FileSystemUtils.CreateText(path);
		streamWriter.Write(jObject2.ToString(Formatting.None));
	}

	public static ResourceErrorManifest LoadFromFile(IBILogger biLogger, string path)
	{
		try
		{
			ResourceErrorManifest resourceErrorManifest = new ResourceErrorManifest();
			using (JsonTextReader jsonTextReader = new JsonTextReader(FileSystemUtils.OpenText(path)))
			{
				while (jsonTextReader.Read() && jsonTextReader.TokenType != JsonToken.StartArray)
				{
				}
				while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.StartObject)
				{
					jsonTextReader.Read();
					string errorType = jsonTextReader.ReadAsString();
					jsonTextReader.Read();
					string message = jsonTextReader.ReadAsString();
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					while (jsonTextReader.Read() && jsonTextReader.TokenType != JsonToken.StartArray)
					{
					}
					while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.StartObject)
					{
						jsonTextReader.Read();
						string key = jsonTextReader.ReadAsString();
						jsonTextReader.Read();
						string value = jsonTextReader.ReadAsString();
						dictionary.Add(key, value);
						jsonTextReader.Read();
					}
					jsonTextReader.Read();
					jsonTextReader.Read();
					long num = (long)jsonTextReader.Value;
					resourceErrorManifest.errors.Add(new ResourceError
					{
						ErrorType = errorType,
						Message = message,
						Details = dictionary,
						Count = (uint)num
					});
					jsonTextReader.Read();
				}
			}
			return resourceErrorManifest;
		}
		catch (Exception ex)
		{
			biLogger?.Send(ClientBusinessEventType.ResourceError, new Wizards.Models.ClientBusinessEvents.ResourceError
			{
				Message = "Failed to parse ResourceErrorManifest " + ex.Message,
				Error = ex.Message,
				EventTime = DateTime.UtcNow
			});
			return new ResourceErrorManifest();
		}
	}
}
