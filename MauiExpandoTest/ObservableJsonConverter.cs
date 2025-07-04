using System.Dynamic;
using System.Globalization;

namespace MauiExpandoTest;

/// <summary>
/// Provides a mechanism to convert an <see cref="ExpandoObject"/> into an observable JSON-compatible value.
/// </summary>
/// <remarks>This converter is typically used in scenarios where dynamic JSON-like objects need to be wrapped in
/// an observable structure. The <see cref="Convert"/> method transforms an <see cref="ExpandoObject"/> into an instance
/// of <c>ObservableJsonValue</c>. The <see cref="ConvertBack"/> method is not implemented and will throw a <see
/// cref="NotImplementedException"/> if called.</remarks>
public class ObservableJsonConverter : IValueConverter
{
	/// <summary>
	/// Converts the specified value to an instance of <see cref="ObservableJsonValue"/> if the value is an <see
	/// cref="ExpandoObject"/>.
	/// </summary>
	/// <param name="value">The value to convert. If the value is not an <see cref="ExpandoObject"/>, the method returns <see
	/// langword="null"/>.</param>
	/// <param name="targetType">The type to convert to. This parameter is not used in the current implementation.</param>
	/// <param name="parameter">An optional parameter for the conversion. This parameter is not used in the current implementation.</param>
	/// <param name="culture">The culture information to use during the conversion. This parameter is not used in the current implementation.</param>
	/// <returns>An instance of <see cref="ObservableJsonValue"/> if <paramref name="value"/> is an <see cref="ExpandoObject"/>;
	/// otherwise, <see langword="null"/>.</returns>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is ExpandoObject obj)
		{
			return new ObservableJsonValue(obj);
		}

		return null;
	}

	/// <summary>
	/// Converts a value back to its source type in a data binding scenario.
	/// </summary>
	/// <param name="value">The value produced by the binding target to be converted back.</param>
	/// <param name="targetType">The type to which the value should be converted.</param>
	/// <param name="parameter">An optional parameter to use during the conversion process.</param>
	/// <param name="culture">The culture to use during the conversion.</param>
	/// <returns>The converted value, or <see langword="null"/> if the conversion cannot be performed.</returns>
	/// <exception cref="NotImplementedException">This method is not implemented.</exception>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
