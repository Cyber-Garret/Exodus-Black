using Neira.Bot.UI.ViewModel;

using System.Windows;

namespace Neira.Bot.UI.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainViewModel();
		}
	}
}
