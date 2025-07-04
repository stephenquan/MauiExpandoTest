using System.Dynamic;
using System.Text.Json;

namespace MauiExpandoTest;

/// <summary>
/// Provides utility methods for working with JSON data and converting it into <see cref="ExpandoObject"/> instances.
/// </summary>
/// <remarks>This class is designed to facilitate the conversion of JSON strings into dynamic objects represented
/// by <see cref="ExpandoObject"/>. The resulting <see cref="ExpandoObject"/> can be used to dynamically access
/// properties and nested structures.</remarks>
public static class ExpandoObjectHelper
{
	/// <summary>
	/// Converts a JSON string into an <see cref="ExpandoObject"/> representation.
	/// </summary>
	/// <param name="json">The JSON string to convert. Must represent a JSON object.</param>
	/// <returns>An <see cref="ExpandoObject"/> containing the properties and values from the JSON object. If the JSON string is
	/// empty or does not represent a valid JSON object, an empty <see cref="ExpandoObject"/> is returned.</returns>
	public static ExpandoObject ConvertFromJson(string json)
	{
		ExpandoObject obj = new ExpandoObject();
		JsonDocument? doc = JsonDocument.Parse(json);
		if (doc is not null && doc.RootElement.ValueKind == JsonValueKind.Object)
		{
			ConvertFromJson(obj, doc.RootElement.EnumerateObject());
		}
		return obj;
	}

	/// <summary>
	/// Converts a JSON object represented by a <see cref="JsonElement.ObjectEnumerator"/> into an <see
	/// cref="ExpandoObject"/>.
	/// </summary>
	/// <remarks>This method recursively processes JSON objects and arrays, converting them into nested <see
	/// cref="ExpandoObject"/> instances or <see cref="List{T}"/> collections, respectively. Supported JSON value types
	/// include strings, numbers, booleans, objects, and arrays. Unsupported or unrecognized JSON value types are
	/// ignored.</remarks>
	/// <param name="obj">The <see cref="ExpandoObject"/> that will be populated with the properties and values from the JSON object. Must be
	/// an instance of <see cref="IDictionary{TKey, TValue}"/>.</param>
	/// <param name="objectEnumerator">An enumerator for the JSON object's properties, typically obtained from <see cref="JsonElement.EnumerateObject"/>.</param>
	static void ConvertFromJson(ExpandoObject obj, JsonElement.ObjectEnumerator objectEnumerator)
	{
		if (obj is not IDictionary<string, object> dict)
		{
			return;
		}
		foreach (var property in objectEnumerator)
		{
			switch (property.Value.ValueKind)
			{
				case JsonValueKind.String:
					if (property.Value.GetString() is string str)
					{
						dict[property.Name] = str;
					}
					break;

				case JsonValueKind.Number:
					if (property.Value.TryGetInt32(out int intValue))
					{
						dict[property.Name] = intValue;
					}
					else if (property.Value.TryGetInt64(out long longValue))
					{
						dict[property.Name] = longValue;
					}
					else if (property.Value.TryGetSingle(out float floatValue))
					{
						dict[property.Name] = floatValue;
					}
					else if (property.Value.TryGetDouble(out double doubleValue))
					{
						dict[property.Name] = doubleValue;
					}
					else
					{
						//dict[property.Name] = property.Value.GetString();
					}
					break;

				case JsonValueKind.True:
				case JsonValueKind.False:
					dict[property.Name] = property.Value.GetBoolean();
					break;

				case JsonValueKind.Object:
					var nestedObj = new ExpandoObject();
					ConvertFromJson(nestedObj, property.Value.EnumerateObject());
					dict[property.Name] = nestedObj;
					break;

				case JsonValueKind.Array:
					var nestedArray = new List<ExpandoObject>();
					for (int i = 0; i < property.Value.GetArrayLength(); i++)
					{
						var value = property.Value[i];
						switch (value.ValueKind)
						{
							case JsonValueKind.Object:
								var newArrayItem = new ExpandoObject();
								ConvertFromJson(newArrayItem, value.EnumerateObject());
								nestedArray.Add(newArrayItem);
								break;
						}
					}
					dict[property.Name] = nestedArray;
					break;
			}
		}
	}
}
