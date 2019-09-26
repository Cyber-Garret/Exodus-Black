using Neira.Bot.UI.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Neira.Bot.UI.View
{
	/// <summary>
	/// Interaction logic for ClanWindow.xaml
	/// </summary>
	public partial class ClanWindow : Window
	{
		public Clan Clan { get; set; }
		public ClanWindow(Clan c)
		{
			InitializeComponent();
			Clan = c;
			DataContext = Clan;
		}

		private void Accept_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
