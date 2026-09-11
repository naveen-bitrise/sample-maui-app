using SampleMauiApp.Core;

namespace SampleMauiApp;

public partial class MainPage : ContentPage
{
	readonly CounterService counter;

	public MainPage(CounterService counter)
	{
		InitializeComponent();
		this.counter = counter;
		CounterBtn.Text = counter.Describe();
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		counter.Increment();
		CounterBtn.Text = counter.Describe();
		SemanticScreenReader.Announce(CounterBtn.Text);
	}

	private void OnResetClicked(object? sender, EventArgs e)
	{
		counter.Reset();
		CounterBtn.Text = counter.Describe();
		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}
