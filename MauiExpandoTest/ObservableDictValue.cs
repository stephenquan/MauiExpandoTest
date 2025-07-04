using System.ComponentModel;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace MauiExpandoTest;

/// <summary>
/// Represents a dynamic, observable dictionary-like structure that allows access to nested properties within an <see
/// cref="ExpandoObject"/> hierarchy.
/// </summary>
/// <remarks>This class provides indexed access to properties within a nested <see cref="ExpandoObject"/>
/// structure using a specified path. Changes to the dictionary values raise the <see
/// cref="INotifyPropertyChanged.PropertyChanged"/> event, enabling data binding scenarios.</remarks>
public partial class ObservableDictValue : INotifyPropertyChanged
{
	/// <summary>
	/// Gets the root object that serves as a dynamic container for storing key-value pairs.
	/// </summary>
	/// <remarks>The <see cref="ExpandoObject"/> is particularly useful for scenarios where the structure of the 
	/// data is not known at compile time. It can be used to store and manipulate dynamic data.</remarks>
	public ExpandoObject RootObject { get; }

	/// <summary>
	/// Gets the underlying dictionary used to store key-value pairs.
	/// </summary>
	/// <remarks>This property provides access to the internal dictionary for retrieving stored data. Modifications
	/// to the dictionary are not allowed directly through this property.</remarks>
	public IDictionary<string, object?>? InnerDict { get; }

	/// <summary>
	/// Gets the relative path to the current object within the <see cref="ExpandoObject"/> hierarchy.
	/// </summary>
	public string Path { get; } = string.Empty;

	/// <summary>
	/// Gets or sets the value associated with the specified key in the underlying dictionary.
	/// </summary>
	/// <remarks>When retrieving a value, if the key does not exist or the dictionary is null, the getter returns
	/// <see langword="null"/>. When setting a value, if the key is null or empty, the operation is ignored. If the value
	/// is <see langword="null"/>, the key is removed from the dictionary. Changes to the dictionary trigger the <see
	/// cref="PropertyChanged"/> event with the key as part of the event argument.</remarks>
	/// <param name="key">The key whose associated value is to be retrieved or set. Cannot be <see langword="null"/> or empty.</param>
	/// <returns></returns>
	public object? this[string key]
	{
		get
		{
			if (InnerDict is null || string.IsNullOrEmpty(key))
			{
				return null;
			}
			if (InnerDict.TryGetValue(key, out object? value))
			{
				return value;
			}
			return null;
		}
		set
		{
			if (InnerDict is null || string.IsNullOrEmpty(key))
			{
				return;
			}
			if (value is null)
			{
				if (InnerDict.ContainsKey(key))
				{
					InnerDict.Remove(key);
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
				}
				return;
			}
			if (InnerDict.ContainsKey(key))
			{
				object? currentValue = InnerDict[key];
				if (currentValue is not null && currentValue.Equals(value))
				{
					return; // No change, do not raise PropertyChanged
				}
				InnerDict[key] = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
				return;
			}
			InnerDict.Add(key, value);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ObservableDictValue"/> class, which provides a mechanism to observe
	/// changes to a nested property within an <see cref="ExpandoObject"/>.
	/// </summary>
	/// <remarks>This constructor traverses the specified path within the <paramref name="rootObject"/> to locate
	/// the target nested property. If the property is found and implements <see cref="INotifyPropertyChanged"/>, changes
	/// to the property will trigger the <see cref="PropertyChanged"/> event.  The <paramref name="path"/> must be a valid
	/// dot-separated string representing keys in the nested dictionary structure. Empty or invalid keys are ignored during
	/// traversal.</remarks>
	/// <param name="rootObject">The root <see cref="ExpandoObject"/> that contains the nested property to observe.</param>
	/// <param name="path">The dot-separated path to the nested property within the <paramref name="rootObject"/>. Each segment of the path
	/// represents a key in a nested dictionary structure.</param>
	public ObservableDictValue(ExpandoObject rootObject, string path)
	{
		RootObject = rootObject;
		InnerDict = rootObject as IDictionary<string, object?>;
		Path = path;

		var keys = path.Split('.');

		if (InnerDict is null)
		{
			return;
		}

		foreach (string key in path.Split("."))
		{
			if (string.IsNullOrEmpty(key))
			{
				continue;
			}
			Match match = Regex.Match(key, @"^(\w+)\[(\d+)\]$");
			if (match.Success)
			{
				string propertyName = match.Groups[1].Value; // Extract the property name before the index
				int propertyIndex = int.Parse(match.Groups[2].Value); // Extract the index after the property name
				if (InnerDict.TryGetValue(propertyName, out object? value)
					&& value is IList<object?> list
					&& propertyIndex < list.Count
					&& list[propertyIndex] is IDictionary<string, object?> itemDict)
				{
					// If the value is a list, we can access the item at the specified index
					InnerDict = itemDict;
					continue;
				}
				return;
			}
			if (InnerDict.TryGetValue(key, out object? innerValue))
			{
				if (innerValue is IDictionary<string, object?> nestedObject)
				{
					InnerDict = nestedObject;
					continue;
				}
				return;
			}
		}

		if (InnerDict is INotifyPropertyChanged innerNotify)
		{
			innerNotify.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName is string propertyName)
				{
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{propertyName}]"));
				}
			};
		}
	}

	/// <summary>
	/// Occurs when a property value changes.
	/// </summary>
	/// <remarks>This event is typically used to notify subscribers that a property of the object has been updated.
	/// It is commonly implemented in classes that support data binding.</remarks>
	public event PropertyChangedEventHandler? PropertyChanged;
}
