using System.Windows;
using JuliusSweetland.OptiKids.Properties;
using JuliusSweetland.OptiKids.Services;
using JuliusSweetland.OptiKids.UI.ViewModels;
using MahApps.Metro.Controls;

namespace JuliusSweetland.OptiKids.UI.Windows
{
    /// <summary>
    /// Interaction logic for ManagementWindow.xaml
    /// </summary>
    public partial class ManagementWindow : MetroWindow
    {
        public ManagementWindow(IAudioService audioService)
        {
            InitializeComponent();

            //Instantiate ManagementViewModel and set as DataContext of ManagementView
            var managementViewModel = new ManagementViewModel(audioService);
            this.ManagementView.DataContext = managementViewModel;
        }
    }
}
