using System.Windows.Controls;
using CoffeeShop.ViewModels.AdminVM;

namespace CoffeeShop.View.Admin
{
    /// <summary>
    /// Interaction logic for Admin_DiscountManagementPage.xaml
    /// </summary>
    public partial class DiscountManagementPage : Page
    {
        public DiscountManagementPage()
        {
            InitializeComponent();
            this.DataContext = new AdminDiscountViewModel();
        }
    }
}
