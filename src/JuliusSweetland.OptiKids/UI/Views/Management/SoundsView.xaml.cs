using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.OptiKids.Properties;

namespace JuliusSweetland.OptiKids.UI.Views.Management
{
    /// <summary>
    /// Interaction logic for SoundsView.xaml
    /// </summary>
    public partial class SoundsView : UserControl
    {
        public SoundsView()
        {
            InitializeComponent();
        }

        private void LookupPronunciationFile_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "Pronunciation Files (*.json)|*.json"
            };
            var result = dlg.ShowDialog();
            if (result == true)
            {
                Settings.Default.PronunciationFile = dlg.FileName;
            }
        }
    }
}
