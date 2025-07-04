using SQuan.Helpers.Maui.Mvvm;

namespace MauiExpandoTest;

/// <summary>
/// Represents a text entry control that synchronizes its Text property with a bindable
/// <see cref="Value"/> property.
/// </summary>
/// <remarks>The <see cref="Value"/> property provides a nullable string alternative to the Text
/// property,  allowing for easier binding scenarios where null values are significant. Changes to either
/// Text or <see cref="Value"/> automatically update the other property.</remarks>
public partial class MyTextEntry : Entry
{
	/// <summary>
	/// Gets or sets the current value associated with the property.
	/// </summary>
	[BindableProperty] public partial string? Value { get; set; } = null;

	int changing = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="MyTextEntry"/> class.
	/// </summary>
	/// <remarks>This constructor sets up property change synchronization between the Text and <see cref="Value"/>
	/// properties. Changes to one property automatically update the other, ensuring consistency.</remarks>
	public MyTextEntry()
	{
		PropertyChanged += (sender, e) =>
		{
			switch (e.PropertyName)
			{
				case nameof(Text):
					if (changing == 0)
					{
						changing++;
						Value = !string.IsNullOrEmpty(Text) ? Text : null;
						changing--;
					}
					break;
				case nameof(Value):
					if (changing == 0)
					{
						changing++;
						Text = (Value is null) ? string.Empty : Value;
						changing--;
					}
					break;
			}
		};
	}
}
