using LO_ASM_MATE_CONTROL.Services.Providers;
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
using System.Windows.Shapes;

namespace LO_ASM_MATE_CONTROL.MVVM.Views
{
    /// <summary>
    /// Interaction logic for SearchArticleWindow.xaml
    /// </summary>
    public partial class SearchArticleWindow : Window
    {
        private readonly IPackagingUnitProvider _packagingUnitProvider;

        public SearchArticleWindow(IPackagingUnitProvider packagingUnitProvider)
        {
            InitializeComponent();
            _packagingUnitProvider = packagingUnitProvider;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string codeArticle = ArticleCodeTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(codeArticle))
            {
                ResultTextBlock.Text = "❌ Veuillez entrer un code article.";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            var packagingUnits = _packagingUnitProvider.GetAllPackagingUnits();
            int totalQuantity = packagingUnits
                .Where(pu => pu.ComponentBarcode.Equals(codeArticle, System.StringComparison.OrdinalIgnoreCase))
                .Where(pu => !(pu.UID ?? string.Empty).Trim().StartsWith("DMY", StringComparison.OrdinalIgnoreCase))
                .Sum(pu => pu.Quantity);

            if (totalQuantity > 0)
            {
                ResultTextBlock.Text = $"✅ Quantité totale : {totalQuantity}";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                ResultTextBlock.Text = "❌ Aucun stock trouvé.";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
