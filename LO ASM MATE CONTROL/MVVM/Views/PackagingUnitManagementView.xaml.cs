using LO_ASM_MATE_CONTROL.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LO_ASM_MATE_CONTROL.MVVM.Views
{
    /// <summary>
    /// Interaction logic for PackagingUnitManagementView.xaml
    /// </summary>
    public partial class PackagingUnitManagementView : UserControl
    {
        public PackagingUnitManagementView()
        {
            InitializeComponent();
        }

        private void LogPannelTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ScrollViewerLog.ScrollToBottom();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is PackagingUnitManagementViewModel viewModel)
                {
                    viewModel.ReadPu();
                }
            }
        }
    }
}
