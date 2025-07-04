using System.Dynamic;
using System.Globalization;

namespace MauiExpandoTest;

/// <summary>
/// Provides conversion logic for transforming an <see cref="ExpandoObject"/> into an observable dictionary-like
/// structure.
/// </summary>
/// <remarks>This converter is typically used in data binding scenarios where an <see cref="ExpandoObject"/> needs
/// to be represented as an observable dictionary with support for dynamic keys.</remarks>
public class ObservableDictConverter : IValueConverter
{
	/// <summary>
	/// Converts the specified value into an observable dictionary value based on the provided parameters.
	/// </summary>
	/// <remarks>This method is designed to convert <see cref="ExpandoObject"/> instances into observable dictionary
	/// values. If the <paramref name="parameter"/> is a string, it is used as the key for the conversion; otherwise, an
	/// empty key is applied.</remarks>
	/// <param name="value">The value to convert. Must be an <see cref="ExpandoObject"/> to perform the conversion.</param>
	/// <param name="targetType">The target type of the conversion. This parameter is not used in the current implementation.</param>
	/// <param name="parameter">An optional parameter specifying the key to use during conversion. If not a string, an empty key is used.</param>
	/// <param name="culture">The culture information to use during conversion. This parameter is not used in the current implementation.</param>
	/// <returns>An instance of <see cref="ObservableDictValue"/> if <paramref name="value"/> is an <see cref="ExpandoObject"/>;
	/// otherwise, <see langword="null"/>.</returns>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is IDictionary<string, object?> dict)
		{
			return new ObservableDictValue(dict, parameter is string key ? key : string.Empty);
		}

		return null;
	}

	/// <summary>
	/// Converts a value back to its source type in a data binding scenario.
	/// </summary>
	/// <param name="value">The value produced by the binding target to be converted.</param>
	/// <param name="targetType">The type of the binding source property.</param>
	/// <param name="parameter">An optional parameter to use during the conversion process.</param>
	/// <param name="culture">The culture to use in the conversion.</param>
	/// <returns>The converted value, or <see langword="null"/> if the conversion cannot be performed.</returns>
	/// <exception cref="NotImplementedException">This method is not implemented.</exception>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
