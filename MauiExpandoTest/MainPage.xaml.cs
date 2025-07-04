namespace MauiExpandoTest;

/// <summary>
/// Represents the main page of the application, providing dynamic properties for data binding and user interaction.
/// </summary>
/// <remarks>The <see cref="MainPage"/> class serves as the entry point for the application's user interface. It
/// utilizes dynamic properties, which are populated from a JSON structure, to enable flexible data binding and dynamic
/// updates. The page includes a button that increments a counter and updates various properties when clicked.  The <see
/// cref="Properties"/> object is dynamically populated with nested data, including a counter, greeting messages, a
/// person object, and a list of countries. These properties can be bound to UI elements for dynamic updates.</remarks>
public partial class MainPage : ContentPage
{
	/// <summary>
	/// Gets or sets a collection of dynamic properties associated with the object.
	/// </summary>
	/// <remarks>This property allows for flexible storage of additional metadata or attributes. The structure and
	/// content of the dynamic object depend on the specific use case.</remarks>
	public dynamic Properties { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MainPage"/> class.
	/// </summary>
	/// <remarks>This constructor sets up the <see cref="MainPage"/> by initializing its properties and binding
	/// context. It uses a JSON string to populate dynamic properties via an <see cref="ExpandoObjectHelper"/> helper.
	/// Additionally, it configures data bindings for UI elements, such as the <see cref="CounterBtn"/> button.</remarks>
	public MainPage()
	{
		//Properties = new ExpandoObject();
		//Properties.Count = 0;
		//Properties.Hello = "Hello, World!";
		//Properties.Welcome = "Welcome to \n.NET Multi-platform App UI";
		//Properties.Nested = new ExpandoObject();
		//Properties.Nested.Person = new ExpandoObject();
		//Properties.Nested.Person.Name = "John Doe";
		//Properties.Nested.Person.Age = 30;
		//Properties.Nested.Person.IsEmployed = true;
		//Properties.Countries = new List<ExpandoObject>() { new ExpandoObject(), new ExpandoObject(), new ExpandoObject() };
		//Properties.Countries[0].Name = "USA";
		//Properties.Countries[0].Population = 347275807;
		//Properties.Countries[1].Name = "France";
		//Properties.Countries[1].Population = 66650804;
		//Properties.Countries[2].Name = "Australia";
		//Properties.Countries[2].Population = 26974026;

		string json = """
        {
            "Count": 0,
            "Hello": "Hello, World!",
            "Welcome": "Welcome to \n.NET Multi-platform App UI",
            "Nested": {
                "Person": {
                    "Name": "John Doe",
                    "Age": 30,
                    "IsEmployed": true
                }
            },
            "Countries": [
                { "Name": "USA", "Population": 347275807 },
                { "Name": "France", "Population": 66650804 },
                { "Name": "Australia", "Population": 26974026 }
            ]
        }
        """;

		Properties = ExpandoObjectHelper.ConvertFromJson(json);

		BindingContext = this;

		InitializeComponent();

		CounterBtn.SetBinding(Button.TextProperty, new Binding("[Count]", stringFormat: "Clicked {0} times!"));
	}

	/// <summary>
	/// Handles the counter button click event, updating various properties and announcing the button text.
	/// </summary>
	/// <remarks>This method increments counters, appends exclamation marks to string properties, toggles boolean
	/// values,  and updates nested object properties. It also uses <see cref="SemanticScreenReader.Announce"/> to announce
	/// the text of the counter button.</remarks>
	/// <param name="sender">The source of the event. Can be <see langword="null"/>.</param>
	/// <param name="e">The event data associated with the click event.</param>
	void OnCounterClicked(object? sender, EventArgs e)
	{
		Properties.Count++;

		Properties.Hello += "!";
		Properties.Welcome += "!";
		Properties.Nested.Person.Name += "!";
		Properties.Nested.Person.Age++;
		Properties.Nested.Person.IsEmployed = !Properties.Nested.Person.IsEmployed;

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}
