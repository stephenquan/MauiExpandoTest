using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiExpandoTest;

public partial class MyObject05WithArray : DynamicObject, INotifyPropertyChanged, IDictionary<string, object>
{
	ExpandoObject InternalValue { get; } = new();

	[JsonIgnore]
	public dynamic Dynamic => (dynamic)InternalValue;

	static string kDelimiter = ":";

	public MyObject05WithArray()
	{
		((INotifyPropertyChanged)InternalValue).PropertyChanged += (s, e) =>
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{e.PropertyName}]"));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
		};
	}

	public object? GetPropertyValue(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		if (InternalValue is not IDictionary<string, object> dict)
		{
			return null;
		}
		var keys = key.Split(kDelimiter);
		if (keys.Length > 1)
		{
			return GetPropertyValue(dict, keys);
		}
		if (dict.TryGetValue(key, out object? value))
		{
			return value;
		}
		return null;
	}

	object? GetPropertyValue(IDictionary<string, object> dict, string[] keys)
	{
		for (int i = 0; i < keys.Length - 1; i++)
		{
			if (dict.TryGetValue(keys[i], out object? subObject) && subObject is IDictionary<string, object> subDict)
			{
				dict = subDict;
				continue;
			}
			return null;
		}
		string lastKey = keys[^1];
		if (dict.TryGetValue(lastKey, out object? value))
		{
			return value;
		}
		return null;
	}

	public void SetPropertyValue(string key, object? value)
	{
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		if (InternalValue is not IDictionary<string, object> dict)
		{
			return;
		}
		var keys = key.Split(kDelimiter);
		if (keys.Length > 1)
		{
			SetNestedPropertyValue(dict, keys, value);
			return;
		}
		if (value is null)
		{
			if (dict.ContainsKey(key))
			{
				dict.Remove(key);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
			}
			return;
		}
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, value);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
			return;
		}
		object oldValue = dict[key];
		if (oldValue.Equals(value))
		{
			return;
		}
		dict[key] = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
	}

	void SetNestedPropertyValue(IDictionary<string, object> dict, string[] keys, object? value)
	{
		string prefix = "";
		string delimiter = "";
		for (int i = 0; i < keys.Length - 1; i++)
		{
			string key = keys[i];
			if (dict.TryGetValue(key, out object? subObject) && subObject is IDictionary<string, object> subDict)
			{
				dict = subDict;
			}
			else
			{
				var newObject = new ExpandoObject();
				if (newObject is not IDictionary<string, object> newDict)
				{
					return;
				}
				((INotifyPropertyChanged)newObject).PropertyChanged += (s, e) =>
				{
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{prefix}{delimiter}{e.PropertyName}]"));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
				};
				dict[key] = newObject;
				dict = newDict;
			}
			prefix += delimiter + key;
			delimiter = kDelimiter;
		}
		string lastKey = keys[^1];
		if (value is null)
		{
			if (dict.ContainsKey(lastKey))
			{
				dict.Remove(lastKey);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{prefix + delimiter + lastKey}]"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
			}
			return;
		}
		if (!dict.ContainsKey(lastKey))
		{
			dict.Add(lastKey, value);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{prefix + delimiter + lastKey}]"));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
			return;
		}
		object oldValue = dict[lastKey];
		if (oldValue.Equals(value))
		{
			return;
		}
		dict[lastKey] = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{prefix + delimiter + lastKey}]"));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
	}

	public bool IsEmpty()
		=> (InternalValue is not IDictionary<string, object> dict || dict.Count == 0);

	public void Clear()
	{
		if (InternalValue is IDictionary<string, object> dict)
		{
			if (dict.Count > 0)
			{
				dict.Clear();
				Invalidate();
				return;
			}
		}
	}

	public override bool TryGetMember(GetMemberBinder binder, out object? result)
	{
		result = GetPropertyValue(binder.Name);
		return true;
	}

	public override bool TrySetMember(SetMemberBinder binder, object? value)
	{
		SetPropertyValue(binder.Name, value);
		return true;
	}

	public object? this[string key]
	{
		get => GetPropertyValue(key);
		set => SetPropertyValue(key, value);
	}

	public string GetJson()
	{
		return System.Text.Json.JsonSerializer.Serialize(InternalValue, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
	}

	public void SetJson(string json)
	{
		Clear();

		if (string.IsNullOrEmpty(json))
		{
			return;
		}

		JsonDocument? doc = JsonDocument.Parse(json);
		if (doc is null || doc.RootElement.ValueKind != JsonValueKind.Object)
		{
			return;
		}

		SetJsonObject(InternalValue, doc.RootElement.EnumerateObject());
	}

	void SetJsonObject(ExpandoObject obj, JsonElement.ObjectEnumerator objectEnumerator, string prefix = "", string delimiter = "")
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
					dict[property.Name] = property.Value.GetString() ?? string.Empty;
					break;
				case JsonValueKind.Number:
					if (property.Value.TryGetInt32(out int intValue))
					{
						dict[property.Name] = intValue;
					}
					else if (property.Value.TryGetDouble(out double doubleValue))
					{
						dict[property.Name] = doubleValue;
					}
					break;
				case JsonValueKind.True:
				case JsonValueKind.False:
					dict[property.Name] = property.Value.GetBoolean();
					break;
				case JsonValueKind.Object:
					var newObject = new ExpandoObject();
					((INotifyPropertyChanged)newObject).PropertyChanged += (s, e) =>
					{
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{prefix + delimiter + property.Name}{kDelimiter}{e.PropertyName}]"));
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
					};
					SetJsonObject(newObject, property.Value.EnumerateObject(), prefix + delimiter + property.Name, kDelimiter);
					dict[property.Name] = newObject;
					break;
				case JsonValueKind.Array:
					var newArray = new List<MyObject05WithArray>();
					for (int i = 0; i < property.Value.GetArrayLength(); i++)
					{
						if (property.Value[i].ValueKind == JsonValueKind.Object)
						{
							string json = System.Text.Json.JsonSerializer.Serialize(property.Value[i]);
							var newArrayItem = new MyObject05WithArray();
							newArrayItem.SetJson(json);
							((INotifyPropertyChanged)newArrayItem).PropertyChanged += (s, e) =>
							{
								PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{prefix + delimiter + property.Name}{kDelimiter}{i}{kDelimiter}{e.PropertyName}{kDelimiter}{i}]"));
								PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
							};
							//SetJsonObject(newArrayItem, property.Value[i].EnumerateObject(), prefix + delimiter + property.Name + kDelimiter + i);
							newArray.Add(newArrayItem);
						}
					}
					dict[property.Name] = newArray;
					break;
			}
		}
	}

	public void Invalidate()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
	}

	public string Json
	{
		get => GetJson();
		set => SetJson(value);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public void Add(string key, object value)
	{
		throw new NotImplementedException();
	}

	bool IDictionary<string, object>.ContainsKey(string key)
	{
		throw new NotImplementedException();
	}

	bool IDictionary<string, object>.Remove(string key)
	{
		throw new NotImplementedException();
	}

	bool IDictionary<string, object>.TryGetValue(string key, [MaybeNullWhen(false)] out object value)
	{
		throw new NotImplementedException();
	}

	[JsonIgnore]
	object IDictionary<string, object>.this[string key]
	{
		get => throw new NotImplementedException();
		set => throw new NotImplementedException();
	}

	[JsonIgnore]
	ICollection<string> IDictionary<string, object>.Keys
		=> throw new NotImplementedException();

	[JsonIgnore]
	ICollection<object> IDictionary<string, object>.Values
		=> throw new NotImplementedException();

	public void Add(KeyValuePair<string, object> item)
	{
		throw new NotImplementedException();
	}

	/*
    void IDictionary<string, object>.Clear()
    {
        throw new NotImplementedException();
    }
    */

	public bool Contains(KeyValuePair<string, object> item)
	{
		throw new NotImplementedException();
	}

	public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	public bool Remove(KeyValuePair<string, object> item)
	{
		throw new NotImplementedException();
	}

	[JsonIgnore]
	public int Count
		=> throw new NotImplementedException();

	[JsonIgnore]
	public bool IsReadOnly
		=> throw new NotImplementedException();

	public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		if (InternalValue is IDictionary<string, object> dict)
		{
			return dict.GetEnumerator();
		}
		return Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
