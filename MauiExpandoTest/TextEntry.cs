using SQuan.Helpers.Maui.Mvvm;

namespace MauiExpandoTest;

public partial class TextEntry : Entry
{
    [BindableProperty] public partial string? Value { get; set; } = null;

    int Lock { get; set; } = 0;

    public TextEntry()
    {
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Text):
                    if (Lock == 0)
                    {
                        Lock++;
                        Value = !string.IsNullOrEmpty(Text) ? Text : null;
                        Lock--;
                    }
                    break;
                case nameof(Value):
                    if (Lock == 0)
                    {
                        Lock++;
                        Text = (Value is null) ? string.Empty : Value;
                        Lock--;
                    }
                    break;
            }
        };
    }
}
