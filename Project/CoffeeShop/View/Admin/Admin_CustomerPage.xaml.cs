using CoffeeShop.ViewModels.AdminVM;
using System.Windows.Controls;

namespace CoffeeShop.View.Admin
{
    /// <summary>
    /// Interaction logic for Admin_CustomerPage.xaml
    /// </summary>
    public partial class AdminCustomerManagementPage : Page
    {
        public AdminCustomerManagementPage()
        {
            InitializeComponent();
            this.DataContext = new AdminCustomerViewModel();
        }
    }
}
