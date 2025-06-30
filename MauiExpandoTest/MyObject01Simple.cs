using System.ComponentModel;
using System.Dynamic;
using System.Text.Json;

namespace MauiExpandoTest;

public partial class MyObject01Simple : DynamicObject, INotifyPropertyChanged
{
	ExpandoObject InternalValue { get; } = new();
	public dynamic Dynamic => (dynamic)InternalValue;

	public MyObject01Simple()
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
		if (dict.TryGetValue(key, out object? value))
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

		foreach (var property in doc.RootElement.EnumerateObject())
		{
			switch (property.Value.ValueKind)
			{
				case JsonValueKind.String:
					SetPropertyValue(property.Name, property.Value.GetString());
					break;
				case JsonValueKind.Number:
					if (property.Value.TryGetInt32(out int intValue))
					{
						SetPropertyValue(property.Name, intValue);
					}
					else if (property.Value.TryGetDouble(out double doubleValue))
					{
						SetPropertyValue(property.Name, doubleValue);
					}
					else
					{
						SetPropertyValue(property.Name, property.Value.ToString());
					}
					break;
				case JsonValueKind.True:
				case JsonValueKind.False:
					SetPropertyValue(property.Name, property.Value.GetBoolean());
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
}

