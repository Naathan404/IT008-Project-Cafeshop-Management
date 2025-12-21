using System.Windows;
using System.Windows.Controls;
using CoffeeShop.ViewModels.GeneralVM;

namespace CoffeeShop.View.General
{
    /// <summary>
    /// Interaction logic for AccountPage.xaml
    /// </summary>
    public partial class AccountPage : Page
    {
        public AccountPage(CoffeeShop.Models.Staff staff, Window parentWindow)
        {
            InitializeComponent();
            this.DataContext = new AccountInfoViewModel(staff, parentWindow);
        }
    }
}
