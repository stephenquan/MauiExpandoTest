using CommunityToolkit.Maui.Markup;
using SQuan.Helpers.Maui.Mvvm;

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
	/// Gets or sets the selected date and time value.
	/// </summary>
	[BindableProperty, NotifyPropertyChangedFor(nameof(TimePart))]
	public partial DateTime? Value { get; set; } = null;

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

	int changing = 0;

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
		};

		var timePicker = new Microsoft.Maui.Controls.TimePicker()
		{
			Margin = new Thickness(0, 0, 0, -13),
		};

		var clearButton = new Button()
		{
			Text = "×",
			FontSize = 20,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Command = new Command(() =>
			{
				Value = (DateTime?)null;
			}),
		};

		this.PropertyChanged += (s, e) =>
		{
			switch (e.PropertyName)
			{
				case nameof(Value):
					if (changing == 0)
					{
						changing++;
						if (Value is DateTime dateTime)
						{
							datePicker.Date = dateTime - dateTime.TimeOfDay;
							timePicker.Time = dateTime.TimeOfDay;
						}
						else
						{
							datePicker.Date = DateTime.Today;
							timePicker.Time = TimeSpan.Zero;
						}
						changing--;
					}
					break;
			}
		};

		datePicker.DateSelected += (s, e) =>
		{
			if (changing == 0)
			{
				changing++;
				if (Value is DateTime dateTime)
				{
					Value = datePicker.Date - datePicker.Date.TimeOfDay + dateTime.TimeOfDay;
				}
				else
				{
					Value = datePicker.Date - datePicker.Date.TimeOfDay + timePicker.Time;
				}
				changing--;
			}
		};

		timePicker.TimeSelected += (s, e) =>
		{
			if (changing == 0)
			{
				changing++;
				if (Value is DateTime dateTime)
				{
					Value = dateTime - dateTime.TimeOfDay + timePicker.Time;
				}
				else
				{
					datePicker.Date = DateTime.Today;
					Value = DateTime.Today + timePicker.Time;
				}
				changing--;
			}
		};

		clearButton.Bind(
			Button.OpacityProperty,
			static (MyDateTimePicker t) => t.Value,
			source: this,
			convert: (DateTime? v) => v is null ? 0 : 1.0);

		this.Content = new HorizontalStackLayout()
		{
			Spacing = 10,
			Children =
			{
				datePicker,
				timePicker,
				clearButton
			}
		};
	}
}
