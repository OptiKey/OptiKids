using System;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.OptiKids.Properties;
using JuliusSweetland.OptiKids.UI.ViewModels;

namespace JuliusSweetland.OptiKids.UI.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void LookupQuizFile_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "Quiz Files (*.json)|*.json"
            };
            var result = dlg.ShowDialog();
            if (result == true)
            {
                Settings.Default.QuizFile = dlg.FileName;
            }
        }
    }
}
