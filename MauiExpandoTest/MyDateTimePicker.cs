using CommunityToolkit.Maui.Markup;

namespace MauiExpandoTest;

/// <summary>
/// A custom date and time picker control that allows users to select both a date and a time.
/// </summary>
/// <remarks>The <see cref="MyDateTimePicker"/> combines a <see cref="Microsoft.Maui.Controls.DatePicker"/> and a 
/// <see cref="Microsoft.Maui.Controls.TimePicker"/> into a single control, enabling users to select a  complete <see
/// cref="DateTime"/> value. It also includes a clear button to reset the selection.  The <see cref="Value"/> property
/// represents the selected date and time, while the <see cref="TimePart"/>  property provides access to the time
/// portion of the selection. Changes to either property automatically  update the other.  This control supports two-way
/// data binding for both the <see cref="Value"/> and <see cref="TimePart"/>  properties, making it suitable for use in
/// MVVM scenarios.</remarks>
public partial class MyDateTimePicker : ContentView
{
	/// <summary>
	/// The bindable property for the <see cref="Value"/> property.
	/// </summary>
	public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(DateTime?), typeof(MyDateTimePicker),
		defaultValueCreator: (b) => ((MyDateTimePicker)b).OnCreateDefaultValue());


	/// <summary>
	/// Gets or sets the selected date and time value.
	/// </summary>
	//[BindableProperty, NotifyPropertyChangedFor(nameof(TimePart))]
	public partial DateTime? Value { get; set; } = null;


	bool creatingDefaultValue = false;

	public partial DateTime? Value
	{
		get => creatingDefaultValue ? field : (DateTime?)GetValue(ValueProperty);
		set => SetValue(ValueProperty, field = value);
	}

	object? OnCreateDefaultValue()
	{
		creatingDefaultValue = true;
		var result = Value;
		creatingDefaultValue = false;
		return result;
	}

	/// <summary>
	/// Gets or sets the time component of the <see cref="Value"/> property.
	/// </summary>
	/// <remarks>Setting this property updates the <see cref="Value"/> property by preserving its date component 
	/// and replacing its time component with the specified <see cref="TimeSpan"/> value. If <see cref="Value"/>  is <see
	/// langword="null"/>, it initializes <see cref="Value"/> to today's date combined with the specified time.</remarks>
	internal TimeSpan TimePart
	{
		get => Value?.TimeOfDay ?? TimeSpan.Zero;
		set
		{
			if (Value.HasValue)
			{
				Value = Value.Value.Subtract(Value.Value.TimeOfDay).Add(value); ;
			}
			else
			{
				Value = DateTime.Today + value;
			}
			OnPropertyChanged(nameof(Value));
			OnPropertyChanged(nameof(TimePart));
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MyDateTimePicker"/> class.
	/// </summary>
	/// <remarks>This custom date-time picker combines a <see cref="Microsoft.Maui.Controls.DatePicker"/> and a 
	/// <see cref="Microsoft.Maui.Controls.TimePicker"/> to allow users to select both a date and a time. It also includes
	/// a clear button to reset the selected value.  The <see cref="Microsoft.Maui.Controls.DatePicker"/> is bound to the
	/// <c>Value</c> property,  representing the date portion of the selection, while the <see
	/// cref="Microsoft.Maui.Controls.TimePicker"/>  is bound to the <c>TimePart</c> property, representing the time
	/// portion.  The clear button resets the <c>Value</c> property to <see langword="null"/> and hides itself when  no
	/// value is selected.</remarks>
	public MyDateTimePicker()
	{
		var datePicker = new Microsoft.Maui.Controls.DatePicker()
		{
			Format = "D",
			VerticalOptions = LayoutOptions.Center,
		};

		datePicker.SetBinding(
			Microsoft.Maui.Controls.DatePicker.DateProperty,
			static (MyDateTimePicker t) => t.Value,
			BindingMode.TwoWay,
			source: this);

		var timePicker = new Microsoft.Maui.Controls.TimePicker()
		{
			Format = "t",
			VerticalOptions = LayoutOptions.Center,
		};

		timePicker.SetBinding(
			Microsoft.Maui.Controls.TimePicker.TimeProperty,
			static (MyDateTimePicker t) => t.TimePart,
			BindingMode.TwoWay,
			source: this);

		var clearButton = new Button()
		{
			Text = "X",
			MinimumHeightRequest = 22,
			MinimumWidthRequest = 22,
			Command = new Command(() =>
			{
				Value = (DateTime?)null;
			}),
		}.CenterVertical();

		clearButton.Bind(
			Button.IsVisibleProperty,
			static (MyDateTimePicker t) => t.Value,
			source: this,
			convert: (DateTime? value) => value is not null);

		clearButton.SetBinding(
			Button.IsVisibleProperty,
			static (MyDateTimePicker t) => t.Value,
			BindingMode.OneWay,
			source: this,
			converter: new CommunityToolkit.Maui.Converters.IsNotNullConverter());

		this.Content = new HorizontalStackLayout()
		{
			Spacing = 10,
			Children =
			{
				datePicker,
				timePicker,
				clearButton,
			}
		};
	}
}
