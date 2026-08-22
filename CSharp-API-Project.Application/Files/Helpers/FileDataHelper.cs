using System.Text.Json;
using System.Text.Json.Nodes;

namespace CSharpAPIProject.Application.Files.Helpers;

public static class FileDataHelper
{
	public static string? AddFileUri(string? data)
	{
		if (!TryParse(data, out var obj))
			return data;

		var storedFileId = GetStoredFileId(obj);
		if (storedFileId == null)
			return data;

		obj!["fileUri"] = $"/api/Files/{storedFileId}";
		return obj.ToJsonString();
	}

	public static Guid? GetStoredFileId(string? data)
	{
		if (!TryParse(data, out var obj))
			return null;

		return GetStoredFileId(obj);
	}

	private static Guid? GetStoredFileId(JsonObject? obj)
	{
		var property = obj!.FirstOrDefault(p => string.Equals(p.Key, "storedFileId", StringComparison.OrdinalIgnoreCase));

		if (property.Key == null || !Guid.TryParse(property.Value?.ToString(), out var storedFileId) || storedFileId == Guid.Empty)
			return null;

		return storedFileId;
	}

	private static bool TryParse(string? data, out JsonObject? obj)
	{
		obj = null;

		if (string.IsNullOrWhiteSpace(data))
			return false;

		try
		{
			obj = JsonNode.Parse(data) as JsonObject;
			return obj != null;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
