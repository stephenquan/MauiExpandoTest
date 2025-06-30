namespace MauiExpandoTest;

public partial class MainPage : ContentPage
{
	//public MyObject01Simple Properties { get; } = new();
	//public MyObject02SimpleNested Properties { get; } = new();
	//public MyObject03OneWayNested Properties { get; } = new();
	//public MyObject04TwoWayNested Properties { get; } = new();
	public MyObject05WithArray Properties { get; } = new();

	public MainPage()
	{
		//Properties.Dynamic.Count = 0;
		//Properties.Dynamic.Hello = "Hello, World!";
		//Properties.Dynamic.Welcome = "Welcome to .NET Multi-platform App UI";

		Properties.Json = """
            {
                "Count": 0,
                "Hello": "Hello, World!",
                "Welcome": "Welcome to .NET Multi-platform App UI",
                "Extra": {
                    "Nested": {
                        "NumberValue": 42,
                        "TextValue": "Nested Text"
                    }
                },
                "People": [
                    {"Name": "John Doe", "Age": 30},
                    {"Name": "Jane Smith", "Age": 25},
                    {"Name": "Alice Johnson", "Age": 28}
                ]
            }
            """;

		BindingContext = this;

		InitializeComponent();
	}

	void OnCounterClicked(object? sender, EventArgs e)
	{
		Properties.Dynamic.Count++;
		Properties.Dynamic.Hello += "!";
		Properties.Dynamic.Welcome += "!";

		#region MyObject02 and above
		Properties.Dynamic.Extra.Nested.NumberValue++;
		#endregion

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}
