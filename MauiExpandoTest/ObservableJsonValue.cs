using System.ComponentModel;
using System.Dynamic;

namespace MauiExpandoTest;

/// <summary>
/// Represents an observable JSON value that dynamically reflects changes to its underlying data structure.
/// </summary>
/// <remarks>This class wraps an <see cref="ExpandoObject"/> and provides a JSON representation of its contents.
/// The JSON output is automatically updated whenever the underlying data structure changes, including nested objects
/// and collections. Changes to properties or items within the <see cref="RootDict"/> trigger the <see
/// cref="PropertyChanged"/> event for the <see cref="Json"/> property.</remarks>
public partial class ObservableJsonValue : INotifyPropertyChanged
{
	/// <summary>
	/// Gets the root dictionary containing key-value pairs for configuration or data storage.
	/// </summary>
	/// <remarks>This property is typically used to store or retrieve structured data in a flexible format. Keys
	/// should be unique within the dictionary, and values can represent various types of data.</remarks>
	public IDictionary<string, object?>? RootDict { get; }

	/// <summary>
	/// Gets the JSON representation of the <see cref="RootDict"/>.
	/// </summary>
	public string Json => System.Text.Json.JsonSerializer.Serialize(RootDict, new System.Text.Json.JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
	});

	/// <summary>
	/// Initializes a new instance of the <see cref="ObservableJsonValue"/> class, which observes changes to a JSON-like
	/// dictionary structure.
	/// </summary>
	/// <remarks>This class is designed to monitor changes in a JSON-like dictionary structure and raise property
	/// change notifications. If the provided dictionary implements <see cref="INotifyPropertyChanged"/>, the <see
	/// cref="Json"/> property will automatically update when the dictionary changes.</remarks>
	/// <param name="rootDict">The root dictionary to observe. If the dictionary implements <see cref="INotifyPropertyChanged"/>, changes to its
	/// properties will trigger updates.</param>
	public ObservableJsonValue(IDictionary<string, object?>? rootDict)
	{
		this.RootDict = rootDict;

		if (rootDict is INotifyPropertyChanged rootNotify)
		{
			rootNotify.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
		}

		if (rootDict is not null)
		{
			Subscribe(rootDict);
		}
	}

	/// <summary>
	/// Subscribes to property change notifications for the objects contained within the specified dictionary.
	/// </summary>
	/// <remarks>This method recursively traverses the dictionary and its nested structures, subscribing to property
	/// change  events for any objects that implement <see cref="INotifyPropertyChanged"/>. When a property change occurs, 
	/// the <see cref="PropertyChanged"/> event is raised for the current instance, with the property name set to "Json". 
	/// Nested dictionaries and lists of dynamic objects (<see cref="ExpandoObject"/>) are also processed recursively. This
	/// ensures that property changes in deeply nested structures are captured.</remarks>
	/// <param name="dict">A dictionary containing objects to monitor for property changes. The values in the dictionary can be  objects
	/// implementing <see cref="INotifyPropertyChanged"/>, nested dictionaries, or lists of dynamic objects.</param>
	void Subscribe(IDictionary<string, object?> dict)
	{
		foreach (var kvp in dict)
		{
			if (kvp.Value is INotifyPropertyChanged innerNotify)
			{
				innerNotify.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
			}
			if (kvp.Value is IDictionary<string, object?> innerDict)
			{
				Subscribe(innerDict);
			}
			if (kvp.Value is IList<object?> innerList)
			{
				foreach (var item in innerList)
				{
					if (item is INotifyPropertyChanged itemNotify)
					{
						itemNotify.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Json)));
					}
					if (item is IDictionary<string, object?> itemDict)
					{
						Subscribe(itemDict);
					}
				}
			}
		}
	}

	/// <summary>
	/// Occurs when a property value changes.
	/// </summary>
	/// <remarks>This event is typically used to notify subscribers that a property of the object has been updated.
	/// It is commonly implemented in classes that support data binding or observable patterns.</remarks>
	public event PropertyChangedEventHandler? PropertyChanged;
}
