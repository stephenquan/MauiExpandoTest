namespace MauiExpandoTest;

using System.Collections;
using System.ComponentModel;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// 
/// </summary>
public partial class DynamicProperties : DynamicObject, INotifyPropertyChanged, IEnumerable<KeyValuePair<string, object?>>, IDictionary<string, object?>
{
	Dictionary<string, object?> dict = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicProperties"/> class.
	/// </summary>
	/// <remarks>This constructor sets up the dynamic properties by registering JSON property changes. Ensure that
	/// the <c>dict</c> object is properly initialized before calling this constructor.</remarks>
	public DynamicProperties()
	{
		RegisterJsonPropertyChanges(dict);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicProperties"/> class using the specified JSON string.
	/// </summary>
	/// <remarks>The JSON string is parsed and converted into a dictionary of dynamic properties. Changes to the
	/// properties are tracked automatically after initialization.</remarks>
	/// <param name="json">A JSON-formatted string representing the dynamic properties to initialize. Cannot be null or empty.</param>
	public DynamicProperties(string json)
	{
		DynamicPropertiesJsonHelper.ConvertFromJson(dict, json);
		RegisterJsonPropertyChanges(dict);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicProperties"/> class using the specified JSON document.
	/// </summary>
	/// <remarks>This constructor parses the provided JSON document and converts its properties into a dynamic
	/// dictionary. Ensure that the <paramref name="doc"/> parameter is not null and contains a valid JSON
	/// object.</remarks>
	/// <param name="doc">A <see cref="JsonDocument"/> containing the JSON data to initialize the dynamic properties. The document must
	/// represent a valid JSON object.</param>
	public DynamicProperties(JsonDocument doc)
	{
		DynamicPropertiesJsonHelper.ConvertFromJsonDocument(dict, doc);
		RegisterJsonPropertyChanges(dict);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicProperties"/> class with the specified dictionary.
	/// </summary>
	/// <remarks>The provided dictionary is copied into an internal dictionary to ensure encapsulation. Changes to
	/// the original dictionary after initialization will not affect the <see cref="DynamicProperties"/>
	/// instance.</remarks>
	/// <param name="dict">A dictionary containing the initial set of dynamic properties. Keys represent property names, and values represent
	/// their corresponding values.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="dict"/> is <see langword="null"/>.</exception>
	public DynamicProperties(IDictionary<string, object?> dict)
	{
		if (dict is null)
		{
			throw new ArgumentNullException(nameof(dict), "Dictionary cannot be null.");
		}
		this.dict = new Dictionary<string, object?>(dict);
		RegisterJsonPropertyChanges(this.dict);
	}

	/// <summary>
	/// Retrieves the value of a property by its name.
	/// </summary>
	/// <remarks>This method attempts to retrieve the value associated with the specified property name  from an
	/// internal dictionary. If the property name does not exist in the dictionary,  the method returns <see
	/// langword="null"/>.</remarks>
	/// <param name="propertyName">The name of the property whose value is to be retrieved.  This parameter cannot be null or empty.</param>
	/// <returns>The value of the property if found; otherwise, <see langword="null"/>.</returns>
	public object? GetPropertyValue(string propertyName)
	{
		return dict.TryGetValue(propertyName, out var value) ? value : null;
	}

	/// <summary>
	/// Sets the value of a specified property in the internal dictionary.
	/// </summary>
	/// <remarks>If the property already exists in the dictionary and the new value is equal to the current value, 
	/// no changes are made. If the value is updated or the property is added or removed,  the method raises property
	/// change notifications for the specified property and the JSON representation.</remarks>
	/// <param name="propertyName">The name of the property to set. Cannot be null or empty.</param>
	/// <param name="value">The value to assign to the property. If <paramref name="value"/> is <see langword="null"/>,  the property is
	/// removed from the dictionary.</param>
	public void SetPropertyValue(string propertyName, object? value)
	{
		if (value is null)
		{
			if (dict.ContainsKey(propertyName))
			{
				dict.Remove(propertyName);
				OnPropertyChanged($"Item[{propertyName}]");
			}
			return;
		}

		if (dict.ContainsKey(propertyName))
		{
			object? currentValue = dict[propertyName];
			if (currentValue is not null && currentValue.Equals(value))
			{
				return; // No change needed
			}
			// Update the existing value
			dict[propertyName] = value;
			OnPropertyChanged($"Item[{propertyName}]");
			return;
		}

		dict.Add(propertyName, value);
		OnPropertyChanged($"Item[{propertyName}]");
	}

	/// <summary>
	/// Gets or sets the value of the property with the specified name.
	/// </summary>
	/// <param name="propertyName">The name of the property to retrieve or set. Cannot be null or empty.</param>
	/// <returns></returns>
	public object? this[string propertyName]
	{
		get => GetPropertyValue(propertyName);
		set => SetPropertyValue(propertyName, value);
	}

	/// <summary>
	/// Gets or sets the JSON representation of the underlying dictionary.
	/// </summary>
	/// <remarks>Setting this property updates the dictionary by deserializing the provided JSON string. Any
	/// existing entries in the dictionary are cleared before the update. Changes to the dictionary trigger the  <see
	/// cref="OnPropertyChanged"/> event.</remarks>
	public string Json
	{
		get => System.Text.Json.JsonSerializer.Serialize(dict, new System.Text.Json.JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
		});
	}

	void RegisterJsonPropertyChanges(IDictionary<string, object?> dict)
	{
		if (dict is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Json));
		}
		foreach (var kvp in dict)
		{
			if (kvp.Value is IDictionary<string, object?> nestedDict)
			{
				RegisterJsonPropertyChanges(nestedDict);
				continue;
			}
			if (kvp.Value is IList<object?> nestedList)
			{
				foreach (var item in nestedList)
				{
					if (item is IDictionary<string, object?> itemDict)
					{
						RegisterJsonPropertyChanges(itemDict);
					}
				}
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="propertyName"></param>
	public void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="binder"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public override bool TryGetMember(GetMemberBinder binder, out object? result)
	{
		result = GetPropertyValue(binder.Name);
		return true;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="binder"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public override bool TrySetMember(SetMemberBinder binder, object? value)
	{
		SetPropertyValue(binder.Name, value);
		return true;
	}

	/// <summary>
	/// 
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
	{
		foreach (var kvp in dict)
		{
			yield return kvp;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <exception cref="NotImplementedException"></exception>
	public void Add(string key, object? value) => dict.Add(key, value);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public bool ContainsKey(string key) => dict.ContainsKey(key);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public bool Remove(string key) => dict.Remove(key);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public bool TryGetValue(string key, out object? value) => dict.TryGetValue(key, out value);

	/// <summary>
	/// 
	/// </summary>
	[JsonIgnore]
	public ICollection<string> Keys => dict.Keys;

	/// <summary>
	/// 
	/// </summary>
	[JsonIgnore]
	public ICollection<object?> Values => dict.Values;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="item"></param>
	/// <exception cref="NotImplementedException"></exception>
	public void Add(KeyValuePair<string, object?> item) => dict.Add(item.Key, item.Value);

	/// <summary>
	/// 
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	public void Clear() => dict.Clear();

	/// <summary>
	/// 
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public bool Contains(KeyValuePair<string, object?> item) => dict.Contains(item);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="array"></param>
	/// <param name="arrayIndex"></param>
	/// <exception cref="NotImplementedException"></exception>
	public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public bool Remove(KeyValuePair<string, object?> item)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// 
	/// </summary>
	[JsonIgnore]
	public int Count => dict.Count;

	/// <summary>
	/// 
	/// </summary>
	[JsonIgnore]
	public bool IsReadOnly => false;
}
