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
	/// Gets the root dictionary containing key-value pairs where keys are strings and values are objects.
	/// </summary>
	/// <remarks>This property provides access to the underlying data structure used for storing
	/// application-specific information.</remarks>
	public IDictionary<string, object?>? RootDict { get; }

	/// <summary>
	/// Gets the underlying dictionary used to store key-value pairs.
	/// </summary>
	/// <remarks>This property provides access to the internal dictionary for retrieving stored data.</remarks>
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
	/// Initializes a new instance of the <see cref="ObservableDictValue"/> class, navigating through a hierarchical
	/// dictionary structure based on the specified path.
	/// </summary>
	/// <remarks>This constructor navigates through the provided dictionary hierarchy using the specified path. If
	/// the path includes array indexing (e.g., "key[index]"), it attempts to access the corresponding list and retrieve
	/// the dictionary at the specified index.  If the path cannot be fully resolved, the traversal stops at the last valid
	/// point. If the resolved dictionary implements <see cref="INotifyPropertyChanged"/>, changes to its properties will
	/// trigger <see cref="PropertyChanged"/> events for this instance.</remarks>
	/// <param name="rootDict">The root dictionary to start navigation from. Can be <see langword="null"/>.</param>
	/// <param name="path">The dot-separated path used to traverse the dictionary hierarchy. Supports array indexing in the format
	/// "key[index]".</param>
	public ObservableDictValue(IDictionary<string, object?>? rootDict, string path)
	{
		this.RootDict = rootDict;
		this.InnerDict = rootDict;
		Path = path;

		var keys = path.Split('.');

		foreach (string key in path.Split("."))
		{
			if (string.IsNullOrEmpty(key))
			{
				continue;
			}
			if (InnerDict is null)
			{
				return;
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
