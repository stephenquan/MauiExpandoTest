using System.Dynamic;
using System.Text.Json;

namespace MauiExpandoTest;

/// <summary>
/// Provides utility methods for working with JSON data and converting it into <see cref="ExpandoObject"/> instances.
/// </summary>
/// <remarks>This class is designed to facilitate the conversion of JSON strings into dynamic objects represented
/// by <see cref="ExpandoObject"/>. The resulting <see cref="ExpandoObject"/> can be used to dynamically access
/// properties and nested structures.</remarks>
public static class DynamicHelper
{
	/// <summary>
	/// Converts a JSON string into a dynamic object representation.
	/// </summary>
	/// <param name="json">The JSON string to be converted. Must represent a valid JSON object.</param>
	/// <returns>A dynamic object containing the data from the JSON string. The returned object will be empty if the JSON string
	/// does not represent a valid JSON object.</returns>
	public static dynamic ConvertFromJson(string json)
	{
		ExpandoObject obj = new ExpandoObject();
		JsonDocument? doc = JsonDocument.Parse(json);
		if (doc is not null && doc.RootElement.ValueKind == JsonValueKind.Object)
		{
			ConvertFromJsonObject(obj, doc.RootElement);
		}
		return obj;
	}

	/// <summary>
	/// Populates a dictionary with key-value pairs extracted from a JSON object.
	/// </summary>
	/// <remarks>This method recursively processes nested JSON objects and arrays. For JSON properties of type <see
	/// cref="JsonValueKind.Object"/>, a nested dictionary is created and populated. For JSON properties of type <see
	/// cref="JsonValueKind.Array"/>, a list is created and populated.  Supported JSON value types include strings,
	/// numbers, booleans, objects, and arrays. Null values are represented as <see langword="null"/> in the
	/// dictionary.</remarks>
	/// <param name="dict">The dictionary to populate with data from the JSON object. Keys are derived from the property names in the JSON
	/// object, and values are converted based on the JSON property's type.</param>
	/// <param name="jsonObject">The JSON object to convert. Each property in the JSON object is processed and added to the dictionary.</param>
	public static void ConvertFromJsonObject(IDictionary<string, object?> dict, JsonElement jsonObject)
	{
		foreach (var jsonProperty in jsonObject.EnumerateObject())
		{
			switch (jsonProperty.Value.ValueKind)
			{
				case JsonValueKind.String:
					dict[jsonProperty.Name] = ConvertFromJsonString(jsonProperty.Value) ?? null;
					break;

				case JsonValueKind.Number:
					dict[jsonProperty.Name] = ConvertFromJsonNumber(jsonProperty.Value) ?? null;
					break;

				case JsonValueKind.True:
				case JsonValueKind.False:
					dict[jsonProperty.Name] = jsonProperty.Value.GetBoolean();
					break;

				case JsonValueKind.Object:
					var nestedObj = new ExpandoObject();
					ConvertFromJsonObject(nestedObj, jsonProperty.Value);
					dict[jsonProperty.Name] = nestedObj;
					break;

				case JsonValueKind.Array:
					var nestedArray = new List<object?>();
					ConvertFromJson(nestedArray, jsonProperty.Value);
					dict[jsonProperty.Name] = nestedArray;
					break;
			}
		}
	}

	/// <summary>
	/// Converts a JSON array into a list of objects, recursively processing nested arrays and objects.
	/// </summary>
	/// <remarks>This method processes each element in the provided JSON array and converts it into an appropriate
	/// .NET object. Supported JSON value types include strings, numbers, objects, and arrays. Nested arrays and objects
	/// are recursively converted. Null values are added to the list for unsupported or null JSON values.</remarks>
	/// <param name="array">The list to populate with the converted objects. This list will be modified during the method execution.</param>
	/// <param name="jsonArray">The JSON array to convert. Must be a valid <see cref="JsonElement"/> representing an array.</param>
	public static void ConvertFromJson(List<object?> array, JsonElement jsonArray)
	{
		for (int i = 0; i < jsonArray.GetArrayLength(); i++)
		{
			var jsonValue = jsonArray[i];
			switch (jsonValue.ValueKind)
			{
				case JsonValueKind.String:
					array.Add(ConvertFromJsonString(jsonValue) ?? null);
					break;

				case JsonValueKind.Number:
					array.Add(ConvertFromJsonNumber(jsonValue) ?? null);
					break;

				case JsonValueKind.Object:
					var newArrayItem = new ExpandoObject();
					ConvertFromJsonObject(newArrayItem, jsonValue);
					array.Add(newArrayItem);
					break;

				case JsonValueKind.Array:
					var nestedArray = new List<object?>();
					ConvertFromJson(nestedArray, jsonValue);
					array.Add(nestedArray);
					break;
			}
		}
	}

	/// <summary>
	/// Converts a JSON number represented by a <see cref="JsonElement"/> into its corresponding .NET numeric type.
	/// </summary>
	/// <remarks>This method attempts to convert the JSON number to the most appropriate .NET numeric type based on
	/// its precision. If the number fits within the range of an <see cref="int"/>, it will be returned as an <see
	/// cref="int"/>. If it exceeds the range of an <see cref="int"/> but fits within a <see cref="long"/>, it will be
	/// returned as a <see cref="long"/>. For floating-point numbers, the method will return a <see cref="float"/> or <see
	/// cref="double"/> depending on the precision.</remarks>
	/// <param name="number">A <see cref="JsonElement"/> containing the JSON number to convert. The element must represent a valid numeric
	/// value.</param>
	/// <returns>The converted numeric value as an <see cref="object"/>. The return type will be one of the following: <see
	/// cref="int"/>, <see cref="long"/>, <see cref="float"/>, or <see cref="double"/>, depending on the precision of the
	/// JSON number. Returns <see langword="null"/> if the JSON number cannot be converted.</returns>
	public static object? ConvertFromJsonNumber(JsonElement number)
	{
		if (number.TryGetInt32(out int intValue))
		{
			return intValue;
		}
		if (number.TryGetInt64(out long longValue))
		{
			return longValue;
		}
		else if (number.TryGetSingle(out float floatValue))
		{
			return floatValue;
		}
		else if (number.TryGetDouble(out double doubleValue))
		{
			return doubleValue;
		}
		return null;
	}

	/// <summary>
	/// Converts a <see cref="JsonElement"/> representing a JSON string into a .NET string.
	/// </summary>
	/// <remarks>This method returns <see langword="null"/> if the <paramref name="jsonElement"/> does not represent
	/// a JSON string.</remarks>
	/// <param name="jsonElement">The <see cref="JsonElement"/> to convert. Must have a <see cref="JsonValueKind"/> of <see
	/// cref="JsonValueKind.String"/>.</param>
	/// <returns>The string value contained in the <paramref name="jsonElement"/> if its <see cref="JsonValueKind"/> is <see
	/// cref="JsonValueKind.String"/>;  otherwise, <see langword="null"/>.</returns>
	public static string? ConvertFromJsonString(JsonElement jsonElement)
	{
		if (jsonElement.ValueKind == JsonValueKind.String)
		{
			return jsonElement.GetString();
		}
		return null;
	}
}
