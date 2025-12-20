using CoffeeShop.Service;
using CoffeeShop.Service.Interfaces;
using CoffeeShop.ViewModels.StaffVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CoffeeShop.ViewModels.AdminVM;

namespace CoffeeShop.View.Admin
{
    /// <summary>
    /// Interaction logic for DepotManagementPage.xaml
    /// </summary>
    public partial class DepotManagementPage : Page
    {
        public DepotManagementPage()
        {
            InitializeComponent();
            IDialogService dialogService = new WindowService();
            this.DataContext = new AdminDepotViewModel(dialogService);
        }
    }
}
