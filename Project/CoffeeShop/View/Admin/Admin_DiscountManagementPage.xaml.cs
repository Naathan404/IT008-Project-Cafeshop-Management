using CoffeeShop.Service;
using CoffeeShop.Service.Interfaces;
using CoffeeShop.ViewModels.AdminVM;
using CoffeeShop.ViewModels.StaffVM;
using System.Windows.Controls;

namespace CoffeeShop.View.Admin
{
    public partial class DiscountManagementPage : Page
    {
        public DiscountManagementPage()
        {
            InitializeComponent();
            IDialogService dialogService = new WindowService();
            this.DataContext = new AdminDiscountViewModel(dialogService);
        }
    }
}
