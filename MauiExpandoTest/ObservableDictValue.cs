using System.ComponentModel;
using System.Dynamic;

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
	/// Gets the underlying dynamic object that stores additional properties and values.
	/// </summary>
	/// <remarks>The <see cref="ExpandoObject"/> allows adding, removing, and modifying properties at runtime. This
	/// property is read-only and provides access to the dynamic object for advanced scenarios.</remarks>
	public ExpandoObject InnerObject { get; }

	/// <summary>
	/// Gets the relative path to the current object within the <see cref="ExpandoObject"/> hierarchy.
	/// </summary>
	public string Path { get; } = string.Empty;

	/// <summary>
	/// Gets or sets the value associated with the specified key in the underlying dictionary.
	/// </summary>
	/// <remarks>This indexer provides access to the values stored in the underlying dictionary represented by <see
	/// cref="InnerObject"/>. If the underlying object is not a dictionary, the getter will return <see langword="null"/>
	/// and the setter will perform no operation. Setting a value to <see langword="null"/> removes the key from the
	/// dictionary if it exists. Changes to the dictionary trigger the <see cref="PropertyChanged"/> event with the key
	/// formatted as "Item[key]".</remarks>
	/// <param name="key">The key whose associated value is to be retrieved or modified. Cannot be <see langword="null"/>.</param>
	/// <returns></returns>
	public object? this[string key]
	{
		get
		{
			if (InnerObject is not IDictionary<string, object> innerDict)
			{
				return null;
			}
			if (innerDict is not null && innerDict.TryGetValue(key, out object? value))
			{
				return value;
			}
			return null;
		}
		set
		{
			if (InnerObject is not IDictionary<string, object> innerDict)
			{
				return;
			}
			if (value is null)
			{
				if (innerDict.ContainsKey(key))
				{
					innerDict.Remove(key);
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
				}
				return;
			}
			if (innerDict.ContainsKey(key))
			{
				if (innerDict[key].Equals(value))
				{
					return; // No change, do not raise PropertyChanged
				}
				innerDict[key] = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
				return;
			}
			innerDict.Add(key, value);
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
		InnerObject = rootObject;
		Path = path;

		var keys = path.Split('.');

		foreach (string key in path.Split("."))
		{
			if (string.IsNullOrEmpty(key))
			{
				continue;
			}
			if (InnerObject is IDictionary<string, object> innerDict
				&& innerDict.TryGetValue(key, out object? innerValue)
				&& innerValue is ExpandoObject nestedObject)
			{
				InnerObject = nestedObject;
			}
		}

		if (InnerObject is INotifyPropertyChanged innerNotify)
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
