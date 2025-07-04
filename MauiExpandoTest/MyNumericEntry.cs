using SQuan.Helpers.Maui.Mvvm;

namespace MauiExpandoTest;

/// <summary>
/// Represents a numeric entry field that synchronizes its text with a numeric value.
/// </summary>
/// <remarks>The <see cref="MyNumericEntry"/> class extends the functionality of a standard entry field by 
/// providing automatic synchronization between the Text property and the <see cref="Value"/> property.
/// When the text changes, the <see cref="Value"/> property is updated to reflect the parsed numeric value. Similarly,
/// when the <see cref="Value"/> property changes, the Text property is updated to reflect the string
/// representation of the value.  The <see cref="Value"/> property supports multiple numeric types, including <see
/// langword="int"/>, <see langword="double"/>,  <see langword="long"/>, and <see langword="float"/>. If the text cannot
/// be parsed into a numeric value, the <see cref="Value"/> property is set to <see langword="null"/>.</remarks>
public partial class MyNumericEntry : Entry
{
	/// <summary>
	/// Gets or sets the value associated with this property.
	/// </summary>
	[BindableProperty] public partial object? Value { get; set; } = null;

	int changing = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="MyNumericEntry"/> class.
	/// </summary>
	/// <remarks>This constructor sets up property change handling for the Text <see
	/// cref="Value"/> properties. Changes to the Text property are automatically parsed into a numeric value
	/// and assigned to the <see cref="Value"/> property. Similarly, changes to the <see cref="Value"/> property are
	/// reflected in the Text property as a string representation. If parsing of Text fails,
	/// the <see cref="Value"/> property is set to <see langword="null"/>.</remarks>
	public MyNumericEntry()
	{
		PropertyChanged += (sender, e) =>
		{
			switch (e.PropertyName)
			{
				case nameof(Text):
					if (changing == 0)
					{
						changing++;
						if (string.IsNullOrEmpty(Text))
						{
							Value = null;
						}
						else if (int.TryParse(Text, out int intValue))
						{
							Value = intValue;
						}
						else if (double.TryParse(Text, out double doubleValue))
						{
							Value = doubleValue;
						}
						else if (long.TryParse(Text, out long longValue))
						{
							Value = longValue;
						}
						else if (float.TryParse(Text, out float floatValue))
						{
							Value = floatValue;
						}
						else
						{
							// If parsing fails, we can either set Value to null or keep the previous value.
							// Here we choose to set it to null.
							Value = null;
						}
						changing--;
					}
					break;
				case nameof(Value):
					if (changing == 0)
					{
						changing++;
						Text = (Value is null) ? string.Empty : Value.ToString();
						changing--;
					}
					break;
			}
		};
	}
}
