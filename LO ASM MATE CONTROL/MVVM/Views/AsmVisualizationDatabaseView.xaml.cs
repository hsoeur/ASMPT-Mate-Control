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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LO_ASM_MATE_CONTROL.MVVM.Views
{
    /// <summary>
    /// Interaction logic for AsmVisualizationDatabaseView.xaml
    /// </summary>
    public partial class AsmVisualizationDatabaseView : System.Windows.Controls.UserControl
    {
        public AsmVisualizationDatabaseView()
        {
            InitializeComponent();
        }

        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (DataContext is AsmVisualizationDatabaseViewModel viewModel)
                {
                    viewModel.SearchKeyword = SearchTextBox.Text;
                }

            }
        }
    }
}
